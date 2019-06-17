using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;

namespace prototype_p2p
{
    class Program
    {
        public static int NetworkPort = 0;
        public static Server ServerInstance = null;
        public static Client ClientInstance = new Client();
        public static Chain ProjectD = new Chain();
        public static string NodeName = "Unknown";
        private static readonly List<string> validActions = new List<string> { "1", "2", "3", "4", "5", "7", "8", "9", "10" };
        public static readonly List<string> existingRoles = new List<string> { "Politie", "OM", "Gemeente", "Reclassering" };
        public static string currentRole = "";
        public static string pathKeyPrivate = @"Keys\\Private";
        public static string pathKeyPublic = @"Keys\\Public";
        public static FormGenericGUI genericGUIForm;
        public static FlushBlock flushMsgAndSend;
        


        //restricting usage of most commonly used ports 25:SMTP 80:HTTP 443:HTTPS 20,21:FTP 23:telnet 143:IMAP 3389:RDP 22:SSH 53:DNS 67,68:DHCP 110:POP3
        public static readonly List<int> portBlacklist = new List<int> { 0, 20, 21, 22, 23, 25, 53, 67, 68, 80, 110, 143, 443, 3389 };

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            ConfigFile configData = new ConfigFile();

            // Checks if the directory entered in the config file exists, and if it does not it creates the default directory.
            if (!Directory.Exists(pathKeyPublic))
            {
                pathKeyPublic = @"Keys\\Public";
                Directory.CreateDirectory(pathKeyPublic);
            }

            // Creates the directories where the public keys of the roles with the same name should be placed.
            if (!Directory.Exists(pathKeyPublic + "\\Gemeente")|| !Directory.Exists(pathKeyPublic + "\\Politie") || !Directory.Exists(pathKeyPublic + "\\OM") || !Directory.Exists(pathKeyPublic + "\\Reclassering"))
            {
                Directory.CreateDirectory(pathKeyPublic + "\\Gemeente");
                Directory.CreateDirectory(pathKeyPublic + "\\Politie");
                Directory.CreateDirectory(pathKeyPublic + "\\OM");
                Directory.CreateDirectory(pathKeyPublic + "\\Reclassering");
            }

            // Checks if the directory entered in the config file exists, and if it does not it creates the default directory.
            if (!Directory.Exists(pathKeyPrivate))
            {
                pathKeyPrivate = @"Keys\\Private";
                Directory.CreateDirectory(pathKeyPrivate);
            }

            ParseKeyID keyIDPaths = new ParseKeyID(pathKeyPrivate, pathKeyPublic);

            BootConfigurator bootConfigurator = new BootConfigurator();

            // The boot configurator form only launches if one or more of the required values are not loaded from the config file.
            if (!existingRoles.Contains(currentRole) || !bootConfigurator.ValidatePortNumberEntry(NetworkPort.ToString()) || NodeName == "" || NodeName == "Unknown")
            {
                Application.Run(bootConfigurator);
            }
            

            // Attempts to load the saved chain, if no saved chain exists it creates a new one.
            ProjectD.ReadChain();
            if (ProjectD.ChainList == null)
            {
                ProjectD.SetupChain();
            }

            if (NetworkPort > 0)
            {
                ServerInstance = new Server();
                ServerInstance.Initialize();
            }

            flushMsgAndSend = new FlushBlock(currentRole, ClientInstance); //Place this after the chain, clientinstance and nodename have been initialized.

            genericGUIForm = new FormGenericGUI(keyIDPaths, configData, ClientInstance, ServerInstance);

            // Launches the main form after all required values are entered in the BootConfigurator form.
            Application.Run(genericGUIForm);

            // Saves the chain currently in memory after closing the main form.
            ProjectD.SaveChainStateToDisk(ProjectD);

            ClientInstance.Exit();
            }
        }
    }

