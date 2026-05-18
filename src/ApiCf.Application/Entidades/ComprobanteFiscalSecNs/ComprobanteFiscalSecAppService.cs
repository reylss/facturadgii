
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Linq.Extensions;
using AutoMapper;
using ApiCf.Authorization;
using ApiCf.Entidades.ComprobanteFiscalSecNs.Dto;
using ApiCf.SharedNs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApiCf.Entidades.ComprobanteFiscalSecNs
{
    //
    public class ComprobanteFiscalSecAppService : IApplicationService, IComprobanteFiscalSecAppService
    {
        private readonly IComprobanteFiscalSecRepository _ComprobanteFiscalSecRepository;
        public ComprobanteFiscalSecAppService(IRepository<ComprobanteFiscalSec> repository,
                                    IComprobanteFiscalSecRepository ComprobanteFiscalSecRepository) 
        {
            _ComprobanteFiscalSecRepository = ComprobanteFiscalSecRepository;
        }
        // [AbpAuthorize]
        [AbpAuthorize]
        public Task<string> GetBuscarRespuesta()
        {
            var result = _ComprobanteFiscalSecRepository.ObtenerDatoComprobanteFiscal().Result.FirstOrDefault();
            return Task.FromResult("ok");

        }


        [UnitOfWork]
        [RemoteService(false)]
        public virtual async void ObtenerComprobanteFisca()
        {
            _ComprobanteFiscalSecRepository.ObtenerComprobanteFiscal();


        }


    }
}








