namespace ApiCf.Entidades.FacturaDGIINs.Dto
{
    public class MedicalEcfProcessResultDto
    {
        public bool Success { get; set; }

        public string AuditId { get; set; }

        public string EcfType { get; set; }

        public string EcfNumber { get; set; }

        public string GenerationStatus { get; set; }

        public string SignatureStatus { get; set; }

        public string ValidationStatus { get; set; }

        public string SendStatus { get; set; }

        public string DgiiStatus { get; set; }

        public string ReprintStatus { get; set; }

        public string CancelStatus { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public static MedicalEcfProcessResultDto Fail(string errorCode, string errorMessage)
        {
            return new MedicalEcfProcessResultDto
            {
                Success = false,
                ErrorCode = errorCode,
                ErrorMessage = errorMessage,
                GenerationStatus = "SKIPPED",
                SignatureStatus = "SKIPPED",
                ValidationStatus = "SKIPPED",
                SendStatus = "SKIPPED",
                DgiiStatus = "SKIPPED",
                ReprintStatus = "SKIPPED",
                CancelStatus = "SKIPPED"
            };
        }
    }
}
