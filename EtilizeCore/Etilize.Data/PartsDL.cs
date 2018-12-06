namespace Etilize.Data
{
    using Etilize.Models;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.Globalization;

    public class PartsDL : BaseDL, IDataService
    {
        public PartsDL(string connectionValue)
        {
            base.ConnectionValue = connectionValue;
        }

        private List<ComponentPart> Convert(DataTable dataTable)
        {
            List<ComponentPart> list = new List<ComponentPart>();
            if (dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    ComponentPart item = new ComponentPart {
                        PartNumber = (row["PartNumber"] != DBNull.Value) ? row["PartNumber"].ToString() : string.Empty,
                        Vendor = (row["PartNumber"] != DBNull.Value) ? row["PartNumber"].ToString() : string.Empty,
                        ProductPicture = (row["PartNumber"] != DBNull.Value) ? row["PartNumber"].ToString() : string.Empty,
                        DownloadDT = DateTime.Parse(row["DownloadDT"].ToString())
                    };
                    list.Add(item);
                }
            }
            return list;
        }

        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public void Edit(int id)
        {
            throw new NotImplementedException();
        }

        public void Get(int id)
        {
            throw new NotImplementedException();
        }

        public List<ComponentPart> GetComponentsByPartNumber(string partNumbers, string daysToUpdate)
        {
            List<ComponentPart> list;
            try
            {
                base.OpenDbConnection();
                DataTable dataTable = new DataTable();
                new OleDbDataAdapter("SELECT PartNumber, Vendor, ProductPicture, DownloadDT FROM Parts WHERE PartNumber IN(" + partNumbers + ") AND DateDiff('d', DownloadDT,  DATE()) <= " + daysToUpdate, base.DbConnection).Fill(dataTable);
                base.CloseDbConnection();
                list = this.Convert(dataTable);
            }
            catch (Exception exception)
            {
                base.CloseDbConnection();
                throw new Exception(exception.Message);
            }
            return list;
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public int Save(ComponentPart component)
        {
            base.OpenDbConnection();
            OleDbCommand command3 = new OleDbCommand();
            string[] strArray = new string[] { "Insert into Parts(PartNumber, Vendor, ProductPicture, DownloadDT) values ('", component.PartNumber, "','", component.Vendor, "','", component.ProductPicture, "', @DownloadDT)" };
            command3.CommandText = string.Concat(strArray);
            command3.CommandType = CommandType.Text;
            OleDbCommand command = command3;
            command.Parameters.AddWithValue("@DownloadDT", DateTime.Now.ToString(CultureInfo.InvariantCulture));
            command.Connection = base.DbConnection;
            command.ExecuteNonQuery();
            using (OleDbCommand command2 = new OleDbCommand("SELECT @@IDENTITY;", base.DbConnection))
            {
                return (int) command2.ExecuteScalar();
            }
        }
    }
}

