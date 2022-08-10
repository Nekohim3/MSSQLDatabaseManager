using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.Practices.Prism.ViewModel;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.Win32;
using MSSQLDatabaseManager.Entities;
using Newtonsoft.Json;

namespace MSSQLDatabaseManager.Utils
{
    public static class SQLService
    {
        public static List<string> GetSQLInstances()
        {
            Logger.Info("GetSQLInstances()");
            try
            {
                var registryView = Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32;
                using (var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, registryView))
                {
                    var instanceKey = hklm.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server\Instance Names\SQL", false);

                    var lst = instanceKey?.GetValueNames().Select(instanceName => instanceName).ToList();
                    Logger.Info("GetSQLInstances() succ");
                    if (lst != null)
                        foreach (var item in lst)
                            Logger.Info(item);

                    return lst;
                }
            }
            catch (Exception ex)
            {
                throw ex;
                Logger.ErrorQ(ex, "GetSQLInstances() -> Exception");
                return null;
            }
        }

        public static List<NDatabase> GetDatabases(string instanceName)
        {
            Logger.Info($"GetDatabases({instanceName})");
            var lst = new List<NDatabase>();
            try
            {
                using (var con = new SqlConnection(g.GetConnString(instanceName)))
                {
                    con.Open();
                    using (var cmd = new SqlCommand("SELECT * from sys.databases", con))
                    using (var dr = cmd.ExecuteReader())
                        while (dr.Read())
                            if (dr[0].ToString() != "master" && dr[0].ToString() != "tempdb" && dr[0].ToString() != "model" && dr[0].ToString() != "msdb" && dr[12].ToString() == "0")
                                lst.Add(new NDatabase(instanceName, dr[0].ToString(), int.Parse(dr[1].ToString()), ""));
                    con.Close();
                }

                return lst;

            }
            catch (Exception ex)
            {
                throw ex;
                Logger.ErrorQ(ex, $"GetDatabases({instanceName}) -> Exception");
                return null;
            }
        }

        public static void UnsetDatabase(NDatabase db)
        {
            using (var con = new SqlConnection(g.GetConnString(db.InstanceName)))
            {
                con.Open();

                new SqlCommand($"ALTER DATABASE {db.Name} SET single_user with rollback immediate;", con).ExecuteNonQuery();
                new SqlCommand($"ALTER DATABASE {db.Name} MODIFY NAME = {db.IdName};",               con).ExecuteNonQuery();
                new SqlCommand($"ALTER DATABASE {db.IdName} SET MULTI_USER;",                        con).ExecuteNonQuery();

                con.Close();
            }
        }

        public static void SetDatabase(NDatabase db)
        {
            using (var con = new SqlConnection(g.GetConnString(db.InstanceName)))
            {
                con.Open();

                new SqlCommand($"ALTER DATABASE {db.Name} SET single_user;",             con).ExecuteNonQuery();
                new SqlCommand($"ALTER DATABASE {db.Name} MODIFY NAME = {db.BaseName};", con).ExecuteNonQuery();
                new SqlCommand($"ALTER DATABASE {db.BaseName} SET MULTI_USER;",          con).ExecuteNonQuery();

                con.Close();
            }
        }

        public static void ClearDatabase(InstanceDb instance, NDatabase db)
        {
            using (var con = new SqlConnection(g.GetConnString(db.InstanceName)))
            {
                con.Open();

                new SqlCommand("EXEC sp_msforeachtable 'ALTER TABLE ? NOCHECK CONSTRAINT all'", con).ExecuteNonQuery();
                new SqlCommand("EXEC sp_msforeachtable 'DELETE FROM ?'", con).ExecuteNonQuery();
                new SqlCommand("EXEC sp_msforeachtable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all'", con).ExecuteNonQuery();

                con.Close();
            }

            ShrinkLogAndFileStream(instance, db);
        }

