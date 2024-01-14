using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlaBlaApp.Model
{
    public class Case
    {
        [Key]
        public string Number { get; set; }
        public string Type { get; set; }
        public string Instance { get; set; }
        public string Subject { get; set; }
        public string Result { get; set; }
        public virtual Court Court { get; set; }
        public virtual ICollection<Article> Articles { get; set; } = new List<Article>();

    }
}
