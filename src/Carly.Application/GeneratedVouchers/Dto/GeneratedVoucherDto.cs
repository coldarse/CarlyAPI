using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Carly.Vouchers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.GeneratedVouchers.Dto
{
    [AutoMap(typeof(GeneratedVoucher))]
    public class GeneratedVoucherDto : EntityDto<int>
    {
        public string Code { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool isRedeemed { get; set; }
        public string Type { get; set; }
        public int RedeemedByPackage { get; set; }
    }
}
