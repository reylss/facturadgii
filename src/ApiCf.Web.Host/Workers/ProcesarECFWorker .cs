using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Threading.BackgroundWorkers;
using Abp.Threading.Timers;
using Abp.Timing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ApiCf.Authorization.Users;
using ApiCf.Entidades.FacturaDGIINs;
using ApiCf.Web.Host.Workers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ApiCf.Workers
{
    public class ProcesarECFWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ILogger<IFacturaDGIIRepository> _logger;

        private readonly IFacturaDGIIRepository _facturaECF;
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        private String ToketJob;
        public ProcesarECFWorker(AbpTimer timer, IFacturaDGIIRepository facturaECF, ILogger<IFacturaDGIIRepository> logger)
            : base(timer)
        {
            _logger = logger;
            _facturaECF = facturaECF;
            string TiempoEnvioArchivoMilisegundos = config["JobControl:TiempoProcesarArchivoMilisegundos"];

            if (!string.IsNullOrEmpty(TiempoEnvioArchivoMilisegundos) && int.TryParse(TiempoEnvioArchivoMilisegundos, out int milisegundos))
            {
                Timer.Period = milisegundos; // ya está en milisegundos
            }
            else
            {
                Timer.Period = 1; // 1 milisegundo por defecto
            }

        }
        [UnitOfWork]
        protected override void DoWork()
        {
            this.ToketJob = config["RutaCertificado:ToketJob"];
            //var ApagadarEnvioTradicional = config["RutaCertificado:ApagadarProcesoEnc"];
            int intervalo = 1;
            bool enabled = true;
            String IntervalType = "milisegundo";
            try
            {
                intervalo =int.Parse( config["Jobs:Job3:IntervalValue"]);
                 enabled =bool.Parse( config["Jobs:Job3:Enabled"]);
                IntervalType = config["Jobs:Job3:IntervalType"];

              if(enabled == true)
                  {

                    if (ControlEjecucion.ControlEjecucionJobPorMilisegundos(config, intervalo) == true)
                    {

                        _facturaECF.ObtenerDatosFactura(String.Empty, this.ToketJob, "JOB");
                        CurrentUnitOfWork.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex,
                    "Error crítico inesperado en ProcesarDatos.  IntervalType:Milesima de segundo"  );
                


            }


        }

    }
}



