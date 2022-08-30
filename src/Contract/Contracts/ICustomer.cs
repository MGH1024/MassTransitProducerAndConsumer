using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Contracts
{
    public interface ICustomer
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }
}
