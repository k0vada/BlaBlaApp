using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlaBlaApp.Model
{
    public class Article
    {
        public int ArticleId { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Case> Cases { get; set; } = new List<Case>();
    }
}
