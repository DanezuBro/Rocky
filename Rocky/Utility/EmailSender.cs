using Mailjet.Client;
using Mailjet.Client.Resources;
using Microsoft.AspNetCore.Identity.UI.Services;
using Newtonsoft.Json.Linq;

namespace Rocky.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        public MailJetSettings _mailJetSettings {get; set;}

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

    public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return Execute(email, subject, htmlMessage);
        }

        public async Task Execute(string email, string subject, string body)
        {
            _mailJetSettings = _configuration.GetSection("MailJet").Get<MailJetSettings>();

            MailjetClient client = new MailjetClient(_mailJetSettings.ApiKey, _mailJetSettings.SecretKey)
            {
                //Version = ApiVersion.V3_1,
            };
            MailjetRequest request = new MailjetRequest
            {
                Resource = SendV31.Resource,
            }
             .Property(Send.Messages, new JArray {
                     new JObject {
                                {
                                       "From",
                                               new JObject
                                               {
                                                                    {"Email", "scortarudaniel@protonmail.com"},
                                                                    {"Name", "Email trimis din aplicatia Rocky_App"}
                                                 }
                                         }, 
                                         {
                                            "To",
                                                   new JArray {
                                                                    new JObject {
                                                                                               { "Email", email },
                                                                                              {  "Name","RockyApp_User"   }
                                                                                         }
                                                                     }
                                                     }, 
                                                     {"Subject", subject } , 
                                                     { "HTMLPart",body }
                                         }
                      });
            /*MailjetResponse response = */ await client.PostAsync(request);
        }

    }
}
