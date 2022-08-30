using Contract.Bus;
using Contract.Contracts;
using MassTransit;
using Microsoft.Extensions.Configuration;

namespace Contract.LatestConsumer
{
    public class CustomerConsumer : IConsumer<ICustomer>
    {
        private readonly ICustomerBusControl _customerBus;

        public CustomerConsumer(ICustomerBusControl customerBus)
        {
            _customerBus = customerBus;
        }

        public async Task Consume(ConsumeContext<ICustomer> context)
        {
            await Task.Delay(100);
            await _customerBus.Publish<IPerson>(new
            {
                context.Message.Id,
                context.Message.Name,
            });
            Console.WriteLine(context.Message.Id.ToString());
        }
    }
}
