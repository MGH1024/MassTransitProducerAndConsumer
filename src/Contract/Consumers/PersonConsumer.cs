using MassTransit;

namespace Contract.LatestConsumer
{
    public class PersonConsumer : IConsumer<IPerson>
    {
        public async Task Consume(ConsumeContext<IPerson> context)
        {
            await Task.Delay(100);
            Console.WriteLine(context.Message.Id.ToString());
        }
    }
}
