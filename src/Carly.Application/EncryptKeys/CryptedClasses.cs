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
        public string packageid { get; set; }
        public string claimDate { get; set; }
        public string signature { get; set; }
    }

    public class GetPackageByIdDto
    {
        public string id { get; set; }
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

    public class getSaleDto
    {
        public string id { get; set; }
        public string signature { get; set; }
    }

    public class createSalesDto
    {
        public string package { get; set; }
        public int selectedPrincipal { get; set; }
        public string selectedAddOns { get; set; }
        public string premium { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string postcode { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string signature { get; set; }
        public string claimedvoucher { get; set; }
    }
}
