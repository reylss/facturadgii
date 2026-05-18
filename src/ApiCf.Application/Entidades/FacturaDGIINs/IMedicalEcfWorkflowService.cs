using ApiCf.Entidades.FacturaDGIINs.Dto;
using System.Threading.Tasks;

namespace ApiCf.Entidades.FacturaDGIINs
{
    public interface IMedicalEcfWorkflowService
    {
        Task<MedicalEcfProcessResultDto> ExecuteWorkflowAsync(MedicalEcfProcessRequestDto request);
    }
}
