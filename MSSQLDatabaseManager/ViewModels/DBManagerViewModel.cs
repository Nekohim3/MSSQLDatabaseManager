using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using MSSQLDatabaseManager.Entities;
using MSSQLDatabaseManager.Utils;

namespace MSSQLDatabaseManager.ViewModels
{
    public class DbManagerViewModel : NotificationObject
    {

        #region Properties

        #region AddInstance

        private ObservableCollection<string> _addInstanceList;

        public ObservableCollection<string> AddInstanceList
        {
            get => _addInstanceList;
            set
            {
                _addInstanceList = value;
                RaisePropertyChanged(() => AddInstanceList);
            }
        }

        private string _selectedAddInstance;

        public string SelectedAddInstance
        {
            get => _selectedAddInstance;
            set
            {
                _selectedAddInstance = value;
                RaisePropertyChanged(() => SelectedAddInstance);
                if (!string.IsNullOrEmpty(_selectedAddInstance))
                {
                    AddDatabaseList = new ObservableCollection<NDatabase>(SQLService.GetDatabases($"{_selectedAddInstance}"));
                }
            }
        }

        private ObservableCollection<NDatabase> _addDatabaseList;

        public ObservableCollection<NDatabase> AddDatabaseList
        {
            get => _addDatabaseList;
            set
            {
                _addDatabaseList = value;
                RaisePropertyChanged(() => AddDatabaseList);
            }
        }

        private NDatabase _selectedAddDatabase;

        public NDatabase SelectedAddDatabase
        {
            get => _selectedAddDatabase;
            set
            {
                _selectedAddDatabase = value;
                RaisePropertyChanged(() => SelectedAddDatabase);
                RaiseCanExecChanged();
            }
        }

        private string _addDatabaseLogin;

        public string AddDatabaseLogin
        {
            get => _addDatabaseLogin;
            set
            {
                _addDatabaseLogin = value;
                RaisePropertyChanged(() => AddDatabaseLogin);
            }
        }

        private string _addDatabasePassword;

        public string AddDatabasePassword
        {
            get => _addDatabasePassword;
            set
            {
                _addDatabasePassword = value;
                RaisePropertyChanged(() => AddDatabasePassword);
            }
        }

        #endregion

        private ObservableCollection<InstanceDb> _instanceList;

        public ObservableCollection<InstanceDb> InstanceList
        {
            get => _instanceList;
            set
            {
                _instanceList = value;
                RaisePropertyChanged(() => InstanceList);
            }
        }

        private InstanceDb _selectedInstance;

        public InstanceDb SelectedInstance
        {
            get => _selectedInstance;
            set
            {
                _selectedInstance = value;
                RaisePropertyChanged(() => SelectedInstance);
                if (SelectedInstance != null)
                {
                    if (DatabaseList != null && DatabaseList.Count != 0)
                        g.Settings.CheckDbList(DatabaseList.ToList());
                    var dbList      = SQLService.GetDatabases(_selectedInstance.InstanceName).Where(x => x.Name.StartsWith(_selectedInstance.DatabaseName));
                    var savedDbList = g.Settings.GetSavedDbList();
                    DatabaseList = new ObservableCollection<NDatabase>();
                    foreach (var x in dbList)
                        DatabaseList.Add(new NDatabase(_selectedInstance.InstanceName, x.Name, x.Id, savedDbList.FirstOrDefault(c => c.InstanceName == x.InstanceName && c.Id == x.Id)?.Comment));
                }
                else
                    DatabaseList = new ObservableCollection<NDatabase>();

                RaiseCanExecChanged();
            }
        }

        private ObservableCollection<NDatabase> _databaseList;

        public ObservableCollection<NDatabase> DatabaseList
        {
            get => _databaseList;
            set
            {
                _databaseList = value;
                RaisePropertyChanged(() => DatabaseList);
            }
        }

        private NDatabase _selectedDatabase;

        public NDatabase SelectedDatabase
        {
            get => _selectedDatabase;
            set
            {
                if (_selectedDatabase != null)
                    g.Settings.CheckDb(_selectedDatabase);
                _selectedDatabase = value;
                RaisePropertyChanged(() => SelectedDatabase);
                RaiseCanExecChanged();
            }
        }

        private bool _addInstanceMode;

        public bool AddInstanceMode
        {
            get => _addInstanceMode;
            set
            {
                _addInstanceMode = value;
                RaisePropertyChanged(() => AddInstanceMode);
            }
        }

        #endregion

        #region Commands

        public DelegateCommand AddDatabaseCmd        { get; }
        public DelegateCommand RemoveDatabaseCmd     { get; }
        public DelegateCommand ConfirmAddDatabaseCmd { get; }
        public DelegateCommand CancelAddDatabaseCmd  { get; }
        public DelegateCommand RefreshDatabasesCmd   { get; }

        public DelegateCommand SetCmd            { get; }
        public DelegateCommand UnsetCmd          { get; }
        public DelegateCommand BackupCmd         { get; }
        public DelegateCommand RestoreSetCmd     { get; }
        public DelegateCommand RestoreUnsetCmd   { get; }
        public DelegateCommand DeleteDatabaseCmd { get; }

        #endregion

        #region Ctor

