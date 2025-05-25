using System;
using Npgsql;
using System.Data;

namespace Inventorifo.Lib
{
    public class LibDb
    {
        
        public String CONNSTR = "Server=localhost;Port=5432;User Id=postgres;Password=6dbad1f65d69313c39c75834b017716a;Database=inventorifo;Pooling=true;MinPoolSize=1;MaxPoolSize=200;";
        public String CONNSTR2 = "Server=192.168.192.123;Port=5432;User Id=postgres;Password=6dbad1f65d69313c39c75834b017716a;Database=inventorifo;Pooling=true;MinPoolSize=1;MaxPoolSize=200;";
        public String CONNSTR3 = "Server=192.168.1.123;Port=5432;User Id=postgres;Password=6dbad1f65d69313c39c75834b017716a;Database=inventorifo;Pooling=true;MinPoolSize=1;MaxPoolSize=200;";
        public LibDb()
        {
        }
        public NpgsqlConnection getConn()
        {
            NpgsqlConnection conn = null;
            try
            {
                conn = new NpgsqlConnection(CONNSTR);
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return conn;
        }
        public NpgsqlConnection getConn2()
        {
            NpgsqlConnection conn = null;
            try
            {
                conn = new NpgsqlConnection(CONNSTR2);
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return conn;
        }
        public NpgsqlConnection getConn3()
        {
            NpgsqlConnection conn = null;
            try
            {
                conn = new NpgsqlConnection(CONNSTR3);
                conn.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return conn;
        }
        
        public Int64 ExecuteScalar(NpgsqlConnection conn, String sql)
        { //Object res = query.ExecuteScalar();
            Int64 id = 0;
            using (var cmd = new NpgsqlCommand(sql, conn))
            {
                object result = cmd.ExecuteScalar(); // Execute the query
                id = (result != null && result != DBNull.Value) ? Convert.ToInt64(result) : 0;
            }
            return id;
        }

        public void ExecuteTrans(NpgsqlConnection conn, String sql)
        {
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
        }

        public Int32 ExecuteCommand(NpgsqlConnection conn, String sql)
        {
            Int32 rowsaffected = 0;
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                rowsaffected = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return rowsaffected;
        }

        public DataTable fillDataTable(NpgsqlConnection conn, string sql)
        {
            DataTable dt = new DataTable();
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand(sql, conn);
                dt.Load(cmd.ExecuteReader());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            return dt;
        }

        public DataTable FlipDataTable(DataTable dt)
        {
            DataTable table = new DataTable();
            //Get all the rows and change into columns
            for (int i = 0; i <= dt.Rows.Count; i++)
            {
                table.Columns.Add(Convert.ToString(i));
            }
            DataRow dr;
            //get all the columns and make it as rows
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                dr = table.NewRow();
                dr[0] = dt.Columns[j].ToString();
                for (int k = 1; k <= dt.Rows.Count; k++)
                    dr[k] = dt.Rows[k - 1][j];
                table.Rows.Add(dr);
            }
            //table.Rows[0].Delete();
            return table;
        }

    }
}
