namespace Etilize.Data
{
    using Etilize.Models;
    using System;
    using System.Data;
    using System.Data.OleDb;

    public class SectionPartDL : BaseDL, IDataService
    {
        public SectionPartDL(string connectionValue)
        {
            base.ConnectionValue = connectionValue;
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

        public int Save(SectionPart sectionPart)
        {
            base.OpenDbConnection();
            OleDbCommand command3 = new OleDbCommand();
            object[] objArray = new object[] { "Insert into SectionByPart(SectionTitle, PartID) Values ('", sectionPart.SectionTitle, "',", sectionPart.Partid, ")" };
            command3.CommandText = string.Concat(objArray);
            command3.CommandType = CommandType.Text;
            OleDbCommand command = command3;
            command.Connection = base.DbConnection;
            command.ExecuteNonQuery();
            using (OleDbCommand command2 = new OleDbCommand("SELECT @@IDENTITY;", base.DbConnection))
            {
                return (int) command2.ExecuteScalar();
            }
        }
    }
}

