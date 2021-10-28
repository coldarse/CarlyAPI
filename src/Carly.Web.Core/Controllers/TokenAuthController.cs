using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Abp.Authorization;
using Abp.Authorization.Users;
using Abp.MultiTenancy;
using Abp.Runtime.Security;
using Abp.UI;
using Carly.Authentication.External;
using Carly.Authentication.JwtBearer;
using Carly.Authorization;
using Carly.Authorization.Users;
using Carly.Models.TokenAuth;
using Carly.MultiTenancy;
using Carly.Packages;
using Carly.CustomerPrincipals;
using Carly.CustomerAddOns;
using Abp.Domain.Repositories;
using Carly.Vouchers.Dto;
using Carly.Vouchers;
using Carly.AddOns;
using Carly.Principals;
using Carly.AddOns.Dto;
using Carly.EncryptKeys;
using Newtonsoft.Json;
using Carly.Sales;
using Carly.Sales.Dto;
using Carly.iPay88s;
using Carly.Payments;
using Abp.Web.Models;
using Carly.Users.Dto;

namespace Carly.Controllers
{
    [Route("api/[controller]/[action]")]
    //[DontWrapResult(WrapOnError = false, WrapOnSuccess = false)]
    public class TokenAuthController : CarlyControllerBase
    {
        private readonly LogInManager _logInManager;
        private readonly ITenantCache _tenantCache;
        private readonly AbpLoginResultTypeHelper _abpLoginResultTypeHelper;
        private readonly TokenAuthConfiguration _configuration;
        private readonly IExternalAuthConfiguration _externalAuthConfiguration;
        private readonly IExternalAuthManager _externalAuthManager;
        private readonly UserRegistrationManager _userRegistrationManager;

        private readonly IRepository<Package> _PackageRepository;
        private readonly IRepository<CustomerPrincipal> _CustomerPrincipalRepository;
        private readonly IRepository<CustomerAddOn> _CustomerAddOnRepository;

        private readonly IRepository<Voucher> _VoucherRepository;

        private readonly IRepository<AddOn> _AddOnRepository;
        private readonly IRepository<Principal> _PrincipalRepository;

        private readonly IRepository<GeneratedVoucher> _GeneratedVoucherRepository;
        private readonly IRepository<Sale> _SaleRepository;

        private readonly IRepository<iPay88> _iPay88Repository;
        private readonly IRepository<Payment> _PaymentRepository;

        public TokenAuthController(
            LogInManager logInManager,
            ITenantCache tenantCache,
            AbpLoginResultTypeHelper abpLoginResultTypeHelper,
            TokenAuthConfiguration configuration,
            IExternalAuthConfiguration externalAuthConfiguration,
            IExternalAuthManager externalAuthManager,
            UserRegistrationManager userRegistrationManager,
            IRepository<Package> PackageRepository,
            IRepository<CustomerPrincipal> CustomerPackageRepository,
            IRepository<CustomerAddOn> CustomerAddOnRepository,
            IRepository<Voucher> VoucherRepository,
            IRepository<AddOn> AddOnRepository,
            IRepository<Principal> PrincipalRepository,
            IRepository<GeneratedVoucher> GeneratedVoucherRepository,
            IRepository<Sale> SaleRepository,
            IRepository<iPay88> iPay88Repository,
            IRepository<Payment> PaymentRepository
            )
        {
            _logInManager = logInManager;
            _tenantCache = tenantCache;
            _abpLoginResultTypeHelper = abpLoginResultTypeHelper;
            _configuration = configuration;
            _externalAuthConfiguration = externalAuthConfiguration;
            _externalAuthManager = externalAuthManager;
            _userRegistrationManager = userRegistrationManager;
            _PackageRepository = PackageRepository;
            _CustomerPrincipalRepository = CustomerPackageRepository;
            _CustomerAddOnRepository = CustomerAddOnRepository;
            _VoucherRepository = VoucherRepository;
            _AddOnRepository = AddOnRepository;
            _PrincipalRepository = PrincipalRepository;
            _GeneratedVoucherRepository = GeneratedVoucherRepository;
            _SaleRepository = SaleRepository;
            _iPay88Repository = iPay88Repository;
            _PaymentRepository = PaymentRepository;
        }


