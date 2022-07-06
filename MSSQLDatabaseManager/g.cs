using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MSSQLDatabaseManager.UI;
using MSSQLDatabaseManager.Utils;

namespace MSSQLDatabaseManager
{
    public static class g
    {
        public static Settings Settings { get; set; }
        public static string     CompName   => Environment.MachineName;
        public static TabManager TabManager { get; set; }

        public static LoadingControlViewModel LoadingControlVM { get; set; }

        public static void Init()
        {
            Settings   = Settings.Load() ?? new Settings();
            TabManager = new TabManager();
            TabManager.InitTabs();
            //TabManager.OpenSettingsTab();
        }

        public static string GetConnString(string instanceName)
        {
            var cred = Settings.GetCredForInstance(instanceName);
            return $"Data Source={CompName}\\{instanceName};Integrated Security={(cred.Equals(default) ? "True" : $"False;User ID={cred.login};Password={cred.pass}")};";
        }

        public static string GetConnString(string instanceName, string dbName)
        {
            var cred = Settings.GetCredForInstance(instanceName);
            return $"Data Source={CompName}\\{instanceName};Integrated Security={(cred.Equals(default) ? "True" : $"False;User ID={cred.login};Password={cred.pass}")};Initial Catalog={dbName}";
        }
        
        public static void StartLongOperation(Action act, Action fin = null)
        {
            new Thread(() =>
                       {
                           LoadingControlVM.IsVisible   = true;
                           LoadingControlVM.LoadingText = "Prepare operation";
                           act.Invoke();
                           fin?.Invoke();
                           LoadingControlVM.LoadingText += " Done!";
                           Thread.Sleep(1000);
                           LoadingControlVM.IsVisible   =  false;
                           LoadingControlVM.LoadingText = "";
                       }).Start();
        }
    }

    public static class SqlTypeTable
    {
        private static Dictionary<string, Type> Table;

        static SqlTypeTable()
        {
            Table = new Dictionary<string, Type>();
            Table.Add("bigint",         typeof(long));
            Table.Add("binary",         typeof(byte[]));
            Table.Add("bit",            typeof(bool));
            Table.Add("char",           typeof(string));
            Table.Add("date",           typeof(DateTime));
            Table.Add("datetime",       typeof(DateTime));
            Table.Add("datetime2",      typeof(DateTime));
            Table.Add("datetimeoffset", typeof(DateTimeOffset));
            Table.Add("decimal",        typeof(decimal));
            Table.Add("varbinary(max)", typeof(byte[]));
            Table.Add("float",          typeof(double));
            Table.Add("image",          typeof(byte[]));
            Table.Add("int",            typeof(int));
            Table.Add("money",          typeof(decimal));
            Table.Add("nchar",          typeof(string));
            Table.Add("ntext",          typeof(string));
            Table.Add("numeric",        typeof(decimal));
            Table.Add("nvarchar",       typeof(string));
            Table.Add("real",           typeof(float));
            Table.Add("rowversion",     typeof(byte[]));
            Table.Add("smalldatetime",  typeof(DateTime));
            Table.Add("smallint",       typeof(short));
            Table.Add("smallmoney",     typeof(decimal));
            Table.Add("sql_variant",               typeof(object));
            Table.Add("text",               typeof(string));
            Table.Add("time",               typeof(TimeSpan));
            Table.Add("timestamp",               typeof(byte[]));
            Table.Add("tinyint",               typeof(byte));
            Table.Add("uniqueidentifier",               typeof(Guid));
            Table.Add("varbinary",               typeof(byte[]));
            Table.Add("varchar",               typeof(string));
        }

        public static Type GetType(string key) => Table[key];
    }
}
