using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Carly.AddOns.Dto
{
    public class GiftDto
    {
        public int addOnId { get; set; }
        public string addOnName { get; set; }
        public int principalId { get; set; }
        public string principalName { get; set; }
    }
}
