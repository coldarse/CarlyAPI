using Abp.Application.Services;
using Abp.Domain.Repositories;
using Carly.CustomerAddOns;
using Carly.CustomerPrincipals.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.CustomerPrincipals
{
    public class CustomerPrincipalAppService : CrudAppService<CustomerPrincipal, CustomerPrincipalDto>
    {

        private readonly IRepository<CustomerPrincipal> _CustomerPrincipalRepository;
        private readonly IRepository<CustomerAddOn> _CustomerAddOnRepository;
        public CustomerPrincipalAppService(IRepository<CustomerPrincipal, int> repository, IRepository<CustomerPrincipal> CustomerPrincipalRepository, IRepository<CustomerAddOn> CustomerAddOnRepository) : base(repository)
        {
            _CustomerPrincipalRepository = CustomerPrincipalRepository;
            _CustomerAddOnRepository = CustomerAddOnRepository;
        }

        public List<CustomerPrincipal> GetSelectedCustomerPrincipal(int id)
        {
            List<CustomerAddOn> tempAddOn = _CustomerAddOnRepository.GetAll().ToList();
            List<CustomerPrincipal> tempPrincipal = _CustomerPrincipalRepository.GetAll().Where(g => g.PackageId.ToString().Equals(id.ToString())).ToList();

            return tempPrincipal;
        }

        public void DeleteCustomerPrincipal(int id)
        {
            List<CustomerAddOn> tempAddOn = _CustomerAddOnRepository.GetAll().Where(f => f.CustomerPrincipalId.ToString().Equals(id.ToString())).ToList();
        
            foreach(var addon in tempAddOn)
            {
                _CustomerAddOnRepository.Delete(addon.Id);
            }

            _CustomerPrincipalRepository.Delete(id);
        }
    }
}
