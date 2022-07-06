using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;

namespace MSSQLDatabaseManager.Entities
{
    public class InstanceDb : NotificationObject
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

        private string _databaseName;

        public string DatabaseName
        {
            get => _databaseName;
            set
            {
                _databaseName = value;
                RaisePropertyChanged(() => DatabaseName);
            }
        }

        private string _login;

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                RaisePropertyChanged(() => Login);
            }
        }

        private string _password;

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                RaisePropertyChanged(() => Password);
            }
        }

        public InstanceDb(string instance, string database, string login, string password)
        {
            InstanceName = instance;
            DatabaseName = database;
            Login = login;
            Password = password;
        }

        public string DisplayName => $"{InstanceName}/{DatabaseName}";
    }
}
