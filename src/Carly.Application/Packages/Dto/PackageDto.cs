using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Carly.CustomerPrincipals;
using Carly.CustomerPrincipals.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Packages.Dto
{
    [AutoMap(typeof(Package))]
    public class PackageDto : EntityDto<int>
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
        public List<CustomerPrincipalDto> Principals { get; set; }
        public string Status { get; set; }
    }
}
