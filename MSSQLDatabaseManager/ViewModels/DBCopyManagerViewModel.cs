using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using MSSQLDatabaseManager.Entities;
using MSSQLDatabaseManager.Utils;

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
                IsRightSourceFile = _selectedSourceRight == "File";
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
                if (!_isLeftSourceFile)
                {
                    InstanceListLeft     = new ObservableCollection<InstanceDb>(g.Settings.InstanceList);
                    SelectedInstanceLeft = InstanceListLeft.FirstOrDefault();
                }
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
                if (!_isRightSourceFile)
                {
                    InstanceListRight     = new ObservableCollection<InstanceDb>(g.Settings.InstanceList);
                    SelectedInstanceRight = InstanceListRight.FirstOrDefault();
                }
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
                if (value != null)
                {
                    DatabaseListLeft = new ObservableCollection<NDatabase>(SQLService.GetDatabases(_selectedInstanceLeft.InstanceName).Where(x => x.Name.StartsWith(_selectedInstanceLeft.DatabaseName))
                                                                                     .ToList());
                    SelectedDatabaseLeft = DatabaseListLeft.FirstOrDefault();
                }
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

        private ObservableCollection<NTabCol> _tabColListLeft;

        public ObservableCollection<NTabCol> TabColListLeft
        {
            get => _tabColListLeft;
            set
            {
                _tabColListLeft = value;
                RaisePropertyChanged(() => TabColListLeft);
            }
        }

        private NTabCol _selectedTabColLeft;

        public NTabCol SelectedTabColLeft
        {
            get => _selectedTabColLeft;
            set
            {
                if (_selectedTabColLeft != null)
                    _selectedTabColLeft.OnChangedIsExpanded -= ChangedIsExpandedLeft;
                _selectedTabColLeft = value;
                RaisePropertyChanged(() => SelectedTabColLeft);
                if (_selectedTabColLeft != null)
                    _selectedTabColLeft.OnChangedIsExpanded += ChangedIsExpandedLeft;
            }
        }

        private NTabCol _selectedTabColRight;

        public NTabCol SelectedTabColRight
        {
            get => _selectedTabColRight;
            set
            {
                _selectedTabColRight = value;
                RaisePropertyChanged(() => SelectedTabColRight);
            }
        }

        private ObservableCollection<NTabCol> _tabColListRight;

        public ObservableCollection<NTabCol> TabColListRight
        {
            get => _tabColListRight;
            set
            {
                _tabColListRight = value;
                RaisePropertyChanged(() => TabColListRight);
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
                if (value != null)
                {
                    DatabaseListRight = new ObservableCollection<NDatabase>(SQLService.GetDatabases(_selectedInstanceRight.InstanceName)
                                                                                      .Where(x => x.Name.StartsWith(_selectedInstanceRight.DatabaseName)).ToList());
                    SelectedDatabaseRight = DatabaseListRight.FirstOrDefault();
                }
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

        public DelegateCommand GetSchemaLeftCmd  { get; }
        public DelegateCommand GetSchemaRightCmd { get; }

        #endregion

        #region Ctor

        public DBCopyManagerViewModel()
        {
            GetSchemaLeftCmd  = new DelegateCommand(OnGetSchemaLeft);
            GetSchemaRightCmd = new DelegateCommand(OnGetSchemaRight);

            SourceListLeft      = new ObservableCollection<string>() {"Database", "File"};
            SourceListRight     = new ObservableCollection<string>() {"Database", "File"};
            SelectedSourceLeft  = SourceListLeft.First();
            SelectedSourceRight = SourceListRight.First();
        }

        #endregion

        #region CmdExec

        private void OnGetSchemaLeft()
        {
            SelectedDatabaseLeft.LoadSchema();
            TabColListLeft = new ObservableCollection<NTabCol>();
            TabColListLeft.AddRange(SelectedDatabaseLeft.Tables.Select(x => new NTabCol(x)));
            //foreach (var x in SelectedDatabaseLeft.Tables)
            //{
            //    TabColListLeft.Add(new NTabCol(x));
            //    TabColListLeft.AddRange(x.Columns.Select(c => new NTabCol(c)));
            //}
        }

        private void OnGetSchemaRight()
        {
            SelectedDatabaseRight.LoadSchema();
            TabColListRight = new ObservableCollection<NTabCol>();
            //foreach (var x in SelectedDatabaseRight.Tables)
            //{
            //    TabColListRight.Add(new NTabCol(x));
            //    TabColListRight.AddRange(x.Columns.Select(c => new NTabCol(c)));
            //}
        }

        #endregion

        #region Funcs

        private void ChangedIsExpandedLeft(bool isExpanded, NTabCol tabCol)
        {
            var ind = TabColListLeft.IndexOf(tabCol) + 1;
            if (isExpanded)
            {
                foreach (var x in tabCol.Table.Columns)
                    TabColListLeft.Insert(ind++, new NTabCol(x));
            }
            else
            {
                while (ind < TabColListLeft.Count && !TabColListLeft[ind].IsTable)
                    TabColListLeft.RemoveAt(ind);
            }
        }

        #endregion

    }
}
