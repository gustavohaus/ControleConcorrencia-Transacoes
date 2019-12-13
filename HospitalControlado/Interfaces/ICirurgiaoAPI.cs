using Microsoft.AspNetCore.Mvc;
using Refit;
using ServerInfra.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HospitalControlado.Interfaces
{
    public interface ICirurgiaoAPI
    {
        //Refatorar
        [Post("/api/reserva/Cirurgiao")]
        Task<string> ReservarCirurgiao([FromBody] CirurgiaoModel reserva, string guid);

        [Put("/api/efetivar")]
        Task<bool> Commit(string id);

        [Delete("/api/abortar")]
        Task<bool> RollBack(string id);
    }
}
