using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using System;
using System.Net;
using System.Text;
using static QRCoder.PayloadGenerator.ShadowSocksConfig;

namespace ApiCf.Web.Host.Workers
{
    public class ControlEjecucion
    {

        /// <summary>
        /// Controla la ejecución de un job basado en intervalos de minutos
        /// </summary>
        /// <param name="config">Configuración de la aplicación</param>
        /// <param name="intervalMinutes">Intervalo en minutos para la ejecución</param>
        /// <returns>True si el job debe ejecutarse</returns>
        public static bool ControlEjecucionJobPorMinutos(IConfiguration config, int intervalMinutes)
        {
            DateTime now = DateTime.Now;
            string lastExecutionKey = "JobControl:TiempoEnvioArchivoMinutos";

            // Obtener la última ejecución desde configuración o usar valor por defecto
            string lastExecutionStr = config[lastExecutionKey];

            if (DateTime.TryParse(lastExecutionStr, out DateTime lastExecution))
            {
                TimeSpan timeSinceLastExecution = now - lastExecution;
                return timeSinceLastExecution.TotalMinutes >= intervalMinutes;
            }

            // Si no hay registro previo, permitir ejecución
            return true;
        }

        /// <summary>
        /// Controla la ejecución de un job basado en intervalos de milisegundos
        /// </summary>
        /// <param name="config">Configuración de la aplicación</param>
        /// <param name="intervalMilliseconds">Intervalo en milisegundos para la ejecución</param>
        /// <returns>True si el job debe ejecutarse</returns>
        public static bool ControlEjecucionJobPorMilisegundos(IConfiguration config, int intervalMilliseconds)
        {
            DateTime now = DateTime.Now;
            string lastExecutionKey = "JobControl:TiempoProcesarArchivoMilisegundos";

            // Obtener la última ejecución desde configuración o usar valor por defecto
            string lastExecutionStr = config[lastExecutionKey];

            if (DateTime.TryParse(lastExecutionStr, out DateTime lastExecution))
            {
                TimeSpan timeSinceLastExecution = now - lastExecution;
                return timeSinceLastExecution.TotalMilliseconds >= intervalMilliseconds;
            }

            // Si no hay registro previo, permitir ejecución
            return true;
        }

        /// <summary>
        /// Controla la ejecución de un job basado en intervalos de horas
        /// </summary>
        /// <param name="config">Configuración de la aplicación</param>
        /// <param name="intervalHours">Intervalo en horas para la ejecución</param>
        /// <returns>True si el job debe ejecutarse</returns>
        public static bool ControlEjecucionJobPorHoras(IConfiguration config, int intervalHours)
        {
            DateTime now = DateTime.Now;
            string lastExecutionKey = "JobControl:TiempoRespuetaHoras";

            // Obtener la última ejecución desde configuración o usar valor por defecto
            string lastExecutionStr = config[lastExecutionKey];

            if (DateTime.TryParse(lastExecutionStr, out DateTime lastExecution))
            {
                TimeSpan timeSinceLastExecution = now - lastExecution;
                return timeSinceLastExecution.TotalHours >= intervalHours;
            }

            // Si no hay registro previo, permitir ejecución
            return true;
        }

        /// <summary>
        /// Método genérico que combina los tres tipos de intervalos
        /// </summary>
        /// <param name="config">Configuración de la aplicación</param>
        /// <param name="tipoTiempo">Tipo de tiempo: "Minutes", "Hours", "Milliseconds"</param>
        /// <param name="valorTiempo">Valor del intervalo de tiempo</param>
        /// <returns>True si el job debe ejecutarse</returns>
        public static bool ControlEjecucionJobGenerico(IConfiguration config, string tipoTiempo, int valorTiempo)
        {
            return tipoTiempo.ToLower() switch
            {
                "minutes" or "minuto" => ControlEjecucionJobPorMinutos(config, valorTiempo),
                "hours" or "hora" => ControlEjecucionJobPorHoras(config, valorTiempo),
                "milliseconds" or "milisegundo" => ControlEjecucionJobPorMilisegundos(config, valorTiempo),
                _ => throw new ArgumentException($"Tipo de tiempo no válido: {tipoTiempo}. Use 'Minutes', 'Hours' o 'Milliseconds'")
            };
        }
    }

 
}

