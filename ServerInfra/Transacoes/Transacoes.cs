using ServerInfra.Enum;
using ServerInfra.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerInfra.Transacoes
{
    public class AnestesistaTransaction // Lista ->;
    {
        public Guid Id { get; set; }
        public EStatus Status { get; set; } //(a).
        public DateTime Inicio { get; set; }
        public DateTime Termino { get; set; }
        public AnestesistaModel RegistroAtual { get; set; } //(b).
        public AnestesistaModel RegistroAlterado { get; set; } //(c).

        public AnestesistaTransaction AlterarStatus(EStatus status)
        {
            Status = status;

            return this;
        }

        public AnestesistaTransaction FinalizarTransacao()
        {
            Termino = DateTime.Now;
            return this;
        }
    }

    public class CirurgiaoTransaction //Lista ->
    {
        public Guid Id { get; set; }
        public EStatus Status { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Termino { get; set; }
        public CirurgiaoModel RegistroAtual { get; set; }
        public CirurgiaoModel RegistroAlterado { get; set; }

        public CirurgiaoTransaction AlterarStatus(EStatus status)
        {
            Status = status;

            return this;
        }

        public CirurgiaoTransaction FinalizarTransacao()
        {
            Termino = DateTime.Now;
            return this;
        }
    }

    public class ControladorLog //Lista ->
    {
        public Guid GuidTransaction { get; set; }
        public EStatus StatusAnestesista { get; set; }
        public EStatus StatusCirurgiao { get; set; }
        public EStatus StatusQuarto { get; set; }
        public DateTime Inicio { get; set; }
        public DateTime Termino { get; set; }

        public ControladorLog(Guid guidTransaction)
        {
            GuidTransaction = guidTransaction;
            Inicio = DateTime.Now;
        }
        public ControladorLog()
        {

        }


        public ControladorLog FinalizarTransacao()
        {
            Termino = DateTime.Now;

            return this;
        }
        public ControladorLog AlterarStatusAnestesista(EStatus status)
        {
            StatusAnestesista = status;

            return this;
        }
        public ControladorLog AlterarStatusCirurgiao(EStatus status)
        {
            StatusCirurgiao = status;

            return this;
        }
        public ControladorLog AlterarStatusQuarto(EStatus status)
        {
            StatusQuarto = status;

            return this;
        }
    }
}
