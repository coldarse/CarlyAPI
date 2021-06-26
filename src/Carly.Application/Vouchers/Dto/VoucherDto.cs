using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using Carly.AddOns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Vouchers.Dto
{
    [AutoMap(typeof(Voucher))]
    public class VoucherDto : EntityDto<int>
    {
        public string name { get; set; }
        public string code { get; set; }
        public string type { get; set; }
        public float minAmount { get; set; }
        public string description { get; set; }
        public DateTime startDate { get; set; }
        public DateTime stopDate { get; set; }
        public int limit { get; set; }
        public float discountAmount { get; set; }
        public int giftId { get; set; }
    }
}
