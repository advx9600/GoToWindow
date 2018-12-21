using GoToWindow.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GotoWindow2.DB
{
    class TbExecutablePath : Database
    {
        public static List<TbExecutablePathEntity> GetAll()
        {
            var db = new TbExecutablePath();
            db.Open();
            var rd = db.ExeQuerySQL(string.Format("select name,path from {0}", db.TB_EXE_PATH));
            var list = new List<TbExecutablePathEntity>();
            while (rd.Read())
            {
                list.Add(new TbExecutablePathEntity(rd.GetString(0), rd.GetString(1)));
            }
            db.Close();
            return list;
        }

        public static void UpdateOrAdd(TbExecutablePathEntity entity)
        {
            Del(entity);
            var db = new TbExecutablePath();
            db.Open();
            db.ExeSQL(string.Format("insert into {0} (name,path) values('{1}','{2}')", db.TB_EXE_PATH, entity.name, entity.path));
            db.Close();
        }
        public static void Del(TbExecutablePathEntity entity)
        {
            var db = new TbExecutablePath();
            db.Open();
            db.ExeSQL(string.Format("delete from {0} where name='{1}'", db.TB_EXE_PATH, entity.name));
            db.Close();
        }
        //public static void Del(IWindowEntry win)
        //{

        //}
    }

    class TbExecutablePathEntity
    {
        public TbExecutablePathEntity(string name, string path)
        {
            this.name = name;
            this.path = path;
        }
        public string name { get; set; }
        public string path { get; set; }
    }
}
