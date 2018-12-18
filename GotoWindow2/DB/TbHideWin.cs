using GoToWindow.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotoWindow2.DB
{
    class TbHideWin : Database
    {
        public static List<TbHideWinEntry> GetAll()
        {
            List<TbHideWinEntry> list = new List<TbHideWinEntry>();
            TbHideWin db = new TbHideWin();
            db.Open();
            var rd = db.ExeQuerySQL(string.Format("select name,title from {0}", db.TB_HIDEWIN));
            while (rd.Read())
            {
                list.Add(new TbHideWinEntry(rd.GetString(0), rd.GetString(1)));
            }
            db.Close();
            return list;
        }
        public static void Add(IWindowEntry win)
        {
            Del(win);
            TbHideWin db = new TbHideWin();
            db.Open();
            db.ExeSQL(string.Format("insert into {0} (name,title) values('{1}','{2}')", db.TB_HIDEWIN, win.ProcessName, win.Title));
            db.Close();
        }
        public static void Del(IWindowEntry win)
        {
            TbHideWin db = new TbHideWin();
            db.Open();
            db.ExeSQL(string.Format("delete from {0} where name='{1}' and title='{2}'", db.TB_HIDEWIN, win.ProcessName, win.Title));
            db.Close();
        }

        internal static void Del(TbHideWinEntry tag)
        {
            IWindowEntry entry = new WindowEntry();
            entry.ProcessName = tag.name;
            entry.Title = tag.title;
            Del(entry);
        }
    }

    public class TbHideWinEntry
    {
        public TbHideWinEntry(string v1, string v2)
        {
            this.name = v1;
            this.title = v2;
        }

        public string name { get; set; }
        public string title { get; set; }

    }
}
