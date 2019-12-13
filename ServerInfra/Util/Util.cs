using System;
using System.Collections.Generic;
using System.Text;

namespace ServerInfra.Util
{
    public static class Util
    {
        //URL das APIS
        public static string AnestesistaWebApi = "https://localhost:44305/";
        public static string CirurgiaoWebApi = "https://localhost:44351/";
        public static string ControladorWebAPi = "https://localhost:44314/";

        //Diretório das transações
        public static string DiretorioTransacoesQuarto = @"C:\Users\suporte\source\repos\Hospital\Transacoes\TransacoesHospital";
        public static string DiretorioTransacoesAnestesista = @"C:\Users\suporte\source\repos\Hospital\Transacoes\Anestesista";
        public static string DiretorioTransacoesCirurgiao = @"C:\Users\suporte\source\repos\Hospital\Transacoes\TransacoesCirurgiao.json";


        public static string FileQuarto = @"C:\Users\suporte\source\repos\Hospital\HospitalControlado\Quarto.json";
        public static string FileAnestesia = @"C:\Users\suporte\source\repos\Hospital\AnestesistaNew\Anestesista.json";
        public static string FileCirurgiao = @"C:\Users\suporte\source\repos\Hospital\CirurgiaoNew\Cirurgiao.json";
        public static string FileAnestesiaP = @"C:\Users\suporte\source\repos\Hospital\AnestesistaNew\AnestesistaP.json";
        public static string FileCirurgiaoP = @"C:\Users\suporte\source\repos\Hospital\CirurgiaoNew\CirurgiaoP.json";

      //  public static string ProgressoAnestesista = @"C:\Users\suporte\source\repos\Hospital\AnestesistaNew\InProgress.json";
      //  public static string ProgressoCirurgiao = @"C:\Users\suporte\source\repos\Hospital\CirurgiaoNew\InProgress.json";



    }
}
