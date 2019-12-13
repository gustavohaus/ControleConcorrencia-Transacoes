using System;
using System.Collections.Generic;
using System.Text;

namespace ServerInfra.Models
{
    public class QuartoModel
    {
        public int Id { get; set; }
        public string Identificador { get; set; }
        public DateTime Date { get; set; }
        public bool IsReservado { get; set; } = false;

        public QuartoModel(int id, string identificador, DateTime date)
        {
            Id = id;
            Identificador = identificador;
            Date = date;
        }

        public QuartoModel Reservar()
        {
            IsReservado = true;

            return this;
        }


    }
}
