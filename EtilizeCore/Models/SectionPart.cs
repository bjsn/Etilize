using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etilize.Models
{
    public class SectionPart
    {
        public int Id { get; set; }
        public string SectionTitle { get; set; }
        public int Partid { get; set; }

        public List<SectionPartDetail> SectionDetails { get; set; }
    }
}
