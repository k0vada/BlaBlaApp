using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlaBlaApp.Model
{
    public class Court
    {
        public int CourtId { get; set; }
        public string Name { get; set; }
        public string Judge { get; set; }
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    }
}
