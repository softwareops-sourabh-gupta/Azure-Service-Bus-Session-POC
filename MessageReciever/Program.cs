using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace MessageReciever
{
    class Program
    {
        static void Main(string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }
        static async Task MainAsync(string[] args)
        {

            Console.WriteLine("Hello World!");
            var connectionString = "********************************************************************************************************";
            var queueName = "PocQueue";
            var client = new ServiceBusClient(connectionString);

            ServiceBusSessionReceiver receiver = null;
            do
            {
                try
                {
                    var cts = new CancellationTokenSource();
                    var token = cts.Token;
                    cts.CancelAfter(TimeSpan.FromSeconds(15));
                    receiver = await client.AcceptNextSessionAsync(queueName, new ServiceBusSessionReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock }, token);
                }
                catch (Exception ex)
                {
                    receiver = null;
                }
                if (receiver != null)
                {
                    Console.WriteLine($"reciever started for session id : {receiver.SessionId}.");

                    ServiceBusReceivedMessage message = null;
                    do
                    {
                        message = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(1));
                        if (message != null)
                        {
                            try
                            {
                                Console.WriteLine($"Received: '{message.Body}', Ack: Complete");
                                await receiver.CompleteMessageAsync(message);
                            }
                            catch
                            {
                                Console.WriteLine($"Received: '{message.Body}', Ack: Abondon");
                                await receiver.AbandonMessageAsync(message);
                            }
                        }
                    }
                    while (message != null);
                    await receiver.CloseAsync();
                }
            }
            while (receiver != null);
        }

    }
}
