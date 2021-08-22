using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.LogoLinks.Dto
{
    [AutoMap(typeof(LogoLink))]
    public class LogoLinkDto : EntityDto<int>
    {
        public string Link { get; set; }
    }
}
