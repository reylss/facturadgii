using Abp.Application.Services.Dto;

namespace ApiCf.SharedNs
{
    public class PagedAndSortedRequest : PagedResultRequestDto, IPagedAndSortedResultRequest, IPagedResultRequest, ILimitedResultRequest, ISortedResultRequest
    {
        public string Sorting { get; set; }
        public string Filter { get; set; }    
        public string FilterProperties { get; set; }
    }
}







