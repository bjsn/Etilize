using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etilize.Models
{
    public class ProposalContentByPart
    {
        public string PartNumber { get; set; }
        public string VendorName { get; set; }
        public int VendorID { get; set; }
        public DateTime DownloadDT { get; set; }
        public string ProductName { get; set; }
        public string FeatureBullets { get; set; }
        public string MarketingInfo { get; set; }
        public string TechnicalInfo { get; set; }
        public string ProductPicturePath { get; set; }
        public string ProductPictureURL { get; set; }
        public string MfgPartNumber { get; set; }
        public string MfgName { get; set; }
        public string Optional { get; set; } 
        public byte[] Document { get; set; }
        public int MfgID { get; set; }
        public string ProductType { get; set; }

        public bool IsNew { get; set; }
        public bool IsUpdate { get; set; }
    }
}
