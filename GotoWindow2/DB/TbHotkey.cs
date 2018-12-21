using GoToWindow.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GotoWindow2.DB
{
    class TbHotkey : Database
    {
        public static void updateHotkey(IWindowEntry win, bool isUpdatePath)
        {
            var entity = new TbHotKeyEntity(win.ProcessName, win.hotKey, win.ExecutablePath);
            updateHotkey(entity, isUpdatePath);
        }

        internal static void Del(TbHotKeyEntity entity)
        {
            var db = new TbHotkey();
            db.Open();
            db.ExeSQL(string.Format("delete from {0} where name='{1}'", db.TB_HOTKEY, entity.name));
            db.Close();
        }

        public static List<TbHotKeyEntity> GetAll()
        {
            List<TbHotKeyEntity> list = new List<TbHotKeyEntity>();
            var db = new TbHotkey();
            db.Open();
            using (var dr = db.ExeQuerySQL(String.Format("select name,key,exe_path from {0}", db.TB_HOTKEY)))
            {
                while (dr.Read())
                {
                    var data = new TbHotKeyEntity(dr.GetString(0), dr.GetInt32(1), dr.GetString(2));
                    list.Add(data);
                }
            }
            db.Close();
            return list;
        }

        internal static void updateHotkey(TbHotKeyEntity entity, bool isUpdateExePath)
        {
            TbHotkey db = new TbHotkey();
            db.Open();
            using (var dr = db.ExeQuerySQL(string.Format("select key from {0} where name='{1}'", db.TB_HOTKEY, entity.name)))
            {
                // 把原来先设置好的快捷键取消
                db.ExeSQL(String.Format("update {0} set key=0 where key={1}", db.TB_HOTKEY, entity.key));
                if (dr.Read())
                {
                    if (isUpdateExePath)
                        db.ExeSQL(String.Format("update {0} set key={1}, exe_path='{2}' where name='{3}'", db.TB_HOTKEY, entity.key, entity.executablePath, entity.name));
                    else
                        db.ExeSQL(String.Format("update {0} set key={1}  where name='{2}'", db.TB_HOTKEY, entity.key, entity.name));
                }
                else
                {
                    db.ExeSQL(string.Format("insert into {0} (name,key,exe_path) values('{1}',{2},'{3}')", db.TB_HOTKEY, entity.name, entity.key, entity.executablePath));
                }
            }
            db.Close();

        }
    }

    public class TbHotKeyEntity
    {
        public TbHotKeyEntity(string name, int key, string executablePath)
        {
            this.name = name;
            this.key = key;
            this.executablePath = executablePath;
        }

        public string name { get; set; }
        public int key { get; set; }
        public string executablePath { get; set; }
        public string bindKey { get { return string.Format("{0}", (Key)key); } }
    }
}
