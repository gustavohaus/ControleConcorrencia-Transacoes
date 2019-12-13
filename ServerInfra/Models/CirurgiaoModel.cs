using System;
using System.Collections.Generic;
using System.Text;

namespace ServerInfra.Models
{
    public class CirurgiaoModel
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Especialidade { get; set; }
        public DateTime Date { get; set; }
        public bool IsReservado { get; set; } = false;

        public CirurgiaoModel(int id, string nome, string especialidade, DateTime date)
        {
            Id = id;
            Nome = nome;
            Especialidade = especialidade;
            Date = date;
        }

        public CirurgiaoModel Reservar()
        {
            IsReservado = true;

            return this;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
