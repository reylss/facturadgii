using Abp.Application.Services.Dto;
using Abp.AutoMapper;

namespace ApiCf.Entidades.ComprobanteFiscalSecNs.Dto
{
    [AutoMap(typeof(ComprobanteFiscalSec))]
    public class ComprobanteFiscalSecDto : EntityDto<int>
    {
        public string NUMRELCOM { get; set; }
        public string NUMCOMPROBANTE { get; set; }
        public string NUMCOM { get; set; }
        public string STSCOM { get; set; }
        public string FECSTS { get; set; }
        public string IDENOTACRED { get; set; }
        public string IDEFACT { get; set; }
        public string FECREPORTE { get; set; }
        public string FECENVIO { get; set; }
        public string IDEFACT_AFECT { get; set; }
        public string SLDOFACTMONEDA { get; set; }
        public string SLDOFACTLOCAL { get; set; }
        public string STSDGII { get; set; }
        public string FACTURAQR { get; set; }


    }
}




