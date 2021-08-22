using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.LogoLinks
{
    public class LogoLink : Entity<int>
    {
        public string Link { get; set; }
    }
}
