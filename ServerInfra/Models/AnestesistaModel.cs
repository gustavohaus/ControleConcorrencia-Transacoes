using System;
using System.Collections.Generic;
using System.Text;

namespace ServerInfra.Models
{
    public class AnestesistaModel:ICloneable
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        public DateTime Date { get; set; }
        public bool IsReservado { get; set; } = false;

        public AnestesistaModel(int id, string nome, DateTime date)
        {

            Id = id;
            Nome = nome;
            Date = date;

        }

        public AnestesistaModel Reservar()
        {
            IsReservado = true;

            return this;
        }

        public AnestesistaModel Commit()
        {
            IsReservado = true;

            return this;
        }

        public AnestesistaModel Roolback()
        {
            IsReservado = false;

            return this;
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
