using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.RabbitMQ;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMqDeadLetterFunctionApp.Functions;

public class RabbitMqDeadLetterPocWithChannel
{
    [FunctionName("RabbitMqDeadLetterPocWithChannel")]
    public static void Run([RabbitMQTrigger("primaryQueue", ConnectionStringSetting = "RabbitMqConnectionString")] byte[] myQueueItem, ILogger log)
    {
        try
        {
            var messageContent = Encoding.UTF8.GetString(myQueueItem);
            log.LogInformation($"C# Queue trigger function processed: {messageContent}");

            // Simulate message processing
            if (!string.IsNullOrWhiteSpace(messageContent))
            {
                throw new Exception("Simulated processing error");
            }

        }
        catch (Exception ex)
        {
            log.LogError($"Error processing message: {ex.Message}");
            // Return the message to send it to the dead-letter queue
            DeadLetterMessage(myQueueItem, log);
        }
    }

    private static void DeadLetterMessage(byte[] message, ILogger log)
    {
        var factory = new ConnectionFactory() { Uri = new Uri(Environment.GetEnvironmentVariable("RabbitMqConnectionString")) };

        using (var connection = factory.CreateConnection())
        using (var channel = connection.CreateModel())
        {
            var properties = channel.CreateBasicProperties();
            properties.Persistent = true;

            // Declare dead-letter queue if not already declared
            channel.QueueDeclare(queue: "deadLetterQueue", durable: true, exclusive: false, autoDelete: false, arguments: null);

            // Publish to dead-letter queue
            channel.BasicPublish(exchange: "deadLetterExchange", routingKey: "deadLetter", basicProperties: properties, body: message);

            log.LogInformation("Message dead-lettered to myqueue_deadletter");
        }
    }
}
