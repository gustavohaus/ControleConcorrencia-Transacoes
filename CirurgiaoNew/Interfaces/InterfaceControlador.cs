using Refit;
using ServerInfra.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CirurgiaoNew.Interfaces
{
    public interface InterfaceControlador
    {
        [Get("/api/ObterStatusTransacaoCirurgiao")]
        Task<EStatus> ConsultarStatusCirurgiao(string guid);

        // public EStatus ConsultarStatusTransacao(string guid)
    }
}
