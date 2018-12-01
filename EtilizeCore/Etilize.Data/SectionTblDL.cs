namespace Etilize.Data
{
    using System;
    using System.Data;
    using System.Data.OleDb;

    public class SectionTblDL : BaseDL, IDataService
    {
        public SectionTblDL(string connectionValue)
        {
            base.ConnectionValue = connectionValue;
        }

        private byte[] Convert(DataTable dataTable)
        {
            byte[] buffer;
            try
            {
                buffer = (dataTable.Rows.Count <= 0) ? null : ((byte[]) dataTable.Rows[0][0]);
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
            return buffer;
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

        public byte[] GetWordDocBySectionName(string sectionName)
        {
            byte[] buffer;
            try
            {
                base.OpenDbConnectionWithSecurity();
                DataTable dataTable = new DataTable();
                new OleDbDataAdapter("SELECT Word_Doc FROM Section_tbl WHERE Section_Name = '" + sectionName + "';", base.DbConnection).Fill(dataTable);
                base.CloseDbConnection();
                buffer = this.Convert(dataTable);
            }
            catch (Exception exception1)
            {
                throw new Exception(exception1.Message);
            }
            return buffer;
        }

        public void Save()
        {
            throw new NotImplementedException();
        }
    }
}

