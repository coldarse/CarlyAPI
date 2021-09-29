using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Linq.Extensions;
using Carly.iPay88s.Dto;

namespace Carly.iPay88s
{
    public class iPay88AppService : CrudAppService<iPay88, iPay88Dto>
    {
        public iPay88AppService(IRepository<iPay88, int> repository) : base(repository)
        {
        }
    }
}
