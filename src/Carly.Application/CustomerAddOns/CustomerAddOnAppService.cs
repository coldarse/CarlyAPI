using Abp.Application.Services;
using Abp.Domain.Repositories;
using Carly.CustomerAddOns.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.CustomerAddOns
{
    public class CustomerAddOnAppService : CrudAppService<CustomerAddOn, CustomerAddOnDto>
    {
        public CustomerAddOnAppService(IRepository<CustomerAddOn, int> repository) : base(repository)
        {
        }
    }
}
