using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts.Events
{
    public record class ProductCreatedEvent(int ProductId, string Name, DateTime CreatedAt);
}
