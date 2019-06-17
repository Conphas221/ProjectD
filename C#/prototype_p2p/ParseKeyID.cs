using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace prototype_p2p
{
    public class ParseKeyID
    {
        public string[] privateKeyArrayPathAppended;
        public string[] publicKeyArrayPathAppended;
        public string[] publicKeyArrayNoPathAppended;
        public string[] privateKeyArrayNoPathAppended;
        public Dictionary<string, string> roleKeyPaths = new Dictionary<string, string>();

        char[] strSeperatorKeyInput = new char[] { ';', ',', ' ' };

        /* Constructor that builds arrays containing the keyfile paths
         * no path appended format example: "examplekey_private.asc"
         * path appended format example: "..\\..\\Keys\Private\examplekey_private.asc"
        */
        public ParseKeyID(string keysPathPrivate, string keysPathPublic)
        {
            try
            {
                privateKeyArrayPathAppended = Directory.GetFiles(keysPathPrivate);
                publicKeyArrayPathAppended = Directory.GetFiles(keysPathPublic);
                publicKeyArrayNoPathAppended = Directory.GetFiles(keysPathPublic).Select(p => Path.GetFileName(p)).ToArray();
                privateKeyArrayNoPathAppended = Directory.GetFiles(keysPathPrivate).Select(p => Path.GetFileName(p)).ToArray();
                LoadRolePublicKeyPaths();
            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show("Keys directory not found!");
            }
        }

        public void LoadRolePublicKeyPaths()
        {
            try
            {
                roleKeyPaths.Add(Program.existingRoles.ElementAt(0), Directory.GetFiles(Program.pathKeyPublic + "\\" + Program.existingRoles.ElementAt(0))[0]);
                roleKeyPaths.Add(Program.existingRoles.ElementAt(1), Directory.GetFiles(Program.pathKeyPublic + "\\" + Program.existingRoles.ElementAt(1))[0]);
                roleKeyPaths.Add(Program.existingRoles.ElementAt(2), Directory.GetFiles(Program.pathKeyPublic + "\\" + Program.existingRoles.ElementAt(2))[0]);
                roleKeyPaths.Add(Program.existingRoles.ElementAt(3), Directory.GetFiles(Program.pathKeyPublic + "\\" + Program.existingRoles.ElementAt(3))[0]);
            }
            catch(Exception e)
            {
                if (e is IndexOutOfRangeException)
                {
                    MessageBox.Show("One or more public key files are missing from the designated role folders.\n\nPlease add the missing keys to ensure that the program will function correctly.");
                }
                else
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

        // Only used by the GUI, used to populate the available key boxes.
        public string ReturnLoadedKeyPathsAsStringNoPathPrefixed(bool privateKeys = false)
        {
            string[] keyArray = !privateKeys ? publicKeyArrayNoPathAppended : privateKeyArrayNoPathAppended;

            string keyPaths = "";
            for (int i = 0; i < keyArray.Length; i++)
            {
                keyPaths = keyPaths + keyArray[i] + Environment.NewLine;
            }
            return keyPaths;
        }
        }
    }
    



