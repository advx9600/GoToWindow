using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoToWindow.Api;
using System.Data.SQLite;

namespace GotoWindow2.DB
{
    public interface IDatabase
    {
        //void OnCreateTB();
    }

    public abstract class Database : IDatabase
    {
        SQLiteConnection mCon;
        public readonly string TB_HOTKEY = "tb_hotkey";
        public readonly string TB_HIDEWIN = "tb_hidewin";
        public void Open()
        {
            mCon = new SQLiteConnection("Data Source=gotoWindow.db;Version=3;");
            mCon.Open();
            using (var dr = ExeQuerySQL("SELECT type FROM sqlite_master WHERE type='table'"))
            {
                if (!dr.Read())
                {
                    ExeSQL(string.Format("CREATE TABLE {0} (id integer primary key autoincrement,name varchar(100),key integer,exe_path varchar(255))", TB_HOTKEY));
                    ExeSQL(string.Format("create table {0} (id INTEGER primary key autoincrement,name varchar(100),title varchar(100))", TB_HIDEWIN));
                }
            }
        }

        public void Close()
        {
            mCon.Close();
        }

        public SQLiteCommand GetSQLCmd(string sql)
        {
            SQLiteCommand cmd = new SQLiteCommand();
            //Console.WriteLine(sql);
            cmd.Connection = mCon;
            cmd.CommandText = sql;
            return cmd;
        }
        public void ExeSQL(string sql)
        {
            var cmd = GetSQLCmd(sql);
            //Console.WriteLine(sql);
            cmd.ExecuteNonQuery();
        }

        public SQLiteDataReader ExeQuerySQL(string sql)
        {
            var cmd = GetSQLCmd(sql);
            return cmd.ExecuteReader();
        }

    }

}
