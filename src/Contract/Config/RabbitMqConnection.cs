using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Config
{
    public class RabbitMq
    {
        public RabbitMqConnection ConsumerConnection { get; set; }
        public RabbitMqConnection ProducerConnection { get; set; }
        public string[] ClusterServers { get; set; }
    }

    public class RabbitMqConnection
    {
        public string Host { get; set; }
        public string Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string VirtualHost { get; set; }
        public string ReceiveEndpoint { get; set; }
        public Uri HostAddress
        {
            get
            {
                return new Uri($"rabbitmq://{Username}:{Password}@{Host}:{Port}/{VirtualHost}");
            }
        }
    }
}
