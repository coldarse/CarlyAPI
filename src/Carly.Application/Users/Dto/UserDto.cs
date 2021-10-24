using System;
using System.ComponentModel.DataAnnotations;
using Abp.Application.Services.Dto;
using Abp.Authorization.Users;
using Abp.AutoMapper;
using Carly.Authorization.Users;

namespace Carly.Users.Dto
{
    [AutoMapFrom(typeof(User))]
    public class UserDto : EntityDto<long>
    {
        [Required]
        [StringLength(AbpUserBase.MaxUserNameLength)]
        public string UserName { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxNameLength)]
        public string Name { get; set; }

        [Required]
        [StringLength(AbpUserBase.MaxSurnameLength)]
        public string Surname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(AbpUserBase.MaxEmailAddressLength)]
        public string EmailAddress { get; set; }

        public bool IsActive { get; set; }

        public string FullName { get; set; }

        public DateTime? LastLoginTime { get; set; }

        public DateTime CreationTime { get; set; }

        public string[] RoleNames { get; set; }

    }

    public class EmailContentDto
    {
        public string Subject { get; set; }
        public string emailTo { get; set; }
        public string VehicleOwnerName { get; set; }
        public string VehicleRegistrationNumber { get; set; }
        public string CoveragePeriod { get; set; }
        public string AddOns { get; set; }
        public string ImageLink { get; set; }       
        public string Price { get; set; }
        public string ViewQuoteLink { get; set; }

    }

    public class SalesReceiptContentDto
    {
        public string Subject { get; set; }
        public string emailTo { get; set; }
        public string VehicleOwnerName { get; set; }
        public string VehicleICNumber { get; set; }
        public string VehicleOwnerAddress { get; set; }
        public string VehicleRegistrationNumber { get; set; }
        public string Insurer { get; set; }
        public string InvoiceNo { get; set; }
        public string CoveragePeriod { get; set; }
        public string TypeOfCover { get; set; }
        public string SumInsured { get; set; }
        public string TransactionDate { get; set; }
        public string CardHolderName { get; set; }
        public string AuthCode { get; set; }
        public string PaymentMethod { get; set; }
        public string BasicPremium { get; set; }
        public string loading1 { get; set; }
        public string loading2 { get; set; }
        public string NCD { get; set; }
        public string AddOns1 { get; set; }
        public string AddOns1Price { get; set; }      
        public string AddOns2 { get; set; }
        public string AddOns2Price { get; set; }
        public string AddOns3 { get; set; }
        public string AddOns3Price { get; set; }
        public string AddOns4 { get; set; }
        public string AddOns4Price { get; set; }
        public string AddOns5 { get; set; }
        public string AddOns5Price { get; set; }
        public string AddOns6 { get; set; }
        public string AddOns6Price { get; set; }
        public string AddOns7 { get; set; }
        public string AddOns7Price { get; set; }
        public string AddOns8 { get; set; }
        public string AddOns8Price { get; set; }
        public string AddOns9 { get; set; }
        public string AddOns9Price { get; set; }
        public string AddOns10 { get; set; }
        public string AddOns10Price { get; set; }
        public string AddOns11 { get; set; }
        public string AddOns11Price { get; set; }
        public string AddOns12 { get; set; }
        public string AddOns12Price { get; set; }
        public string AddOns13 { get; set; }
        public string AddOns13Price { get; set; }
        public string AddOns14 { get; set; }
        public string AddOns14Price { get; set; }
        public string AddOns15 { get; set; }
        public string AddOns15Price { get; set; }
        public string GrossPremium { get; set; }
        public string ServiceTax { get; set; }
        public string StampDuty { get; set; }
        public string AdminFee { get; set; }
        public string MyegDelivery { get; set; }
        public string RoadTaxRenewal { get; set; }
        public string TotalPayablePremium { get; set; }
        public string LogoImg { get; set; }
    }
}
