using GoToWindow.Api;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GoToWindow.Windows
{
    class Database
    {
        SQLiteConnection mCon;
        public readonly string TB_HOTKEY = "tb_hotkey";
        public void Open()
        {
            mCon = new SQLiteConnection("Data Source=gotoWindow.db;Version=3;");
            mCon.Open();
            using (var dr = ExeQuerySQL("SELECT type FROM sqlite_master WHERE type='table'"))
            {
                if (!dr.Read())
                {
                    string sql = string.Format("CREATE TABLE {0} (id integer primary key autoincrement,name varchar(100),key integer)", TB_HOTKEY);
                    ExeSQL(sql);
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
            cmd.ExecuteNonQuery();
        }

        public SQLiteDataReader ExeQuerySQL(string sql)
        {
            var cmd = GetSQLCmd(sql);
            return cmd.ExecuteReader();
        }
        public static void updateHotkey(IWindowEntry win)
        {
            Database db = new Database();
            db.Open();
            using (var dr = db.ExeQuerySQL(string.Format("select key from {0} where name='{1}'", db.TB_HOTKEY, win.ProcessName)))
            {
                if (dr.Read())
                {
                    db.ExeSQL(String.Format("update {0} set key={1} where name='{2}'", db.TB_HOTKEY, win.hotKey, win.ProcessName));
                }
                else
                {
                    db.ExeSQL(string.Format("insert into {0} (name,key) values('{1}',{2})", db.TB_HOTKEY, win.ProcessName, win.hotKey));
                }
            }
            db.Close();
        }



        public static List<TbHotKeyEntry> GetAllHotKey()
        {
            List<TbHotKeyEntry> list = new List<TbHotKeyEntry>();
            var db = new Database();
            db.Open();
            using (var dr = db.ExeQuerySQL(String.Format("select name,key from {0}", db.TB_HOTKEY)))
            {
                while (dr.Read())
                {
                    var data = new TbHotKeyEntry(dr.GetString(0), dr.GetInt32(1));
                    list.Add(data);
                }
            }
            db.Close();
            return list;
        }
    }
    public class TbHotKeyEntry
    {
        public TbHotKeyEntry(string v1, int v2)
        {
            this.name = v1;
            this.key = v2;
        }

        public string name { get; set; }
        public int key { get; set; }
    }
}
