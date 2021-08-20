using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Sales.Dto
{
    public class SaleDto : EntityDto<int>
    {
        public int Package { get; set; }
        public int SelectedPrincipal { get; set; }
        public string SelectedAddOns { get; set; }
        public float Premium { get; set; }
        public DateTime TransactionDateTime { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Postcode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ClaimedVoucher { get; set; }
    }
}
