using Contract;
using Contract.Config;
using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Contract.Contracts;

namespace Contract.Services
{
    public class SenderService : ISenderService
    {
        private readonly IBus _bus;

        public SenderService(IBus bus)
        {
            _bus = bus;
        }

        public async Task Publish(Customer customer)
        {
            await _bus.Publish<ICustomer>(new
            {
                customer.Id,
                customer.Name,
            });
        }


        public async Task Send(Customer customer)
        {
            var endpoint = await _bus.GetSendEndpoint(new Uri("queue:testqueue1"));

            await endpoint.Send<ICustomer>(new
            {
                customer.Id,
                customer.Name
            });
        }
    }
}
