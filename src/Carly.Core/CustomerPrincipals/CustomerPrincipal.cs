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
        public string Description { get; set; }
        public string ImageLink { get; set; }
        public float Premium { get; set; }
        public List<CustomerAddOn> AddOns { get; set; }
        public int PackageId { get; set; }
        public float Loading1 { get; set; }
        public float Loading2 { get; set; }
        public float Excess { get; set; }
        public float NCDA { get; set; }
        public float NCDP { get; set; }
        public float SumInsured { get; set; }
        public float GrossPremium { get; set; }
    }
}
