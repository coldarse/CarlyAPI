using Abp.Domain.Entities;
using Carly.AddOns;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Vouchers
{
    public class Voucher : Entity<int>
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
        public bool isGenerated { get; set; }

    }
}
