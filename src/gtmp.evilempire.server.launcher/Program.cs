using gtmp.evilempire.db;
using gtmp.evilempire.entities;
using System;
using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

namespace gtmp.evilempire.server.launcher
{
    static class Constants
    {
        public const string SettingsTemplateFile = "settings.template.xml";
        public const string SettingsUserFile = "settings.user.xml";
        public const string SettingsTransformationFile = "settings.xsl";
        public const string SettingFile = "settings.xml";
        public const string ServerExecutable = "GrandTheftMultiplayer.Server.exe";

        internal static class ExitCodes
        {
            public const int ServerSettingsTransformationFailed = -100;
            public const int DatabaseCheckFailed = -200;
            public const int DatabasePopulationFailed = -201;
        }
    }

    static class Program
    {
        public static int Main()
        {
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

            var exitCode = ((Func<int>)(() =>
            {
                Logo();
                if (!TransformServerSettings())
                {
                    return Constants.ExitCodes.ServerSettingsTransformationFailed;
                }
                if (!CheckDatabase())
                {
                    return Constants.ExitCodes.DatabaseCheckFailed;
                }
                if (!DatabasePopulation())
                {
                    return Constants.ExitCodes.DatabasePopulationFailed;
                }
                RunServer();

                return 0;
            }))();

            if (exitCode != 0)
            {
                using (ConsoleColor.Yellow.Foreground())
                {
                    Console.WriteLine("launcher will exit without success.");
                }
                Console.ReadKey();
            }

            return exitCode;
        }

        static void Logo()
        {
            using (ConsoleColor.Cyan.Foreground())
            {
                Console.WriteLine("GTMP Evil Empire Server Launcher");
                Console.WriteLine();
            }
        }

        static bool TransformServerSettings()
        {
            if (!ExecuteWithConsoleOutput("Check settings template file ... ", WrapWithFailReason(() => CheckFile(Constants.SettingsTemplateFile), $"{Constants.SettingsTemplateFile} file missing")))
            {
                return false;
            }
            if (!ExecuteWithConsoleOutput("Check settings user file ... ", WrapWithFailReason(() => CheckFile(Constants.SettingsUserFile), $"{Constants.SettingsUserFile} file missing")))
            {
                return false;
            }
            if (!ExecuteWithConsoleOutput("Check settings transformation file ... ", WrapWithFailReason(() => CheckFile(Constants.SettingsTransformationFile), $"{Constants.SettingsTransformationFile} file missing")))
            {
                return false;
            }

            if (CheckFile(Constants.SettingFile))
            {
                if (!ExecuteWithConsoleOutput("Remove existing transformation file ... ",  WrapWithFailReason(() => DeleteFile(Constants.SettingFile), $"Unable to delete existing {Constants.SettingsUserFile}")))
                {
                    return false;
                }
            }

            if (!ExecuteWithConsoleOutput("Transform settings file ... ", TransformSettingsFile))
            {
                return false;
            }

            return true;
        }

        static void RunServer()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Constants.ServerExecutable,
                WorkingDirectory = Environment.CurrentDirectory,
            };

            ExecuteWithConsoleOutput("Starting GTMP server instance ... ", WrapWithFailReason(() => Process.Start(processStartInfo) != null, "Unable to spawn new process"));
        }

        static bool TransformSettingsFile(out string failReason)
        {
            try
            {
                using (var stylesheetStream = File.Open(Constants.SettingsTransformationFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (var stylesheetReader = XmlReader.Create(stylesheetStream))
                    {
                        var xslTransform = new XslCompiledTransform(false);
                        xslTransform.Load(stylesheetReader, new XsltSettings(true, false), null);

                        using (var inputStream = File.Open(Constants.SettingsTemplateFile, FileMode.Open, FileAccess.Read, FileShare.Read))
                        {
                            using (var inputReader = XmlReader.Create(inputStream))
                            {
                                using (var outputStream = File.Create(Constants.SettingFile))
                                {
                                    using (var outputWrite = XmlWriter.Create(outputStream))
                                    {
                                        xslTransform.Transform(inputReader, outputWrite);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                failReason = $"Error during transformation\n{ex}";
                return false;
            }
            failReason = null;
            return true;
        }

        static bool CheckDatabase()
        {
            if (!ExecuteWithConsoleOutput("Check database directory and entity integrity ... ", WrapWithFailReason(() =>
                {
                    using (var dbe = new DbEnvironment(evilempire.Constants.Database.DatabasePath))
                    {
                        dbe.Select<User, string>("0");
                    }
                    return true;
                },
                "unable to check database")))
            {
                return false;
            }
            return true;
        }

        static bool DatabasePopulation()
        {
            if (!ExecuteWithConsoleOutput("Check database template directory ... ", WrapWithFailReason(() => CheckDirectory(evilempire.Constants.Database.DatabaseTemplatePath), $"{evilempire.Constants.Database.DatabaseTemplatePath} missing")))
            {
                return false;
            }

            using (var dbe = new DbEnvironment(evilempire.Constants.Database.DatabasePath))
            {
                var dbt = new DbTemplate(evilempire.Constants.Database.DatabaseTemplatePath);
                foreach(var template in dbt.Templates)
                {
                    if (!ExecuteWithConsoleOutput($"Populate db environment using template {template} ... ", WrapWithFailReason(() => dbt.PopulateByTemplate(template, dbe), "failed")))
                    {
                        break;
                    }
                }
            }

            return true;
        }

        delegate bool WrappedConsoleExecution(out string failReason);
        static bool ExecuteWithConsoleOutput(string message, WrappedConsoleExecution fn)
        {
            Console.Write(message);
            string failReason = null;
            if (!(fn?.Invoke(out failReason) ?? false))
            {
                using (ConsoleColor.Red.Foreground())
                {
                    Console.WriteLine("Failed");
                }
                if (failReason != null)
                {
                    Console.Write("     ");
                    Console.WriteLine(failReason);
                }
                return false;
            }
            using (ConsoleColor.Green.Foreground())
            {
                Console.WriteLine("OK");
            }
            return true;
        }

        static bool CheckDirectory(string directory)
        {
            return Directory.Exists(directory);
        }

        static bool CheckFile(string file)
        {
            return File.Exists(file);
        }

        static bool DeleteFile(string file)
        {
            try
            {
                File.Delete(file);
            }
            catch (IOException)
            {
                return false;
            }
            return true;
        }

        static WrappedConsoleExecution WrapWithFailReason(Func<bool> fn, string failReason)
        {
            return (out string a) =>
            {
                if (!(fn?.Invoke() ?? false))
                {
                    a = failReason;
                    return false;
                }
                a = null;
                return true;
            };
        }
    }
}