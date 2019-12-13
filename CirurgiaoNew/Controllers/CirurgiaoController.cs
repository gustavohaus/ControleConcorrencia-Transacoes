using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CirurgiaoNew.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Refit;
using ServerInfra.Enum;
using ServerInfra.Models;
using ServerInfra.Transacoes;
using ServerInfra.Util;

namespace CirurgiaoNew.Controllers
{
    public class CirurgiaoController : Controller
    {
        private InterfaceControlador _apiHospital = RestService.For<InterfaceControlador>(Util.ControladorWebAPi);
        public CirurgiaoController()
        {

        }


        [HttpPost]
        [Route("api/reserva/Cirurgiao")]
        public IActionResult ReservarCirurgiao([FromBody] CirurgiaoModel reserva, string guid)
        {
            lock (reserva) //Métodos	de	Controle	de	Concorrência:	 Travas	(locks)	e	bloqueios;	
            {
                try
                {
                    //Cria uma nova transação
                    CirurgiaoTransaction trans = new CirurgiaoTransaction();

                    //SALVA STATUS DA TRANSAÇÃO E DATA\HORARIO DE INICIO 
                    trans.Status = EStatus.Preparado;
                    trans.Inicio = DateTime.Now;


                    List<CirurgiaoModel> items = new List<CirurgiaoModel>();
                    //Leio todos os cirurgioes do JSON PRINCIPAL
                    using (StreamReader r = new StreamReader(Util.FileCirurgiao))
                    {
                        //Converto em JSON
                        string json = r.ReadToEnd();
                        items = JsonConvert.DeserializeObject<List<CirurgiaoModel>>(json);
                    }


                    bool disponivel = false;
                    //Itero pela coleção, comparando com os ids da reserva
                    foreach (var item in items.Where(x => x.IsReservado == false))
                    {
                        //SE NA LISTA PRINCIPAL EXISTIR ALGUM ID DE IGUAL AO DA RESERVA,
                        if (reserva.Id == item.Id)
                            disponivel = true;
                    }

                    if (!disponivel)
                        return BadRequest();
                    else
                    {
                        var registro = items.FirstOrDefault(x => x.Id == reserva.Id);
                        trans.RegistroAtual = (CirurgiaoModel)registro.Clone();

                        registro.Reservar();

                        trans.RegistroAlterado = registro;
                    }


                    using (StreamWriter file = System.IO.File.CreateText(Util.FileCirurgiaoP))
                    {
                        JsonSerializer serializer = new JsonSerializer();
                        //COMITA A TRANSAÇÃO NO ARQUIVO "SUJO"
                        serializer.Serialize(file, items);
                    }



                    //Abrir Arquivo de processos
                    List<CirurgiaoTransaction> logs = new List<CirurgiaoTransaction>();

                    using (StreamReader r = new StreamReader(Util.DiretorioTransacoesCirurgiao))
                    {
                        //Converto em JSON
                        string json = r.ReadToEnd();
                        logs = JsonConvert.DeserializeObject<List<CirurgiaoTransaction>>(json);

                        if (logs == null)
                            logs = new List<CirurgiaoTransaction>();
                    }

                    logs.Add(trans);

                    // Salva o log
                    using (StreamWriter file = System.IO.File.CreateText(string.Format("{0}", Util.DiretorioTransacoesCirurgiao)))
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
                List<CirurgiaoTransaction> trans = new List<CirurgiaoTransaction>();

                //Leio o log da transação / arquivo intermediário
                using (StreamReader r = new StreamReader(string.Format("{0}", Util.DiretorioTransacoesCirurgiao)))
                {
                    //Converto em JSON
                    string json = r.ReadToEnd();
                    trans = JsonConvert.DeserializeObject<List<CirurgiaoTransaction>>(json);
                }

                //GET Registro ALTERADO
                CirurgiaoModel item = trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).RegistroAlterado;


                List<CirurgiaoModel> items = new List<CirurgiaoModel>();

                //Leio todos os cirurgioes do JSON PRINCIPAL
                using (StreamReader r = new StreamReader(Util.FileAnestesia))
                {
                    //Converto em JSON
                    string json = r.ReadToEnd();
                    items = JsonConvert.DeserializeObject<List<CirurgiaoModel>>(json);
                }


                items.RemoveAll(x => x.Id == item.Id);
                items.Add(item);
                items.OrderBy(x => x.Id);


                using (StreamWriter file = System.IO.File.CreateText(Util.FileCirurgiao))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //COMITA A TRANSAÇÃO NO ARQUIVO PRINCIPAL
                    serializer.Serialize(file, items);
                }

                //ALTERA STATUS DA TRANSAÇÃO E DATA\HORARIO DE TERMINO E SALVA


                trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).AlterarStatus(EStatus.Efetivado);
                trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).FinalizarTransacao();

                using (StreamWriter file = System.IO.File.CreateText(string.Format("{0}.json", Util.DiretorioTransacoesCirurgiao)))
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
        }

        [HttpDelete]
        [Route("api/abortar")]
        public IActionResult RollBack(string id)
        {
            try
            {
                //Cria uma nova transação
                List<CirurgiaoTransaction> trans = new List<CirurgiaoTransaction>();

                //Leio o log da transação / arquivo intermediário
                using (StreamReader r = new StreamReader(string.Format("{0}.json", Util.DiretorioTransacoesCirurgiao)))
                {
                    //Converto em JSON
                    string json = r.ReadToEnd();
                    trans = JsonConvert.DeserializeObject<List<CirurgiaoTransaction>>(json);
                }

                //GET Registro Limpo
                CirurgiaoModel item = trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).RegistroAtual;


                List<CirurgiaoModel> items = new List<CirurgiaoModel>();

                //Leio todos os cirurgioes do JSON PRINCIPAL
                using (StreamReader r = new StreamReader(Util.FileAnestesiaP))
                {
                    //Converto em JSON
                    string json = r.ReadToEnd();
                    items = JsonConvert.DeserializeObject<List<CirurgiaoModel>>(json);
                }

                items.RemoveAll(x => x.Id == item.Id);
                items.Add(item);
                items.OrderBy(x => x.Id);

                using (StreamWriter file = System.IO.File.CreateText(Util.FileCirurgiaoP)) // Salva no banco Intermediario
                {
                    JsonSerializer serializer = new JsonSerializer();
                    //COMITA A TRANSAÇÃO NO ARQUIVO PRINCIPAL
                    serializer.Serialize(file, items);
                }


                trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).AlterarStatus(EStatus.Efetivado);
                trans.FirstOrDefault(x => x.Id == Guid.Parse(id)).FinalizarTransacao();

                using (StreamWriter file = System.IO.File.CreateText(string.Format("{0}.json", Util.DiretorioTransacoesCirurgiao)))
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