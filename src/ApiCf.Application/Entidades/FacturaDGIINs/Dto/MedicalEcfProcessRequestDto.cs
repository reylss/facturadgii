using System.ComponentModel.DataAnnotations;

namespace ApiCf.Entidades.FacturaDGIINs.Dto
{
    public class MedicalEcfProcessRequestDto
    {
        [Required]
        public string EcfType { get; set; }

        [Required]
        public string EcfNumber { get; set; }

        [Required]
        public string PatientOrCompanyName { get; set; }

        [Required]
        public string BillingScenario { get; set; }

        public bool IsInsuranceCase { get; set; }

        public string ArsCode { get; set; }

        public string CorporateClientCode { get; set; }

        public string ProcedureCode { get; set; }

        public string StudyOrLabCode { get; set; }

        public string DoctorFeeCode { get; set; }

        public decimal TotalAmount { get; set; }

        public bool ShouldGenerateXml { get; set; } = true;

        public bool ShouldSign { get; set; } = true;

        public bool ShouldValidate { get; set; } = true;

        public bool ShouldSend { get; set; } = true;

        public bool ShouldQueryStatus { get; set; } = true;

        public bool ShouldAudit { get; set; } = true;

        public FacturaDGIIDto Factura { get; set; }
    }
}
