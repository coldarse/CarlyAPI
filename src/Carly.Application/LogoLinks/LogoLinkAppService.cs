using Abp.Application.Services;
using Abp.Domain.Repositories;
using Carly.LogoLinks.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.LogoLinks
{
    public class LogoLinkAppService : CrudAppService<LogoLink, LogoLinkDto>
    {
        public LogoLinkAppService(IRepository<LogoLink, int> repository) : base(repository)
        {
        }

        public List<LogoLink> GetAllLogoLink()
        {
            List<LogoLink> tempLogo = Repository.GetAll().ToList();
            return tempLogo;
        }
    }
}
