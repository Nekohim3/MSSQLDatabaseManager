using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Microsoft.Practices.Prism.Commands;
using Microsoft.Practices.Prism.ViewModel;
using MSSQLDatabaseManager.ViewModels;
using MSSQLDatabaseManager.Views;

namespace MSSQLDatabaseManager.Utils
{
    public enum TabType
    {
        DbManager = 0,
        DbDataCopy    = 1,
        Settings = 99
    }

    public class Tab : NotificationObject
    {
        private string _tabName;

        public string TabName
        {
            get => _tabName;
            set
            {
                _tabName = value;
                RaisePropertyChanged(() => TabName);
            }
        }

        private bool _isClosable;

        public bool IsClosable
        {
            get => _isClosable;
            set
            {
                _isClosable = value;
                RaisePropertyChanged(() => IsClosable);
            }
        }

        private Page _page;

        public Page Page
        {
            get => _page;
            set
            {
                _page = value;
                RaisePropertyChanged(() => Page);
            }
        }

        public DelegateCommand CloseCmd { get; }

        public Tab(string tabName, TabType type, bool isClosable = false)
        {
            TabName    = tabName;
            IsClosable = isClosable;
            if (type == TabType.DbManager)
            {
                Page             = new DBManagerView();
                Page.DataContext = new DbManagerViewModel();
            }
            if (type == TabType.DbDataCopy)
            {
                Page             = new DBCopyManagerView();
                Page.DataContext = new DBCopyManagerViewModel();
            }
            if (type == TabType.Settings)
            {
                Page             = new SettingsView();
                Page.DataContext = new SettingsViewModel();
            }
            //CloseCmd = new DelegateCommand(OnClose);
        }

        private void OnClose()
        {
            //g.TabManager.CloseTab(this);
        }
    }
}
