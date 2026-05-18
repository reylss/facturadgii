using AutoMapper;

namespace ApiCf.Entidades.FacturaDGIINs.Dto
{
    public class FacturaDGIIMapProfile : Profile
    {
        public FacturaDGIIMapProfile()
        {
            CreateMap<FacturaDGII, FacturaDGIIDto>();
            CreateMap<FacturaDGIIDto, FacturaDGII>();
        }
    }
}







