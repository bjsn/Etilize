using Etilize.Models;
using ExcelDataReader;
using Microsoft.Office.Interop.Excel;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace EtilizeDocument
{
    public class ExcelManager
    {
        public readonly string[] columnNames = { "PartNumber", "PartDescription", "VendorName", "ProductCat", "Optional", "SDADocName", "EtilizeStatus" };

        public ExcelManager() 
        {}

        public List<string> GetCSVDataByProperty(string csvPath, string property)
        {
            List<string> ListByProperty = new List<string>();
            try
            {
                Hashtable ht = new Hashtable();
                using (TextFieldParser parser = new TextFieldParser(csvPath))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    while (!parser.EndOfData)
                    {
                        try
                        {
                            string[] fields = parser.ReadFields();
                            ht.Add(fields[0], fields[1]);
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message);
                        }

                        foreach (string key in ht.Keys)
                        {
                            Console.WriteLine(String.Format("{0} : {1}", key, ht[key]));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return ListByProperty;
        }

        /// <summary>
        /// Get the information by column from an excel file
        /// </summary>
        /// <param name="csvPath"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        public List<ExcelPartRequest> GetExcelDataByColumnName(string filePath)
        {
            List<ExcelPartRequest> listParts = new List<ExcelPartRequest>();
            try
            {
                Application xlsApp = new Application();
                Workbook wb = xlsApp.Workbooks.Open(filePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                               Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                               Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                               Type.Missing, Type.Missing);
                Sheets sheets = wb.Worksheets;
                Worksheet ws = (Worksheet)sheets.get_Item(1);
                List<int> rowPositions = new List<int>();
                
                int counter = 1;
                foreach (Range column in ws.UsedRange.Columns)
                {
                    var columXlsName = (string)(column.Cells[1, 1] as Range).Value;
                    foreach (var columnRequestName in columnNames)
                    {
                        if (columXlsName.ToString().Equals(columnRequestName))
                        {
                            rowPositions.Add(counter);
                        }
                    }
                    counter++;
                }

                if (rowPositions.Count <= columnNames.Length)
                {
                    foreach (Range row in ws.UsedRange.Rows)
                    {
                        try
                        {
                            string PartNumber = "";
                            string PartDescription = "";
                            string VendorName = "";
                            string ProductCat = "";
                            string Optional = "";
                            string SDADocName = "";
                            string EtilizeStatus = "";

                            /// "PartNumber", "PartDescription", "VendorName", "ProductCat", "Optional", "SDADocName", "EtilizeStatus"
                            try
                            {
                                PartNumber = Convert.ToString((row.Cells[rowPositions[0]] as Range).Value);
                                PartDescription = Convert.ToString((row.Cells[rowPositions[1]] as Range).Value);
                                VendorName = Convert.ToString((row.Cells[rowPositions[2]] as Range).Value);
                                ProductCat = Convert.ToString((row.Cells[rowPositions[3]] as Range).Value);
                                Optional = Convert.ToString((row.Cells[rowPositions[4]] as Range).Value);
                                SDADocName = Convert.ToString((row.Cells[rowPositions[5]] as Range).Value);
                                EtilizeStatus = Convert.ToString((row.Cells[rowPositions[6]] as Range).Value);
                            }
                            catch (Exception e)
                            {
                            }
                            if (!string.IsNullOrEmpty(VendorName) && !string.IsNullOrEmpty(PartNumber)) 
                            {
                                listParts.Add(new ExcelPartRequest
                                {
                                    PartNumber = PartNumber,
                                    PartDescription = PartDescription,
                                    VendorName = VendorName,
                                    ProductCat = ProductCat,
                                    Optional = Optional,
                                    SDADocName = SDADocName,
                                    EtilizeStatus = EtilizeStatus
                                });
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message);
                        }
                    }
                }
                //remove the first one because are the column titles
                listParts.RemoveAt(0);
                wb.Close();
                xlsApp.Quit();
                Marshal.ReleaseComObject(xlsApp);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
            return listParts;
        }

        public List<ExcelPartRequest> GetExcelDataByColumnNameDataReader(string filePath)
        {
            List<ExcelPartRequest> list = new List<ExcelPartRequest>();
            try
            {
                using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream, null))
                    {
                        do
                        {
                            if (reader.Read())
                            {
                                while (reader.Read())
                                {
                                    string SDADocName = Convert.ToString(reader.GetValue(5));
                                    //bussiness exclusion, remove all the SDA documents that finish with (Repeat) 'cause is a repeated doc section
                                    if (!SDADocName.ToUpper().EndsWith("(REPEAT)"))
                                    {
                                        ExcelPartRequest item = new ExcelPartRequest
                                        {
                                            PartNumber = Convert.ToString(reader.GetValue(0)),
                                            PartDescription = Convert.ToString(reader.GetValue(1)),
                                            VendorName = Convert.ToString(reader.GetValue(2)),
                                            ProductCat = Convert.ToString(reader.GetValue(3)),
                                            Optional = Convert.ToString(reader.GetValue(4)),
                                            SDADocName = Convert.ToString(reader.GetValue(5)),
                                            EtilizeStatus = Convert.ToString(reader.GetValue(6))
                                        };
                                        list.Add(item);
                                    }
                                    continue;
                                }
                            }
                        }
                        while (reader.NextResult());
                    }
                }
                //list.RemoveAt(0);
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
            return list;
        }

        //public void UpdateEtilizeStatusExcelDoc(string filePath, List<ExcelPartRequest> listPartProcess)
        //{
        //    try
        //    {
        //        Application o = (Application) Activator.CreateInstance(Marshal.GetTypeFromCLSID(new Guid("00024500-0000-0000-C000-000000000046")));
        //        Workbook workbook = o.Workbooks.Open(filePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing, Type.Missing);
        //        Sheets worksheets = workbook.Worksheets;
        //        Worksheet worksheet = (Worksheet) worksheets.get_Item(1);
        //        List<int> list = new List<int>();
        //        int item = 1;
        //        foreach (Microsoft.Office.Interop.Excel.Range range in worksheet.UsedRange.Columns)
        //        {
        //            string str = (string) (range.Cells[1, 1] as Microsoft.Office.Interop.Excel.Range).get_Value(Missing.Value);
        //            string[] columnNames = this.columnNames;
        //            int index = 0;
        //            while (true)
        //            {
        //                if (index >= columnNames.Length)
        //                {
        //                    item++;
        //                    break;
        //                }
        //                string str2 = columnNames[index];
        //                if (str.ToString().Equals(str2))
        //                {
        //                    list.Add(item);
        //                }
        //                index++;
        //            }
        //        }
        //        if (list.Count <= this.columnNames.Length)
        //        {
        //            foreach (Microsoft.Office.Interop.Excel.Range range2 in worksheet.UsedRange.Rows)
        //            {
        //                try
        //                {
        //                    Func<ExcelPartRequest, bool> predicate = null;
        //                    string PartNumber = "";
        //                    try
        //                    {
        //                        if (<UpdateEtilizeStatusExcelDoc>o__SiteContainer13.<>p__Site17 == null)
        //                        {
        //                            CSharpArgumentInfo[] argumentInfo = new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.IsStaticType | CSharpArgumentInfoFlags.UseCompileTimeType, null), CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) };
        //                            <UpdateEtilizeStatusExcelDoc>o__SiteContainer13.<>p__Site17 = CallSite<Func<CallSite, Type, object, object>>.Create(Microsoft.CSharp.RuntimeBinder.Binder.InvokeMember(CSharpBinderFlags.None, "ToString", null, typeof(ExcelManager), argumentInfo));
        //                        }
        //                        PartNumber = (string) <UpdateEtilizeStatusExcelDoc>o__SiteContainer13.<>p__Site17.Target(<UpdateEtilizeStatusExcelDoc>o__SiteContainer13.<>p__Site17, typeof(Convert), (range2.Cells[list[0], Missing.Value] as Microsoft.Office.Interop.Excel.Range).get_Value(Missing.Value));
        //                        if (predicate == null)
        //                        {
        //                            predicate = x => x.PartNumber.Equals(PartNumber);
        //                        }
        //                        ExcelPartRequest request = (from x in listPartProcess.Where<ExcelPartRequest>(predicate) select x).FirstOrDefault<ExcelPartRequest>();
        //                        if ((request != null) && request.Found)
        //                        {
        //                            (range2.Cells[list[6], Missing.Value] as Microsoft.Office.Interop.Excel.Range).set_Value(Missing.Value, "Etilize");
        //                        }
        //                    }
        //                    catch (Exception)
        //                    {
        //                    }
        //                }
        //                catch (Exception exception1)
        //                {
        //                    throw new Exception(exception1.Message);
        //                }
        //            }
        //        }
        //        o.DisplayAlerts = false;
        //        workbook.SaveAs(filePath, XlFileFormat.xlOpenXMLWorkbook, Type.Missing, Type.Missing, false, false, XlSaveAsAccessMode.xlNoChange, XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing, Missing.Value, Missing.Value);
        //        workbook.Close(false, Missing.Value, Missing.Value);
        //        o.Quit();
        //        Marshal.ReleaseComObject(o);
        //    }
        //    catch (Exception exception3)
        //    {
        //        throw new Exception(exception3.Message);
        //    }
        //}

        public void UpdateEtilizeStatusExcelDoc(string filePath, List<ExcelPartRequest> listPartProcess) 
        {
            try
            {
                Application xlsApp = new Application();
                Workbook wb = xlsApp.Workbooks.Open(filePath, Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                               Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                               Type.Missing, Type.Missing, Type.Missing, Type.Missing,
                                                               Type.Missing, Type.Missing);

                Sheets sheets = wb.Worksheets;
                Worksheet ws = (Worksheet)sheets.get_Item(1);
                List<int> rowPositions = new List<int>();

                int counter = 1;
                foreach (Range column in ws.UsedRange.Columns)
                {
                    var columXlsName = (string)(column.Cells[1, 1] as Range).Value;
                    foreach (var columnRequestName in columnNames)
                    {
                        if (columXlsName.ToString().Equals(columnRequestName))
                        {
                            rowPositions.Add(counter);
                        }
                    }
                    counter++;
                }

                if (rowPositions.Count <= columnNames.Length)
                {
                    foreach (Range row in ws.UsedRange.Rows)
                    {
                        try
                        {
                            string PartNumber = "";
                            try
                            {
                                PartNumber = Convert.ToString((row.Cells[rowPositions[0]] as Range).Value);
                                var part = listPartProcess.Where(x => x.PartNumber.Equals(PartNumber)).Select(x => x).FirstOrDefault();
                                if (part != null) 
                                {
                                    bool foundInEtilize = part.Found;
                                    if (foundInEtilize)
                                    {
                                        (row.Cells[rowPositions[6]] as Range).Value = "Etilize";
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                            }
                        }
                        catch (Exception e)
                        {
                            throw new Exception(e.Message);
                        }
                    }
                }

                xlsApp.DisplayAlerts = false;
                wb.SaveAs(filePath, Microsoft.Office.Interop.Excel.XlFileFormat.xlWorkbookDefault, Type.Missing, Type.Missing, false, false, XlSaveAsAccessMode.xlNoChange, XlSaveConflictResolution.xlLocalSessionChanges, Type.Missing, Type.Missing); 
                wb.Close(false);
                xlsApp.Quit();
                Marshal.ReleaseComObject(xlsApp);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

    }
}
