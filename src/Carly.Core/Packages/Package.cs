using Abp.Domain.Entities;
using Carly.CustomerPrincipals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Packages
{
    public class Package : Entity<int>
    {
        public string OwnerName { get; set; }
        public string OwnerNRIC { get; set; }
        public string OwnerEmail { get; set; }
        public string OwnerPhoneNo { get; set; }
        public string VehicleModel { get; set; }
        public string VehicleRegNo { get; set; }
        public string VehicleYear { get; set; }
        public string CoverType { get; set; }
        public string CoveragePeriod { get; set; }
        public List<CustomerPrincipal> Principals { get; set; }
        public string Status { get; set; }
        public float Roadtax { get; set; }
        public float Delivery { get; set; }
        public float AdminFee { get; set; }
    }
}
