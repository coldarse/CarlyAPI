using Abp.Application.Services;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Carly.Authorization;
using Carly.Sales.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Sales
{
    [AbpAuthorize(PermissionNames.Pages_Sales)]
    public class SaleAppService : CrudAppService<Sale, SaleDto, int, PagedSaleResultRequestDto>
    {
        public SaleAppService(IRepository<Sale, int> repository) : base(repository)
        {
        }

        public List<Sale> GetAllSale()
        {
            List<Sale> tempSale = Repository.GetAll().ToList();

            var orderbydesc = from s in tempSale orderby s.Id descending select s;

            return orderbydesc.ToList();
        }

        //protected override IQueryable<Sale> CreateFilteredQuery(PagedSaleResultRequestDto input)
        //{
        //    return Repository.GetAllIncluding()
        //        .WhereIf(input.Keyword.ToString().IsNullOrWhiteSpace(), x => x.Package == input.Keyword);
        //}
    }
}
