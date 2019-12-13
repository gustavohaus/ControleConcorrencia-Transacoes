using Refit;
using ServerInfra.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnestesistaNew.Interface
{
    public interface InterfaceControlador
    {
        [Get("/api/ObterStatusTransacaoAnestesista")]
        Task<EStatus> ConsultarStatusAnestesista(string identificador);
    }
}


