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

    public class iPay88ResponseDto
    {
        public string MerchantCode { get; set; }
        public int PaymentId { get; set; }
        public string RefNo { get; set; }
        public float Amount { get; set; }
        public string Currency { get; set; }
        public string Remark { get; set; }
        public string TransId { get; set; }
        public string AuthCode { get; set; }
        public string Status { get; set; }
        public string ErrDesc { get; set; }
        public string Signature { get; set; }
        public string CCName { get; set; }
        public string CCNo { get; set; }
        public string S_bankname { get; set; }
        public string S_country { get; set; }
        public string carly_signature { get; set; }
    }

    public class PaymentRequestDto
    {
        public string MerchantCode { get; set; }
        public int PaymentId { get; set; }
        public string RefNo { get; set; }
        public float Amount { get; set; }
        public string Currency { get; set; }
        public string ProdDesc { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserContact { get; set; }
        public string Remark { get; set; }
        public string Lang { get; set; }
        public string SingatureType { get; set; }
        public string Signature { get; set; }
        public string ResponseURL { get; set; }
        public string BackendURL { get; set; }
        public string Status { get; set; }
        public string carly_signature { get; set; }
    }
}
