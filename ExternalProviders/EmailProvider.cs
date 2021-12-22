using Nancy;
using Nancy.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalProviders
{
    public class EmailProvider : NancyModule
    {
        public EmailProvider()
        {
            Post("/api/sendemail/", parameters =>
            {
                var item = this.Bind<EmailRequest>();

                var response = new EmailResponse
                {
                    CustomerId = item.CustomerId,
                    EmailSent = true
                };

                return Response.AsJson(response);
            });
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
}
