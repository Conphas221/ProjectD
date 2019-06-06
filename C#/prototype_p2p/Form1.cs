﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Windows.Forms;

namespace prototype_p2p
{
    public partial class Form1 : Form
    {
        ParseKeyID keyIDPaths;
        ConfigFile configData;
        Client ClientInstance;
        Server ServerInstance;
        int chainCount;

        public Form1(ParseKeyID keyIDPaths, ConfigFile configData, Client clientInstance, Server ServerInstance)
        {
            this.keyIDPaths = keyIDPaths;
            this.configData = configData;
            this.ClientInstance = clientInstance;
            this.ServerInstance = ServerInstance;
            InitializeComponent();
            richTextBoxKeyPaths.Text = keyIDPaths.ReturnAllLoadedKeyPathsAsStringNoPathPrefixed();
            ServerInitAt.Text = ServerInstance.serverInitAt;
            UpdatecomboBoxBlockDecryptNumberDropDown();
            chainCount = Program.ProjectD.ChainList.Count;
            for(int i=0; i < keyIDPaths.KeyArrayNoPathAppended.Length; i++)
            {
                checkedListBoxPublicKeysToEncryptFor.Items.Add(keyIDPaths.KeyArrayNoPathAppended[i], false);
            }
            //List<string> items = checkedListBoxPublicKeysToEncryptFor.CheckedItems.Cast<string>().ToList();
            this.comboBoxBlockDecryptNumber.DropDown +=
                new System.EventHandler(comboBoxBlockDecryptNumber_DropDown);
        }

        private void comboBoxBlockDecryptNumber_DropDown(object sender, System.EventArgs e)
        {
            if (chainCount != Program.ProjectD.ChainList.Count)
            {
                UpdatecomboBoxBlockDecryptNumberDropDown();
            }
        }
        private void UpdatecomboBoxBlockDecryptNumberDropDown()
        {
            comboBoxBlockDecryptNumber.Items.Clear();
            for (int i=1; i < Program.ProjectD.ChainList.Count; i++)
            {
                comboBoxBlockDecryptNumber.Items.Add(i);
            }
        }


        private void DisplayChainFromGUI(object sender, EventArgs e)
        {

            SimpleReportViewer.ShowDialog(JsonConvert.SerializeObject(Program.ProjectD, Formatting.Indented), "Chain data", this);
        }

        private void DecryptFromGUI(object sender, EventArgs e)
        {
            if (Program.ProjectD.ChainList.Count > 1) //1 and not 0 because the genesis block counts as one.
            {
                string blockNumber;
                int blockNumerInt;
                {
                    blockNumber = comboBoxBlockDecryptNumber.Text;
                    if (int.TryParse(blockNumber, out int inputBlockNumber)) //checks if the given input is a string. If not the user is told to enter a number. No more crashes because you accidently pressed enter.
                    {

                        if (inputBlockNumber >= Program.ProjectD.ChainList.Count)
                        {
                            MessageBox.Show("The block number you enter must correspond to an existing block. Try again.");
                        }
                        else
                        {
                            blockNumerInt = Math.Abs(inputBlockNumber);
                            string encryptedDataFromChain = Program.ProjectD.ChainList[blockNumerInt].MessageList[0].Data;
                            string privateKeyPathDecrypt = keyIDPaths.ParseAndReturnVerifiedKeyPathGUI(PrivateKeyDecrypt.Text);
                            string publicKeyPathDecrypt = keyIDPaths.ParseAndReturnVerifiedKeyPathGUI(PublicKeyVerify.Text);
                            DecryptAndVerifyString.Decrypt(encryptedDataFromChain, privateKeyPathDecrypt, publicKeyPathDecrypt,true);
                        }
                    }
                    else
                    {
                        MessageBox.Show("The number of the block has to be a number. Try again.");
                    }
                }

               
            }
            else
            {
                MessageBox.Show("There are no blocks to decrypt!");
            }
            comboBoxBlockDecryptNumber.Text = "Select block number";
            PublicKeyVerify.Text = "";
            PrivateKeyDecrypt.Text = "";
        }

        private void DisplayAllKeysGUI(object sender, EventArgs e)
        {

            SimpleReportViewer.ShowDialog(keyIDPaths.ReturnAllLoadedKeyPathsAsString(), "All known keys", this);
        }

        private void EncryptfromGUI(object sender, EventArgs e)
        {
            string[] receiverNames = ReceiverNameTextBox.Lines;         
            string receiverNamesForImprovedMultiEnc = ReceiverNameTextBox.Text;


            string privKeyPath = keyIDPaths.ParseAndReturnVerifiedKeyPathGUI(PrivateKeyIdTextBox.Text);

            List<string> items = checkedListBoxPublicKeysToEncryptFor.CheckedItems.Cast<string>().ToList();
            //string[] recipientKeyPathsArr = keyIDPaths.BuildVerifiedKeyIdPathArrayGUI(ReceiverKeyIdTextBox.Text);
            int cnt = 0;
            string[] recipientKeyPathsArr = new string[items.Count];
            foreach (string keyPath in items)
            {
                recipientKeyPathsArr[cnt] = (Program.pathKey + "\\" + keyPath);
                cnt++;
            }
            

            string inputData = Prompt.ShowDialog("Enter the data you want to encrypt", "Data entry");

            string encData = EncryptFileMultipleRecipients.MultiRecipientStringEncrypter(inputData, privKeyPath, recipientKeyPathsArr, true);
            Console.WriteLine(encData);

            Program.flushMsgAndSend.Flush(receiverNamesForImprovedMultiEnc, encData);
            Program.ProjectD.SaveChainStateToDisk(Program.ProjectD);
            foreach (int i in checkedListBoxPublicKeysToEncryptFor.CheckedIndices)
            {
                checkedListBoxPublicKeysToEncryptFor.SetItemCheckState(i, CheckState.Unchecked);
            }
            ReceiverKeyIdTextBox.Text = "";
            PrivateKeyIdTextBox.Text = "";
        }

        private void ToggleLoadConfigSettings(object sender, EventArgs e)
        {

            configData.ToggleAutoLoadConfigValues(true);


        }

        private void ConnectServer(object sender, EventArgs e)
        {
            string serverURL = ServerUrlTextBox.Text;
            ClientInstance.Handshake($"{serverURL}/Chain");
        }

        private void SaveNameAndPortToConfig_Click(object sender, EventArgs e)
        {
            configData.SaveCurrentPortAndNameToConfigValues(Program.NodeName, Program.NetworkPort);
        }
    }
}
