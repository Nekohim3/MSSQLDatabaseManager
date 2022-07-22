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

        private bool _darkRedThemeChecked;

        public bool DarkRedThemeChecked
        {
            get => _darkRedThemeChecked;
            set
            {
                if(!value && !_darkBlueThemeChecked && !_lightRedThemeChecked && !_lightBlueThemeChecked) return;
                _darkRedThemeChecked = value;
                RaisePropertyChanged(() => DarkRedThemeChecked);
                if (_darkRedThemeChecked)
                {
                    DarkBlueThemeChecked  = false;
                    LightBlueThemeChecked = false;
                    LightRedThemeChecked  = false;
                    (App.Current as App).ChangeSkin(Skin.DarkRed);
                    g.Settings.Theme = Skin.DarkRed;
                    g.Settings.Save();
                }
            }
        }

        private bool _darkBlueThemeChecked;

        public bool DarkBlueThemeChecked
        {
            get => _darkBlueThemeChecked;
            set
            {
                if (!value && !_darkRedThemeChecked && !_lightRedThemeChecked && !_lightBlueThemeChecked) return;
                _darkBlueThemeChecked = value;
                RaisePropertyChanged(() => DarkBlueThemeChecked);
                if (_darkBlueThemeChecked)
                {
                    DarkRedThemeChecked   = false;
                    LightBlueThemeChecked = false;
                    LightRedThemeChecked  = false;
                    (App.Current as App).ChangeSkin(Skin.DarkBlue);
                    g.Settings.Theme = Skin.DarkBlue;
                    g.Settings.Save();
                }
            }
        }

        private bool _lightRedThemeChecked;

        public bool LightRedThemeChecked
        {
            get => _lightRedThemeChecked;
            set
            {
                if (!value && !_darkBlueThemeChecked && !_darkRedThemeChecked && !_lightBlueThemeChecked) return;
                _lightRedThemeChecked = value;
                RaisePropertyChanged(() => LightRedThemeChecked);
                if (_lightRedThemeChecked)
                {
                    DarkBlueThemeChecked  = false;
                    LightBlueThemeChecked = false;
                    DarkRedThemeChecked   = false;
                    (App.Current as App).ChangeSkin(Skin.LightRed);
                    g.Settings.Theme = Skin.LightRed;
                    g.Settings.Save();
                }
            }
        }

        private bool _lightBlueThemeChecked;

        public bool LightBlueThemeChecked
        {
            get => _lightBlueThemeChecked;
            set
            {
                if (!value && !_darkBlueThemeChecked && !_lightRedThemeChecked && !_darkRedThemeChecked) return;
                _lightBlueThemeChecked = value;
                RaisePropertyChanged(() => LightBlueThemeChecked);
                if (_lightBlueThemeChecked)
                {
                    DarkBlueThemeChecked = false;
                    DarkRedThemeChecked  = false;
                    LightRedThemeChecked = false;
                    (App.Current as App).ChangeSkin(Skin.LightBlue);
                    g.Settings.Theme = Skin.LightBlue;
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
            if (App.Skin == Skin.DarkRed)
                _darkRedThemeChecked = true;
            else if (App.Skin == Skin.DarkBlue)
                _darkBlueThemeChecked = true;
            else if (App.Skin == Skin.LightRed)
                _lightRedThemeChecked = true;
            else
                _lightBlueThemeChecked = true;
            RaisePropertyChanged(() => DarkRedThemeChecked);
            RaisePropertyChanged(() => DarkBlueThemeChecked);
            RaisePropertyChanged(() => LightRedThemeChecked);
            RaisePropertyChanged(() => LightBlueThemeChecked);
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
