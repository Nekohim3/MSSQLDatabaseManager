﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Forms;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.WindowsAPICodePack.Dialogs;
using MSSQLDatabaseManager.Entities;
using MSSQLDatabaseManager.UI;
using MSSQLDatabaseManager.Utils;
using Application = System.Windows.Application;

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
                    var dbList      = SQLService.GetDatabases(_selectedInstance.InstanceName).Where(x => x.Name.StartsWith(_selectedInstance.DatabaseName)).ToList();
                    var usedDb      = dbList.FirstOrDefault(x => x.Name == _selectedInstance.DatabaseName);
                    if (usedDb != null)
                        usedDb.IsUsed = true;
                    var savedDbList = g.Settings.GetSavedDbList();
                    Application.Current.Dispatcher.Invoke(() =>
                                                          {
                                                              DatabaseList = new ObservableCollection<NDatabase>();
                                                              foreach (var x in dbList)
                                                                  DatabaseList.Add(new NDatabase(_selectedInstance.InstanceName, x.Name, x.Id,
                                                                                                 savedDbList.FirstOrDefault(c => c.InstanceName == x.InstanceName && c.Id == x.Id)?.Comment, x.IsUsed));
                                                          });
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

        public DelegateCommand SetCmd               { get; }
        public DelegateCommand UnsetCmd             { get; }
        public DelegateCommand BackupCmd            { get; }
        public DelegateCommand RestoreSetCmd        { get; }
        public DelegateCommand RestoreInsCmd        { get; }
        public DelegateCommand RestoreUnsetCmd      { get; }
        public DelegateCommand CopyDatabaseSetCmd   { get; }
        public DelegateCommand CopyDatabaseUnsetCmd { get; }
        //public DelegateCommand         DeleteDatabaseCmd    { get; }
        public DelegateCommand         TestCmd              { get; }
        public DelegateCommand<object> DeleteDatabaseCmd    { get; }

        #endregion

        #region Ctor

        public DbManagerViewModel()
        {
            AddDatabaseCmd    = new DelegateCommand(OnAddDatabase,    () => !AddInstanceMode);
            RemoveDatabaseCmd = new DelegateCommand(OnRemoveDatabase, () => SelectedInstance != null && !AddInstanceMode);

            ConfirmAddDatabaseCmd = new DelegateCommand(OnConfirmAddDatabase, () => AddInstanceMode && SelectedAddDatabase != null);
            CancelAddDatabaseCmd  = new DelegateCommand(OnCancelAddDatabase,  () => AddInstanceMode);

            RefreshDatabasesCmd = new DelegateCommand(OnRefreshDatabases, () => SelectedInstance != null);

            SetCmd               = new DelegateCommand(OnSet,    () => SelectedDatabase != null && SelectedDatabase.Name != SelectedInstance.DatabaseName);
            UnsetCmd             = new DelegateCommand(OnUnset,  () => SelectedDatabase != null && SelectedDatabase.Name == SelectedInstance.DatabaseName);
            BackupCmd            = new DelegateCommand(OnBackup, () => SelectedDatabase != null);
            RestoreSetCmd        = new DelegateCommand(OnRestoreSet);
            RestoreInsCmd        = new DelegateCommand(OnRestoreIns, () => SelectedDatabase != null);
            RestoreUnsetCmd      = new DelegateCommand(OnRestoreUnset);
            CopyDatabaseSetCmd   = new DelegateCommand(OnCopyDatabaseSet,   () => SelectedDatabase       != null);
            CopyDatabaseUnsetCmd = new DelegateCommand(OnCopyDatabaseUnset, () => SelectedDatabase       != null);
            DeleteDatabaseCmd    = new DelegateCommand<object>(OnDeleteDatabase, (x) => SelectedDatabase != null);

            TestCmd = new DelegateCommand(OnTest);

            RefreshDatabases();
        }

        #endregion

        #region CmdExec

        private void OnTest()
        {

        }

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
            g.StartLongOperation(() =>
                                 {
                                     g.LoadingControlVM.LoadingText = $"Set database [{SelectedInstance.DatabaseName}]";
                                     var usedDb = DatabaseList.FirstOrDefault(x => x.IsUsed);
                                     if (usedDb != null)
                                         SQLService.UnsetDatabase(usedDb);
                                     SQLService.SetDatabase(SelectedDatabase);
                                     RefreshDatabases();
                                 });
        }

        private void OnUnset()
        {
            g.StartLongOperation(() =>
                                 {
                                     g.LoadingControlVM.LoadingText = $"Unset database [{SelectedInstance.DatabaseName}]";
                                     SQLService.UnsetDatabase(SelectedDatabase);
                                     RefreshDatabases();
                                 });
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
                          DefaultFileName  = ReplaceInvalidChars($"{SelectedDatabase.IdName}{(!string.IsNullOrEmpty(SelectedDatabase.Comment) ? $"-{SelectedDatabase.Comment}" : "")}"),
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
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\DbData"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\DbData");
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/", "\\")}"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/", "\\")}");
            var ofd = new CommonOpenFileDialog()
            {
                Title = $"Select backup for restore {SelectedInstance.DatabaseName} type database",
                IsFolderPicker = false,
                InitialDirectory = $"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\",
                Filters = { new CommonFileDialogFilter("MSSQL DB Backup", "bak") }
            };

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                g.StartLongOperation(() =>
                {
                    var id = SQLService.RestoreDatabase(SelectedInstance, ofd.FileName,
                                               $"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/", "\\")}");
                    RefreshDatabases();
                    g.LoadingControlVM.LoadingText += $" Done!\nSet database [{SelectedInstance.DatabaseName}]...";
                    var usedDb = DatabaseList.FirstOrDefault(x => x.IsUsed);
                    if (usedDb != null)
                        SQLService.UnsetDatabase(usedDb);
                    SQLService.SetDatabase(DatabaseList.FirstOrDefault(x => x.Id == id));
                    RefreshDatabases();
                });
            }
        }

        private void OnRestoreIns()
        {
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\DbData"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\DbData");
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/",         "\\")}"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/", "\\")}");
            var ofd = new CommonOpenFileDialog()
                      {
                          Title            = $"Select backup for restore {SelectedInstance.DatabaseName} type database",
                          IsFolderPicker   = false,
                          InitialDirectory = $"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\",
                          Filters          = { new CommonFileDialogFilter("MSSQL DB Backup", "bak") }
                      };

            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (g.MsgShow($"Restore backup in selected database [{SelectedDatabase.Name}]?", "Restore confirmation", NMsgButtons.YesNo) == NMsgReply.Yes)
                    g.StartLongOperation(() =>
                                         {
                                             var set = false;
                                             if (SelectedDatabase.IsUsed)
                                                 set = true;
                                             SQLService.DeleteDatabase(SelectedInstance, SelectedDatabase.Name); 
                                             var id = SQLService.RestoreDatabase(SelectedInstance, ofd.FileName,
                                              $"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/", "\\")}");
                                             if (set)
                                             {
                                                 RefreshDatabases();
                                                 g.LoadingControlVM.LoadingText += $" Done!\nSet database [{SelectedInstance.DatabaseName}]...";
                                                 var usedDb = DatabaseList.FirstOrDefault(x => x.IsUsed);
                                                 if (usedDb != null)
                                                     SQLService.UnsetDatabase(usedDb);
                                                 SQLService.SetDatabase(DatabaseList.FirstOrDefault(x => x.Id == id));
                                             }
                                             RefreshDatabases();
                                         });
            }
            
        }



        private void OnRestoreUnset()
        {
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\DbData"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\DbData");
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/",         "\\")}"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/", "\\")}");
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
                                         SQLService.RestoreDatabase(SelectedInstance, ofd.FileName,
                                                                    $"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/", "\\")}");
                                         RefreshDatabases();
                                     });
            }
        }

        private void OnCopyDatabaseUnset()
        {
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\Backups"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\Backups");
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}");
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp");
            g.StartLongOperation(() =>
            {
                var sel = SelectedDatabase;
                SQLService.BackupDatabase(SelectedInstance, SelectedDatabase,
                                          $"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp\\temp.bak");
                var id = SQLService.RestoreDatabase(SelectedInstance, $"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp\\temp.bak",
                                           $"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/", "\\")}");
                RefreshDatabases();
                var db = DatabaseList.FirstOrDefault(x => x.Id == id);
                db.Comment = $"[copied from {sel.Id}] {sel.Comment}";
                g.Settings.CheckDb(db);
            },
                                 () =>
                                 {
                                     if (File.Exists($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp\\temp.bak"))
                                         File.Delete($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp\\temp.bak");
                                     if (Directory.Exists($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp"))
                                         Directory.Delete($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp");
                                 });
        }

        private void OnCopyDatabaseSet()
        {
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\Backups"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\Backups");
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/",         "\\")}"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}");
            if (!Directory.Exists($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/",         "\\")}\\Temp"))
                Directory.CreateDirectory($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp");
            g.StartLongOperation(() =>
                                 {
                                     var sel = SelectedDatabase;
                                     SQLService.BackupDatabase(SelectedInstance, SelectedDatabase,
                                                               $"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp\\temp.bak");
                                     var id = SQLService.RestoreDatabase(SelectedInstance, $"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp\\temp.bak",
                                                                         $"{g.Settings.DirForDbData}\\DbData\\{SelectedInstance.DisplayName.Replace("/",                    "\\")}");
                                     RefreshDatabases();
                                     var db = DatabaseList.FirstOrDefault(x => x.Id == id);
                                     db.Comment = $"[copied from {sel.Id}] {sel.Comment}";
                                     g.Settings.CheckDb(db);
                                     g.LoadingControlVM.LoadingText += $" Done!\nSet database [{SelectedInstance.DatabaseName}]...";
                                     var usedDb = DatabaseList.FirstOrDefault(x => x.IsUsed);
                                     if (usedDb != null)
                                         SQLService.UnsetDatabase(usedDb);
                                     SQLService.SetDatabase(DatabaseList.FirstOrDefault(x => x.Id == id));
                                     RefreshDatabases();
                                 },
                                 () =>
                                 {
                                     if (File.Exists($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp\\temp.bak"))
                                         File.Delete($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp\\temp.bak");
                                     if (Directory.Exists($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp"))
                                         Directory.Delete($"{g.Settings.DirForDbData}\\Backups\\{SelectedInstance.DisplayName.Replace("/", "\\")}\\Temp");
                                 });
        }

        private void OnDeleteDatabase(object selectedItems)
        {
            var lst = (selectedItems as ObservableCollection<object>).Cast<NDatabase>().ToList();
            var str = string.Join("\n", lst.Select(x => x.Name));
            if (g.MsgShow($"Delete selected databases?\n{str}", "Delete confirmation", NMsgButtons.YesNo) == NMsgReply.Yes)
                g.StartLongOperation(() =>
                                     {
                                         
                                         SQLService.DeleteDatabase(SelectedInstance, lst.Select(x => x.Name).ToList());
                                         RefreshDatabases();
                                     });
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
            CopyDatabaseSetCmd.RaiseCanExecuteChanged();
            RestoreInsCmd.RaiseCanExecuteChanged();
        }

        #endregion

        #region Funcs

        public string ReplaceInvalidChars(string filename)
        {
            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

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
            SelectedDatabase = selDb  == null ? null : DatabaseList.FirstOrDefault(x => x.Id                                            == selDb.Id);
            g.Settings.CheckDbList(DatabaseList.ToList());
            RaiseCanExecChanged();
        }

        #endregion
    }
}
