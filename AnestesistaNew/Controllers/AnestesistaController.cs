using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AnestesistaNew.Interface;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Refit;
using ServerInfra.Enum;
using ServerInfra.Models;
using ServerInfra.Transacoes;
using ServerInfra.Util;

namespace AnestesistaNew.Controllers
{
    public class AnestesistaController : Controller
    {

        private InterfaceControlador _apiHospital = RestService.For<InterfaceControlador>(Util.ControladorWebAPi);
        public AnestesistaController() // Quando é  assionado
        {

        }
        [HttpPost]
        [Route("api/reserva/anestesista")]
        public IActionResult ReservarAnestesista([FromBody] AnestesistaModel reserva, string guid)
        {
            lock (reserva) //Métodos	de	Controle	de	Concorrência:	 Travas	(locks)	e	bloqueios;	
            {
                try
                {
                    //Cria uma nova transação
                    AnestesistaTransaction trans = new AnestesistaTransaction();

                    //SALVA STATUS DA TRANSAÇÃO E DATA\HORARIO DE INICIO 
                    trans.Status = EStatus.Preparado;
                    trans.Inicio = DateTime.Now;

                    List<AnestesistaModel> items = new List<AnestesistaModel>();
                    //Leio todos os Anestesistas do JSON(Banco) PRINCIPAL
                    using (StreamReader r = new StreamReader(Util.FileAnestesia))
                    {
                        //Converto em JSON
                        string json = r.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<AnestesistaModel>>(json);
                    }

                    bool disponivel = false; //Verifica se o item a ser reservado esta disponivel
                    foreach (var item in items.Where(x => x.IsReservado == false))
                    {
                        if (reserva.Id == item.Id)
                            disponivel = true;
                    }


                    if (!disponivel)
                        return BadRequest();
                    else
                    {
                        var registro = items.FirstOrDefault(x => x.Id == reserva.Id);
                        trans.RegistroAtual = (AnestesistaModel)registro.Clone();

                        registro.Reservar();

                        trans.RegistroAlterado = registro;

                    }

                    using (StreamWriter file = System.IO.File.CreateText(Util.FileAnestesiaP))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        //COMITA A TRANSAÇÃO NO ARQUIVO INTERMEDIARIO
                        serializer.Serialize(file, items);
                    }



                    trans.Id = Guid.Parse(guid);

                    List<AnestesistaTransaction> logs = new List<AnestesistaTransaction>();

                    using (StreamReader r = new StreamReader(string.Format("{0}.json", Util.DiretorioTransacoesAnestesista)))
                    {
                        //Converto em JSON
                        string json = r.ReadToEnd();
                        logs = JsonConvert.DeserializeObject<List<AnestesistaTransaction>>(json);

                        if (logs == null)
                            logs = new List<AnestesistaTransaction>();
                    }

                    logs.Add(trans);

                    // Salva Log Atualizado.
                    using (StreamWriter file = System.IO.File.CreateText(string.Format("{0}.json", Util.DiretorioTransacoesAnestesista)))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        //SALVO LOG DA TRANSAÇÃO
                        serializer.Serialize(file, logs);
                    }




                    return Ok(trans.Id.ToString());
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
        }

        [HttpPut]
        [Route("api/efetivar")]
        public IActionResult Commit(string id)
        {
            try
            {
                //Cria uma nova transação
                List<AnestesistaTransaction> trans = new List<AnestesistaTransaction>();

                //Leio o log da transação / arquivo intermediário
                using (StreamReader r = new StreamReader(string.Format("{0}.json", Util.DiretorioTransacoesAnestesista)))
                {
                    //Converto em JSON
                    string json = r.ReadToEnd();
                    trans = JsonConvert.DeserializeObject<List<AnestesistaTransaction>>(json);
                }

                //GET ESTADO ALTERADO
                AnestesistaModel item = trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).RegistroAlterado;



                List<AnestesistaModel> items = new List<AnestesistaModel>();

                //Leio todos os cirurgioes do JSON PRINCIPAL
                using (StreamReader r = new StreamReader(Util.FileAnestesia))
                {
                    //Converto em JSON
                    string json = r.ReadToEnd();
                    items = JsonConvert.DeserializeObject<List<AnestesistaModel>>(json);
                }

                items.RemoveAll(x => x.Id == item.Id);
                items.Add(item);
                items.OrderBy(x => x.Id);

                using (StreamWriter file = System.IO.File.CreateText(Util.FileAnestesia)) // Salva no banco final
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //COMITA A TRANSAÇÃO NO ARQUIVO PRINCIPAL
                    serializer.Serialize(file, items);
                }

