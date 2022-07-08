using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using MSSQLDatabaseManager.UI;
using MSSQLDatabaseManager.Utils;
using MSSQLDatabaseManager.ViewModels;

namespace MSSQLDatabaseManager
{
    public enum Skin { White, Dark }
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Skin Skin { get; set; } = Skin.Dark;
        protected override void OnStartup(StartupEventArgs e)
        {
            Logger.Init();
            g.Init();
            base.OnStartup(e);

#if DEBUG

#else
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Application.Current.DispatcherUnhandledException += CurrentOnDispatcherUnhandledException;
#endif

            var f  = new MainWindow();
            var vm = new MainWindowViewModel();
            f.DataContext   = vm;
            f.ShowDialog();
            
        }

        public void ChangeSkin(Skin newSkin)
        {
            Skin = newSkin;

            foreach (ResourceDictionary dict in Resources.MergedDictionaries)
            {

                if (dict is SkinResourceDictionary skinDict)
                    skinDict.UpdateSource();
                else
                    dict.Source = dict.Source;
            }
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
