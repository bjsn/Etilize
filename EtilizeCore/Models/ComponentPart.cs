using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ComponentPart
    {
        public int Id { get; set; }
        public string PartNumber { get; set; }
        public string Vendor { get; set; }
        public string ProductPicture { get; set; }
        public DateTime DownloadDT { get; set; }
        public List<SectionPart> SectionParts { get; set; }
    }
}
