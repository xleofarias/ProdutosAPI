using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkerService.Events
{
    public record UserCreatedEvent(int UserId, string Name, DateTime CreatedAt);
}

///(Dica de Sênior: No mundo corporativo, nós geralmente criamos um terceiro projeto só de "Class Library" (Bilioteca de Classes) chamado Contratos, e tanto a API quanto o Worker instalam ele para não precisarmos copiar e colar código. Mas para aprender, o copy/paste funciona perfeitamente).
