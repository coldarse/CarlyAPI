using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.CustomerAddOns.Dto
{
    [AutoMap(typeof(CustomerAddOn))]
    public class CustomerAddOnDto : EntityDto<int>
    {
        public string addonname { get; set; }
        public string desc { get; set; }
        public float price { get; set; }
        public int CustomerPrincipalId { get; set; }
    }
}
