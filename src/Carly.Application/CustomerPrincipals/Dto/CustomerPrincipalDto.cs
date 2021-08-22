using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Carly.CustomerAddOns;
using Carly.CustomerAddOns.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.CustomerPrincipals.Dto
{
    [AutoMap(typeof(CustomerPrincipal))]
    public class CustomerPrincipalDto : EntityDto<int>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageLink { get; set; }
        public float Premium { get; set; }
        public List<CustomerAddOnDto> AddOns { get; set; }
        public int PackageId { get; set; }
    }
}
