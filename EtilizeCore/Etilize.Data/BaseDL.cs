namespace Etilize.Data
{
    using System;
    using System.Data.OleDb;
    using System.Runtime.CompilerServices;

    public class BaseDL
    {
        public void CloseDbConnection()
        {
            this.DbConnection.Close();
        }

        public OleDbConnection OpenDbConnection()
        {
            OleDbConnection connection = new OleDbConnection {
                ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + this.ConnectionValue + ";Jet OLEDB:Database Password=" + this.DbPwd
            };
            this.DbConnection = connection;
            this.DbConnection.Open();
            return this.DbConnection;
        }

        public OleDbConnection OpenDbConnectionWithSecurity()
        {
            OleDbConnection connection = new OleDbConnection {
                ConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + this.ConnectionValue + ";Jet OLEDB:Database Password=" + this.DbPwd
            };
            this.DbConnection = connection;
            this.DbConnection.Open();
            return this.DbConnection;
        }

        public OleDbConnection DbConnection { get; set; }

        public string ConnectionValue { get; set; }

        public string DbPwd { get; set; }
    }
}

