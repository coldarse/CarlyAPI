using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Carly.AddOns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Principals.Dto
{
	[AutoMap(typeof(Principal))]
	public class PrincipalDto : EntityDto<int>
	{
		public string Name { get; set; }
		public List<AddOn> AddOns { get; set; }
	}
}
