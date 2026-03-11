using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService.Events
{
    public record ProductCreatedEvent(int ProductId, string Name, DateTime CreatedAt);
}
