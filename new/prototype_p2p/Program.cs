using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reflection;

namespace prototype_p2p
{
    class Program
    {
        public static int NetworkPort = 0;
        public static Server ServerInstance = null;
        public static Client ClientInstance = new Client();
        public static Chain ProjectD = new Chain();
        public static string NodeName = "Unknown";
        private static readonly List<string> validActions = new List<string> { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
        public static string pathKey = @"..\\..\\Keys";
        public static string pathMessages = @"..\\..\\Messages";
        public static string[] keyArrayPathAppended;
        //public static string[] keyArrayPathAppended = Directory.GetFiles(pathKey);


        //restricting usage of most commonly used ports 25:SMTP 80:HTTP 443:HTTPS 20,21:FTP 23:telnet 143:IMAP 3389:RDP 22:SSH 53:DNS 67,68:DHCP 110:POP3
        private static List<int> portBlacklist = new List<int> { 0, 20, 21, 22, 23, 25, 53, 67, 68, 80, 110, 143, 443, 3389 }; //The blacklist can be implemented with a user editable config file in the future


        static void Main(string[] args)
        {
            //Console.WriteLine("Messages directory exists:" + Directory.Exists(@"Messages"));
            Console.WriteLine("Default Keys directory exists:" + Directory.Exists(pathKey));
            Console.WriteLine("Config.ini exists:" + File.Exists("Config.ini"));

            Dictionary<string, string> configSettings = new Dictionary<string, string>();
            try
            {   // Open the text file using a stream reader.
                using (StreamReader sr = new StreamReader("Config.ini"))            //this can be used to create a config file system to remember settings
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        string[] keyvalue = line.Split('=');
                        if (keyvalue.Length == 2)
                        {
                            configSettings.Add(keyvalue[0], keyvalue[1]);
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:" + "\n" + e.Message);
            }
            foreach (var pair in configSettings)
            {
                Console.WriteLine($"Setting:{pair.Key} Value:{pair.Value}");
            }


            if (configSettings.TryGetValue("useConfigFile", out string value))
            {
                if (value.ToLower() == "true")
                {
                    if (configSettings.TryGetValue("NetworkPort", out string portVal))
                    {
                        if (int.TryParse(portVal, out int portValInt))
                        {
                            if (!portBlacklist.Contains(portValInt))
                            {
                                NetworkPort = Math.Abs(portValInt);
                            }

                        }
                    }
                    if (configSettings.TryGetValue("NodeName", out string nodeNameVal))
                    {
                        NodeName = nodeNameVal;
                    }
                    if (configSettings.TryGetValue("pathKey", out string altKeyPath))
                    {
                        pathKey = altKeyPath;
                    }

                }
            }
            try
            {
                keyArrayPathAppended = Directory.GetFiles(pathKey);
            }
            catch (DirectoryNotFoundException)
            {
                Console.WriteLine("Keys directory not found!");
            }
            for (int i = 0; i < keyArrayPathAppended.Length; i++)
            {
                Console.WriteLine(keyArrayPathAppended[i] + " key ID:" + i);
            }

            /*
            try
            {   // Open the text file using a stream reader.
               using (StreamReader sr = new StreamReader("Config.ini"))            //this can be used to create a config file system to remember settings
                {   
                    String line = sr.ReadToEnd(); //Load the file contents as a string, and write the string to the console.
                    char[] stringSeperator = new char[] { ';' }; //the text extracted from the file will be split on this character, more can be added if needed.
                    string[] configSplit = line.Split(stringSeperator, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i<configSplit.Length; i++)
                    {
                        Console.Write(configSplit[i] + "=" + i);
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("The file could not be read:"+"\n"+e.Message);
            }
            

            string[] lines = { "First line", "Second line", "Third line" };

            using (StreamWriter outputFile = new StreamWriter(Path.Combine(pathMessages, "WriteLines.txt")))  //code to create new data files.
            {
                foreach (string line in lines)
                    outputFile.WriteLine(line);
            }
            */




            while (NetworkPort == 0)
            {
                Console.Write("Enter network port: ");
                if (int.TryParse(Console.ReadLine(), out int port)) //checks if the given input is a string. If not the user is told to enter a number. No more crashes because you accidently pressed enter.
                {
                    port = Math.Abs(port); //can't enter a negative number as a port now
                    if (!portBlacklist.Contains(port)) //checks if the entered port is on the blacklist
                    {
                        NetworkPort = port;
                        break;
                    }
                    Console.Write("Pick a port number that does not match any of the following: ");
                    Console.WriteLine(string.Join<int>(", ", portBlacklist)); //lists all blacklisted ports to the user so he can avoid them.
                    Console.Write("Enter network port: ");

                }
                else
                {
                    Console.WriteLine("A port has to be a number. Try again.");
                }
            }


            if (NodeName == "Unknown")
            {
                Console.Write("Enter Node name: ");
                NodeName = Console.ReadLine();
            }


            //Eerst ProjectD.ReadChain() --> Als geen resultaat, dan SetupChain()
            ProjectD.ReadChain();
            if (ProjectD.ChainList == null)
            {
                ProjectD.SetupChain();
            }

            //    if (args.Length >= 1)
            //       NetworkPort = int.Parse(args[0]);
            //    if (args.Length >= 2)
            //        NodeName = args[1];

            if (NetworkPort > 0)
            {
                ServerInstance = new Server();
                ServerInstance.Initialize();
            }
            if (NodeName != "Unkown")
            {
                Console.WriteLine($"Your node name is {NodeName}");
            }

            Console.WriteLine("--------------------------------------");
            Console.WriteLine("1. Setup a connection with a server");
            Console.WriteLine("2. Add unencrypted data to chain");
            Console.WriteLine("3. Display records");
            Console.WriteLine("4. Exit the program");
            Console.WriteLine("5. List all keys in the keys directory");
            Console.WriteLine("6. Encrypt a message, encryption key ID's are listed under 5");
            Console.WriteLine("7. Decrypt a stored message");
            Console.WriteLine("8. Multi encryption method");
            Console.WriteLine("9. Toggle loading from config");
            Console.WriteLine("--------------------------------------");



            int instruction = 0;
            while (instruction != 4)
            {
                switch (instruction)
                {
                    case 1:
                        Console.WriteLine("Enter the URL and port of the server:");
                        string serverURL = Console.ReadLine();
                        ClientInstance.Handshake($"{serverURL}/Chain");
                        break;
                    case 2:
                        Console.Write("Enter the name(s) of the intended recipient(s): ");
                        string receiverName = Console.ReadLine();
                        Console.WriteLine("Enter the data:");
                        string data = Console.ReadLine();
                        ProjectD.CreateMessage(new Message(NodeName, receiverName, data));
                        ProjectD.ProcessMessageQueue(NodeName);
                        ClientInstance.SendToAll(JsonConvert.SerializeObject(ProjectD));
                        break;
                    case 3:
                        Console.WriteLine("Chain");
                        Console.WriteLine(JsonConvert.SerializeObject(ProjectD, Formatting.Indented));
                        break;
                    case 5:
                        string[] keyArray = Directory.GetFiles(pathKey).Select(p => Path.GetFileName(p)).ToArray(); //Select statement with lambda is necessary to display file names without the relative path appended.
                        //Lists every file found in the map pathKey is pointing to
                        for (int i = 0; i < keyArray.Length; i++)
                        {
                            Console.WriteLine(keyArray[i] + " key ID:" + i);
                        }
                        //for (int i = 0; i < keyArrayPathAppended.Length; i++)
                        //{
                        //    Console.WriteLine(keyArrayPathAppended[i] + " key ID:" + i);
                        //}
                        break;
                    case 6:
                        Console.WriteLine("Enter the name of the receiver");
                        string receiverNameFor6 = Console.ReadLine();


                        Console.WriteLine("Enter the data you want encrypted: ");
                        string dataToBeEncrypted = Console.ReadLine();
                        Console.WriteLine("Enter the ID of the private key you want to sign with");
                        string privateKeyPath = ParseKeyID.ParseAndReturnVerifiedKeyPath(); //the user looks up the private and public key �D's with the option 5 menu and then chooses the encryption keys with the ID"s linked to the keys.
                        Console.WriteLine("Enter the ID of the public key you want to encrypt for");
                        string publicKeyPath = ParseKeyID.ParseAndReturnVerifiedKeyPath();


                        string encryptedData = SignAndEncryptString.StringEncrypter(dataToBeEncrypted, privateKeyPath, publicKeyPath);
                        Console.WriteLine(encryptedData);
                        ProjectD.CreateMessage(new Message(NodeName, receiverNameFor6, encryptedData));
                        ProjectD.ProcessMessageQueue(NodeName);
                        ClientInstance.SendToAll(JsonConvert.SerializeObject(ProjectD));

                        break;
                    case 7:

                        int blockNumber = 0;
                        while (blockNumber <= 0)
                        {
                            Console.Write("Enter the number of the block you want to decrypt: ");
                            if (int.TryParse(Console.ReadLine(), out int inputBlockNumber)) //checks if the given input is a string. If not the user is told to enter a number. No more crashes because you accidently pressed enter.
                            {

                                if (inputBlockNumber >= ProjectD.ChainList.Count)
                                {
                                    Console.WriteLine("The number you enter must correspond to an existing block. Try again.");
                                }
                                else
                                {
                                    blockNumber = Math.Abs(inputBlockNumber);
                                }
                            }
                            else
                            {
                                Console.WriteLine("The number of the block has to be a number. Try again.");
                            }
                        }
                        Console.WriteLine("Encrypted data:");

                        string encryptedDataFromChain = ProjectD.ChainList[blockNumber].MessageList[0].Data;
                        Console.WriteLine(encryptedDataFromChain);

                        Console.Write("Enter the ID of the private key you want to use to decrypt: ");
                        string privateKeyPathDecrypt = ParseKeyID.ParseAndReturnVerifiedKeyPath(); //the user looks up the private and public key �D's with the option 5 menu and then chooses the encryption keys with the ID"s linked to the keys.
                        Console.Write("Enter the ID of the public key of the sender: ");
                        string publicKeyPathDecrypt = ParseKeyID.ParseAndReturnVerifiedKeyPath();


                        DecryptAndVerifyString.Decrypt(encryptedDataFromChain, privateKeyPathDecrypt, publicKeyPathDecrypt);
                        //DecryptAndVerifyString.DecryptMulti(encryptedDataFromChain, privateKeyPathDecrypt);

                        break;
                    case 8:

                        Console.WriteLine("Enter the names of the designated recipients");
                        string receiverNamesForImprovedMultiEnc = Console.ReadLine();

                        Console.WriteLine("Enter data you want to encrypt:");
                        string inputData = Console.ReadLine();

                        Console.WriteLine("Enter the ID of the private key you want to sign with");
                        string privKeyPath = ParseKeyID.ParseAndReturnVerifiedKeyPath();


                        Console.WriteLine("Enter the public key ID's for every recipient");
                        string[] recipientKeyPathsArr = ParseKeyID.BuildVerifiedKeyIdPathArray();




                        string encData = EncryptFileMultipleRecipients.MultiRecipientStringEncrypter(inputData, privKeyPath, recipientKeyPathsArr);
                        Console.WriteLine(encData);

                        ProjectD.CreateMessage(new Message(NodeName, receiverNamesForImprovedMultiEnc, encData));
                        ProjectD.ProcessMessageQueue(NodeName);
                        ClientInstance.SendToAll(JsonConvert.SerializeObject(ProjectD));
                        break;
                    case 9:

                        try
                        {
                            if (configSettings.TryGetValue("useConfigFile", out string useConfigVal))
                            {
                                if (useConfigVal.ToLower() == "true")
                                {
                                    configSettings["useConfigFile"] = "false";
                                }
                                else if (useConfigVal.ToLower() == "false")
                                {
                                    configSettings["useConfigFile"] = "true";
                                }
                            }
                            using (StreamWriter file = new StreamWriter("Config.ini"))
                            {
                                file.WriteLine("//== Use two or more = characters in one line to prevent the program from loading it");
                                foreach (var entry in configSettings)
                                {
                                    file.WriteLine("{0}{1}{2}", entry.Key, "=", entry.Value);
                                }
                            }
                            Console.WriteLine("Change will go in effect next application restart.");
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Console.WriteLine("No access authorization!");
                        }
                        break;
                }





                        Console.Write("Enter the number of the action you want to execute: ");
                        string action = Console.ReadLine();
                        if (validActions.Contains(action))
                        {
                            instruction = int.Parse(action);
                        }
                        else
                        {
                            Console.WriteLine("Please pick a valid action!");
                            instruction = 0;
                        }
                        using (StreamWriter file = File.CreateText(@"chain.json")) //placed it here as well to save the chain after every action
                        {
                            JsonSerializer serializer = new JsonSerializer();
                            serializer.Serialize(file, ProjectD);
                        }
                }

                using (StreamWriter file = File.CreateText(@"chain.json"))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(file, ProjectD);
                }

                ClientInstance.Exit();
            }
        }
    }

