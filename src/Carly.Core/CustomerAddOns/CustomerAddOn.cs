using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.CustomerAddOns
{
    public class CustomerAddOn : Entity<int>
    {
        public string addonname { get; set; }
        public string desc { get; set; }
        public float price { get; set; }
        public int CustomerPrincipalId { get; set; }
    }
}
