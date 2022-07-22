using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;

namespace MSSQLDatabaseManager.Utils
{
    public class DbTable : NotificationObject
    {
        private List<DbProperty> _propertyList;

        public List<DbProperty> PropertyList
        {
            get => _propertyList;
            set
            {
                _propertyList = value;
                RaisePropertyChanged(() => PropertyList);
            }
        }

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        public List<List<object>> Rows { get; set; }

        public void AddRow(List<object> data)
        {
            Rows.Add(data);
        }
    }

    public class DbProperty : NotificationObject
    {
        private string _type;

        public string Type
        {
            get => _type;
            set
            {
                _type = value;
                RaisePropertyChanged(() => Type);
            }
        }

        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged(() => Name);
            }
        }

        private bool _nullable;

        public bool Nullable
        {
            get => _nullable;
            set
            {
                _nullable = value;
                RaisePropertyChanged(() => Nullable);
            }
        }

        public DbProperty()
        {

        }

        public DbProperty(string type, string name, bool nullable)
        {
            Type = type;
            Name = name;
            Nullable = nullable;
        }
    }

    public class DbData : NotificationObject
    {

    }
}
