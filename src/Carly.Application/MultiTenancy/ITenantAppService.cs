using Abp.Application.Services;
using Carly.MultiTenancy.Dto;

namespace Carly.MultiTenancy
{
    public interface ITenantAppService : IAsyncCrudAppService<TenantDto, int, PagedTenantResultRequestDto, CreateTenantDto, TenantDto>
    {
    }
}

