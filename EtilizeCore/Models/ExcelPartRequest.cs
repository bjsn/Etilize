using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class ExcelPartRequest
    {
        public string PartNumber { get; set; }
        public string PartDescription { get; set; }
        public string VendorName { get; set; }
        public int VendorId { get; set; }
        public string ProductCat { get; set; }
        public string Optional { get; set; }
        public string SDADocName { get; set; }
        public string EtilizeStatus { get; set; }
        public byte[] Word_Doc { get; set; }
        public bool Found { get; set; }
    }
}
