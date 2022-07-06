using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace MSSQLDatabaseManager.ViewModels
{
    public class SettingsViewModel : NotificationObject
    {
        #region Properties

        private string _pathToData;

        public string PathToData
        {
            get => _pathToData;
            set
            {
                _pathToData = value;
                RaisePropertyChanged(() => PathToData);
                RaiseCanExecChanged();
            }
        }

        #endregion

        #region Commands

        public DelegateCommand BrowsePathCmd { get; }
        public DelegateCommand SavePathCmd { get; }

        public DelegateCommand SaveAllCmd { get; }

        #endregion

        #region Ctor

        public SettingsViewModel()
        {
            BrowsePathCmd = new DelegateCommand(OnBrowse);
            SavePathCmd   = new DelegateCommand(OnSavePath, () => PathToData != g.Settings.DirForDbData);

            SaveAllCmd = new DelegateCommand(OnSaveAll);

            PathToData = g.Settings.DirForDbData;
        }

        #endregion

        #region CmdExec

        private void OnBrowse()
        {
            var ofd = new CommonOpenFileDialog()
                      {
                          IsFolderPicker = true,
                          Multiselect = false,
                          DefaultDirectory = "C:/"
                      };
            if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (Directory.Exists(ofd.FileName))
                    PathToData = ofd.FileName;
                else
                    MessageBox.Show("Папки не существует!");
            }
        }

        private void OnSavePath()
        {
            g.Settings.SetDir(PathToData);
            RaiseCanExecChanged();
        }

        private void OnSaveAll()
        {
            OnSavePath();
        }

        private void RaiseCanExecChanged()
        {
            BrowsePathCmd.RaiseCanExecuteChanged();
            SavePathCmd.RaiseCanExecuteChanged();
            SaveAllCmd.RaiseCanExecuteChanged();
        }

        #endregion

        #region Funcs

        

        #endregion

    }
}
