using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Carly.Authorization;
using Carly.Vouchers.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Vouchers
{
    [AbpAuthorize(PermissionNames.Pages_Vouchers)]
    public class VoucherAppService : CrudAppService<Voucher, VoucherDto>
    {
        public VoucherAppService(IRepository<Voucher, int> repository) : base(repository)
        {
        }
    }
}
