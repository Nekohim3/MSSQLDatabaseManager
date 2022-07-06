using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;

namespace MSSQLDatabaseManager.Entities
{
    public class NTable : NotificationObject
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

        private ObservableCollection<NColumn> _columns;

        public ObservableCollection<NColumn> Columns
        {
            get => _columns;
            set
            {
                _columns = value;
                RaisePropertyChanged(() => Columns);
            }
        }
    }
}
