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
    public class TransferenciaECFWorker : PeriodicBackgroundWorkerBase, ISingletonDependency
    {
        private readonly ILogger<IFacturaDGIIRepository> _logger;

        private readonly IFacturaDGIIRepository _facturaECF;
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        private String ToketJob;
        public TransferenciaECFWorker(AbpTimer timer, IFacturaDGIIRepository facturaECF, ILogger<IFacturaDGIIRepository> logger)
            : base(timer)
        { 

            _logger=logger;
            _facturaECF = facturaECF;
            // var nombreCertificado = configurations["RutaCertificado:CertificadoPFX"];
            string TiempoEnvioArchivoMinutos = config["JobControl:TiempoEnvioArchivoMinutos"];

            if (!string.IsNullOrEmpty(TiempoEnvioArchivoMinutos) && int.TryParse(TiempoEnvioArchivoMinutos, out int minutos))
            {
                Timer.Period = minutos; // ya está en milisegundos
            }
            else
            {
                Timer.Period = 25;// mintos
            }
             
        }
        [UnitOfWork]
        protected override void DoWork()
        {
            this.ToketJob = config["RutaCertificado:ToketJob"];
            int intervalo = 20;
            bool enabled = true;
            String IntervalType = "minuto";
            try
            {
                intervalo = int.Parse(config["Jobs:Job1:IntervalValue"]);
                enabled = bool.Parse(config["Jobs:Job1:Enabled"]);
                IntervalType = config["Jobs:Job1:IntervalType"];

                if (enabled == true)
                {

                    if (ControlEjecucion.ControlEjecucionJobPorMinutos(config, intervalo) == true)
                    {

                        _facturaECF.EnviarEncfXML();
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



