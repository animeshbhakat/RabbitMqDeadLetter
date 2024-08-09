using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMqDeadLetterPoc
{
    private readonly IModel _model;

    public RabbitMqDeadLetterPoc(IModel model)
    {
        _model = model;
    }
    [FunctionName("RabbitMqDeadLetterPoc")]
    public async Task Run(
        [RabbitMQTrigger("dummyQueue", ConnectionStringSetting = "RabbitMqConnectionString")] BasicDeliverEventArgs message,
        ILogger log)
    {
        var deliveryTag = message.DeliveryTag;

        try
        {
            // Process the message
            var body = message.Body.ToArray();
            var messageBody = Encoding.UTF8.GetString(body);
            log.LogInformation($"Processing message: {messageBody}");

            // Simulate message processing logic
            if (messageBody.Contains("reject"))
            {
                throw new Exception("Simulated processing failure");
            }

            // Acknowledge successful processing
            _model.BasicAck(deliveryTag, false);
        }
        catch (Exception ex)
        {
            log.LogError($"Exception occurred: {ex.Message}");

            // Reject and optionally requeue the message
            _model.BasicPublish("dlx", "deadLetter");
        }
    }
}
