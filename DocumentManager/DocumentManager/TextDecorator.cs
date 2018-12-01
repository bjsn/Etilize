using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Word;


namespace DocumentManager
{
    public class TextDecorator : Decorator
    {


        public TextDecorator(WordDocument baseComponent) : base(baseComponent)
        {}

        public override void Default()
        {
            base.Default();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }




    }
}
