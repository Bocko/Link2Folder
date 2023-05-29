using Microsoft.Win32;
using System.Diagnostics;
using System.Security.Principal;

namespace Link2Folder
{
    class Program
    {
        private const string RegistryKeyName = "link2folder";
        private const string AppName = "Link2Folder";

        private const string SetupArg = "setup";

        private const string UrlPrefix = RegistryKeyName + "://";
        private const string BackslashCode = "%5C";

        private const string FileExplorerLocation = @"C:\Windows\explorer.exe";

        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case SetupArg:
                    {
                        //SetupRegistryKeys();
                        if (IsAdministrator())
                        {
                            RegistryKeysSetup();

                            Console.WriteLine("\nPress Any Key To Close.");
                            Console.ReadKey();
                        }
                        else
                        {
                            StartAsAdmin();
                        }
                    }
                    break;
                    default:
                    {
                        string path = CleanPath(args[0]);
                        OpenFolder(path);
                    }
                    break;
                }

            }
            else
            {
                Console.WriteLine("No Argument(s) Recieved");
            }
        }

        #region Registry Setup

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

        private static void DeleteOldRegistryKeys()
        {
            Registry.ClassesRoot.DeleteSubKeyTree(RegistryKeyName);
        }

        private static string GetCurrentPathToExe()
        {
            return $"{AppDomain.CurrentDomain.BaseDirectory}{AppDomain.CurrentDomain.FriendlyName}.exe";
        }

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

            proc.Start();
        }

        #endregion Registry Setup

        #region Folder Opening

        private static string CleanPath(string path)
        {
            path = path.Replace(UrlPrefix, "");
            path = path.Replace(BackslashCode, @"\");
            path = path.Replace("/", "");

            FileAttributes attr = File.GetAttributes(path);

            //detect whether its a directory or file
            if (!attr.HasFlag(FileAttributes.Directory))
            {
                //hopefully forcing it to be a folder only
                path = path.Substring(0, path.LastIndexOf(@"\") + 1);
            }

            return path;
        }

        private static void OpenFolder(string path)
        {
            Process.Start(FileExplorerLocation, path);
        }

        #endregion Folder Opening
    }
}