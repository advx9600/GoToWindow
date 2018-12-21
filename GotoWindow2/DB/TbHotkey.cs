using GoToWindow.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotoWindow2.DB
{
    class TbHotkey : Database
    {
        public static void updateHotkey(IWindowEntry win)
        {
            TbHotkey db = new TbHotkey();
            db.Open();
            using (var dr = db.ExeQuerySQL(string.Format("select key from {0} where name='{1}'", db.TB_HOTKEY, win.ProcessName)))
            {
                // 把原来先设置好的快捷键取消
                db.ExeSQL(String.Format("update {0} set key=0 where key={1}", db.TB_HOTKEY, win.hotKey));
                if (dr.Read())
                {
                    db.ExeSQL(String.Format("update {0} set key={1} exe_path='{2}' where name='{3}'", db.TB_HOTKEY, win.ExecutablePath, win.hotKey, win.ProcessName));
                }
                else
                {
                    db.ExeSQL(string.Format("insert into {0} (name,key,exe_path) values('{1}',{2},'{3}')", db.TB_HOTKEY, win.ProcessName, win.hotKey, win.ExecutablePath));
                }
            }
            db.Close();
        }



        public static List<TbHotKeyEntry> GetAllHotKey()
        {
            List<TbHotKeyEntry> list = new List<TbHotKeyEntry>();
            var db = new TbHotkey();
            db.Open();
            using (var dr = db.ExeQuerySQL(String.Format("select name,key,exe_path from {0}", db.TB_HOTKEY)))
            {
                while (dr.Read())
                {
                    var data = new TbHotKeyEntry(dr.GetString(0), dr.GetInt32(1), dr.GetString(2));
                    list.Add(data);
                }
            }
            db.Close();
            return list;
        }
    }

    public class TbHotKeyEntry
    {
        public TbHotKeyEntry(string v1, int v2, string v3)
        {
            this.name = v1;
            this.key = v2;
            this.executablePath = v3;
        }

        public string name { get; set; }
        public int key { get; set; }
        public string executablePath { get; set; }
    }
}
