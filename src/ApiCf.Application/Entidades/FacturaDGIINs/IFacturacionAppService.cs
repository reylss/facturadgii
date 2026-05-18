using Abp.Application.Services;
using ApiCf.Entidades.FacturaDGIINs.Dto;
using ApiCf.SharedNs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ApiCf.Entidades.FacturaDGIINs
{
    public interface IFacturacionAppService : IApplicationService
    {
        Task<MedicalEcfProcessResultDto> ProcesarComprobanteMedicoEcf(MedicalEcfProcessRequestDto request);

    }
}







