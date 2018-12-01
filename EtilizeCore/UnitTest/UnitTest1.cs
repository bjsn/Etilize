using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using EtilizeDocument;

namespace UnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestCSV()
        {
            ExcelManager manager = new ExcelManager();
            manager.GetCSVDataByProperty(@"C:\CorsPro\PQuote\Assemblies\Cabling parts.xls", "Part Number");
        }

        [TestMethod]
        public void TestExcel()
        {
            ExcelManager manager = new ExcelManager();
            string[] parameters = {"Part Number", "Vendor"};
            manager.GetExcelDataByColumnName(@"C:\CorsPro\PQuote\Assemblies\Cabling parts.xls");
        }

    }
}
