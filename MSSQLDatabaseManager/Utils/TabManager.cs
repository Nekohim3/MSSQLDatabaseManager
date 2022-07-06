using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Prism.ViewModel;
using MSSQLDatabaseManager.ViewModels;

namespace MSSQLDatabaseManager.Utils
{
    public class TabManager : NotificationObject
    {
        private ObservableCollection<Tab> _tabList;

        public ObservableCollection<Tab> TabList
        {
            get => _tabList;
            set
            {
                _tabList = value;
                RaisePropertyChanged(() => TabList);
            }
        }

        private Tab _selectedTab;

        public Tab SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                RaisePropertyChanged(() => SelectedTab);
                ((DbManagerViewModel)g.TabManager.TabList[0].Page.DataContext).SelectedDatabase = null;
            }
        }

        //public bool AnyRunning => TabList.Any(x => x.IsAccount && x.Account.Running.HasValue);
        //public bool AllStopped => TabList.Where(x => x.IsAccount).All(x => !x.Account.Running.HasValue);

        public TabManager()
        {
            TabList = new ObservableCollection<Tab>();
        }

        public void InitTabs()
        {
            TabList.Add(new Tab("Db manager",   TabType.DbManager));
            TabList.Add(new Tab("Db data copy", TabType.DbDataCopy));
            TabList.Add(new Tab("Settings", TabType.Settings));
            SelectedTab = TabList.FirstOrDefault();
        }

        //public void OpenSettingsTab()
        //{
        //    TabList.Add(new Tab(null, "+"));
        //    SelectedTab = TabList.FirstOrDefault();
        //}

        //public void OpenTab(Account acc, bool sw = false, bool auto = false)
        //{
        //    var ids = TabList.Where(x => x.IsAccount).Select(x => x.Account.Id).ToList();
        //    for (var i = 0; i <= ids.Count; i++)
        //    {
        //        if (i < ids.Count)
        //        {
        //            if (acc.Id < ids[i])
        //            {
        //                TabList.Insert(i, new Tab(acc));
        //                break;
        //            }
        //        }
        //        else
        //        {
        //            TabList.Insert(TabList.Count - 1, new Tab(acc));
        //        }
        //    }
        //    if (sw)
        //        SelectedTab = TabList.FirstOrDefault(x => x.Account == acc);
        //    //acc.Start(auto);
        //    RaisePropertyChanged(() => AnyRunning);
        //    RaisePropertyChanged(() => AllStopped);
        //}

        //public void CloseTab(Tab tab)
        //{
        //    if (!tab.IsAccount) return;
        //    if (tab.Account.Closing) return;
        //    var ind = TabList.IndexOf(tab);
        //    tab.Account.Stop();
        //    TabList.Remove(tab);
        //    SelectedTab = ind == 0 ? TabList[0] : TabList[ind - 1];
        //    RaisePropertyChanged(() => AnyRunning);
        //    RaisePropertyChanged(() => AllStopped);
        //}
    }
}
