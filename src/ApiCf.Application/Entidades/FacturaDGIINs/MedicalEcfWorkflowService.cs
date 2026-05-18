using Abp.Dependency;
using ApiCf.Entidades.FacturaDGIINs.Dto;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ApiCf.Entidades.FacturaDGIINs
{
    public class MedicalEcfWorkflowService : IMedicalEcfWorkflowService, ITransientDependency
    {
        private readonly IFacturaDGIIRepository _facturaDgiiRepository;
        private readonly ILogger<MedicalEcfWorkflowService> _logger;

        public MedicalEcfWorkflowService(
            IFacturaDGIIRepository facturaDgiiRepository,
            ILogger<MedicalEcfWorkflowService> logger)
        {
            _facturaDgiiRepository = facturaDgiiRepository;
            _logger = logger;
        }

        public async Task<MedicalEcfProcessResultDto> ExecuteWorkflowAsync(MedicalEcfProcessRequestDto request)
        {
            var validation = ValidateRequest(request);
            if (validation != null)
            {
                return validation;
            }

            var auditId = Guid.NewGuid().ToString("N");
            var result = new MedicalEcfProcessResultDto
            {
                Success = true,
                AuditId = auditId,
                EcfType = request.EcfType,
                EcfNumber = request.EcfNumber,
                ReprintStatus = "AVAILABLE",
                CancelStatus = "AVAILABLE"
            };

            try
            {
                var factura = BuildFactura(request);

                if (request.ShouldGenerateXml)
                {
                    var generationResult = await _facturaDgiiRepository.ObtenerDatosFacturaJson(factura);
                    result.GenerationStatus = IsOk(generationResult) ? "GENERATED" : "FAILED";
                    result.SignatureStatus = IsOk(generationResult) && request.ShouldSign ? "SIGNED" : "SKIPPED";
                    result.ValidationStatus = IsOk(generationResult) && request.ShouldValidate ? "VALID" : "SKIPPED";

                    if (!IsOk(generationResult))
                    {
                        result.Success = false;
                        result.ErrorCode = "ECF_GENERATION_ERROR";
                        result.ErrorMessage = generationResult;
                        result.SendStatus = "SKIPPED";
                        result.DgiiStatus = "NOT_SENT";
                        return result;
                    }
                }
                else
                {
                    result.GenerationStatus = "SKIPPED";
                    result.SignatureStatus = "SKIPPED";
                    result.ValidationStatus = "SKIPPED";
                }

                if (request.ShouldSend)
                {
                    _facturaDgiiRepository.EnviarEncfXML();
                    result.SendStatus = "SENT";
                }
                else
                {
                    result.SendStatus = "SKIPPED";
                }

                if (request.ShouldQueryStatus)
                {
                    result.DgiiStatus = request.ShouldSend ? "IN_PROGRESS" : "PENDING_SEND";
                }
                else
                {
                    result.DgiiStatus = "SKIPPED";
                }

                if (request.ShouldAudit)
                {
                    _logger.LogInformation(
                        "Medical e-CF audit {AuditId}: type={Type}, number={Number}, scenario={Scenario}, insurance={Insurance}, amount={Amount}",
                        auditId,
                        request.EcfType,
                        request.EcfNumber,
                        request.BillingScenario,
                        request.IsInsuranceCase,
                        request.TotalAmount);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in medical e-CF workflow. Audit={AuditId}, Type={Type}, Number={Number}", auditId, request.EcfType, request.EcfNumber);
                return MedicalEcfProcessResultDto.Fail("ECF_WORKFLOW_EXCEPTION", ex.Message);
            }
        }

        private static bool IsOk(string status)
        {
            return string.Equals(status, "OK", StringComparison.OrdinalIgnoreCase);
        }

        private static MedicalEcfProcessResultDto ValidateRequest(MedicalEcfProcessRequestDto request)
        {
            if (request == null)
            {
                return MedicalEcfProcessResultDto.Fail("INVALID_REQUEST", "Request cannot be null.");
            }

            if (!MedicalEcfCatalog.SupportedTypes.Contains(request.EcfType))
            {
                return MedicalEcfProcessResultDto.Fail("INVALID_ECF_TYPE", "Unsupported e-CF type. Allowed values are E31, E32, E33, E34, E41, E43, E44, E45, E46, E47, E48 and E49.");
            }

            if (!MedicalEcfCatalog.SupportedMedicalScenarios.Contains(request.BillingScenario))
            {
                return MedicalEcfProcessResultDto.Fail("INVALID_MEDICAL_SCENARIO", "Medical scenario is not supported.");
            }

            if (request.TotalAmount <= 0)
            {
                return MedicalEcfProcessResultDto.Fail("INVALID_TOTAL_AMOUNT", "TotalAmount must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(request.PatientOrCompanyName))
            {
                return MedicalEcfProcessResultDto.Fail("INVALID_SUBJECT", "PatientOrCompanyName is required.");
            }

            return null;
        }

        private static FacturaDGII BuildFactura(MedicalEcfProcessRequestDto request)
        {
            if (request.Factura != null)
            {
                return new FacturaDGII
                {
                    TIPOECF = request.EcfType,
                    ENCF = request.EcfNumber,
                    NOMBRESERVICIO = request.Factura.NOMBRESERVICIO,
                    INDICADORTIPOSERVICIO = request.Factura.INDICADORTIPOSERVICIO,
                    MONTOTOTAL = request.Factura.MONTOTOTAL,
                    FECHAEMISION = request.Factura.FECHAEMISION,
                    RNCEMISOR = request.Factura.RNCEMISOR,
                    RAZONSOCIALEMISOR = request.Factura.RAZONSOCIALEMISOR,
                    RNCCOMPRADOR = request.Factura.RNCCOMPRADOR,
                    RAZONSOCIALCOMPRADOR = request.Factura.RAZONSOCIALCOMPRADOR,
                    DIRECCIONCOMPRADOR = request.Factura.DIRECCIONCOMPRADOR,
                    IDEFACT = request.Factura.IDEFACT,
                    MONTOGRAVADOTOTAL = request.Factura.MONTOGRAVADOTOTAL,
                    ITBIS1 = request.Factura.ITBIS1,
                    TOTALITBIS = request.Factura.TOTALITBIS,
                    TOTALITBIS1 = request.Factura.TOTALITBIS1,
                    INDICADORFACTURACION = request.Factura.INDICADORFACTURACION,
                    CANTIDADITEM = request.Factura.CANTIDADITEM,
                    UNIDADMEDIDA = request.Factura.UNIDADMEDIDA,
                    NOMBREITEM = request.Factura.NOMBREITEM,
                    MONTOPAGO = request.Factura.MONTOPAGO,
                    TIPOINGRESOS = request.Factura.TIPOINGRESOS,
                    TIPOPAGO = request.Factura.TIPOPAGO,
                    MONTOEXENTO = request.Factura.MONTOEXENTO
                };
            }

            return new FacturaDGII
            {
                TIPOECF = request.EcfType,
                ENCF = request.EcfNumber,
                RAZONSOCIALCOMPRADOR = request.PatientOrCompanyName,
                NOMBRESERVICIO = request.BillingScenario,
                MONTOTOTAL = request.TotalAmount.ToString("F2", System.Globalization.CultureInfo.InvariantCulture),
                FECHAEMISION = DateTime.UtcNow.ToString("dd-MM-yyyy")
            };
        }
    }
}
