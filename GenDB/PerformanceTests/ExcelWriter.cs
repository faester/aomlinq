using System;
using System.Collections.Generic;
using System.Text;
using System.Data.OleDb;
using System.Data;

namespace PerformanceTests
{
    public class ExcelWriter
    {
        string filename = null;
        string sheetname = null;
        OleDbConnection cnn;
        OleDbCommand cmd;
        private static string ConnectionString(string outputfile)
        {
            string cnnstr = "Provider=Microsoft.Jet.OLEDB.4.0;" +
                   "Data Source=" + outputfile + ";" +
                   "Extended Properties=\"Excel 8.0;HDR=YES\"";
            return cnnstr;
        }

        private ExcelWriter() { /* empty */ }

        public ExcelWriter(string filename, string sheetname)
        {
            this.sheetname = sheetname;
            this.filename = filename;
            Init();
        }

        private void Init()
        {
            cnn = new OleDbConnection(ConnectionString(filename));
            cnn.Open();
            cmd = new OleDbCommand();
            cmd.Connection = cnn;
            ConstructWorksheet(sheetname);
        }

        private void ConstructWorksheet(string sheetname)
        {
            try
            {
                cmd.CommandText = "DROP TABLE " + sheetname;
                cmd.ExecuteNonQuery();
            }
            catch (OleDbException oex)
            {
                Console.WriteLine(oex.ToString());
            }

            cmd.CommandText = "CREATE TABLE " + sheetname + " (objectCount int, theTime double)";
            cmd.ExecuteNonQuery();
        }

        public void WriteInformation(int objectCount, float time)
        {
            string timeStr = time.ToString().Replace(',', '.');
            string cmdHeader = "INSERT INTO " + sheetname + " (objectCount, theTime) VALUES (" + objectCount + "," + timeStr + ")";
            cmd.CommandText = cmdHeader;
            cmd.ExecuteNonQuery();
        }

        public void Dispose()
        {
            cnn.Close();
        }
    }
}