        public DbManagerViewModel()
        {
            AddDatabaseCmd    = new DelegateCommand(OnAddDatabase,    () => !AddInstanceMode);
            RemoveDatabaseCmd = new DelegateCommand(OnRemoveDatabase, () => SelectedInstance != null && !AddInstanceMode);

            ConfirmAddDatabaseCmd = new DelegateCommand(OnConfirmAddDatabase, () => AddInstanceMode && SelectedAddDatabase != null);
            CancelAddDatabaseCmd  = new DelegateCommand(OnCancelAddDatabase,  () => AddInstanceMode);

            RefreshDatabasesCmd = new DelegateCommand(OnRefreshDatabases, () => SelectedInstance != null);

            SetCmd            = new DelegateCommand(OnSet,    () => SelectedDatabase != null && SelectedDatabase.Name != SelectedInstance.DatabaseName);
            UnsetCmd          = new DelegateCommand(OnUnset,  () => SelectedDatabase != null && SelectedDatabase.Name == SelectedInstance.DatabaseName);
            BackupCmd         = new DelegateCommand(OnBackup, () => SelectedDatabase != null);
            RestoreSetCmd     = new DelegateCommand(OnRestoreSet);
            RestoreUnsetCmd   = new DelegateCommand(OnRestoreUnset);
            DeleteDatabaseCmd = new DelegateCommand(OnDeleteDatabase, () => SelectedDatabase != null);

            RefreshDatabases();
        }

        #endregion

        #region CmdExec

        private void OnAddDatabase()
        {
            AddInstanceMode     = true;
            AddInstanceList     = new ObservableCollection<string>(SQLService.GetSQLInstances());
            SelectedAddInstance = null;
            SelectedAddDatabase = null;
            AddDatabaseList     = null;
            AddDatabaseLogin    = "sa";
            AddDatabasePassword = "SoftMarine_14";
            RaiseCanExecChanged();
        }

        private void OnRemoveDatabase()
        {
            g.Settings.RemoveInstance(SelectedInstance);
            RefreshDatabases();
        }

        private void OnConfirmAddDatabase()
        {
            g.Settings.AddInstance(new InstanceDb(SelectedAddInstance, SelectedAddDatabase.Name, AddDatabaseLogin, AddDatabasePassword));
            AddInstanceMode = false;
            RefreshDatabases();
        }

        private void OnCancelAddDatabase()
        {
            AddInstanceMode = false;
            RaiseCanExecChanged();
        }

        private void OnRefreshDatabases()
        {
            RefreshDatabases();
        }

        private void OnSet()
        {
            SQLService.SetDatabase(SelectedDatabase);
            RefreshDatabases();
        }

        private void OnUnset()
        {
            SQLService.UnsetDatabase(SelectedDatabase);
            RefreshDatabases();
        }

        private void OnBackup()
        {
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\Backups"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\Backups");
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/",         "\\")}"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}");

            var sfd = new CommonSaveFileDialog()
                      {
                          RestoreDirectory = true,
                          InitialDirectory = $"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\",
                          DefaultFileName  = $"{SelectedDatabase.IdName}{(!string.IsNullOrEmpty(SelectedDatabase.Comment) ? $"-{SelectedDatabase.Comment}" : "")}",
                          DefaultExtension = ".bak",
                          Filters          = { new CommonFileDialogFilter("MSSQL DB Backup", "bak") }
                      };
            if (sfd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                g.StartLongOperation(() => SQLService.BackupDatabase(SelectedInstance, SelectedDatabase, sfd.FileName));
            }

        }

        private void OnRestoreSet()
        {
        }

        private void OnRestoreUnset()
        {
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\DbData"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\DbData");
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/",         "\\")}"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/", "\\")}");
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/",         "\\")}\\Temp"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp");
            var ofd = new CommonOpenFileDialog()
                      {
                          Title            = $"Select backup for restore {SelectedInstance.DatabaseName} type database",
                          IsFolderPicker   = false,
                          InitialDirectory = $"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\",
                          Filters          = { new CommonFileDialogFilter("MSSQL DB Backup", "bak") }
                      };

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                g.StartLongOperation(() =>
                                     {
                                         SQLService.RestoreDatabase(SelectedInstance, $"{SelectedInstance.DatabaseName}_temp", ofd.FileName,
                                                                    $"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp");
                                     });
            }
        }

        private void OnDeleteDatabase()
        {

        }

        private void RaiseCanExecChanged()
        {
            AddDatabaseCmd.RaiseCanExecuteChanged();
            RemoveDatabaseCmd.RaiseCanExecuteChanged();
            ConfirmAddDatabaseCmd.RaiseCanExecuteChanged();
            CancelAddDatabaseCmd.RaiseCanExecuteChanged();
            RefreshDatabasesCmd.RaiseCanExecuteChanged();
            SetCmd.RaiseCanExecuteChanged();
            UnsetCmd.RaiseCanExecuteChanged();
            BackupCmd.RaiseCanExecuteChanged();
            RestoreSetCmd.RaiseCanExecuteChanged();
            RestoreUnsetCmd.RaiseCanExecuteChanged();
            DeleteDatabaseCmd.RaiseCanExecuteChanged();

        }

        #endregion

        #region Funcs

        public void RefreshDatabases()
        {
            InstanceDb selIns = null;
            NDatabase  selDb  = null;
            if (SelectedInstance != null)
                selIns = SelectedInstance;
            if (SelectedDatabase != null)
                selDb = SelectedDatabase;
            SelectedDatabase = null;
            InstanceList     = new ObservableCollection<InstanceDb>(g.Settings.InstanceList);
            SelectedInstance = selIns == null ? InstanceList.FirstOrDefault() : InstanceList.FirstOrDefault(x => x.DisplayName == selIns.DisplayName);
            SelectedDatabase = selDb  == null ? null : DatabaseList.First(x => x.Id                                            == selDb.Id);
            RaiseCanExecChanged();
        }

        #endregion
    }
}
