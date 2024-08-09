using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System;

[assembly: FunctionsStartup(typeof(MyFunctionApp.Startup))]

namespace MyFunctionApp
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // Retrieve connection string from configuration
            var connectionString = Environment.GetEnvironmentVariable("RabbitMqConnectionString");

            builder.Services.AddSingleton(sp =>
            {
                var factory = new ConnectionFactory();
                factory.Uri = new Uri(connectionString); // Set the URI from the connection string
                return factory.CreateConnection();
            });

            builder.Services.AddSingleton(sp =>
            {
                var connection = sp.GetRequiredService<IConnection>();
                return connection.CreateModel(); // Create and return IModel
            });
        }
    }
}
