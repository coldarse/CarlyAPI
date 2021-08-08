using Abp.Application.Services.Dto;

namespace Carly.Packages
{
    public class PagedPackageResultRequestDto : PagedResultRequestDto
    {
        public string Keyword { get; set; }
    }
}