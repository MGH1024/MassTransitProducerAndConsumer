using Contract;
using Contract.Contracts;

namespace Contract.Services
{
    public interface ISenderService
    {
        Task Publish(Customer customer);
        Task Send(Customer customer);
    }
}