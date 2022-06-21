using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace WebApplication2.Services
{
    public class MQService : IMQService
    {
        public string ConsumeMQ()
        {
            var connectioFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                VirtualHost = "/",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            var connection = connectioFactory.CreateConnection();

            var channel = connection.CreateModel();

            var consumer = new EventingBasicConsumer(channel);
            string message = string.Empty;
            consumer.Received += (sender, args) =>
            {
                message = Encoding.UTF8.GetString(args.Body.ToArray());
            };

            channel.BasicConsume("fanout.queue1", false, consumer);
            return message;
        }

        public void CreateMQ(string message)
        {
            var connectioFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                VirtualHost = "/",
                Port = 5672,
                UserName = "guest",
                Password = "guest"
            };

            using var connection = connectioFactory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.ExchangeDeclare("fanout.test", ExchangeType.Fanout, false, false);


            channel.QueueDeclare("fanout.queue1", false, false, true);


            channel.QueueBind("fanout.queue1", "fanout.test", string.Empty);


            channel.BasicPublish("fanout.test", string.Empty, null, Encoding.UTF8.GetBytes(message));

        }
    }
}
