using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Principal;

namespace Link2Folder
{
    class Program
    {
        private const string RegistryKeyName = "link2folder";
        private const string AppName = "Link2Folder";

        private const string SetupArg = "setup";

        private const string BackslashCode = "%5C";
        private const string UrlPrefixForwardSlash = RegistryKeyName + "://";
        private const string UrlPrefixBackSlash = RegistryKeyName + @":\\";

        private static readonly string WindowsLocation = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private const string FileExplorerExecutable = @"\explorer.exe";

        private const uint DirectorySearchDepthLimit = 3;

        [SupportedOSPlatform("windows")]
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case SetupArg:
                    {
                        if (IsAdministrator())
                        {
                            RegistryKeysSetup();
                            WaitForUserInput();
                        }
                        else
                        {
                            StartAsAdmin();
                        }
                    }
                    break;
                    default:
                    {
                        string? path = CleanPath(args[0]);
                        if (path != null)
                        {
                            OpenFolder(path);
                        }
                        else
                        {
                            Console.WriteLine("The Provided Path Is Invalid!");
                            WaitForUserInput();
                        }
                    }
                    break;
                }

            }
            else
            {
                Console.WriteLine("No Argument(s) Received");
                WaitForUserInput();
            }
        }

        private static void WaitForUserInput()
        {
            Console.WriteLine("\nPress Any Key To Close.");
            Console.ReadKey();
        }

        #region Registry Setup

        [SupportedOSPlatform("windows")]
        private static void RegistryKeysSetup()
        {
            Console.WriteLine("Starting Setup");
            if (!Registry.ClassesRoot.GetSubKeyNames().Contains(RegistryKeyName))
            {
                Console.WriteLine("Registry key for app URI Scheme not found!");
            }
            else
            {
                Console.WriteLine("Old registry keys found, deleting them to create new ones!");
                DeleteOldRegistryKeys();
            }

            CreateRegistryKeys();

            Console.WriteLine("Finished Registry Key Creation!");
            Console.WriteLine("Now You Can Use \"link2folder://<path to folder to open>\" In Your Browser!");
        }

        [SupportedOSPlatform("windows")]
        private static void CreateRegistryKeys()
        {
            Console.WriteLine("Creating Registry keys!");

            RegistryKey key;
            key = Registry.ClassesRoot.CreateSubKey(RegistryKeyName);
            key.SetValue("", $"URL: {AppName} Protocol");
            key.SetValue("URL Protocol", "");

            RegistryKey iconKey = key.CreateSubKey("DefaultIcon");
            iconKey.SetValue("", $"{AppName}.exe");

            key = key.CreateSubKey("Shell");
            key = key.CreateSubKey("Open");
            key = key.CreateSubKey("Command");
            key.SetValue("", $"\"{GetCurrentPathToExe()}\" \"%1\"");
        }

        [SupportedOSPlatform("windows")]
        private static void DeleteOldRegistryKeys()
        {
            Registry.ClassesRoot.DeleteSubKeyTree(RegistryKeyName);
        }

        private static string GetCurrentPathToExe()
        {
            return $"{AppDomain.CurrentDomain.BaseDirectory}{AppDomain.CurrentDomain.FriendlyName}.exe";
        }

        [SupportedOSPlatform("windows")]
        private static bool IsAdministrator()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void StartAsAdmin()
        {
            Process proc = new Process
            {
                StartInfo =
                {
                    FileName = GetCurrentPathToExe(),
                    UseShellExecute = true,
                    Verb = "runas",
                    ArgumentList = { "setup" }
                }
            };

            try
            {
                proc.Start();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                Console.WriteLine("Failed To Get Elevated Privileges To Create The Required Registry Keys!");
                WaitForUserInput();
            }
        }

        #endregion Registry Setup

        #region Folder Opening

        private static string? CleanPath(string path)
        {
            Console.WriteLine("Cleaning Provided Path.");
            Console.WriteLine($"Original Path: {path}");
            path = path.Replace(BackslashCode, @"\");
            path = path.Replace(UrlPrefixForwardSlash, "");
            path = path.Replace(UrlPrefixBackSlash, "");
            path = Path.GetFullPath(path);
            Console.WriteLine($"Path After First Cleaning: {path}");

            try
            {
                //hopefully forcing it to be a folder only
                //deleting the end of the path until we find a folder
                uint depth = 0;
                FileAttributes attr = File.GetAttributes(path);
                while (!attr.HasFlag(FileAttributes.Directory))
                {
                    Console.WriteLine($"Path Is Not a Directory! Running Directory Search!\nCurrent Path:{path}");
                    if (depth == DirectorySearchDepthLimit) 
                    {
                        throw new Exception("Reached Depth Limit!");
                    }
                    if(path == "")
                    {
                        throw new Exception("Path String Is Empty Or Became Empty While Cleaning!");
                    }

                    path = path.Substring(0, path.LastIndexOf(@"\"));
                    attr = File.GetAttributes(path);

                    depth++;
                }

                Console.WriteLine($"Path After Directory Matching: {path}");

                return path;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
                return null;
            }
        }

        private static void OpenFolder(string path)
        {
            Console.WriteLine("Opening Provided Path.");
            Process.Start(WindowsLocation + FileExplorerExecutable, path);
        }

        #endregion Folder Opening
    }
}