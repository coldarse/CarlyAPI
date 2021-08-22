using Abp.Domain.Entities;
using Carly.AddOns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Principals
{
	public class Principal : Entity<int>
	{
		public string Name { get; set; }
		public string Description { get; set; }
		public string ImageLink { get; set; }
		public List<AddOn> AddOns { get; set; }
	}
}
