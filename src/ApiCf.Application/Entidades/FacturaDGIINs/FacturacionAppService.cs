using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ApiCf.Authorization;
using ApiCf.Entidades.FacturaDGIINs.Dto;
using ApiCf.SharedNs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ApiCf.Entidades.FacturaDGIINs
{
    // [AbpAuthorize(PermissionNames.Pages_Users)]
    public class FacturacionAppService : ApplicationService, IFacturacionAppService
    {
        private readonly IFacturaDGIIRepository _facturaDGIIRepository;
        private readonly IMedicalEcfWorkflowService _medicalEcfWorkflowService;
        private readonly ILogger<FacturacionAppService> _logger;
       
        public FacturacionAppService(IRepository<FacturaDGII> repository,
                                    IFacturaDGIIRepository facturaDGIIRepository,
                                    IMedicalEcfWorkflowService medicalEcfWorkflowService,
                                    ILogger<FacturacionAppService> logger)
        {
            _facturaDGIIRepository = facturaDGIIRepository;
            _medicalEcfWorkflowService = medicalEcfWorkflowService;
            _logger = logger;
        }

        [AbpAuthorize]
        public async Task<MedicalEcfProcessResultDto> ProcesarComprobanteMedicoEcf(MedicalEcfProcessRequestDto request)
        {
            try
            {
                var result = await _medicalEcfWorkflowService.ExecuteWorkflowAsync(request);
                _logger.LogInformation(
                    "Medical e-CF workflow finished. Type={EcfType}, Number={EcfNumber}, Success={Success}, Audit={AuditId}",
                    request?.EcfType,
                    request?.EcfNumber,
                    result.Success,
                    result.AuditId);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Medical e-CF workflow failed. Type={EcfType}, Number={EcfNumber}", request?.EcfType, request?.EcfNumber);
                return MedicalEcfProcessResultDto.Fail("UNHANDLED_WORKFLOW_ERROR", "Unexpected error during medical e-CF workflow execution.");
            }
        }

        [UnitOfWork]
        [AbpAuthorize]
        public Task<string> EnvioArchivoDgii(string cId, string cToken, string cProcedencia)
        {
            var Listresult = new List<FacturaDGIIDto>();
            var f = new FacturaDGIIDto();
            string nIdeop = "0";

          
            // Validaciones de entrada
            if (cProcedencia != "DBDGII" && cProcedencia != "JOB")
            {
                return Task.FromResult("https-401 => La información no es válida");
            }

            if (cProcedencia == "DBDGII" && String.IsNullOrEmpty(cId))
            {
                return Task.FromResult("https-401 => La información no es válida");
            }

            if (cProcedencia == "DBDGII" && String.IsNullOrEmpty(cToken))
            {
                return Task.FromResult("https-401 => La información no es válida");
            }

            if (cProcedencia == "JOB" && String.IsNullOrEmpty(cToken))
            {
                return Task.FromResult("https-401 => La información no es válida");
            }

            var valor = "OK";
            try
            {
                if (cProcedencia == "DBDGII")
                {
                    var ok = _facturaDGIIRepository.ValidarId(cId, cToken);
                    if (ok.Result == "NOK")
                    {
                        return Task.FromResult("https-401 => La información no es válida");
                    }
                }

                var result = _facturaDGIIRepository.ObtenerDatosFactura(cId, cToken, cProcedencia).Result
                    .FirstOrDefault();
                valor = "OK";
            }
            catch (Exception ex)
            {
                valor = "http-401 => " + ex.Message;
            }

            return Task.FromResult(valor);
        }

        [AbpAuthorize]
        public async Task<string> EnvioArchivoDgiiJson([FromBody] JObject pData)
        {
            if (pData == null)
            {
                return await Task.FromResult("https-401 => Datos no válidos");
            }

            try
            {
                ParametroFactura parametro = pData.ToObject<ParametroFactura>();

                if (parametro?.Datos == null)
                {
                    return await Task.FromResult("https-401 => La información no es válida");
                }

                String result = "OK";

                if (parametro.Datos.TIPOECF.IsNullOrEmpty())
                {
                    return await Task.FromResult("https-401 => La información no es válida");
                }

                result = await _facturaDGIIRepository.ObtenerDatosFacturaJson(parametro.Datos);

                return await Task.FromResult(result);
            }
            catch (Exception ex)
            {
                return await Task.FromResult("Error: " + ex);
            }
        }

        [UnitOfWork]
        public Task<string> GenerateQRCode(ParametroFacturaQR pDatos)
        {
            if (pDatos == null)
            {
                return Task.FromResult("https-401 => Parámetros no válidos");
            }

          
            if (pDatos.Procedencia == "DBDGII" && String.IsNullOrEmpty(pDatos.cEcf))
            {
                return Task.FromResult("https-401 => La información no es válida");
            }

            var valor = "OK";
            try
            {
                if (pDatos.Procedencia == "DBDGII")
                {
                    var ok = _facturaDGIIRepository.getGenerateQRCode(pDatos.cEcf, pDatos.cUrl);
                    if (ok.Result == "NOK")
                    {
                        return Task.FromResult("https-401 => La información no es válida");
                    }
                }
                valor = "OK";
            }
            catch (Exception ex)
            {
                valor = "http-401 => Error procesando QR";
            }

            return Task.FromResult(valor);
        }

        /*[AbpAuthorize]
        [RemoteService(false)]
        public Task<string> GenerarSignXmlWithP12([FromBody] JObject jsonBody)
        {
            if (jsonBody == null)
            {
                return Task.FromResult("https-401 => Datos no válidos");
            }

            try
            {
                jsonParameter eNcf = jsonBody.ToObject<jsonParameter>();

                if (eNcf == null || String.IsNullOrEmpty(eNcf.jsonFactura))
                {
                    return Task.FromResult("https-401 => La información no es válida");
                }

                var valor = "OK";
                try
                {
                    var result = _facturaDGIIRepository.GenerarSignXmlWithP12(eNcf.jsonFactura).Result
                        .FirstOrDefault();

                    valor = "OK";
                }
                catch (Exception ex)
                {
                     valor = "http-401 => " + ex.Message;
                }

                return Task.FromResult(valor);
            }
            catch (Exception ex)
            {
                 return Task.FromResult("https-401 => Error en formato de datos");
            }
        }*/

      /*  [UnitOfWork]
         public Task<string> GenerarQr([FromBody] JObject jsonBody)
        {
            if (jsonBody == null)
            {
                var errorResponse = new
                {
                    result = "Error: Datos no válidos",
                    success = false,
                    error = "JSON body es nulo"
                };
                return Task.FromResult(JsonConvert.SerializeObject(errorResponse));
            }

            try
            {
                jsonParameter cRutaQr = jsonBody.ToObject<jsonParameter>();

                if (cRutaQr == null || String.IsNullOrEmpty(cRutaQr.jsonFactura))
                {
                    var errorResponse = new
                    {
                        result = "Error: La información no es válida",
                        success = false,
                        error = "jsonFactura es requerido"
                    };
                    return Task.FromResult(JsonConvert.SerializeObject(errorResponse));
                }

                var result = _facturaDGIIRepository.GenerarQr(cRutaQr.jsonFactura).Result;
                var response = new
                {
                    result = result,
                    success = true,
                    error = (string)null
                };

                string jsonResponse = JsonConvert.SerializeObject(response);
                return Task.FromResult(jsonResponse);
            }
            catch (Exception ex)
            {
                var errorResponse = new
                {
                    result = "Error interno",
                    success = false,
                    error = ex.Message
                };
                return Task.FromResult(JsonConvert.SerializeObject(errorResponse));
            }
        }

     
        */
        [UnitOfWork]
        public virtual async void EnviarEncfXmlJob()
        {
            try
            {
                _facturaDGIIRepository.EnviarEncfXML();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
