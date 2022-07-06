using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MSSQLDatabaseManager.Utils;
using MSSQLDatabaseManager.ViewModels;

namespace MSSQLDatabaseManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Init();
            g.Init();
            base.OnStartup(e);
//#if DEBUG

//#else
//            AppDomain.CurrentDomain.UnhandledException       += CurrentDomain_UnhandledException;
//            Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
//#endif

            var f  = new MainWindow();
            var vm = new MainWindowViewModel();
            f.DataContext   = vm;
            f.ShowDialog();
        }
        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.ErrorQ(e.ExceptionObject as Exception);
        }

        private void CurrentOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Logger.ErrorQ(e.Exception);
        }
    }
}
