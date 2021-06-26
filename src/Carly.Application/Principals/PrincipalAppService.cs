using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Carly.AddOns;
using Carly.Authorization;
using Carly.Principals.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.Principals
{
    [AbpAuthorize(PermissionNames.Pages_Principals)]
    public class PrincipalAppService : CrudAppService<Principal, PrincipalDto>
    {

        private readonly IRepository<AddOn> _AddOnRepository;
        private readonly IRepository<Principal> _PrincipalRepository;
        public PrincipalAppService(IRepository<Principal, int> repository, IRepository<AddOn> AddOnRepository, IRepository<Principal> PrincipalRepository) : base(repository)
        {
            _AddOnRepository = AddOnRepository;
            _PrincipalRepository = PrincipalRepository;
        }

        public void DeletePrincipal(int id)
        {
            List<AddOn> tempAddOn = _AddOnRepository.GetAll().Where(f => f.PrincipalId.ToString().Equals(id.ToString())).ToList();
            
            foreach(var addon in tempAddOn)
            {
                _AddOnRepository.Delete(addon.Id);
            }

            _PrincipalRepository.Delete(id);
        }

        public Principal GetPrincipal(int id)
        {
            List<AddOn> tempAddOn = _AddOnRepository.GetAll().Where(f => f.PrincipalId.ToString().Equals(id.ToString())).ToList();
            Principal tempPrincipal = _PrincipalRepository.GetAll().Where(g => g.Id.ToString().Equals(id.ToString())).FirstOrDefault();

            return tempPrincipal;
        }

        public List<Principal> GetAllPrincipal()
        {
            List<AddOn> tempAddOn = _AddOnRepository.GetAll().ToList();
            List<Principal> tempPrincipal = _PrincipalRepository.GetAll().ToList();

            return tempPrincipal;
        }


        public async Task<bool> UpdatePrincipal(int id, PrincipalDto principal)
        {
            try
            {
                List<AddOn> tempAddOn = _AddOnRepository.GetAll().Where(f => f.PrincipalId.ToString().Equals(id.ToString())).ToList();
                
                Principal tempPrincipal = _PrincipalRepository.GetAll().Where(g => g.Id.ToString().Equals(id.ToString())).FirstOrDefault();


                for (int x = 0; x < tempPrincipal.AddOns.Count; x++)
                {
                    if (principal.AddOns[x].Id.Equals(tempPrincipal.AddOns[x].Id))
                    {
                        principal.AddOns[x].addonname = tempPrincipal.AddOns[x].addonname;
                    }
                }

                int y = principal.AddOns.Count - tempPrincipal.AddOns.Count;

                for(int z = tempPrincipal.AddOns.Count; z < y + 1; z++)
                {
                    tempPrincipal.AddOns.Add(principal.AddOns[z]);
                }

                await _PrincipalRepository.UpdateAsync(tempPrincipal);
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
            
        }

    }
}
