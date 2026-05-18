using AutoMapper;

namespace ApiCf.Entidades.ComprobanteFiscalSecNs.Dto
{
    public class ComprobanteFiscalSecMapProfile : Profile
    {
        public ComprobanteFiscalSecMapProfile()
        {
            CreateMap<ComprobanteFiscalSec, ComprobanteFiscalSecDto>();
            CreateMap<ComprobanteFiscalSecDto, ComprobanteFiscalSec>();
        }
    }
}







