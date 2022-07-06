using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Serilog;

namespace MSSQLDatabaseManager.Utils
{
    public static class Logger
    {
        public static Serilog.Core.Logger Log;

        public static void Init()
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
            if (!Directory.Exists("Logs"))
                Directory.CreateDirectory("Logs");
            Log = new LoggerConfiguration().WriteTo.File($"Logs/App{timestamp}.log").CreateLogger();
            Info($"Logger init at {timestamp}. Version 1.0.2");
        }

        public static void Error(string msg)
        {
            Log.Error(msg);
        }

        public static void Error(Exception e, string msg = "")
        {
            Log.Error(e, msg);
        }

        public static void ErrorQ(string msg)
        {
            Error(msg);
            MessageBox.Show("Возникла ошибка. Приложение будет закрыто. Подробности находятся в лог файле.", "", MessageBoxButton.OK, MessageBoxImage.Error);
            Process.GetCurrentProcess().Kill();
        }

        public static void ErrorQ(Exception e, string msg = "")
        {
            Error(e, msg);
            MessageBox.Show("Возникла ошибка. Приложение будет закрыто. Подробности находятся в лог файле.", "", MessageBoxButton.OK, MessageBoxImage.Error);
            Process.GetCurrentProcess().Kill();
        }

        public static void Info(string msg)
        {
            Log.Information(msg);
        }
    }
}
