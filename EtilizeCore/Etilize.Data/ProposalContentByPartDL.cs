namespace Etilize.Data
{
    using Etilize.Models;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.OleDb;
    using System.Globalization;

    public class ProposalContentByPartDL : BaseDL, IDataService
    {
        public ProposalContentByPartDL(string connectionValue)
        {
            base.ConnectionValue = connectionValue;
        }

        public List<ProposalContentByPart> GetByPartNumber(string partNumbers)
        {
            List<ProposalContentByPart> list;
            try
            {
                base.OpenDbConnection();
                DataTable dataTable = new DataTable();
                new OleDbDataAdapter("SELECT PartNumber, VendorName, DownloadDT, ProductName, FeatureBullets, MarketingInfo, TechnicalInfo, ProductPicturePath, ProductPictureURL, MfgPartNumber, MfgName, MfgID, ProductType, MfgID, ProductType "
                                    +"FROM ProposalContentByPart " 
                                    +"WHERE PartNumber IN(" + partNumbers + ");", base.DbConnection)
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

        //call just if it is using the PQDB database
        public List<ProposalContentByPart> GetByPartNumberAdminDB(string partNumbers)
        {
            List<ProposalContentByPart> list;
            try
            {
                base.OpenDbConnection();
                DataTable dataTable = new DataTable();
                new OleDbDataAdapter("SELECT PartNumber, VendorName, ProductName, FeatureBullets, MarketingInfo, TechnicalInfo, ProductPicture "
                                    + "FROM ProposalContentByPart "
                                    + "WHERE PartNumber = '" + partNumbers + "';", 
                                    base.DbConnection)
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

        private void Save(ProposalContentByPart proposalContentByPart)
        {
            try
            {
                OleDbCommand command = new OleDbCommand {
                    CommandText = "INSERT INTO ProposalContentByPart(PartNumber, VendorName, DownloadDT, ProductName, FeatureBullets, MarketingInfo, TechnicalInfo, ProductPicturePath, ProductPictureURL, MfgPartNumber, MfgName, MfgID, ProductType) "
                                + "VALUES (@PartNumber, @VendorName, @DownloadDT, @ProductName, @FeatureBullets, @MarketingInfo, @TechnicalInfo, @ProductPicturePath, @ProductPictureURL, @MfgPartNumber, @MfgName, @MfgID, @ProductType)",
                    CommandType = CommandType.Text
                };
                command.Parameters.AddWithValue("@PartNumber", proposalContentByPart.PartNumber.ToString());
                command.Parameters.AddWithValue("@VendorName", proposalContentByPart.VendorName.ToString());
                command.Parameters.AddWithValue("@DownloadDT", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@ProductName", string.IsNullOrEmpty(proposalContentByPart.ProductName) ? "" : proposalContentByPart.ProductName);
                command.Parameters.AddWithValue("@FeatureBullets", string.IsNullOrEmpty(proposalContentByPart.FeatureBullets) ? "" : proposalContentByPart.FeatureBullets);
                command.Parameters.AddWithValue("@MarketingInfo", string.IsNullOrEmpty(proposalContentByPart.MarketingInfo) ? "" : proposalContentByPart.MarketingInfo);
                command.Parameters.AddWithValue("@TechnicalInfo", string.IsNullOrEmpty(proposalContentByPart.TechnicalInfo) ? "" : proposalContentByPart.TechnicalInfo);
                command.Parameters.AddWithValue("@ProductPicturePath", string.IsNullOrEmpty(proposalContentByPart.ProductPicturePath) ? "" : proposalContentByPart.ProductPicturePath);
                command.Parameters.AddWithValue("@ProductPictureURL", string.IsNullOrEmpty(proposalContentByPart.ProductPictureURL) ? "" : proposalContentByPart.ProductPictureURL);
                command.Parameters.AddWithValue("@MfgPartNumber", string.IsNullOrEmpty(proposalContentByPart.MfgPartNumber) ? "" : proposalContentByPart.MfgPartNumber);
                command.Parameters.AddWithValue("@MfgName", string.IsNullOrEmpty(proposalContentByPart.MfgName) ? "" : proposalContentByPart.MfgName);
                command.Parameters.AddWithValue("@MfgID", proposalContentByPart.MfgID);
                command.Parameters.AddWithValue("@ProductType", string.IsNullOrEmpty(proposalContentByPart.ProductType) ? "" : proposalContentByPart.ProductType);
                command.Connection = base.DbConnection;
                command.ExecuteNonQuery();
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
        }

        public void Save(List<ProposalContentByPart> proposalContentByPartList)
        {
            try
            {
                base.OpenDbConnection();
                foreach (ProposalContentByPart part in proposalContentByPartList)
                {
                    if (part.IsNew)
                    {
                        this.Save(part);
                        continue;
                    }
                    if (part.IsUpdate)
                    {
                        this.Save(part);
                    }
                }
                base.CloseDbConnection();
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
        }

        private void Update(ProposalContentByPart proposalContentByPart)
        {
            try
            {
                OleDbCommand command = new OleDbCommand {
                    CommandText = "UPDATE ProposalContentByPart "
                                +" SET DownloadDT = @DownloadDT, ProductName = @ProductName, FeatureBullets = @FeatureBullets, MarketingInfo = @MarketingInfo, " 
                                +"TechnicalInfo = @TechnicalInfo, ProductPicturePath = @ProductPicturePath, ProductPictureURL = @ProductPictureURL, MfgPartNumber = @MfgPartNumber, " 
                                +"MfgName = @MfgName Where PartNumber = '" + proposalContentByPart.PartNumber + "' "
                                +"MfgID = @MfgID, "
                                + "ProductType = @ProductType",
                    CommandType = CommandType.Text
                };
                command.Parameters.AddWithValue("@DownloadDT", DateTime.Now.ToString(CultureInfo.InvariantCulture));
                command.Parameters.AddWithValue("@ProductName", proposalContentByPart.ProductName.ToString());
                command.Parameters.AddWithValue("@FeatureBullets", string.IsNullOrEmpty(proposalContentByPart.FeatureBullets) ? "" : proposalContentByPart.FeatureBullets);
                command.Parameters.AddWithValue("@MarketingInfo", string.IsNullOrEmpty(proposalContentByPart.MarketingInfo) ? "" : proposalContentByPart.MarketingInfo);
                command.Parameters.AddWithValue("@TechnicalInfo", string.IsNullOrEmpty(proposalContentByPart.TechnicalInfo) ? "" : proposalContentByPart.TechnicalInfo);
                command.Parameters.AddWithValue("@ProductPicturePath", string.IsNullOrEmpty(proposalContentByPart.ProductPicturePath) ? "" : proposalContentByPart.ProductPicturePath);
                command.Parameters.AddWithValue("@ProductPictureURL", string.IsNullOrEmpty(proposalContentByPart.ProductPictureURL) ? "" : proposalContentByPart.ProductPictureURL);
                command.Parameters.AddWithValue("@MfgPartNumber", proposalContentByPart.MfgPartNumber.ToString());
                command.Parameters.AddWithValue("@MfgName", proposalContentByPart.MfgName.ToString());
                command.Parameters.AddWithValue("@MfgID", proposalContentByPart.MfgID);
                command.Parameters.AddWithValue("@ProductType", string.IsNullOrEmpty(proposalContentByPart.ProductType) ? "" : proposalContentByPart.ProductType);
                command.Connection = base.DbConnection;
                command.ExecuteNonQuery();
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
        }

        private List<ProposalContentByPart> Convert(DataTable dataTable)
        {
            List<ProposalContentByPart> list = new List<ProposalContentByPart>();
            if (dataTable.Rows.Count > 0)
            {
                foreach (DataRow row in dataTable.Rows)
                {
                    ProposalContentByPart item = new ProposalContentByPart
                    {
                        PartNumber = (row["PartNumber"] != DBNull.Value) ? row["PartNumber"].ToString() : string.Empty,
                        VendorName = (row["VendorName"] != DBNull.Value) ? row["VendorName"].ToString() : string.Empty,
                        ProductName = (row["ProductName"] != DBNull.Value) ? row["ProductName"].ToString() : string.Empty,
                        MarketingInfo = (row["MarketingInfo"] != DBNull.Value) ? row["MarketingInfo"].ToString() : string.Empty,
                        FeatureBullets = (row["FeatureBullets"] != DBNull.Value) ? row["FeatureBullets"].ToString() : string.Empty,
                        TechnicalInfo = (row["TechnicalInfo"] != DBNull.Value) ? row["TechnicalInfo"].ToString() : string.Empty,

                        DownloadDT = row.Table.Columns.Contains("DownloadDT") ? ((row["DownloadDT"] != DBNull.Value) ? DateTime.Parse(row["DownloadDT"].ToString()) : DateTime.MinValue) : DateTime.MinValue,
                        ProductPicturePath = row.Table.Columns.Contains("ProductPicturePath") ? ((row["ProductPicturePath"] != DBNull.Value) ? row["ProductPicturePath"].ToString() : string.Empty) : string.Empty,
                        ProductPictureURL = row.Table.Columns.Contains("ProductPictureURL") ? ((row["ProductPictureURL"] != DBNull.Value) ? row["ProductPictureURL"].ToString() : string.Empty) : string.Empty,
                        MfgPartNumber = row.Table.Columns.Contains("MfgPartNumber") ? ((row["MfgPartNumber"] != DBNull.Value) ? row["MfgPartNumber"].ToString() : string.Empty) : string.Empty,
                        MfgName = row.Table.Columns.Contains("MfgName") ? ((row["MfgName"] != DBNull.Value) ? row["MfgName"].ToString() : string.Empty) : string.Empty,
                        MfgID = row.Table.Columns.Contains("MfgID") ? ((row["MfgID"] != DBNull.Value) ? Int32.Parse(row["MfgID"].ToString()) : 0) : 0,
                        ProductType = row.Table.Columns.Contains("ProductType") ? ((row["ProductType"] != DBNull.Value) ? row["ProductType"].ToString() : string.Empty) : string.Empty,
                        Document = row.Table.Columns.Contains("ProductPicture") ? ((row["ProductPicture"] != DBNull.Value) ? ((byte[])row["ProductPicture"]) : null) : null
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

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}

