using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contract.Utility
{
    public class ExchangeNameFormatter : IEntityNameFormatter
    {
        public string FormatEntityName<T>()
        {
            var entityType = typeof(T);
            return GenerateStandardExchangeName(entityType);
        }

        public static string GetStandardClassName(Type entityType)
        {
            return entityType.IsInterface && entityType.Name.StartsWith('I') ?
                 $"{entityType.Name.Remove(0, 1)}" :
                 entityType.Name;
        }

        private static string GenerateStandardExchangeName(Type entityType)
        {
            return $"{entityType.Namespace}.{GetStandardClassName(entityType)}";
        }
    }
}
