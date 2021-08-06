using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.GeneratedVouchers.Dto
{
    public class GeneratedVoucherDisplayDto
    {
        public string Code { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool isRedeemed { get; set; }
        public string Type { get; set; }
        public string RedeemedByVehicleReg { get; set; }
    }
}
