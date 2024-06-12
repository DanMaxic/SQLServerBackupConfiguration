using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo.Agent;
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Dac;

namespace SQLServerBackupConfiguration
{   
    class Program
    {
        public static string SQLServerAdress = null;
        public static string BackupPath = SQLServerBackupConfiguration.Properties.Settings.Default.DefualtStorePath+SQLServerBackupConfiguration.Properties.Settings.Default.DefualtStoreFormat;

        
        public static string ServerName = "";
        public static string P_ServerName = "";

        public static bool P_Trusted = false;
        public static string P_Username = null;
        public static string P_Password = null;

        static void Main(string[] args)
        {
            //Arange ArgsDefualt
            foreach (string arg in args)
            {
                if (arg.ToLower().Contains("-srv=") || (arg.ToLower().Contains("-s=")))
                {
                    P_ServerName = (arg.Remove(0, arg.IndexOf('=') + 1));
                }
                if (arg.ToLower().Contains("-password=") || (arg.ToLower().Contains("-p=")))
                {
                    P_Password = arg.Remove(0, arg.IndexOf('=') + 1);
                }
                if (arg.ToLower().Contains("-user=") || (arg.ToLower().Contains("-u=")))
                {
                    P_Username = arg.Remove(0, arg.IndexOf('=') + 1);
                }
                if (arg.ToLower().Contains("-trusted=") || (arg.ToLower().Contains("-t=")))
                {
                    P_Trusted = true;
                }
            }

            //constract ConnectionString
            {
                SQLServerAdress = "";
                //Server
                if (string.IsNullOrEmpty(P_ServerName))
                {
                    SQLServerAdress = SQLServerAdress + SQLServerBackupConfiguration.Properties.Settings.Default.DefualtServerConnectionString;
                }
                else
                {
                    SQLServerAdress = SQLServerAdress + string.Format("Server={0};", P_ServerName);
                }
                //Username
                if (string.IsNullOrEmpty(P_Username))
                {
                    SQLServerAdress = SQLServerAdress + SQLServerBackupConfiguration.Properties.Settings.Default.DefualtAuthnticationConnectionString;
                }
                else
                {
                    SQLServerAdress = SQLServerAdress + string.Format("User Id={0};", P_Username);
                }
                //Pasword
                if (string.IsNullOrEmpty(P_Password))
                {
                }
                else
                {
                    SQLServerAdress = SQLServerAdress + string.Format("Password={0};", P_Password);
                }
                if (P_Trusted)
                {
                    SQLServerAdress = SQLServerAdress + "Trusted_Connection=True;";
                }


            }

            {
                ServerName = GetServerName(SQLServerAdress);

                //Fix BackupPath
                string CurrentDate = DateTime.Now.ToString("yyyyMMddHHmm");
                BackupPath = BackupPath.Replace("{SERVERNAME}", ServerName);
                BackupPath = BackupPath.Replace("{DATE}", CurrentDate);
                Println(string.Format("Conneced to :{0}", ServerName));
                Println(string.Format("Backing up to to :{0}", BackupPath));

                if (!Directory.Exists(BackupPath))
                {
                    Println("Creating folder :" + BackupPath);
                    Directory.CreateDirectory(BackupPath);
                }

                try
                {
                    Server SRVER = new Server(new ServerConnection(new System.Data.SqlClient.SqlConnection(SQLServerAdress)));
                    Println("" + SRVER.EngineEdition);
                }
                catch (Exception ex)
                {
                    Println("Failed to connect to server,exiting" );
                    Environment.ExitCode = -1;
                    return;
                }


                //Server Components
                Println("Capturing server Components");
                CaptureServerComponents(SQLServerAdress, BackupPath);
                //Agent components
                Println("Capturing server Agent components");
                CaptureSQLAgent(SQLServerAdress, BackupPath);
                //Databases
                Println("Capturing server Agent components");
                CaptureDatabases(SQLServerAdress, BackupPath);


                Println("\n\nConfiguration backup finished");
            }

        }

