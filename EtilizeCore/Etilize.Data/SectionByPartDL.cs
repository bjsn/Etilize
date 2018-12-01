namespace Etilize.Data
{
    using Models;
    using System;
    using System.Data;
    using System.Data.OleDb;

    public class SectionByPartDL : BaseDL, IDataService
    {
        public SectionByPartDL(string connectionValue)
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

        public int Save(SectionPartDetail sectionDetail)
        {
            base.OpenDbConnection();
            base.OpenDbConnection();
            OleDbCommand command2 = new OleDbCommand();
            object[] objArray = new object[] { "Insert into SectionByPartDetails(Content, SectionByPartId) values ('", sectionDetail.Content, "',", sectionDetail.SectionByPartId, ")" };
            command2.CommandText = string.Concat(objArray);
            command2.CommandType = CommandType.Text;
            OleDbCommand command = command2;
            command.Connection = base.DbConnection;
            command.ExecuteNonQuery();
            base.CloseDbConnection();
            return 0;
        }
    }
}

