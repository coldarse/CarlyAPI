using Abp.Application.Services.Dto;
using Abp.AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.iPay88s.Dto
{
    [AutoMap(typeof(iPay88))]
    public class iPay88Dto : EntityDto<int>
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
    }
}
