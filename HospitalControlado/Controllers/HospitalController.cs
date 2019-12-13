using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using HospitalControlado.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Refit;
using ServerInfra.Enum;
using ServerInfra.Models;
using ServerInfra.Transacoes;
using ServerInfra.Util;

namespace HospitalControlado.Controllers
{
    public class HospitalController : Controller
    {
        private ICirurgiaoAPI _apiCirurgiao = RestService.For<ICirurgiaoAPI>(Util.CirurgiaoWebApi);
        private IAnestesistaAPI _apiAnestesista = RestService.For<IAnestesistaAPI>(Util.AnestesistaWebApi);

        [HttpPost]
        [Route("api/Hospital/Reservar")]
        public IActionResult ReservarCirurgia([FromBody] ReservaModel reserva)
        {
            RequestProvider.RequestProvider request = new RequestProvider.RequestProvider();

            //bloqueio de Reserva. 
            //Garante que um thread não insere uma seção crítica do código enquanto outro thread está na seção crítica. 
            //Se outro request tentar inserir uma reserva, ele aguardará, bloqueado, até que o objeto dentro da seção seja liberado.
            lock (reserva) //Métodos	de	Controle	de	Concorrência:	 Travas	(locks)	e	bloqueios;	
            {
                string reservaCirurgiaoId;
                string reservarAnestesista;
                DateTime inicio = DateTime.Now;
                ControladorLog log = new ControladorLog(Guid.NewGuid());


                try
                {
                    #region Primeira fase: Fase de Votação 
                    try
                    {

                        reservarAnestesista = _apiAnestesista.ReservarAnestesista(reserva.Anestesista, log.GuidTransaction.ToString()).Result;
                        log.AlterarStatusAnestesista(EStatus.Preparado);

                    }
                    catch (Exception ex)
                    {
                        //Se der erro no momento de reservar anestesista, retorna para usuário BAD request e não continua com a transação
                        log.FinalizarTransacao();
                        SalvarStatusTransacao(log);

                        throw ex;
                    }

                    try
                    {

                        reservaCirurgiaoId = _apiCirurgiao.ReservarCirurgiao(reserva.Cirurgiao, log.GuidTransaction.ToString()).Result;
                        log.AlterarStatusCirurgiao(EStatus.Preparado);
                    }
                    catch (Exception ex)
                    {
                        log.AlterarStatusAnestesista(EStatus.AguardandoAbertar);
                        var res = _apiAnestesista.RollBack(reservarAnestesista).Result;
                        log.AlterarStatusAnestesista(EStatus.Abortado);

                        SalvarStatusTransacao(log);

                        throw ex;
                    }
                    #endregion Primeira fase: Fase de Votação 

                    #region Segunda fase: Fase de Decisão

                    //Se não houve erro em nenhum dos passos acima, realiza a reserva do quarto -> caso ocorra erros (rollback nas transações anteriores).


                    //Reservar quarto () -> Codigo.

                    try
                    {
                        bool quarto = AgendarQuarto(reserva.Quarto);
                        //  LogControlador(reservarAnestesista, reservaCirurgiaoId, inicio);


                        if (!quarto)
                        {

                            log.AlterarStatusAnestesista(EStatus.AguardandoAbertar);
                            log.AlterarStatusCirurgiao(EStatus.AguardandoAbertar);

                            var res = _apiAnestesista.RollBack(reservarAnestesista).Result;
                            var teste = _apiCirurgiao.RollBack(reservaCirurgiaoId).Result;

                            log.AlterarStatusCirurgiao(EStatus.AguardandoAbertar);
                            log.AlterarStatusAnestesista(EStatus.AguardandoAbertar);

                            return BadRequest();
                        }


                    }

                    catch (Exception ex)
                    {

                        SalvarStatusTransacao(log);

                        throw ex;
                    }


                    //Se não houve erro em nenhum dos passos acima, commita as transações

                    try
                    {
                        log.AlterarStatusAnestesista(EStatus.AguardandoEfetivacao);
                        log.AlterarStatusCirurgiao(EStatus.AguardandoEfetivacao);

                        var res1 = _apiCirurgiao.Commit(reservaCirurgiaoId).Result;
                        var res2 = _apiAnestesista.Commit(reservarAnestesista).Result;

                        log.AlterarStatusAnestesista(EStatus.Efetivado);
                        log.AlterarStatusCirurgiao(EStatus.Efetivado);

                        SalvarStatusTransacao(log);
                    }

                    catch (Exception x) //Reetentiva
                    {
                        SalvarStatusTransacao(log); //Salva status para o processo se recompor.
                    }
                    //Commit();
                    #endregion Segunda fase: Fase de Decisão

                    //Retorna que a request foi OK ... Status Code 200.
                    return Ok();
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("api/ObterStatusTransacaoAnestesista")]
        public EStatus ConsultarStatusAnestesista(string guid)
        {
            List<ControladorLog> trans = new List<ControladorLog>();
            using (StreamReader r = new StreamReader(string.Format("{0}.json", Util.DiretorioTransacoesQuarto)))
            {
                //Converto em JSON
                string json = r.ReadToEnd();
                trans = JsonConvert.DeserializeObject<List<ControladorLog>>(json);
            }
            return trans.FirstOrDefault(x => x.GuidTransaction == Guid.Parse(guid)).StatusAnestesista != null ? trans.FirstOrDefault(x => x.GuidTransaction == Guid.Parse(guid)).StatusAnestesista  : EStatus.AguardandoAbertar;
        }

        [HttpGet]
        [Route("api/ObterStatusTransacaoCirurgiao")]
        public EStatus ConsultarStatusCirurgiao(string guid)
        {
            List<ControladorLog> trans = new List<ControladorLog>();
            using (StreamReader r = new StreamReader(string.Format("{0}.json", Util.DiretorioTransacoesQuarto)))
            {
                //Converto em JSON
                string json = r.ReadToEnd();
                trans = JsonConvert.DeserializeObject<List<ControladorLog>>(json);                
            }

            return trans.FirstOrDefault(x => x.GuidTransaction == Guid.Parse(guid)).StatusAnestesista != null ? trans.FirstOrDefault(x => x.GuidTransaction == Guid.Parse(guid)).StatusCirurgiao : EStatus.AguardandoAbertar;
        }

        /* Metodos abaixo para gerar Json a ser utilizado na aplicação */

        public Task SalvarStatusTransacao(ControladorLog log)
        {

            List<ControladorLog> trans = new List<ControladorLog>();
            using (StreamReader r = new StreamReader(string.Format("{0}.json", Util.DiretorioTransacoesQuarto)))
            {
                //Converto em JSON
                string json = r.ReadToEnd();
                trans = JsonConvert.DeserializeObject<List<ControladorLog>>(json);

                if (trans == null)
                    trans = new List<ControladorLog>();
            }

            trans.Add(log);

            using (StreamWriter file = System.IO.File.CreateText(string.Format("{0}.json", Util.DiretorioTransacoesQuarto)))
            {
                JsonSerializer serializer = new JsonSerializer();
                //SALVO LOG DA TRANSAÇÃO
                serializer.Serialize(file, trans);
            }

            return Task.CompletedTask;
        }

        public bool AgendarQuarto(QuartoModel quarto)
        {
            List<QuartoModel> items = new List<QuartoModel>();
            //Leio todos os cirurgioes do JSON PRINCIPAL
            using (StreamReader r = new StreamReader(Util.FileQuarto))
            {
                //Converto em JSON
                string json = r.ReadToEnd();
                items = JsonConvert.DeserializeObject<List<QuartoModel>>(json);
            }


            bool disponivel = false;
            //Itero pela coleção, comparando com os ids da reserva
            foreach (var item in items.Where(x => x.IsReservado == false))
            {
                //SE NA LISTA PRINCIPAL EXISTIR ALGUM ID DE IGUAL AO DA RESERVA, INATIVA-O. 
                if (quarto.Id == item.Id)
                    disponivel = true;
            }

            if (!disponivel)
            {
                return false;
            }

            else
            {
                items.FirstOrDefault(x => x.Id == quarto.Id).Reservar();
            }


            using (StreamWriter file = System.IO.File.CreateText(Util.FileQuarto))
            {
                JsonSerializer serializer = new JsonSerializer();
                //COMITA A TRANSAÇÃO NO ARQUIVO PRINCIPAL
                serializer.Serialize(file, items);
            }

            return true;

        }



        [HttpGet]
        [Route("api/Quarto")]
        public List<QuartoModel> GetQuarto()
        {
            List<QuartoModel> teste = new List<QuartoModel>();

            for (int i = 0; i < 31; i++)
            {
                teste.Add(new QuartoModel(i, "Quarto 308", DateTime.Now.AddDays(i)));
            }
            return teste;
        }


        [HttpGet]
        [Route("api/Cirurgiao")]
        public List<AnestesistaModel> GetCirurgiao()
        {
            List<AnestesistaModel> teste = new List<AnestesistaModel>();

            for (int i = 0; i < 31; i++)
            {
                teste.Add(new AnestesistaModel(i, "Anestesista Teste", DateTime.Now.AddDays(i)));
            }

            return teste;
        }


        [HttpGet]
        [Route("api/Anestesista")]
        public List<CirurgiaoModel> GetAnestesista()
        {
            List<CirurgiaoModel> teste = new List<CirurgiaoModel>();
            for (int i = 0; i < 31; i++)
            {
                teste.Add(new CirurgiaoModel(i, "Gustavo Haus", "Geral", DateTime.Now.AddDays(i)));
            }

            return teste;
        }
    }
}