using Abp.Application.Services.Dto;

namespace Carly.Sales
{
    public class PagedSaleResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}