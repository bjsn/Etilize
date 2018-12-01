namespace Etilize.Data
{
    using Models;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.Linq;

    public class VendorDL : BaseDL, IDataService
    {
        public VendorDL(string connectionValue)
        {
            base.ConnectionValue = connectionValue;
        }

        private List<Vendor> Convert(DataTable dataTable)
        {
            List<Vendor> list = new List<Vendor>();
            if (dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    Vendor item = new Vendor {
                        VendorName = (row["VendorName"] != DBNull.Value) ? row["VendorName"].ToString() : string.Empty,
                        VendorID = int.Parse(row["VendorID"].ToString())
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

        public List<Vendor> GetAllVendors()
        {
            List<Vendor> list;
            try
            {
                base.OpenDbConnection();
                DataTable dataTable = new DataTable();
                new OleDbDataAdapter("SELECT VendorID, VendorName FROM VendorID", base.DbConnection).Fill(dataTable);
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

        public Vendor GetVendorsById(int vendorId)
        {
            Vendor vendor;
            try
            {
                base.OpenDbConnection();
                DataTable dataTable = new DataTable();
                new OleDbDataAdapter("SELECT VendorID, VendorName FROM VendorID Where VendorID = " + vendorId + ";", base.DbConnection).Fill(dataTable);
                base.CloseDbConnection();
                vendor = this.Convert(dataTable).FirstOrDefault<Vendor>();
            }
            catch (Exception exception)
            {
                base.CloseDbConnection();
                throw new Exception(exception.Message);
            }
            return vendor;
        }

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Save(List<Vendor> vendors)
        {
            try
            {
                foreach (Vendor vendor in vendors)
                {
                    Vendor vendorsById = this.GetVendorsById(vendor.VendorID);
                    if ((vendorsById == null) && (vendor.VendorID != 0))
                    {
                        base.OpenDbConnection();
                        OleDbCommand command = new OleDbCommand {
                            CommandText = "Insert Into VendorID(VendorName, VendorID) values (@VendorName, @VendorID)",
                            CommandType = CommandType.Text
                        };
                        command.Parameters.AddWithValue("@VendorName", vendor.VendorName.ToString());
                        command.Parameters.AddWithValue("@VendorID", vendor.VendorID);
                        command.Connection = base.DbConnection;
                        command.ExecuteNonQuery();
                        base.CloseDbConnection();
                    }
                }
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
        }
    }
}

