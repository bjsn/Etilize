using Etilize.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etilize.Data
{
    public class TempTblDL : BaseDL
    {
        public TempTblDL(string connectionValue)
        {
            base.ConnectionValue = connectionValue;
        }

        public List<TempTbl> GetByName(string templateName)
        {
            List<TempTbl> list;
            try
            {
                base.OpenDbConnection();
                DataTable dataTable = new DataTable();
                new OleDbDataAdapter("SELECT Template_Name, Word_Doc, RecSource, RecSourceUpdatedDate, FileExt "
                                    + "FROM Temp_tbl "
                                    + "WHERE Template_Name = '" + templateName + "';", base.DbConnection)
                                    .Fill(dataTable);
                base.CloseDbConnection();
                list = this.Convert(dataTable);
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
            return list;
        }

        private List<TempTbl> Convert(DataTable dataTable)
        {
            List<TempTbl> list = new List<TempTbl>();
            if (dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    TempTbl item = new TempTbl
                    {
                        Template_Name = (row["Template_Name"] != DBNull.Value) ? row["Template_Name"].ToString() : string.Empty,
                        Word_Doc = row.Table.Columns.Contains("Word_Doc") ? ((row["Word_Doc"] != DBNull.Value) ? ((byte[])row["Word_Doc"]) : null) : null,
                        RecSource = (row["RecSource"] != DBNull.Value) ? row["RecSource"].ToString() : string.Empty,
                        RecSourceUpdatedDate = row.Table.Columns.Contains("RecSourceUpdatedDate") ? ((row["RecSourceUpdatedDate"] != DBNull.Value) ? DateTime.Parse(row["RecSourceUpdatedDate"].ToString()) : DateTime.MinValue) : DateTime.MinValue,
                        FileExt = (row["FileExt"] != DBNull.Value) ? row["FileExt"].ToString() : string.Empty
                    };
                    list.Add(item);
                }
            }
            return list;
        }

    }
}