        [HttpPost]
        public async Task<bool> SendSalesReceiptEmail([FromBody] SalesReceiptContentDto salesReceiptContentDto)
        {
            string EmailSubject = salesReceiptContentDto.Subject;

            string referenceNo = salesReceiptContentDto.InvoiceNo;

            for(int i = 0; i < 3; i++)
            {
                iPay88 tempiPay88 = _iPay88Repository.FirstOrDefault(x => x.RefNo == referenceNo && x.Status == "1");

                if(tempiPay88 is not null)
                {
                    salesReceiptContentDto.CardHolderName = tempiPay88.CCName;
                    salesReceiptContentDto.AuthCode = tempiPay88.AuthCode;

                    if (tempiPay88.PaymentId == 6)
                    {
                        salesReceiptContentDto.PaymentMethod = "Maybank2U";
                    }
                    else if (tempiPay88.PaymentId == 8)
                    {
                        salesReceiptContentDto.PaymentMethod = "Alliance Online (Personal)";
                    }
                    else if (tempiPay88.PaymentId == 10)
                    {
                        salesReceiptContentDto.PaymentMethod = "AmBank";
                    }
                    else if (tempiPay88.PaymentId == 14)
                    {
                        salesReceiptContentDto.PaymentMethod = "RHB Bank";
                    }
                    else if (tempiPay88.PaymentId == 15)
                    {
                        salesReceiptContentDto.PaymentMethod = "Hong Leong Bank";
                    }
                    else if (tempiPay88.PaymentId == 20)
                    {
                        salesReceiptContentDto.PaymentMethod = "CIMB Clicks";
                    }
                    else if (tempiPay88.PaymentId == 31)
                    {
                        salesReceiptContentDto.PaymentMethod = "Public Bank";
                    }
                    else if (tempiPay88.PaymentId == 102)
                    {
                        salesReceiptContentDto.PaymentMethod = "Bank Rakyat";
                    }
                    else if (tempiPay88.PaymentId == 103)
                    {
                        salesReceiptContentDto.PaymentMethod = "Affin Bank";
                    }
                    else if (tempiPay88.PaymentId == 122)
                    {
                        salesReceiptContentDto.PaymentMethod = "Pay4Me (Delay Payment)";
                    }
                    else if (tempiPay88.PaymentId == 124)
                    {
                        salesReceiptContentDto.PaymentMethod = "BSN";
                    }
                    else if (tempiPay88.PaymentId == 134)
                    {
                        salesReceiptContentDto.PaymentMethod = "Bank Islam";
                    }
                    else if (tempiPay88.PaymentId == 152)
                    {
                        salesReceiptContentDto.PaymentMethod = "UOB Bank";
                    }
                    else if (tempiPay88.PaymentId == 166)
                    {
                        salesReceiptContentDto.PaymentMethod = "Bank Muamalat";
                    }
                    else if (tempiPay88.PaymentId == 167)
                    {
                        salesReceiptContentDto.PaymentMethod = "OCBC Bank";
                    }
                    else if (tempiPay88.PaymentId == 168)
                    {
                        salesReceiptContentDto.PaymentMethod = "Standard Chartered Bank";
                    }
                    else if (tempiPay88.PaymentId == 178)
                    {
                        salesReceiptContentDto.PaymentMethod = "Maybank2E";
                    }
                    else if (tempiPay88.PaymentId == 198)
                    {
                        salesReceiptContentDto.PaymentMethod = "HSBC Bank";
                    }
                    else if (tempiPay88.PaymentId == 199)
                    {
                        salesReceiptContentDto.PaymentMethod = "Kuwait Finance House";
                    }
                    else if (tempiPay88.PaymentId == 405)
                    {
                        salesReceiptContentDto.PaymentMethod = "Agro bank";
                    }
                    else if (tempiPay88.PaymentId == 18)
                    {
                        salesReceiptContentDto.PaymentMethod = "China UnionPay Online Banking (MYR)";
                    }
                    else if (tempiPay88.PaymentId == 2)
                    {
                        salesReceiptContentDto.PaymentMethod = "Credit Card (MYR)";
                    }
                    else if (tempiPay88.PaymentId == 55)
                    {
                        salesReceiptContentDto.PaymentMethod = "Credit Card (MYR) Pre-Auth";
                    }
                    else if (tempiPay88.PaymentId == 111)
                    {
                        salesReceiptContentDto.PaymentMethod = "Public Bank EPP (Instalment Payment)";
                    }
                    else if (tempiPay88.PaymentId == 112)
                    {
                        salesReceiptContentDto.PaymentMethod = "Maybank EzyPay (Visa/Mastercard Instalment Payment)";
                    }
                    else if (tempiPay88.PaymentId == 115)
                    {
                        salesReceiptContentDto.PaymentMethod = "Maybank EzyPay (AMEX Instalment Payment)";
                    }
                    else if (tempiPay88.PaymentId == 157)
                    {
                        salesReceiptContentDto.PaymentMethod = "HSBC (Instalment Payment)";
                    }
                    else if (tempiPay88.PaymentId == 174)
                    {
                        salesReceiptContentDto.PaymentMethod = "CIMB Easy Pay (Instalment Payment)";
                    }
                    else if (tempiPay88.PaymentId == 179)
                    {
                        salesReceiptContentDto.PaymentMethod = "Hong Leong Bank EPP-MIGS (Instalment Payment)";
                    }
                    else if (tempiPay88.PaymentId == 430)
                    {
                        salesReceiptContentDto.PaymentMethod = "OCBC Instalment";
                    }
                    else if (tempiPay88.PaymentId == 433)
                    {
                        salesReceiptContentDto.PaymentMethod = "Hong Leong Bank EPP-MPGS (Instalment Payment)";
                    }
                    else if (tempiPay88.PaymentId == 534)
                    {
                        salesReceiptContentDto.PaymentMethod = "RHB (Instalment Payment)";
                    }
                    else if (tempiPay88.PaymentId == 606)
                    {
                        salesReceiptContentDto.PaymentMethod = "Ambank EPP";
                    }
                    else if (tempiPay88.PaymentId == 727)
                    {
                        salesReceiptContentDto.PaymentMethod = "Standard Chartered Bank Instalment";
                    }
                    else if (tempiPay88.PaymentId == 22)
                    {
                        salesReceiptContentDto.PaymentMethod = "Kiple Online";
                    }
                    else if (tempiPay88.PaymentId == 48)
                    {
                        salesReceiptContentDto.PaymentMethod = "PayPal (MYR)";
                    }
                    else if (tempiPay88.PaymentId == 210)
                    {
                        salesReceiptContentDto.PaymentMethod = "Boost Wallet Online";
                    }
                    else if (tempiPay88.PaymentId == 244)
                    {
                        salesReceiptContentDto.PaymentMethod = "MCash";
                    }
                    else if (tempiPay88.PaymentId == 382)
                    {
                        salesReceiptContentDto.PaymentMethod = "NETS QR Online";
                    }
                    else if (tempiPay88.PaymentId == 523)
                    {
                        salesReceiptContentDto.PaymentMethod = "GrabPay Online";
                    }
                    else if (tempiPay88.PaymentId == 538)
                    {
                        salesReceiptContentDto.PaymentMethod = "Touch 'n Go eWallet";
                    }
                    else if (tempiPay88.PaymentId == 542)
                    {
                        salesReceiptContentDto.PaymentMethod = "Maybank PayQR Online";
                    }
                    else if (tempiPay88.PaymentId == 801)
                    {
                        salesReceiptContentDto.PaymentMethod = "ShopeePay Online";
                    }
                    else if (tempiPay88.PaymentId == 890)
                    {
                        salesReceiptContentDto.PaymentMethod = "BNPL MobyPay";
                    }


                    string EmailBody = "<html lang =\"en\">"
                        + "<table border =\"0\" cellspacing =\"0\" width =\"100%\" "
                        + "style =\"background:#fff; font-family:quicksand; font-size:18px; line-height:24px\"> "
                        + "<tbody><tr>"
                        + "<td bgcolor =\"#ffffff\" >"
                        + "<table width =\"600\" align = \"center\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\" bgcolor = \"#ffffff\"> "
                        + "<tbody><tr>"
                        //+ "<td valign = \"top\" width = \"45\" ></td>"
                        + "<td valign = \"top\" style = \"font-family:quicksand; color:#000000; font-size:11px\">"
                        + "<table width = \"100%\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\">"
                        + "<tbody><tr>"
                        + "<td width = \"600\" style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style = \"text-align:center; margin: 10px 0\">"
                        + "<img src = \"{LogoImg}\" alt = \"Carly\" width = \"158\" height = \"73\" data-image-whitelisted = \"\" class =\"CToWUd\"/>"
                        + "</p></td>"
                        + "<td width = \"600\" style=\"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style = \"text-align:left; margin: 1px 0\">My Works Sdn Bhd(1340955 - H)</p>"
                        + "<p style = \"text-align:left; margin: 1px 0\">No. 1, Solok Sultan Mohamed 1,</p>"
                        + "<p style = \"text-align:left; margin: 1px 0\">Pusat Perdagangan Bandar Sultan Saleiman 4,</p>"
                        + "<p style = \"text-align:left; margin: 1px 0\">42000 Port Klang, Selangor.</p>"
                        + "<p style = \"text-align:left; margin: 1px 0\">T: +6017 - 865 6141</p>"
                        + "<p style = \"text-align:left; margin: 1px 0\">E: hello @carly.com.my</p>"
                        + "</td></tr>"
                        + "<tr><td width = \"600\" style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style= \"text-align:right; font-family:quicksand; font-size:32px; line-height:20px; font-weight:bold; color:#00008B;\">INVOICE</p>"
                        + "</td></tr>"
                        + "</tbody></table>"
                        + "<tbody><tr><td>"
                        + "<table width = \"100%\" border= \"0\" cellspacing= \"0\" cellpadding= \"0\">"
                        + "<tbody><tr><td style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style= \"inline-size: 100px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold\">Onwer</p>"
                        + "</td><td style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style= \"inline-size: 400px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold; overflow-wrap: break-word;\">{VehicleOwnerName}</p>"
                        + "</td></tr>"
                        + "<tr><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style= \"inline-size: 100px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold\">ID Number</p>"
                        + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style = \"inline-size: 400px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold; overflow-wrap: break-word;\">{VehicleICNumber}</p>"
                        + "</td></tr>"
                        + "<tr><td style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style= \"inline-size: 100px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold\">Address</p>"
                        + "</td><td style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style= \"inline-size: 400px; font-family:quicksand; font-size:15px; line-height:15px; font-weight:bold; overflow-wrap: break-word;\">{VehicleOwnerAddress}</p>"
                        + "</td></tr>"
                        + "</tbody></table>"
                        + "<table width = \"100%\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\">"
                        + "<td valign = \"top\" width= \"45\" ></td>"
                        + "<tr><td style= \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style= \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">Vehicle Reg.No.</p>"
                        + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style = \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">{VehicleRegistrationNumber}</p>"
                        + "</td><td><p style = \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:10px; font-weight:bold; overflow-wrap: break-word;\">Insurer</p>"
                        + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style = \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:10px; font-weight:bold; overflow-wrap: break-word;\">{Insurer}</p>"
                        + "</td></tr><tr><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style = \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:10px; font-weight:bold; overflow-wrap: break-word;\">Sum Insured</p>"
                        + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style= \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">{SumInsured}</p>"
                        + "</td><td><p style = \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">Type of Cover</p>"
                        + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style = \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">{TypeOfCover}</p>"
                        + "</td></tr>"
                        + "<tr><td><p style = \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">Invoice no</p>"
                        + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style = \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">{InvoiceNo}</p>"
                        + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style = \"inline-size: 120px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">Period of Cover</p>"
                        + "</td><td style = \"background:#fff; border-radius: 0 0 1rem 1rem\">"
                        + "<p style = \"inline-size: 180px; font-family:quicksand; font-size:15px; line-height:12px; font-weight:bold; overflow-wrap: break-word;\">{CoveragePeriod}</p>"
                        + "</td></tr></table>"
                        + "</td></tr></tbody></td><td valign = \"top\" width= \"45\"></td></tr></tbody></table>"
                        + "<table width = \"600\" align = \"center\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\" bgcolor = \"#f4f4f4\">"
                        + "<tbody><tr><td valign = \"top\" width = \"45\"></td><td align = \"center\" valign = \"top\">"
                        + "<table border = \"0\" cellspacing = \"0\" cellpadding = \"0\" width = \"100%\">"
                        + "<tbody><tr><td align = \"left\" height = \"20\"></td>"
                        + "</tr></tbody></table>"
                        + "<table border = \"0\" cellspacing = \"0\" cellpadding = \"0\" width = \"100%\">"
                        + "<tbody><tr><td width = \"55%\" align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold; color:#00af41\"></td></tr></tbody></table>"
                        + "<table width = \"100%\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\">"
                        + "<tbody><tr><td valign = \"top\" width = \"207\" style = \"max-width:207px; display:block\">"
                        + "<table width = \"100%\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\">"
                        + "<tbody><tr><td align = \"left\" valign = \"top\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">Transaction Details</td>"
                        + "</tr><tr><td align = \"center\" valign = \"middle\" height = \"10\"></td></tr>"
                        + "<tr><td valign = \"top\">"
                        + "<table width = \"100%\" border = \"0\" cellpadding = \"0\" cellspacing = \"0\">"
                        + "<tbody><tr><td align = \"left\" valign = \"top\" style = \"padding:0cm 0cm 0cm 0cm\">"
                        + "<table width = \"100%\" border = \"0\" cellpadding = \"0\" cellspacing = \"0\">"
                        + "<tbody><tr><td valign = \"top\">"
                        + "<table width = \"100%\" border = \"0\" cellspacing = \"0\" cellpadding = \"0\">"
                        + "<tbody><tr><td align = \"left\" valign = \"top\">"
                        + "<table border = \"0\" cellspacing = \"0\" cellpadding = \"0\" width = \"100%\">"
                        + "<tbody><tr><td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:12px; color:#9e9e9e; line-height:16px\">Transaction Date</span><br/>"
                        + "<span style = \"font-family:quicksand; font-size:12px;line-height:16px;font-weight:bold\">{TransactionDate}</span>"
                        + "</td></tr></tbody></table></td></tr><tr></tr><tr><td align = \"left\" valign = \"top\">"
                        + "<table border = \"0\" cellspacing = \"0\" cellpadding=\"0\" width=\"100%\">"
                        + "<tbody><tr><td align = \"left\" style = \"font-family:quicksand; font-size:14px;font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:12px; color:#9e9e9e; line-height:14px\">Cardholder's Name</span><br />"
                        + "<span style = \"font-family:quicksand; font-size:12px; line-height:16px; font-weight:bold\">{CardHolderName}</span>"
                        + "</td></tr></tbody></table></td></tr>"
                        + "<tr><td align = \"left\" valign =\"top\"><table border = \"0\" cellspacing =\"0\" cellpadding=\"0\" width=\"100%\">"
                        + "<tbody><tr><td align = \"left\" style =\"font-family:quicksand; font-size:14px;font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:12px; color:#9e9e9e; line-height:14px\">Authorization Code</span><br/>"
                        + "<span style = \"font-family:quicksand; font-size:12px; line-height:16px; font-weight:bold\">{AuthCode}</span>"
                        + "</td></tr></tbody></table></td></tr>"
                        + "<tr><td height =\"3\"></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table>"
                        + "</td></tr></tbody></table></td><td valign = \"top\" width=\"9\"></td><td valign = \"top\" width=\"10\" bgcolor=\"#f5f5f3\"></td>"
                        + "<td valign = \"top\" width=\"280\" style=\"max-width:280px\">"
                        + "<table width = \"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">"
                        + "<tbody><tr><td align = \"left\" valign=\"top\" style=\"font-family:quicksand; font-size:14px; font-weight:bold;\">Receipt Summary</td></tr>"
                        + "<tr><td align = \"center\" valign= \"middle\" height= \"10\" ></td></tr>"
                        + "<tr><td valign = \"top\">"
                        + "<table width =\"100%\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\" bgcolor=\"#ffffff\" style=\"border:1px solid #dddddd\">"
                        + "<tbody><tr><td align = \"left\" valign=\"top\" style=\"padding:0cm 0cm 0cm 0cm\">"
                        + "<table width = \"100%\" border=\"0\" cellpadding=\"0\" cellspacing=\"0\">"
                        + "<tbody><tr><td valign = \"top\" ><table width =\"100%\" border=\"0\" cellspacing=\"0\" cellpadding=\"0\">"
                        + "<tbody><tr><td align = \"left\" valign=\"top\"><table border = \"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\">"
                        + "<tr><td height = \"10px\" align=\"left\"></td><td height = \"10px\" colspan=\"2\" align=\"left\"></td><td height = \"10px\" align=\"left\"></td></tr>"
                        + "<tbody><tr><td height = \"5px\" align=\"left\"></td><td height = \"5px\" colspan=\"2\" align=\"left\" style=\"font-family:quicksand; font-size:11px;line-height:18px; color:#9e9e9e\">"
                        + "Payment Method:<br /><span style = \"font-weight:bold;color:#000000\"> {PaymentMethod}&nbsp;&nbsp;</span></td>"
                        + "<td height = \"5px\" align=\"left\"></td></tr><tr><td height = \"5px\" align =\"left\"></td><td height = \"5px\" colspan =\"2\" align =\"left\"></td>"
                        + "<td height = \"5px\" align =\"left\"></td></tr>"
                        + "<tr><td height = \"3px\" align =\"left\"></td><td height = \"3px\" colspan =\"2\" align =\"left\" style =\"border-top:1px dashed #9e9e9e\"></td>"
                        + "<td height = \"3px\" align =\"left\"></td></tr>"
                        + "<tr><td align = \"left\" width =\"15\"></td><td width = \"171\" align =\"left\" style =\"font-family:quicksand; font-weight:normal; color:#000000\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; color:#9e9e9e; line-height:21px\"> Description:</span></td>"
                        + "<td width = \"80\" align =\"left\" style =\"font-family:quicksand; font-weight:normal; color:#000000\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; color:#9e9e9e; line-height:28px\"> &nbsp; &nbsp; &nbsp; &nbsp; Amount:</span></td>"
                        + "<td align = \"left\" width =\"15\"></td></tr>"
                        + "<tr><td height = \"3px\" align =\"left\"></td>"
                        + "<td height = \"3px\" colspan =\"2\" align =\"left\"></td>"
                        + "<td height = \"3px\" align =\"left\"></td></tr>"
                        + "<tr><td height = \"5px\" align =\"left\"></td><td height = \"5px\" colspan =\"2\" align =\"left\" style =\"border-top:1px dashed #9e9e9e\"></td>"
                        + "<td height = \"5px\" align =\"left\"></td></tr>"
                        + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"></span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; font-weight:bold\"> Basic Premium</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:11px; line-height:18px\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; font-weight:bold\"> &nbsp; &nbsp; &nbsp; &nbsp; RM 1,265.91 </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; + Loading 1 </span></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {loading1}</span></td>"
                        + "<td align = \"right\" width =\"15\"></td><td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp;</span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; + Loading 2</span></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {loading2} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; - No Claim Discount (NCD 45 %) </span></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {NCD} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\">&nbsp; &nbsp;</span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr><td align = \"right\" width =\"15\"></td><td align = \"right\">&nbsp;</td>"
                        + "<td align = \"right\">&nbsp;</td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr style = \"color:#000000\">"
                        + "<td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"></span>"
                        + "<span style = \"font-family:quicksand; font-size:11px;font-weight:bold\"> Selected Add Ons:</span></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp;</span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>";

                    if (salesReceiptContentDto.AddOns1.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns1}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns1Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns2.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns2}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns2Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns3.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns3}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns3Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns4.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns4}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns4Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns5.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns5}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns5Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns6.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns6}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns6Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns7.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns7}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns7Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns8.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns8}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns8Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns9.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns9}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns9Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns10.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns10}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns10Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns11.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns11}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns11Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns12.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns12}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns12Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns13.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns13}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns13Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns14.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns14}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns14Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }
                    if (salesReceiptContentDto.AddOns15.Length > 0)
                    {
                        EmailBody += "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold; display: block;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px; display: block;\"> &nbsp; {AddOns15}</span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AddOns15Price} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr><tr>";
                    }


                    EmailBody += "<td align = \"right\" width =\"15\"></td><td align = \"right\">&nbsp;</td>"
                        + "<td align = \"right\"> &nbsp;</td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:11px; line-height:18px\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"></span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; font-weight:bold\"> Gross Premium </span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:11px; line-height:18px\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {GrossPremium} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; Server Tax @ 6 %</span></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px;font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {ServiceTax} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; Stamp Duty </span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px;font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {StampDuty} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr>"
                        + "<td align = \"right\" width =\"15\"></td>"
                        + "<td align = \"right\">&nbsp;</td>"
                        + "<td align = \"right\">&nbsp;</td>"
                        + "<td align = \"right\" width =\"15\"></td>"
                        + "</tr>"
                        + "<tr style = \"color:#000000\">"
                        + "<td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; Admin Fee </span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {AdminFee} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; MyEG + Delivery </span></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {MyegDelivery} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr style = \"color:#000000\"><td align =\"left\"width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp;</span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; Roadtax Renewal </span></td>"
                        + "<td align = \"left\" style = \"font-family:quicksand; font-size:14px;font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px;line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {RoadTaxRenewal} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr>"
                        + "<tr><td height = \"10px\" align =\"left\"></td>"
                        + "<td height = \"10px\" colspan =\"2\" align =\"left\"></td>"
                        + "<td height = \"10px\" align =\"left\"></td></tr>"
                        + "<tr><td height = \"10px\" align =\"left\"></td>"
                        + "<td height = \"10px\" colspan =\"2\" align =\"left\" style =\"border-top:1px dashed #9e9e9e\"></td>"
                        + "<td height = \"10px\" align =\"left\"></td></tr>"
                        + "<tr style = \"color:#000000\"><td align =\"left\" width =\"15\"></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"></span>"
                        + "<span style = \"font-family:quicksand; font-size:11px; font-weight:bold\"> Total Payable Premium </span></td>"
                        + "<td align = \"left\" style =\"font-family:quicksand; font-size:14px; font-weight:bold;\">"
                        + "<span style = \"font-family:quicksand; font-size:11px; line-height:18px\"> &nbsp; &nbsp; &nbsp; &nbsp; {TotalPayablePremium} </span></td>"
                        + "<td align = \"right\" width =\"15\"></td></tr></tbody>"
                        + "</table></td></tr>"
                        + "<tr><td align = \"left\" valign =\"top\">"
                        + "<table border = \"0\" cellspacing=\"0\" cellpadding=\"0\" width=\"100%\">"
                        + "<tbody><tr><td align = \"left\" style=\"font-family:quicksand; font-size:15px; font-weight:bold;\"></td></tr></tbody>"
                        + "</table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody></table></td></tr></tbody>"
                        + "</table></td></tr></tbody></table><table border = \"0\" cellpadding=\"0\" cellspacing=\"0\" width=\"100%\">"
                        + "<tbody><tr><td height = \"20\"></td></tr></tbody>"
                        + "</table></td><td valign=\"top\" width=\"45\">"
                        + "</td></tr></tbody></table></td></tr></tbody></table></html>";


                    string filepath = "";

                    EmailBody = EmailBody.Replace(@"{VehicleOwnerName}", salesReceiptContentDto.VehicleOwnerName)
                        .Replace(@"{VehicleICNumber}", salesReceiptContentDto.VehicleICNumber)
                        .Replace(@"{VehicleOwnerAddress}", salesReceiptContentDto.VehicleOwnerAddress)
                        .Replace(@"{VehicleRegistrationNumber}", salesReceiptContentDto.VehicleRegistrationNumber)
                        .Replace(@"{Insurer}", salesReceiptContentDto.Insurer)
                        .Replace(@"{InvoiceNo}", salesReceiptContentDto.VehicleRegistrationNumber)
                        .Replace(@"{CoveragePeriod}", salesReceiptContentDto.CoveragePeriod)
                        .Replace(@"{TypeOfCover}", salesReceiptContentDto.TypeOfCover)
                        .Replace(@"{SumInsured}", salesReceiptContentDto.SumInsured)
                        .Replace(@"{TransactionDate}", salesReceiptContentDto.TransactionDate)
                        .Replace(@"{CardHolderName}", salesReceiptContentDto.CardHolderName)
                        .Replace(@"{AuthCode}", salesReceiptContentDto.AuthCode)
                        .Replace(@"{PaymentMethod}", salesReceiptContentDto.PaymentMethod)
                        .Replace(@"{BasicPremium}", salesReceiptContentDto.BasicPremium)
                        .Replace(@"{loading1}", salesReceiptContentDto.loading1)
                        .Replace(@"{loading2}", salesReceiptContentDto.loading2)
                        .Replace(@"{NCD}", salesReceiptContentDto.NCD)
                        .Replace(@"{AddOns1}", salesReceiptContentDto.AddOns1)
                        .Replace(@"{AddOns1Price}", salesReceiptContentDto.AddOns1Price)
                        .Replace(@"{AddOns2}", salesReceiptContentDto.AddOns2)
                        .Replace(@"{AddOns2Price}", salesReceiptContentDto.AddOns2Price)
                        .Replace(@"{AddOns3}", salesReceiptContentDto.AddOns3)
                        .Replace(@"{AddOns3Price}", salesReceiptContentDto.AddOns3Price)
                        .Replace(@"{AddOns4}", salesReceiptContentDto.AddOns4)
                        .Replace(@"{AddOns4Price}", salesReceiptContentDto.AddOns4Price)
                        .Replace(@"{AddOns5}", salesReceiptContentDto.AddOns5)
                        .Replace(@"{AddOns5Price}", salesReceiptContentDto.AddOns5Price)
                        .Replace(@"{AddOns6}", salesReceiptContentDto.AddOns6)
                        .Replace(@"{AddOns6Price}", salesReceiptContentDto.AddOns6Price)
                        .Replace(@"{AddOns7}", salesReceiptContentDto.AddOns7)
                        .Replace(@"{AddOns7Price}", salesReceiptContentDto.AddOns7Price)
                        .Replace(@"{AddOns8}", salesReceiptContentDto.AddOns8)
                        .Replace(@"{AddOns8Price}", salesReceiptContentDto.AddOns8Price)
                        .Replace(@"{AddOns9}", salesReceiptContentDto.AddOns9)
                        .Replace(@"{AddOns9Price}", salesReceiptContentDto.AddOns9Price)
                        .Replace(@"{AddOns10}", salesReceiptContentDto.AddOns10)
                        .Replace(@"{AddOns10Price}", salesReceiptContentDto.AddOns10Price)
                        .Replace(@"{AddOns11}", salesReceiptContentDto.AddOns11)
                        .Replace(@"{AddOns11Price}", salesReceiptContentDto.AddOns11Price)
                        .Replace(@"{AddOns12}", salesReceiptContentDto.AddOns12)
                        .Replace(@"{AddOns12Price}", salesReceiptContentDto.AddOns12Price)
                        .Replace(@"{AddOns13}", salesReceiptContentDto.AddOns13)
                        .Replace(@"{AddOns13Price}", salesReceiptContentDto.AddOns13Price)
                        .Replace(@"{AddOns14}", salesReceiptContentDto.AddOns14)
                        .Replace(@"{AddOns14Price}", salesReceiptContentDto.AddOns14Price)
                        .Replace(@"{AddOns15}", salesReceiptContentDto.AddOns15)
                        .Replace(@"{AddOns15Price}", salesReceiptContentDto.AddOns15Price)
                        .Replace(@"{GrossPremium}", salesReceiptContentDto.GrossPremium)
                        .Replace(@"{ServiceTax}", salesReceiptContentDto.ServiceTax)
                        .Replace(@"{StampDuty}", salesReceiptContentDto.StampDuty)
                        .Replace(@"{AdminFee}", salesReceiptContentDto.AdminFee)
                        .Replace(@"{MyegDelivery}", salesReceiptContentDto.MyegDelivery)
                        .Replace(@"{RoadTaxRenewal}", salesReceiptContentDto.RoadTaxRenewal)
                        .Replace(@"{TotalPayablePremium}", salesReceiptContentDto.TotalPayablePremium)
                        .Replace(@"{LogoImg}", "https://system.carly.com.my/CarlyImage/carly-logo.png");

                    Emails.IEmailAppService emailAppService = new Emails.EmailAppService(SettingManager);

                    bool isEmailSent = await emailAppService.SendEmailAsync(salesReceiptContentDto.emailTo, EmailSubject, EmailBody, filepath);

                    return isEmailSent;
                }
                else
                {
                    continue;
                }
            }
            return false;
        }

        [HttpPost]
        public async Task<Payment> InsertPaymentRequest(string PaymentRequest)
        {
            PaymentRequestDto tempRequest = JsonConvert.DeserializeObject<PaymentRequestDto>(PaymentRequest);
            string carly_signature = tempRequest.carly_signature.Replace(" ", "+");

            tempRequest.carly_signature = "";

            string JSONString = JsonConvert.SerializeObject(tempRequest, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);

            if (!Equals(encryptedString, carly_signature)) { return new Payment(); }

            Payment newPayment = new Payment();
            newPayment.MerchantCode = tempRequest.MerchantCode;
            newPayment.PaymentId = tempRequest.PaymentId;
            newPayment.RefNo = tempRequest.RefNo;
            newPayment.Amount = tempRequest.Amount;
            newPayment.Currency = tempRequest.Currency;
            newPayment.ProdDesc = tempRequest.ProdDesc;
            newPayment.UserName = tempRequest.UserName;
            newPayment.UserEmail = tempRequest.UserEmail;
            newPayment.UserContact = tempRequest.UserContact;
            newPayment.Remark = tempRequest.Remark;
            newPayment.Lang = tempRequest.Lang;
            newPayment.SingatureType = tempRequest.SingatureType;
            newPayment.Signature = tempRequest.Signature;
            newPayment.ResponseURL = tempRequest.ResponseURL;
            newPayment.BackendURL = tempRequest.BackendURL;
            newPayment.Status = tempRequest.Status;

            return await _PaymentRepository.InsertAsync(newPayment);
        }

        [HttpPost]
        [WrapResult(WrapOnError = false, WrapOnSuccess = false)]
        public async Task<string> InsertiPay88Response(iPay88 iPay88Response)
        {

            iPay88 newPayment = iPay88Response;

            iPay88 tempInsert = await _iPay88Repository.InsertAsync(newPayment);

            if(tempInsert != null)
            {
                return "RECEIVEOK";
            }
            else
            {
                return "FAIL";
            }

            
        }

        [HttpGet]
        public bool GetSale(string getSale)
        {

            getSaleDto tempSale = JsonConvert.DeserializeObject<getSaleDto>(getSale);
            string stringid = EncryptKey.Decrypt(tempSale.id);
            string signature = tempSale.signature.Replace(" ", "+");

            tempSale.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempSale, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);

            if (!Equals(encryptedString, signature)) { return false; }


            List<Sale> tempListSale = _SaleRepository.GetAll().ToList();

            foreach(var ts in tempListSale)
            {
                if(ts.Package.ToString() == stringid)
                {
                    return false;
                }
            }

            return true;
        }

        [HttpPost]
        public async Task<Sale> CreateSale(string sales)
        {
            createSalesDto tempsales = JsonConvert.DeserializeObject<createSalesDto>(sales);
            string package = tempsales.package;
            int selectedPrincipal = tempsales.selectedPrincipal;
            string selectedAddOns = tempsales.selectedAddOns;
            string premium = tempsales.premium;
            string address1 = tempsales.address1;
            string address2 = tempsales.address2;
            string postcode = tempsales.postcode;
            string city = tempsales.city;
            string state = tempsales.state;
            string signature = tempsales.signature.Replace(" ", "+");
            string claimedvoucher = tempsales.claimedvoucher;
            string referenceno = tempsales.referenceno;

            int tempPackage = Convert.ToInt32(EncryptKey.Decrypt(package));

            tempsales.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempsales, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);

            if (!Equals(encryptedString, signature)) { return new Sale(); }

            Sale newSales = new Sale();
            newSales.Package = tempPackage;
            newSales.SelectedPrincipal = selectedPrincipal;
            newSales.SelectedAddOns = selectedAddOns;
            newSales.Premium = float.Parse(premium);
            newSales.TransactionDateTime = DateTime.Now;
            newSales.Address1 = address1;
            newSales.Address2 = address2;
            newSales.Postcode = postcode;
            newSales.City = city;
            newSales.State = state;
            newSales.ClaimedVoucher = claimedvoucher;
            newSales.ReferenceNo = referenceno;

            return await _SaleRepository.InsertAsync(newSales);
        }

        [HttpPut]
        public async Task<bool> RedeemVoucher(string redeemvoucher)
        {
            RedeemVoucherDto tempredeemvoucher = JsonConvert.DeserializeObject<RedeemVoucherDto>(redeemvoucher);
            string vouchercode = tempredeemvoucher.vouchercode;
            string packageid = EncryptKey.Decrypt(tempredeemvoucher.packageid);
            string claimDate = tempredeemvoucher.claimDate;
            string signature = tempredeemvoucher.signature.Replace(" ", "+");

            tempredeemvoucher.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempredeemvoucher, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);

            if(!Equals(encryptedString, signature)) { return false; }

            DateTime newClaimDate = Convert.ToDateTime(claimDate);
            List<GeneratedVoucher> tempGenVoucher = _GeneratedVoucherRepository.GetAll().ToList();
            List<Package> tempPackage = _PackageRepository.GetAll().ToList();

            string regno = "";
            foreach(var p in tempPackage)
            {
                if(p.Id.ToString() == packageid)
                {
                    regno = p.VehicleRegNo;
                    break;
                }
            }

            List<Voucher> tempVoucher = _VoucherRepository.GetAll().ToList();
            bool isGeneral = false;
            foreach(var v in tempVoucher)
            {
                if (vouchercode.ToLower().Contains(v.code.ToLower()))
                {
                    if (v.description.ToLower().Contains("general"))
                    {
                        isGeneral = true;
                    }
                }
            }

            
            foreach (var g in tempGenVoucher)
            {
                if (g.Code.ToLower().Equals(vouchercode.ToLower()))
                {
                    if(newClaimDate >= g.StartDate && newClaimDate <= g.EndDate)
                    {
                        if(isGeneral == false)
                        {
                            g.isRedeemed = true;
                            g.RedeemedByPackage = regno;
                            await _GeneratedVoucherRepository.UpdateAsync(g);
                            return true;
                        }
                        else
                        {
                            return true;
                        }
                        
                    }
                }
            }

            return false;

        }


        [HttpPost]
        public string testEncrypt(int id)
        {
            string stringid = id.ToString();
            string encryptedID = EncryptKey.Encrypt(stringid);
            return encryptedID;
        }

        [HttpPost]
        public int testDecrypt(string encryptedID)
        {
            string stringid = EncryptKey.Decrypt(encryptedID);
            //string id = stringid;
            int id = Convert.ToInt32(stringid);
            return id;
        }


        [HttpPost]
        public async Task<IActionResult> iPay88Redirect(iPay88 iPay88Response)
        {
            if (iPay88Response.Status == "1")
            {
                return Redirect("https://system.carly.com.my/CarlyApp/#/thankyou");
            }
            else
            {
                iPay88 newPayment = iPay88Response;

                iPay88 tempInsert = await _iPay88Repository.InsertAsync(newPayment);

                return Redirect("https://system.carly.com.my/CarlyApp/#/failorder");
            }
        }

        [HttpGet]
        public Package GetPackageById(string getpackagebyid)
        {
            GetPackageByIdDto tempgetpackagebyid = JsonConvert.DeserializeObject<GetPackageByIdDto>(getpackagebyid);
            string stringid = EncryptKey.Decrypt(tempgetpackagebyid.id);
            string signature = tempgetpackagebyid.signature.Replace(" ", "+");

            tempgetpackagebyid.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempgetpackagebyid, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);

            if (!Equals(encryptedString, signature)) { return new Package(); }

            List<CustomerPrincipal> tempPrincipal = _CustomerPrincipalRepository.GetAll().Where(f => f.PackageId.ToString().Equals(stringid)).ToList();


            List<CustomerAddOn>[] a = new List<CustomerAddOn>[3];
            int x = 0;
            foreach (var prin in tempPrincipal)
            {
                a[x] = _CustomerAddOnRepository.GetAll().Where(f => f.CustomerPrincipalId.ToString().Equals(prin.Id.ToString())).ToList();
                x += 1;
            }
            //List<CustomerAddOn> tempAddOn = _CustomerAddOnRepository.GetAll().Where(f => f.CustomerPrincipalId.ToString().Equals(principalId.ToString())).ToList();

            Package tempPackage = _PackageRepository.FirstOrDefault(f => f.Id.ToString().Equals(stringid));

            return tempPackage;
        }

        [HttpPost]
        public isValidDto isVoucherValid(string isvouchervalid)
        {
            isVoucherValidDto tempisvouchervalid = JsonConvert.DeserializeObject<isVoucherValidDto>(isvouchervalid);
            string vouchercode = tempisvouchervalid.vouchercode;
            string claimDate = tempisvouchervalid.claimDate;
            string signature = tempisvouchervalid.signature.Replace(" ", "+");

            tempisvouchervalid.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempisvouchervalid, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);
                
            if (!Equals(encryptedString, signature)) { return new isValidDto(); }

            DateTime newclaimDate = Convert.ToDateTime(claimDate);
            isValidDto tempIsValid = new isValidDto();
            if (vouchercode == null)
            {
                tempIsValid.isValid = false;
                tempIsValid.Type = "";
                tempIsValid.minAmount = 0.00f;
                tempIsValid.discountAmount = 0.00f;
                tempIsValid.giftId = 0;
                tempIsValid.reason = "Empty Voucher Code";

                return tempIsValid;
            }

            List<GeneratedVoucher> tempGenVoucher = _GeneratedVoucherRepository.GetAll().ToList();

            foreach(var g in tempGenVoucher)
            {
                if (g.Code.ToLower().Equals(vouchercode.ToLower()))
                {
                    if(g.isRedeemed == true)
                    {
                        tempIsValid.isValid = false;
                        tempIsValid.Type = "";
                        tempIsValid.minAmount = 0.00f;
                        tempIsValid.discountAmount = 0.00f;
                        tempIsValid.giftId = 0;
                        tempIsValid.reason = "Voucher Code has been Claimed";

                        return tempIsValid;
                    }
                }
            }

            List<Voucher> tempVoucher = _VoucherRepository.GetAll().ToList();

            foreach (var d in tempVoucher)
            {
                if (vouchercode.ToLower().Contains(d.code.ToLower().ToString()))
                {

                    if (d.limit == 1)
                    {
                        if (newclaimDate >= d.startDate && newclaimDate <= d.stopDate)
                        {
                            tempIsValid.isValid = true;
                            tempIsValid.Type = d.type;
                            tempIsValid.minAmount = d.minAmount;
                            tempIsValid.discountAmount = d.discountAmount;
                            tempIsValid.giftId = d.giftId;
                            tempIsValid.reason = "Success";

                            return tempIsValid;
                        }
                        else
                        {
                            tempIsValid.isValid = false;
                            tempIsValid.Type = "";
                            tempIsValid.minAmount = 0.00f;
                            tempIsValid.discountAmount = 0.00f;
                            tempIsValid.giftId = 0;
                            tempIsValid.reason = "Voucher Expired/Not yet activated";

                            return tempIsValid;
                        }
                    }

                    string substringed = vouchercode.Substring(0, vouchercode.Length - 2);
                    string temp = substringed.ToUpper().Replace(d.code.ToUpper().ToString(), "");
                    int limit = 0;
                    bool isSucceed = Int32.TryParse(temp, out limit);
                    if (isSucceed)
                    {
                        if (limit <= Convert.ToInt32(d.limit))
                        {
                            if (newclaimDate >= d.startDate && newclaimDate <= d.stopDate)
                            {
                                tempIsValid.isValid = true;
                                tempIsValid.Type = d.type;
                                tempIsValid.minAmount = d.minAmount;
                                tempIsValid.discountAmount = d.discountAmount;
                                tempIsValid.giftId = d.giftId;
                                tempIsValid.reason = "Success";

                                return tempIsValid;
                            }
                            else
                            {
                                tempIsValid.isValid = false;
                                tempIsValid.Type = "";
                                tempIsValid.minAmount = 0.00f;
                                tempIsValid.discountAmount = 0.00f;
                                tempIsValid.giftId = 0;
                                tempIsValid.reason = "Voucher Expired/Not yet activated";

                                return tempIsValid;
                            }
                        }
                        else
                        {
                            tempIsValid.isValid = false;
                            tempIsValid.Type = "";
                            tempIsValid.minAmount = 0.00f;
                            tempIsValid.discountAmount = 0.00f;
                            tempIsValid.giftId = 0;
                            tempIsValid.reason = "Invalid Voucher Code";

                            return tempIsValid;
                        }
                    }
                    else
                    {
                        continue;
                    }

                }
            }

            tempIsValid.isValid = false;
            tempIsValid.Type = "";
            tempIsValid.minAmount = 0.00f;
            tempIsValid.discountAmount = 0.00f;
            tempIsValid.giftId = 0;
            tempIsValid.reason = "Invalid Voucher Code";

            return tempIsValid;
        }

        [HttpGet]
        public GiftDto getGift(string getgift)
        {
            getGiftDto tempgetgift = JsonConvert.DeserializeObject<getGiftDto>(getgift);
            int giftId = tempgetgift.giftId;
            string signature = tempgetgift.signature.Replace(" ", "+");

            tempgetgift.signature = "";

            string JSONString = JsonConvert.SerializeObject(tempgetgift, new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
            });

            string encryptedString = EncryptKey.Encrypt(JSONString);

            if (!Equals(encryptedString, signature)) { return new GiftDto(); }

            AddOn tempAddOn = _AddOnRepository.FirstOrDefault(giftId);
            Principal tempPrincipal = _PrincipalRepository.FirstOrDefault(tempAddOn.PrincipalId);

            GiftDto tempGift = new GiftDto();

            tempGift.addOnId = tempAddOn.Id;
            tempGift.addOnName = tempAddOn.addonname;
            tempGift.principalId = tempPrincipal.Id;
            tempGift.principalName = tempPrincipal.Name;

            return tempGift;
        } 

        [HttpPost]
        public async Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
        {
            var loginResult = await GetLoginResultAsync(
                model.UserNameOrEmailAddress,
                model.Password,
                GetTenancyNameOrNull()
            );

            var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));

            return new AuthenticateResultModel
            {
                AccessToken = accessToken,
                EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds,
                UserId = loginResult.User.Id
            };
        }

        [HttpGet]
        public List<ExternalLoginProviderInfoModel> GetExternalAuthenticationProviders()
        {
            return ObjectMapper.Map<List<ExternalLoginProviderInfoModel>>(_externalAuthConfiguration.Providers);
        }

        [HttpPost]
        public async Task<ExternalAuthenticateResultModel> ExternalAuthenticate([FromBody] ExternalAuthenticateModel model)
        {
            var externalUser = await GetExternalUserInfo(model);

            var loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    {
                        var accessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity));
                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = accessToken,
                            EncryptedAccessToken = GetEncryptedAccessToken(accessToken),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                case AbpLoginResultType.UnknownExternalLogin:
                    {
                        var newUser = await RegisterExternalUserAsync(externalUser);
                        if (!newUser.IsActive)
                        {
                            return new ExternalAuthenticateResultModel
                            {
                                WaitingForActivation = true
                            };
                        }

                        // Try to login again with newly registered user!
                        loginResult = await _logInManager.LoginAsync(new UserLoginInfo(model.AuthProvider, model.ProviderKey, model.AuthProvider), GetTenancyNameOrNull());
                        if (loginResult.Result != AbpLoginResultType.Success)
                        {
                            throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                                loginResult.Result,
                                model.ProviderKey,
                                GetTenancyNameOrNull()
                            );
                        }

                        return new ExternalAuthenticateResultModel
                        {
                            AccessToken = CreateAccessToken(CreateJwtClaims(loginResult.Identity)),
                            ExpireInSeconds = (int)_configuration.Expiration.TotalSeconds
                        };
                    }
                default:
                    {
                        throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(
                            loginResult.Result,
                            model.ProviderKey,
                            GetTenancyNameOrNull()
                        );
                    }
            }
        }

        private async Task<User> RegisterExternalUserAsync(ExternalAuthUserInfo externalUser)
        {
            var user = await _userRegistrationManager.RegisterAsync(
                externalUser.Name,
                externalUser.Surname,
                externalUser.EmailAddress,
                externalUser.EmailAddress,
                Authorization.Users.User.CreateRandomPassword(),
                true
            );

            user.Logins = new List<UserLogin>
            {
                new UserLogin
                {
                    LoginProvider = externalUser.Provider,
                    ProviderKey = externalUser.ProviderKey,
                    TenantId = user.TenantId
                }
            };

            await CurrentUnitOfWork.SaveChangesAsync();

            return user;
        }

        private async Task<ExternalAuthUserInfo> GetExternalUserInfo(ExternalAuthenticateModel model)
        {
            var userInfo = await _externalAuthManager.GetUserInfo(model.AuthProvider, model.ProviderAccessCode);
            if (userInfo.ProviderKey != model.ProviderKey)
            {
                throw new UserFriendlyException(L("CouldNotValidateExternalUser"));
            }

            return userInfo;
        }

        private string GetTenancyNameOrNull()
        {
            if (!AbpSession.TenantId.HasValue)
            {
                return null;
            }

            return _tenantCache.GetOrNull(AbpSession.TenantId.Value)?.TenancyName;
        }

        private async Task<AbpLoginResult<Tenant, User>> GetLoginResultAsync(string usernameOrEmailAddress, string password, string tenancyName)
        {
            var loginResult = await _logInManager.LoginAsync(usernameOrEmailAddress, password, tenancyName);

            switch (loginResult.Result)
            {
                case AbpLoginResultType.Success:
                    return loginResult;
                default:
                    throw _abpLoginResultTypeHelper.CreateExceptionForFailedLoginAttempt(loginResult.Result, usernameOrEmailAddress, tenancyName);
            }
        }

        private string CreateAccessToken(IEnumerable<Claim> claims, TimeSpan? expiration = null)
        {
            var now = DateTime.UtcNow;

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration.Issuer,
                audience: _configuration.Audience,
                claims: claims,
                notBefore: now,
                expires: now.Add(expiration ?? _configuration.Expiration),
                signingCredentials: _configuration.SigningCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }

        private static List<Claim> CreateJwtClaims(ClaimsIdentity identity)
        {
            var claims = identity.Claims.ToList();
            var nameIdClaim = claims.First(c => c.Type == ClaimTypes.NameIdentifier);

            // Specifically add the jti (random nonce), iat (issued timestamp), and sub (subject/user) claims.
            claims.AddRange(new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, nameIdClaim.Value),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.Now.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            });

            return claims;
        }

        private string GetEncryptedAccessToken(string accessToken)
        {
            return SimpleStringCipher.Instance.Encrypt(accessToken, AppConsts.DefaultPassPhrase);
        }
    }
}
