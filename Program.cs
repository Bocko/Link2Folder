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

        private const string BackslashCode = "%5C";
        private const string UrlPrefixForwardSlash = RegistryKeyName + "://";
        private const string UrlPrefixBackSlash = RegistryKeyName + @":\\";

        private static readonly string WindowsLocation = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
        private const string FileExplorerExecutable = @"\explorer.exe";

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
            path = path.Replace(BackslashCode, @"\");
            path = path.Replace(UrlPrefixForwardSlash, "");
            path = path.Replace(UrlPrefixBackSlash, "");
            path = Path.GetFullPath(path);

            try
            {
                //hopefully forcing it to be a folder only
                //deleting the end of the path until we find a folder
                FileAttributes attr = File.GetAttributes(path);
                while (!attr.HasFlag(FileAttributes.Directory))
                {
                    path = path.Substring(0, path.LastIndexOf(@"\"));
                    attr = File.GetAttributes(path);
                }

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
            Process.Start(WindowsLocation + FileExplorerExecutable, path);
        }

        #endregion Folder Opening
    }
}