        static string GetServerName(string SQLServerAdress)
        {
            string rtn = "";
            {
                Server  SRVER = new Server( new ServerConnection(new System.Data.SqlClient.SqlConnection(SQLServerAdress)));
                rtn = SRVER.Name;

                
            }
            return rtn;
        }

        static void CaptureServerComponents(string serverAdress, string BackupPath)
        {
            string EmbeddedPath = BackupPath + @"\SERVER\";
            if (!Directory.Exists(EmbeddedPath))
                Directory.CreateDirectory(EmbeddedPath);
            Server SRVER = new Server(new ServerConnection(new System.Data.SqlClient.SqlConnection(SQLServerAdress)));
           
                //ServerConfiguration
                {

                    Println("   Capturing server ServerConfiguration");
                    string Script = "";
                    Script += string.Format("use [master]; \n\n exec sp_configure '{0}',{1};", "show advanced options", 1) + "\n" + "RECONFIGURE WITH OVERRIDE \n";

                    foreach (ConfigProperty p in SRVER.Configuration.Properties)
                    {
                        Script += string.Format("exec sp_configure '{0}',{1};", p.DisplayName, p.RunValue) + "\n";
                        //p.DisplayName + " [" + p.RunValue.ToString() + "]");
                    }
                    WriteScriptToFile(Script, EmbeddedPath + "ServerSettings.sql");
                }


                //Endpoints
                {
                    Println("   Capturing server Endpoints");
                    string Script = "";

                    foreach (Endpoint endPoint in SRVER.Endpoints)
                    {
                        if (endPoint.IsSystemObject == false)
                        {
                            foreach (string str in endPoint.Script())
                            {
                                Script += str + "\n";
                            }
                        }
                    }
                    WriteScriptToFile(Script, EmbeddedPath + "Endpoints.sql");
                }
                //Credentials
                {
                    Println("   Capturing server Credentials");
                    string Script = "";

                    foreach (Credential cred in SRVER.Credentials)
                    {
                        Script += (String.Format(@"IF NOT EXISTS(select * from master.sys.credentials where [name] = '{0}')
    CREATE CREDENTIAL [{0}] WITH IDENTITY = N'{1}', SECRET = N'___password___'", cred.Name, cred.Identity)) + "\n";
                    }
                    WriteScriptToFile(Script, EmbeddedPath + "Credentials.sql");
                }

                //Audits
                try
                {
                    Println("   Capturing server Audits");
                    string Script = "";

                    Println("Scripting Audits...");

                    foreach (Audit audit in SRVER.Audits)
                    {
                        foreach (string str in audit.Script())
                        {
                            Script += str + "\n";
                        }
                    }

                    foreach (ServerAuditSpecification serverAudit in SRVER.ServerAuditSpecifications)
                    {
                        foreach (string str in serverAudit.Script())
                        {
                            Script += str + "\n";
                        }
                    }


                    foreach (Audit audit in SRVER.Audits)
                    {
                        if (audit.Enabled)
                        {
                            string enableStatement = String.Format(@"ALTER SERVER AUDIT [{0}] WITH (STATE = ON);\n", audit.Name);
                            Script += enableStatement + "\n";
                        }

                    }
                    WriteScriptToFile(Script, EmbeddedPath + "ServerAudits.sql");
                }
                catch (Exception ex)
                {
                }

                //LinkedServer
                {
                    Println("   Capturing server LinkedServer");
                    string Script = "";

                    Println("Scripting LinkedServers...");

                    foreach (LinkedServer audit in SRVER.LinkedServers)
                    {
                        foreach (string str in audit.Script())
                        {
                            Script += str + "\n";
                        }
                    }
                    WriteScriptToFile(Script, EmbeddedPath + "LinkedServers.sql");
                }
                //Logins
                {
                    Println("   Capturing server Logins");
                    string Script = "";

                    using (SqlCommand cmd = new SqlCommand(SQLServerBackupConfiguration.Properties.Settings.Default.CaptureLoginScripts, new SqlConnection(serverAdress)))
                    {
                        cmd.Connection.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Script += reader.GetString(0) + "\n";
                            }
                        }
                    }
                    WriteScriptToFile(Script, EmbeddedPath + "logins.sql");
                }
            
            

        }
        
