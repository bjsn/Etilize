using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etilize.Models
{
    public class EtilizeDocumentConfiguration
    {
        public bool ExcludeIfNoPic { get; set; }
        public bool Picture { get; set; }
        public bool Benefits { get; set; }
        public bool MarketingInfo { get; set; }
        public bool TechInfo { get; set; }
        
    }
}