        public static void ShrinkLogAndFileStream(InstanceDb instance, NDatabase db)
        {
            var conn = new ServerConnection
                       {
                           ServerInstance   = $"{g.CompName}\\{instance.InstanceName}",
                           StatementTimeout = int.MaxValue,
                           LoginSecure      = false,
                           Login            = instance.Login,
                           Password         = instance.Password
                       };

            var srv = new Server(conn);

            var logFilesNames = (from LogFile x in srv.Databases[db.Name].LogFiles select x.Name).ToList();
            conn.Disconnect();

            using (var con = new SqlConnection(g.GetConnString(db.InstanceName, db.Name)))
            {
                con.Open();

                new SqlCommand($"ALTER DATABASE {db.Name} SET RECOVERY SIMPLE", con).ExecuteNonQuery();

                foreach (var x in logFilesNames)
                    new SqlCommand($"DBCC SHRINKFILE ({x}, 5)", con).ExecuteNonQuery();

                for (var i = 0; i < 5; i++)
                    new SqlCommand("EXEC sp_filestream_force_garbage_collection", con).ExecuteNonQuery();

                new SqlCommand($"ALTER DATABASE {db.Name} SET RECOVERY FULL", con).ExecuteNonQuery();

                con.Close();
            }
        }

        

        public static int RestoreDatabase(InstanceDb instance, string bakFilePath, string dataDirPath) 
        {
            Logger.Info("RestoreDatabase()");

            try
            {
                var conn = new ServerConnection
                           {
                               ServerInstance   = $"{g.CompName}\\{instance.InstanceName}",
                               StatementTimeout = int.MaxValue,
                               LoginSecure      = false,
                               Login            = instance.Login,
                               Password         = instance.Password
                           };

                var srv = new Server(conn);

                var res = new Restore();

                var idList = srv.Databases.Cast<Database>().Select(x => x.ID).ToList();
                var id     = 1;
                for (var i = 0; i < idList.Count; i++)
                {
                    if (idList.Contains(id))
                        id++;
                    else
                        break;
                }

                res.Devices.AddDevice(bakFilePath, DeviceType.File);

                res.RelocateFiles.Add(new RelocateFile
                {
                    LogicalFileName = res.ReadFileList(srv).Rows[0][0].ToString(),
                    PhysicalFileName = $"{dataDirPath}\\{id}\\{instance.DatabaseName}.mdf"
                });
                res.RelocateFiles.Add(new RelocateFile
                {
                    LogicalFileName = res.ReadFileList(srv).Rows[1][0].ToString(),
                    PhysicalFileName = $"{dataDirPath}\\{id}\\{instance.DatabaseName}_log.ldf"
                });
                res.RelocateFiles.Add(new RelocateFile
                {
                    LogicalFileName = res.ReadFileList(srv).Rows[2][0].ToString(),
                    PhysicalFileName = $"{dataDirPath}\\{id}\\Filestore"
                });

                if (!Directory.Exists($"{dataDirPath}\\{id}"))
                    Directory.CreateDirectory($"{dataDirPath}\\{id}");

                res.Database                    =  $"{instance.DatabaseName}_{id}";
                res.NoRecovery                  =  false;
                res.ReplaceDatabase             =  true;
                res.PercentCompleteNotification =  1;
                res.PercentComplete             += (sender, args) => { g.LoadingControlVM.LoadingText = $"Restore backup, please wait... [{args.Percent}%]"; };
                res.SqlRestore(srv);
                srv.Refresh();
                conn.Disconnect();
                Logger.Info("RestoreDatabase() succ");
                return id;
            }
            catch (SmoException ex)
            {
                Logger.ErrorQ(ex, "RestoreDatabase -> SmoException");
            }
            catch (IOException ex)
            {
                Logger.ErrorQ(ex, "RestoreDatabase -> IOException");
            }
            catch (Exception ex)
            {
                Logger.ErrorQ(ex, "RestoreDatabase -> Exception");
            }

            return 0;
        }

        public static void DeleteDatabase(InstanceDb instance, List<string> databasesNames)
        {
            foreach (var x in databasesNames)
            {
                DeleteDatabase(instance, x);
            }
        }

        public static void DeleteDatabase(InstanceDb instance, string databaseName)
        {
            g.LoadingControlVM.LoadingText = $"Drop database [{databaseName}]";
            Logger.Info("DeleteDatabase()");

            try
            {
                var conn = new ServerConnection
                           {
                               ServerInstance   = $"{g.CompName}\\{instance.InstanceName}",
                               StatementTimeout = int.MaxValue,
                               LoginSecure      = false,
                               Login            = instance.Login,
                               Password         = instance.Password
                           };

                var srv = new Server(conn);
                srv.KillAllProcesses(databaseName);
                var db = srv.Databases[databaseName];
                
                db.Drop();

                conn.Disconnect();

                Logger.Info("RestoreDatabase() succ");
            }
            catch (SmoException ex)
            {
                throw ex;
                Logger.ErrorQ(ex, "RestoreDatabase -> SmoException");
            }
            catch (IOException ex)
            {
                throw ex;
                Logger.ErrorQ(ex, "RestoreDatabase -> IOException");
            }
            catch (Exception ex)
            {
                throw ex;
                Logger.ErrorQ(ex, "RestoreDatabase -> Exception");
            }
        }



