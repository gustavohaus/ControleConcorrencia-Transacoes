using System;
using System.Collections.Generic;
using System.Text;

namespace ServerInfra.Models
{
    public class ReservaModel
    {
        public AnestesistaModel Anestesista { get; set; }
        public CirurgiaoModel Cirurgiao { get; set; }
        public QuartoModel Quarto { get; set; }
    }
}
