using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Carly.Authorization;
using Carly.iPay88s.Dto;

namespace Carly.iPay88s
{
    [AbpAuthorize(PermissionNames.Pages_PaymentGateway)]
    public class iPay88AppService : CrudAppService<iPay88, iPay88Dto, int, PagediPay88ResultRequestDto>
    {
        public iPay88AppService(IRepository<iPay88, int> repository) : base(repository)
        {
        }

        protected override IQueryable<iPay88> CreateFilteredQuery(PagediPay88ResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.RefNo.Contains(input.Keyword));

        }


        public List<iPay88> GetAlliPay88()
        {
            List<iPay88> tempiPay = Repository.GetAll().ToList();

            var orderbydesc = from s in tempiPay orderby s.Id descending select s;

            return orderbydesc.ToList();
        }

        

    }
}
