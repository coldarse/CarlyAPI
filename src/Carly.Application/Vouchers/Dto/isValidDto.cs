using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Vouchers.Dto
{
    public class isValidDto
    {
        public bool isValid { get; set; }
        public string Type { get; set; }
        public float minAmount { get; set; }
        public float discountAmount { get; set; }
        public int giftId { get; set; }
        public string reason { get; set; }
    }
}
