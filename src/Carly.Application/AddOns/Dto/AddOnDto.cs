using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.AddOns.Dto
{
    [AutoMap(typeof(AddOn))]
    public class AddOnDto : EntityDto<int>
    {
        public string addonname { get; set; }
        public string desc { get; set; }
        public float price { get; set; }
        public int PrincipalId { get; set; }
    }
}
