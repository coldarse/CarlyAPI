using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Carly.Authorization;
using Carly.CustomerAddOns;
using Carly.CustomerPrincipals;
using Carly.EncryptKeys;
using Carly.Packages.Dto;
using Carly.Users.Dto;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Packages
{
    [AbpAuthorize(PermissionNames.Pages_Packages)]
    public class PackageAppService : CrudAppService<Package, PackageDto, int, PagedPackageResultRequestDto>
    {
        private readonly IRepository<Package> _PackageRepository;
        private readonly IRepository<CustomerPrincipal> _CustomerPrincipalRepository;
        private readonly IRepository<CustomerAddOn> _CustomerAddOnRepository;
        public PackageAppService(
            IRepository<Package, int> repository, 
            IRepository<Package> PackageRepository, 
            IRepository<CustomerPrincipal> CustomerPackageRepository, 
            IRepository<CustomerAddOn> CustomerAddOnRepository) : base(repository)
        {
            _PackageRepository = PackageRepository;
            _CustomerPrincipalRepository = CustomerPackageRepository;
            _CustomerAddOnRepository = CustomerAddOnRepository;
        }

        public List<Package> GetAllPackage()
        {
            List<CustomerPrincipal> tempPrincipal = _CustomerPrincipalRepository.GetAll().ToList();
            List<Package> tempPackage = _PackageRepository.GetAll().ToList();

            return tempPackage;
        }

        public Package GetPackageById(int id)
        {
            List<CustomerPrincipal> tempPrincipal = _CustomerPrincipalRepository.GetAll().Where(f => f.PackageId.ToString().Equals(id.ToString())).ToList();

            //int principalId = tempPrincipal[0].Id;

            List<CustomerAddOn>[] a = new List<CustomerAddOn>[3];
            int x = 0;
            foreach (var prin in tempPrincipal)
            {
                a[x] = _CustomerAddOnRepository.GetAll().Where(f => f.CustomerPrincipalId.ToString().Equals(prin.Id.ToString())).ToList();
                x += 1;
            }
            //List<CustomerAddOn> tempAddOn = _CustomerAddOnRepository.GetAll().Where(f => f.CustomerPrincipalId.ToString().Equals(principalId.ToString())).ToList();

            Package tempPackage = _PackageRepository.FirstOrDefault(f => f.Id.ToString().Equals(id.ToString()));

            return tempPackage;
        }

        public void DeletePackage(int id)
        {

            List<CustomerPrincipal> tempPrincipal = _CustomerPrincipalRepository.GetAll().Where(f => f.PackageId.ToString().Equals(id.ToString())).ToList();

            foreach(var principal in tempPrincipal)
            {
                List<CustomerAddOn> tempAddOn = _CustomerAddOnRepository.GetAll().Where(f => f.CustomerPrincipalId.ToString().Equals(principal.Id.ToString())).ToList();

                foreach (var addon in tempAddOn)
                {
                    _CustomerAddOnRepository.Delete(addon.Id);
                }

                _CustomerPrincipalRepository.Delete(principal.Id);
            }

            _PackageRepository.Delete(id);

        }

        public string GenerateLink(int id)
        {
            string stringid = id.ToString();
            string encryptedID = EncryptKey.Encrypt(stringid);
            string generatedLink = "https://system.carly.com.my/CarlyApp/?id=" + encryptedID;

            return generatedLink;
        }

        public EmailContentDto getEmailItems(int id)
        {
            Package tempPackage = _PackageRepository.FirstOrDefault(x => x.Id.ToString().Equals(id.ToString()));
            List<CustomerAddOn> tempCustAddOns = _CustomerAddOnRepository.GetAll().ToList();
            List<CustomerPrincipal> tempCustPrincipal = _CustomerPrincipalRepository.GetAll().Where(y => y.PackageId.ToString().Equals(tempPackage.Id.ToString())).ToList();

            string tempPrinName = tempCustPrincipal.First().Name;
            float tempPrinPrice = tempCustPrincipal.First().Premium;
            int tempAddonCount = tempCustPrincipal.First().AddOns.Count;


            for (int i = 1; i < tempCustPrincipal.Count; i++)
            {
                if(tempPrinPrice > tempCustPrincipal[i].Premium)
                {
                    tempPrinPrice = tempCustPrincipal[i].Premium;
                    tempPrinName = tempCustPrincipal[i].Name;
                    tempAddonCount = tempCustPrincipal[i].AddOns.Count;
                }
            }

            EmailContentDto tempContent = new EmailContentDto();
            tempContent.Subject = "Carly Insurance Quotation (" + tempPackage.VehicleRegNo.ToUpper() + ")";
            tempContent.emailTo = tempPackage.OwnerEmail;
            tempContent.VehicleOwnerName = tempPackage.OwnerName;
            tempContent.VehicleRegistrationNumber = tempPackage.VehicleRegNo;
            tempContent.CoveragePeriod = tempPackage.CoveragePeriod;
            tempContent.AddOns = "UNAVAILABLE";
            if (tempAddonCount > 0)
            {
                tempContent.AddOns = "AVAILABLE";
            }
            tempContent.ImageLink = "https://www.carly.com.my/wp-content/uploads/2021/06/allianz-logo-png-transparent.png";

            float fpremium = (float)Math.Round(tempPrinPrice * 100f) / 100f;
            tempContent.Price = "RM " + fpremium.ToString();

            string stringid = id.ToString();
            string encryptedID = EncryptKey.Encrypt(stringid);
            tempContent.ViewQuoteLink = "https://system.carly.com.my/CarlyApp/?id=" + encryptedID;

            return tempContent;

        }

        protected override IQueryable<Package> CreateFilteredQuery(PagedPackageResultRequestDto input)
        {
            return Repository.GetAllIncluding()
                .WhereIf(!input.Keyword.IsNullOrWhiteSpace(), x => x.VehicleRegNo.Contains(input.Keyword));

        }
    }
}
