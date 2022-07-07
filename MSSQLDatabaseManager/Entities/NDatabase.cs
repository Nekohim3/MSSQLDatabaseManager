using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using Newtonsoft.Json;

namespace MSSQLDatabaseManager.Entities
{
    public class NDatabase : NotificationObject
    {
        private string _instanceName;

        public string InstanceName
        {
            get => _instanceName;
            set
            {
                _instanceName = value;
                RaisePropertyChanged(() => InstanceName);
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

        private int _id;

        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged(() => Id);
            }
        }

        private bool _isUsed;
        [JsonIgnore]
        public bool IsUsed
        {
            get => _isUsed;
            set
            {
                _isUsed = value;
                RaisePropertyChanged(() => IsUsed);
            }
        }

        private string _comment;

        public string Comment
        {
            get => _comment;
            set
            {
                _comment = value;
                RaisePropertyChanged(() => Comment);
            }
        }
        
        [JsonIgnore]
        public string BaseName => Name.Replace($"_{Id}", "");

        [JsonIgnore]
        public string IdName => $"{Name.Replace($"_{Id}", "")}_{Id}";

        private ObservableCollection<NTable> _tables;

        public ObservableCollection<NTable> Tables
        {
            get => _tables;
            set
            {
                _tables = value;
                RaisePropertyChanged(() => Tables);
            }
        }

        public NDatabase(string name, int id)
        {
            Name = name;
            Id   = id;
        }

        public NDatabase(string instanceName, string name, int id, string comment, bool used = false)
        {
            InstanceName = instanceName;
            Name         = name;
            Id           = id;
            Comment      = comment;
            IsUsed       = used;
        }

        public NDatabase()
        {
        }

    }
}
