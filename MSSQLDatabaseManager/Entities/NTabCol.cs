using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;

namespace MSSQLDatabaseManager.Entities
{
    public class NTabCol : NotificationObject
    {
        private bool _isExpanded;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                RaisePropertyChanged(() => IsExpanded);
                ChangedIsExpanded(value);
            }
        }

        private bool _isSelected;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                RaisePropertyChanged(() => IsSelected);
            }
        }

        private NTable _table;

        public NTable Table
        {
            get => _table;
            set
            {
                _table = value;
                RaisePropertyChanged(() => Table);
            }
        }

        private NColumn _column;

        public NColumn Column
        {
            get => _column;
            set
            {
                _column = value;
                RaisePropertyChanged(() => Column);
            }
        }

        public bool IsTable => Table != null;

        public string Name => Table != null ? Table.Name : Column.Name;

        public string Type => Table != null ? string.Empty : Column.AType;

        public Action<bool, NTabCol> OnChangedIsExpanded { get; set; }

        public NTabCol(NTable table)
        {
            Table = table;
        }

        public NTabCol(NColumn col)
        {
            Column = col;
        }

        private void ChangedIsExpanded(bool isExpanded)
        {
            OnChangedIsExpanded?.Invoke(isExpanded, this);
        }
    }
}
