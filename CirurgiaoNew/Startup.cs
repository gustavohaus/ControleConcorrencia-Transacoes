using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CirurgiaoNew.Interfaces;
using CirurgiaoNew.SwaggerConfiguration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Refit;
using ServerInfra.Enum;
using ServerInfra.Transacoes;
using ServerInfra.Util;

namespace CirurgiaoNew
{
    public class Startup
    {
        private InterfaceControlador _apiHospital = RestService.For<InterfaceControlador>(Util.ControladorWebAPi);

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSwagger(Configuration);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseSwagger(Configuration);

            app.UseHttpsRedirection();
            app.UseMvc();


            ///* Regra de inicialização */

            //List<CirurgiaoTransaction> trans = new List<CirurgiaoTransaction>();


            ////Leio o log da transação / arquivo intermediário
            //using (StreamReader r = new StreamReader(string.Format("{0}.json", Util.DiretorioTransacoesCirurgiao)))
            //{
            //    //Converto em JSON
            //    string json = r.ReadToEnd();
            //    trans = JsonConvert.DeserializeObject<List<CirurgiaoTransaction>>(json);
            //}
            //if (trans == null)
            //{
            //    trans = new List<CirurgiaoTransaction>();
            //}

            //foreach (CirurgiaoTransaction item in trans.Where(x => x.Status == EStatus.Preparado))
            //{
            //    var teste = _apiHospital.ConsultarStatusCirurgiao(item.Id.ToString());

            //    if (teste.Status.Equals(EStatus.AguardandoEfetivacao))
            //    {
            //        // Commit(a.GuidProgress.ToString());
            //    }

            //    else if (teste.Status.Equals(EStatus.AguardandoAbertar))
            //    {
            //        // RollBack(a.GuidProgress.ToString());
            //    }

            //}
        }
    }
}
