using GoToWindow.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotoWindow2.DB
{
    class TbHotkey:Database
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
            var db = new TbHotkey();
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
