using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Carly.Authorization;
using Carly.GeneratedVouchers.Dto;
using Carly.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.GeneratedVouchers
{
    [AbpAuthorize(PermissionNames.Pages_GeneratedVouchers)]
    public class GeneratedVoucherAppService : CrudAppService<GeneratedVoucher, GeneratedVoucherDto, int, PagedGeneratedVoucherResultRequestDto>
    {
        public GeneratedVoucherAppService(IRepository<GeneratedVoucher, int> repository) : base(repository)
        {
        }

        protected override IQueryable<GeneratedVoucher> CreateFilteredQuery(PagedGeneratedVoucherResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.Code.Contains(input.Keyword));

        }

    }
}
