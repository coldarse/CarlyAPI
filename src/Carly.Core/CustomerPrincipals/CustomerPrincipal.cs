using Abp.Domain.Entities;
using Carly.CustomerAddOns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.CustomerPrincipals
{
    public class CustomerPrincipal : Entity<int>
    {
        public string Name { get; set; }
        public float Premium { get; set; }
        public List<CustomerAddOn> AddOns { get; set; }
        public int PackageId { get; set; }
    }
}
