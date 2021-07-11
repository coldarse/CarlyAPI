using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.EncryptKeys
{
    public class CryptedClasses
    {
    }

    public class RedeemVoucherDto
    {
        public string vouchercode { get; set; }
        public int packageid { get; set; }
        public string claimDate { get; set; }
        public string signature { get; set; }
    }

    public class GetPackageByIdDto
    {
        public int id { get; set; }
        public string signature { get; set; }
    }

    public class isVoucherValidDto
    {
        public string vouchercode { get; set; }
        public string claimDate { get; set; }
        public string signature { get; set; }
    }

    public class getGiftDto
    {
        public int giftId { get; set; }
        public string signature { get; set; }
    }
}
