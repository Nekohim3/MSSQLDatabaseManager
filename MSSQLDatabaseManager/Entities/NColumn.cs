using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;

namespace MSSQLDatabaseManager.Entities
{
    public class NColumn : NotificationObject
    {
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

        public NColumn()
        {
            
        }

        public NColumn(string name, string type, bool nullable)
        {
            Name = name;
            Type = type;
            Nullable = nullable;
        }
        
    }
}
