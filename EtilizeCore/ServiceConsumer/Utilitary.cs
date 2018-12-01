using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Etilize.Services
{
    public static class Utilitary
    {
        public static string CleanFileName(string filename)
        {
            return Regex.Replace(filename, "[^a-zA-Z0-9_.]+", "", RegexOptions.Compiled);
        }
    }
}
