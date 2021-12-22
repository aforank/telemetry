using Azure.Messaging.ServiceBus;
using System.Runtime.InteropServices;

public class WorkerServiceBus : IHostedService
{
    private readonly ServiceBusClient serviceBusClient;
    private readonly IHttpClientFactory httpClientFactory;
    private readonly HttpClient _httpClient;
    private ServiceBusProcessor processor;

    public WorkerServiceBus(ServiceBusClient serviceBusClient, IHttpClientFactory httpClientFactory)
    {
        this.serviceBusClient = serviceBusClient;
        this.httpClientFactory = httpClientFactory;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var options = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = 2
        };

        processor = this.serviceBusClient.CreateProcessor("payments", options);

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

        var payment = new PaymentRequest();
        payment.CustomerId = "";
        payment.Amount = 1;
        await _httpClient.PostAsJsonAsync<PaymentRequest>("http://localhost:12345/api/reliable/processpayment", payment);

        var sender = this.serviceBusClient.CreateSender("notifications");
        var message = new ServiceBusMessage(arg.Message.Body);
        await sender.SendMessageAsync(message);

        await arg.CompleteMessageAsync(arg.Message);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await processor.StopProcessingAsync();
        await processor.DisposeAsync();
    }
}

public class PaymentRequest
{
    public string CustomerId { get; set; }
    public double Amount { get; set; }
}

public class PaymentResponse
{
    public string CustomerId { get; set; }
    public bool PaymentSucceeded { get; set; }
}