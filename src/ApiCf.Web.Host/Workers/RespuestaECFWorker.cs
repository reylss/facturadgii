using Abp.Dependency;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiCf.Entidades.ComprobanteFiscalSecNs;
using ApiCf.Entidades.FacturaDGIINs;
using ApiCf.Web.Host.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCf.Workers
{
    public class RespuestaECFWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private String ToketJob;
        private readonly ILogger<IComprobanteFiscalSecRepository> _logger;
        private readonly IComprobanteFiscalSecRepository _comprobangteECF;
        IConfiguration config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .Build();
        public RespuestaECFWorker(AbpTimer timer, IComprobanteFiscalSecRepository comprobangteECF, ILogger<IComprobanteFiscalSecRepository> logger)
            : base(timer)
        {
            _logger = logger;
            _comprobangteECF = comprobangteECF;

            string TiempoEnvioArchivoHora = config["JobControl:TiempoRespuetaHoras\"];\r\n"];

            if (!string.IsNullOrEmpty(TiempoEnvioArchivoHora) && int.TryParse(TiempoEnvioArchivoHora, out int hora))
            {
                Timer.Period = hora; // ya está en hora
            }
            else
            {
                Timer.Period = 2;//horas
            }

            _logger = logger;
            //Timer.Period = 5000; //5 seconds (good for tests, but normally will be more)
        }

        [UnitOfWork]
        protected override void DoWork()
        {
            this.ToketJob = config["RutaCertificado:ToketJob"];
            int intervalo = 20;
            bool enabled = true;
            String IntervalType = "hora";
            try
            {
                intervalo = int.Parse(config["Jobs:Job2:IntervalValue"]);
                enabled = bool.Parse(config["Jobs:Job2:Enabled"]);
                IntervalType = config["Jobs:Job2:IntervalType"];

                if (enabled == true)
                {

                    if (ControlEjecucion.ControlEjecucionJobPorHoras(config, intervalo) == true)
                    {

                        _comprobangteECF.ObtenerComprobanteFiscal();
                        CurrentUnitOfWork.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,
                    "Error crítico inesperado en ProcesarDatos.  IntervalType:Milesima de segundo");



            }

              
        }
    }
}