                //ALTERA STATUS DA TRANSAÇÃO E DATA\HORARIO DE TERMINO E SALVA


                trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).AlterarStatus(EStatus.Efetivado);
                trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).FinalizarTransacao();



                //Finaliza transação
                using (StreamWriter file = System.IO.File.CreateText(string.Format("{0}.json", Util.DiretorioTransacoesAnestesista)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //SALVO LOG DA TRANSAÇÃO
                    serializer.Serialize(file, trans);
                }

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [HttpDelete]
        [Route("api/Gatilho")]
        public void Gatilho()
        {
            List<AnestesistaTransaction> trans = new List<AnestesistaTransaction>();


            //Leio o log da transação / arquivo intermediário
            using (StreamReader r = new StreamReader(string.Format("{0}.json", Util.DiretorioTransacoesAnestesista)))
            {
                //Converto em JSON
                string json = r.ReadToEnd();
                trans = JsonConvert.DeserializeObject<List<AnestesistaTransaction>>(json);
            }
            if (trans == null)
            {
                trans = new List<AnestesistaTransaction>();
            }

            foreach (AnestesistaTransaction item in trans.Where(x => x.Status == EStatus.Preparado))
            {
                var teste = _apiHospital.ConsultarStatusAnestesista(item.Id.ToString());

                if (teste.Status.Equals(EStatus.AguardandoEfetivacao))
                {
                    Commit(item.Id.ToString());
                }

                else if (teste.Status.Equals(EStatus.AguardandoAbertar))
                {
                    RollBack(item.Id.ToString());
                }
            }
        }

        [HttpDelete]
        [Route("api/abortar")]
        public IActionResult RollBack(string id)
        {
            try
            {
                //Lista de transacoes
                List<AnestesistaTransaction> trans = new List<AnestesistaTransaction>();


                //Leio o log da transação / arquivo intermediário
                using (StreamReader r = new StreamReader(string.Format("{0}.json", Util.DiretorioTransacoesAnestesista)))
                {
                    //Converto em JSON
                    string json = r.ReadToEnd();
                    trans = JsonConvert.DeserializeObject<List<AnestesistaTransaction>>(json);
                }

                //GET Registro Limpo
                AnestesistaModel item = trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).RegistroAtual;


                List<AnestesistaModel> items = new List<AnestesistaModel>();

                //Leio todos os cirurgioes do JSON "SUJO"
                using (StreamReader r = new StreamReader(Util.FileAnestesiaP))
                {
                    //Converto em JSON
                    string json = r.ReadToEnd();
                    items = JsonConvert.DeserializeObject<List<AnestesistaModel>>(json);
                }



                items.RemoveAll(x => x.Id == item.Id);
                items.Add(item);
                items.OrderBy(x => x.Id);

                using (StreamWriter file = System.IO.File.CreateText(Util.FileAnestesiaP)) // Salva no banco Intermediario
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //COMITA A TRANSAÇÃO NO ARQUIVO PRINCIPAL
                    serializer.Serialize(file, items);
                }

                //Finaliza transação
                trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).AlterarStatus(EStatus.Efetivado);
                trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).FinalizarTransacao();


                using (StreamWriter file = System.IO.File.CreateText(string.Format("{0}.json", Util.DiretorioTransacoesAnestesista)))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //SALVO LOG DA TRANSAÇÃO
                    serializer.Serialize(file, trans);
                }

                return Ok(true);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}