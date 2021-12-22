using Azure.Messaging.ServiceBus;
using Azure.Storage.Blobs;

public class NotificationWorkerServiceBus : IHostedService
{
    private readonly ServiceBusClient serviceBusClient;
    private readonly BlobServiceClient blobClient;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly ILogger<NotificationWorkerServiceBus> logger;
    private readonly HttpClient _httpClient;
    private ServiceBusProcessor processor;

    public NotificationWorkerServiceBus(ServiceBusClient serviceBusClient, BlobServiceClient blobClient, IHttpClientFactory httpClientFactory, ILogger<NotificationWorkerServiceBus> logger)
    {
        this.serviceBusClient = serviceBusClient;
        this.blobClient = blobClient;
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 2
        };

        processor = this.serviceBusClient.CreateProcessor("notifications", options);

        // configure the message and error handler to use
        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;

        await processor.StartProcessingAsync();
    }

    private Task ErrorHandler(ProcessErrorEventArgs arg)
    {
        return Task.CompletedTask;
    }

    private async Task MessageHandler(ProcessMessageEventArgs arg)
    {
        string body = arg.Message.Body.ToString();

        var blobContainerClient = this.blobClient.GetBlobContainerClient("templates");
        var blobClient = blobContainerClient.GetBlobClient("Email.txt");
        var emailBlob = await blobClient.DownloadContentAsync();

        this.logger.LogInformation("Sending email");

        var email = new EmailRequest();
        email.CustomerId = "";
        email.Content = emailBlob.Value.ToString();
        await _httpClient.PostAsJsonAsync<EmailRequest>("http://localhost:12345/api/sendemail", email);

        await arg.CompleteMessageAsync(arg.Message);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await processor.StopProcessingAsync();
        await processor.DisposeAsync();
    }
}

public class EmailRequest
{
    public string CustomerId { get; set; }
    public string Content { get; set; }
}

public class EmailResponse
{
    public string CustomerId { get; set; }
    public bool EmailSent { get; set; }
}