        static void CaptureSQLAgent(string serverAdress, string BackupPath)
        {
            string EmbeddedPath = BackupPath + @"\Agent\";
            if (!Directory.Exists(EmbeddedPath))
                Directory.CreateDirectory(EmbeddedPath);
            Server SRVER = new Server(new ServerConnection(new System.Data.SqlClient.SqlConnection(SQLServerAdress)));

            ///Jobs Backup
            {
                Println("   Capturing SQL Agent Jobs");
                string Script = "";
                foreach (Job job in SRVER.JobServer.Jobs)
                {
                    string script = "";
                    foreach (string str in job.Script())
                    {
                        script += str;
                    }
                    Script += script + "\n";
                }

                WriteScriptToFile(Script, EmbeddedPath + "SQLAgent_Jobs.sql");
            }
            ///JobAgent Alerts
            {
                Println("   Capturing SQL Agent Alerts");

                string Script = "";
                foreach (Alert job in SRVER.JobServer.Alerts)
                {
                    //Println(string.Format("Scripting job:{0}", job.Name));
                        string script = "";
                    foreach (string str in job.Script())
                    {
                        script += str;
                    }
                    Script += script + "\n";
                }

                WriteScriptToFile(Script, EmbeddedPath + "Alerts.sql");
            }
            ///JobAgent Operators
            {
                Println("   Capturing SQL Agent Operators");

                string Script = "";
                foreach (Operator job in SRVER.JobServer.Operators)
                {
                    string script = "";
                    foreach (string str in job.Script())
                    {
                        script += str;
                    }
                    Script += script + "\n";
                }

                WriteScriptToFile(Script, EmbeddedPath + "Operators.sql");
            }

            // Script Proxies
            {
                Println("   Capturing SQL Agent Proxies Accounts");
                string Script = "";
                foreach (ProxyAccount a in SRVER.JobServer.ProxyAccounts)
                {
                    string script = "";
                    foreach (string str in a.Script())
                    {
                        script += str;
                    }
                    Script += script + "\n";
                }
                WriteScriptToFile(Script, EmbeddedPath + "Proxies.sql");
            }
        }

        static void CaptureDatabases(string serverAdress, string BackupPath)
        {
            string EmbeddedPath = BackupPath + @"\Databases\";
            if (!Directory.Exists(EmbeddedPath))
                Directory.CreateDirectory(EmbeddedPath);
            Server SRVER = new Server(new ServerConnection(new System.Data.SqlClient.SqlConnection(SQLServerAdress)));

            foreach(Database DB in SRVER.Databases)
            {
                if(!DB.IsSystemObject)
                {
                    Println(string.Format("   Capturing database {0}",DB.Name));
                    try
                    {
                        CaptureDatabase(serverAdress, DB.Name, EmbeddedPath);
                    }
                    catch (Exception ex)
                    {
                        Println(string.Format("   Capture database {0}, failed exception is: {1}", DB.Name,ex.Message));
                    }
                }
            }
        }

        static void CaptureDatabase(string serverAdress,string database, string BackupPath)
        {
            serverAdress += string.Format("Initial Catalog={0};",database);
            DacServices oo = new DacServices(serverAdress);
            oo.Extract(BackupPath + database + ".dacpac", database, "Application", new Version("1.0.0.0"), "Description", null, null, null);  
        }

        static void WriteScriptToFile(string text, string path)
        {

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(text);
                writer.Flush();
            }
        }

        static void Println(string str)
        {
            Console.WriteLine(str);
        }
    }
}
