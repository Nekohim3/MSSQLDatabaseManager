using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.ViewModel;
using MSSQLDatabaseManager.Entities;
using Newtonsoft.Json;

namespace MSSQLDatabaseManager.Utils
{
    public class Settings
    {
        private Skin _theme;

        public Skin Theme
        {
            get => _theme;
            set
            {
                _theme = value;
                (App.Current as App).ChangeSkin(_theme);
            }
        }

        public string                           DirForDbData { get; set; }
        public ObservableCollection<InstanceDb> InstanceList { get; set; }

        public Settings()
        {
            InstanceList = new ObservableCollection<InstanceDb>();
        }

        public void SetDir(string path)
        {
            DirForDbData = path;
            Save();
        }

        public List<NDatabase> GetSavedDbList()
        {
            return !File.Exists("databases.cfg") ? new List<NDatabase>() : JsonConvert.DeserializeObject<List<NDatabase>>(File.ReadAllText("databases.cfg"));
        }

        public bool AddInstance(InstanceDb db)
        {
            Logger.Info($"AddInstance ({db.InstanceName}\\{db.DatabaseName}) start");
            if (InstanceList.Count(x => x.InstanceName == db.InstanceName && x.DatabaseName == db.DatabaseName) != 0)
            {
                Logger.Info($"AddInstance ({db.InstanceName}\\{db.DatabaseName}) fail: already exist");
                return false;

            }

            InstanceList.Add(db);
            Save();
            Logger.Info($"AddInstance ({db.InstanceName}\\{db.DatabaseName}) succ");
            return true;

        }

        public void RemoveInstance(InstanceDb db)
        {
            var q = InstanceList.Remove(db);
            Logger.Info($"RemoveInstance ({db.InstanceName}_{db.DatabaseName}) {(q ? "succ" : "fail: not found")}");
            Save();
        }

        public void CheckDbList(List<NDatabase> list)
        {
            var savedList = !File.Exists("databases.cfg") ? new List<NDatabase>() : JsonConvert.DeserializeObject<List<NDatabase>>(File.ReadAllText("databases.cfg"));
            var forAdd    = list.Where(x => savedList.Count(c => c.InstanceName == x.InstanceName && c.Id == x.Id) == 0).ToList();
            foreach (var x in forAdd)
                savedList.Add(x);
            var forDelete = savedList.Where(x => list.Count(c => c.InstanceName == x.InstanceName && c.Id == x.Id) == 0).ToList();
            foreach (var x in forDelete)
            {
                savedList.Remove(x);
            }
            var forEdit   = new List<NDatabase>();
            foreach (var x in list)
            {
                var c = savedList.FirstOrDefault(v => v.InstanceName == x.InstanceName && v.Id == x.Id);
                if(c != null && c.Comment != x.Comment)
                    forEdit.Add(x);
            }
            foreach (var x in forEdit)
            {
                var c = savedList.FirstOrDefault(v => v.InstanceName == x.InstanceName && v.Id == x.Id);
                if (c != null)
                    c.Comment = x.Comment;
            }
            File.WriteAllText("databases.cfg", JsonConvert.SerializeObject(savedList));
        }

        public void CheckDb(NDatabase db)
        {
            var savedList = !File.Exists("databases.cfg") ? new List<NDatabase>() : JsonConvert.DeserializeObject<List<NDatabase>>(File.ReadAllText("databases.cfg"));
            var savedDb   = savedList.FirstOrDefault(x => x.InstanceName == db.InstanceName &&  x.Id == db.Id);
            if (savedDb != null)
            {
                if (savedDb.Comment != db.Comment)
                {
                    savedDb.Comment = db.Comment;
                    File.WriteAllText("databases.cfg", JsonConvert.SerializeObject(savedList));
                }
            }
            else
            {
                savedList.Add(db);
                File.WriteAllText("databases.cfg", JsonConvert.SerializeObject(savedList));
            }
        }

        
        public (string login, string pass) GetCredForInstance(string instanceName)
        {
            var instance = InstanceList.FirstOrDefault(x => x.InstanceName == instanceName);
            return instance == null ? (null, null) : (instance.Login, instance.Password);
        }

        public static Settings Load()
        {
            return !File.Exists("config.cfg") ? null : JsonConvert.DeserializeObject<Settings>(File.ReadAllText("config.cfg"));
        }

        public void Save()
        {
            File.WriteAllText("config.cfg", JsonConvert.SerializeObject(this));
        }
    }
}
