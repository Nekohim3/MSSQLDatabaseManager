using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using MSSQLDatabaseManager.Entities;

namespace MSSQLDatabaseManager.ViewModels
{
    public class DBCopyManagerViewModel : NotificationObject
    {
        #region Properties

        private ObservableCollection<string> _sourceListLeft;

        public ObservableCollection<string> SourceListLeft
        {
            get => _sourceListLeft;
            set
            {
                _sourceListLeft = value;
                RaisePropertyChanged(() => SourceListLeft);
            }
        }

        private string _selectedSourceLeft;

        public string SelectedSourceLeft
        {
            get => _selectedSourceLeft;
            set
            {
                _selectedSourceLeft = value;
                RaisePropertyChanged(() => SelectedSourceLeft);
                IsLeftSourceFile = _selectedSourceLeft == "File";
            }
        }

        private ObservableCollection<string> _sourceListRight;

        public ObservableCollection<string> SourceListRight
        {
            get => _sourceListRight;
            set
            {
                _sourceListRight = value;
                RaisePropertyChanged(() => SourceListRight);
            }
        }

        private string _selectedSourceRight;

        public string SelectedSourceRight
        {
            get => _selectedSourceRight;
            set
            {
                _selectedSourceRight = value;
                RaisePropertyChanged(() => SelectedSourceRight);
                _isRightSourceFile = _selectedSourceRight == "File";
            }
        }

        private bool _isLeftSourceFile;

        public bool IsLeftSourceFile
        {
            get => _isLeftSourceFile;
            set
            {
                _isLeftSourceFile = value;
                RaisePropertyChanged(() => IsLeftSourceFile);
            }
        }

        private bool _isRightSourceFile;

        public bool IsRightSourceFile
        {
            get => _isRightSourceFile;
            set
            {
                _isRightSourceFile = value;
                RaisePropertyChanged(() => IsRightSourceFile);
            }
        }

        private ObservableCollection<InstanceDb> _instanceListLeft;

        public ObservableCollection<InstanceDb> InstanceListLeft
        {
            get => _instanceListLeft;
            set
            {
                _instanceListLeft = value;
                RaisePropertyChanged(() => InstanceListLeft);
            }
        }

        private InstanceDb _selectedInstanceLeft;

        public InstanceDb SelectedInstanceLeft
        {
            get => _selectedInstanceLeft;
            set
            {
                _selectedInstanceLeft = value;
                RaisePropertyChanged(() => SelectedInstanceLeft);
            }
        }

        private ObservableCollection<NDatabase> _databaseListLeft;

        public ObservableCollection<NDatabase> DatabaseListLeft
        {
            get => _databaseListLeft;
            set
            {
                _databaseListLeft = value;
                RaisePropertyChanged(() => DatabaseListLeft);
            }
        }

        private NDatabase _selectedDatabaseLeft;

        public NDatabase SelectedDatabaseLeft
        {
            get => _selectedDatabaseLeft;
            set
            {
                _selectedDatabaseLeft = value;
                RaisePropertyChanged(() => SelectedDatabaseLeft);
            }
        }

        private ObservableCollection<InstanceDb> _instanceListRight;

        public ObservableCollection<InstanceDb> InstanceListRight
        {
            get => _instanceListRight;
            set
            {
                _instanceListRight = value;
                RaisePropertyChanged(() => InstanceListRight);
            }
        }

        private InstanceDb _selectedInstanceRight;

        public InstanceDb SelectedInstanceRight
        {
            get => _selectedInstanceRight;
            set
            {
                _selectedInstanceRight = value;
                RaisePropertyChanged(() => SelectedInstanceRight);
            }
        }

        private ObservableCollection<NDatabase> _databaseListRight;

        public ObservableCollection<NDatabase> DatabaseListRight
        {
            get => _databaseListRight;
            set
            {
                _databaseListRight = value;
                RaisePropertyChanged(() => DatabaseListRight);
            }
        }

        private NDatabase _selectedDatabaseRight;

        public NDatabase SelectedDatabaseRight
        {
            get => _selectedDatabaseRight;
            set
            {
                _selectedDatabaseRight = value;
                RaisePropertyChanged(() => SelectedDatabaseRight);
            }
        }

        private string _filePathLeft;

        public string FilePathLeft
        {
            get => _filePathLeft;
            set
            {
                _filePathLeft = value;
                RaisePropertyChanged(() => FilePathLeft);
            }
        }

        private string _filePathRight;

        public string FilePathRight
        {
            get => _filePathRight;
            set
            {
                _filePathRight = value;
                RaisePropertyChanged(() => FilePathRight);
            }
        }

        #endregion

        #region Commands



        #endregion

        #region Ctor

        public DBCopyManagerViewModel()
        {
            SourceListLeft      = new ObservableCollection<string>() { "Database", "File" };
            SourceListRight     = new ObservableCollection<string>() { "Database", "File" };
            SelectedSourceLeft  = SourceListLeft.First();
            SelectedSourceRight = SourceListRight.First();
        }

        #endregion

        #region CmdExec



        #endregion

        #region Funcs

        

        #endregion

    }
}
