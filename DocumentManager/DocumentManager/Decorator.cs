using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentManager
{
    public abstract class Decorator : WordDocument
    {
        protected WordDocument m_baseComponent = null;

        /// <summary>
        /// Constructor
        /// </summary>
        public Decorator(WordDocument baseComponent)
        {
            m_baseComponent = baseComponent;
        }

        //override
        public override void Default()
        {}

    }
}
