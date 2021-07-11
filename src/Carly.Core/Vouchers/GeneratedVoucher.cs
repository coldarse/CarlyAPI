using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Vouchers
{
    public class GeneratedVoucher : Entity<int>
    {
        public string Code { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool isRedeemed { get; set; }
        public string Type { get; set; }
        public int RedeemedByPackage { get; set; }
    }
}
