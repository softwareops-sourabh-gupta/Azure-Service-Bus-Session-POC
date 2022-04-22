using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Messaging.ServiceBus;
using System.Text;

namespace MessageSender
{
    public class SendMessage
    {
        [FunctionName("Send")]
        public  async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            var connectionString = "***********************************************************************************************";
            var queueName = "PocQueue";
            var client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender(queueName);

            try
            {
                
                // Use the producer client to send the batch of messages to the Service Bus queue
                for (int i = 1; i <= 10; i++)
                {
                    var message = new ServiceBusMessage(body: Encoding.UTF8.GetBytes(i.ToString()));
                    if (i % 2 == 0)
                        message.SessionId = "even";
                    else
                        message.SessionId = "odd";
                    //message.MessageId = i.ToString();
                    //message.ScheduledEnqueueTime = DateTime.UtcNow.AddMinutes(5);
                    await sender.SendMessageAsync(message);
                }
               
            }
            finally
            {
                // Calling DisposeAsync on client types is required to ensure that network
                // resources and other unmanaged objects are properly cleaned up.
                await sender.DisposeAsync();
                await client.DisposeAsync();
            }

            return new OkObjectResult("function executed");
        }
    }
}
