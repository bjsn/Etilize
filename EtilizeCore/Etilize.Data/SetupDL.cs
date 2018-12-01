namespace Etilize.Data
{
    using System;
    using System.Data;
    using System.Data.OleDb;

    public class SetupDL : BaseDL
    {
        public SetupDL(string connectionValue, string password)
        {
            base.ConnectionValue = connectionValue;
            base.DbPwd = password;
        }

        public DataTable GetSetupList()
        {
            base.OpenDbConnectionWithSecurity();
            DataTable dataTable = new DataTable();
            new OleDbDataAdapter("SELECT * from Setup", base.DbConnection).Fill(dataTable);
            base.CloseDbConnection();
            return dataTable;
        }

        public int ResetServiceUpdProcessDT()
        {
            OleDbCommand command = new OleDbCommand {
                CommandText = "Update Setup set ServiceUpdProcessDT = null",
                CommandType = CommandType.Text
            };
            base.OpenDbConnectionWithSecurity();
            command.Connection = base.DbConnection;
            int num = command.ExecuteNonQuery();
            base.CloseDbConnection();
            return num;
        }

        public int SetCRMXRefUpdStatus(string active)
        {
            OleDbCommand command = new OleDbCommand {
                CommandText = "Update Setup set CRMXRefUpdStatus = @CRMXRefUpdStatus",
                CommandType = CommandType.Text
            };
            command.Parameters.AddWithValue("@CRMXRefUpdStatus", active);
            base.OpenDbConnection();
            command.Connection = base.DbConnection;
            int num = command.ExecuteNonQuery();
            base.CloseDbConnection();
            return num;
        }

        public int UpdateCRMSystem(string crmSystem)
        {
            int num = 0;
            OleDbCommand command = new OleDbCommand {
                CommandText = "Update Setup set CRMSystem= @CRMSystem",
                CommandType = CommandType.Text
            };
            command.Parameters.AddWithValue("@CRMSystem", crmSystem);
            base.OpenDbConnectionWithSecurity();
            command.Connection = base.DbConnection;
            num = command.ExecuteNonQuery();
            base.CloseDbConnection();
            return num;
        }
    }
}