        //public static List<NTable> GetSchema(string instance, string dbName)
        //{

        //}

        //public static List<NTable> GetSchema(NDatabase db)
        //{
        //    return GetSchema(db.InstanceName, db.Name);
        //}
        ////var reader = new SqlCommand(@"SELECT schema_name(tab.schema_id) as schema_name, tab.name as table_name, col.name as column_name, t.name as data_type,
        //            col.max_length, col.precision, col.scale, col.is_nullable, col.is_identity
        //            FROM sys.tables as tab INNER JOIN 
        //                sys.columns as col ON tab.object_id = col.object_id LEFT JOIN 
        //                sys.types as t ON col.user_type_id = t.user_type_id
        //            ORDER BY  table_name", con).ExecuteReader();
        //var lsts = new List<Field>();
        //    while (reader.Read())
        //lsts.Add(new Field()
        //{
        //    Table    = reader["table_name"].ToString(),
        //    Column   = reader["column_name"].ToString(),
        //    DataType = SqlTypeTable.GetType(reader["data_type"].ToString()).FullName,
        //    Nullable = bool.Parse(reader["is_nullable"].ToString())
        //});

        public static bool BackupDatabase(InstanceDb instance, NDatabase db, string filePath)
        {
            //Logger.Info($"BackupDatabase({serverName}, {databaseName}, {filePath}, vm)");
            try
            {
                var conn = new ServerConnection
                {
                    ServerInstance   = $"{g.CompName}\\{instance.InstanceName}",
                    StatementTimeout = int.MaxValue,
                    LoginSecure      = false,
                    Login            = instance.Login,
                    Password         = instance.Password
                };
                var srv = new Server(conn);

                var bkp = new Backup
                {
                    Action = BackupActionType.Database,
                    Database = db.Name
                };

                bkp.Devices.AddDevice(filePath, DeviceType.File);
                bkp.Incremental = false;
                bkp.Initialize  = true;

                bkp.PercentCompleteNotification = 1;
                bkp.PercentComplete += (sender, args) => { g.LoadingControlVM.LoadingText= $"Creating backup, please wait... [{args.Percent}%]"; };
                bkp.SqlBackup(srv);

                conn.Disconnect();
                //Logger.Info($"BackupDatabase({serverName}, {databaseName}, {filePath}, vm) succ");
                return true;
            }

            catch (SmoException ex)
            {
                throw ex;
                Logger.ErrorQ(ex, "BackupDatabase -> SmoException");
                return false;
            }
            catch (IOException ex)
            {
                throw ex;
                Logger.ErrorQ(ex, "BackupDatabase -> IOException");
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
                Logger.ErrorQ(ex, "BackupDatabase -> Exception");
                return false;
            }
        }

        public static List<NTable> GetSchemaFromDatabase(string instance, string db)
        {
            using (var con = new SqlConnection(g.GetConnString(instance, db)))
            {
                con.Open();

                var schema = con.GetSchema("Tables");

                var tableNames = (from DataRow row in schema.Rows select row[2].ToString()).ToList();

                var tables = new List<NTable>();

                foreach (var table in tableNames.OrderBy(x => x).ToList())
                {
                    var reader   = new SqlCommand($"SELECT TOP (1) * FROM {table}", con).ExecuteReader();
                    var sch      = reader.GetSchemaTable();
                    var columnList = new List<NColumn>();
                    for (var j = 0; j < reader.FieldCount; j++)
                    {
                        var type     = ((Type)sch.Rows[j].ItemArray[12]).FullName;
                        var nullable = ((bool)sch.Rows[j].ItemArray[13]);
                        var name     = sch.Rows[j].ItemArray[0].ToString();
                        columnList.Add(new NColumn(name, type, nullable));
                    }

                    tables.Add(new NTable(table, columnList));
                    reader.Close();
                }

                con.Close();
                return tables;
            }
        }
    }
}
