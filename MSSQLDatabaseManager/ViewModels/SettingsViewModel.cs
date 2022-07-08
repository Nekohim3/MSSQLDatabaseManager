using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private bool _darkThemeChecked;

        public bool DarkThemeChecked
        {
            get => _darkThemeChecked;
            set
            {
                if (!value && !_whiteThemeChecked) return;
                _darkThemeChecked = value;
                RaisePropertyChanged(() => DarkThemeChecked);
                if (_darkThemeChecked)
                {
                    WhiteThemeChecked = false;
                    (App.Current as App).ChangeSkin(Skin.Dark);
                    g.Settings.Theme = Skin.Dark;
                    g.Settings.Save();
                }
            }
        }

        private bool _whiteThemeChecked;

        public bool WhiteThemeChecked
        {
            get => _whiteThemeChecked;
            set
            {
                if(!value && !_darkThemeChecked) return;
                _whiteThemeChecked = value;
                RaisePropertyChanged(() => WhiteThemeChecked); 
                if (_whiteThemeChecked)
                {
                    DarkThemeChecked = false;
                    (App.Current as App).ChangeSkin(Skin.White);
                    g.Settings.Theme = Skin.White;
                    g.Settings.Save();
                }
            }
        }

        #endregion

        #region Commands

        public DelegateCommand BrowsePathCmd { get; }
        public DelegateCommand SavePathCmd   { get; }
        public DelegateCommand OpenDataPathCmd   { get; }

        public DelegateCommand SaveAllCmd { get; }

        #endregion

        #region Ctor

        public SettingsViewModel()
        {
            BrowsePathCmd   = new DelegateCommand(OnBrowse);
            SavePathCmd     = new DelegateCommand(OnSavePath, () => PathToData != g.Settings.DirForDbData);
            OpenDataPathCmd = new DelegateCommand(OnOpenDataPath);

            SaveAllCmd = new DelegateCommand(OnSaveAll);

            PathToData = g.Settings.DirForDbData;
            if (App.Skin == Skin.Dark)
                _darkThemeChecked = true;
            else
                _whiteThemeChecked = true;
            RaisePropertyChanged(() => DarkThemeChecked);
            RaisePropertyChanged(() => WhiteThemeChecked);
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

        private void OnOpenDataPath()
        {
            Process.Start(g.Settings.DirForDbData);
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
