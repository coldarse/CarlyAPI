using Abp.Application.Services;
using Abp.Domain.Repositories;
using Carly.AddOns.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.AddOns
{
    public class AddOnAppService : CrudAppService<AddOn, AddOnDto>
    {
        public AddOnAppService(IRepository<AddOn, int> repository) : base(repository)
        {
        }



    }
}
