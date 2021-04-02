using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;


namespace DES_Sifras
{
    // https://stackoverflow.com/questions/4359234/how-to-implement-system-security-cryptography-des/4360130


    public partial class Form1 : Form
    {
        //  Call this function to remove the key from memory after use for security
        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        public static extern bool ZeroMemory(IntPtr Destination, int Length);

        public Form1()
        {
            InitializeComponent();

        }
        static string GenerateKey()
        {
            // Create an instance of Symetric Algorithm. Key and IV is generated automatically.
            DESCryptoServiceProvider desCrypto = (DESCryptoServiceProvider)DESCryptoServiceProvider.Create();

            // Use the Automatically generated key for Encryption. 
            return ASCIIEncoding.ASCII.GetString(desCrypto.Key);
        }

        static void EncryptFile(string sInputFilename,
           string sOutputFilename,
           string sKey, string mode)
        {


            FileStream fsInput = new FileStream(sInputFilename,
               FileMode.Open,
               FileAccess.Read);

            FileStream fsEncrypted = new FileStream(sOutputFilename,
               FileMode.Create,
               FileAccess.Write);

            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            // Check mode and set it
            if (mode == "CBC") { DES.Mode = CipherMode.CBC; }
            if (mode == "ECB") { DES.Mode = CipherMode.ECB; }
            
            ICryptoTransform desencrypt = DES.CreateEncryptor();
            CryptoStream cryptostream = new CryptoStream(fsEncrypted,
               desencrypt,
               CryptoStreamMode.Write);

            byte[] bytearrayinput = new byte[fsInput.Length];
            fsInput.Read(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Write(bytearrayinput, 0, bytearrayinput.Length);
            cryptostream.Close();
            fsInput.Close();
            fsEncrypted.Close();
        }

        static void DecryptFile(string sInputFilename,
           string sOutputFilename,
           string sKey, string mode)
        {
            DESCryptoServiceProvider DES = new DESCryptoServiceProvider();
            //A 64 bit key and IV is required for this provider.
            //Set secret key For DES algorithm.
            DES.Key = ASCIIEncoding.ASCII.GetBytes(sKey);
            //Set initialization vector.
            DES.IV = ASCIIEncoding.ASCII.GetBytes(sKey);
            if (mode == "CBC") { DES.Mode = CipherMode.CBC; }
            if (mode == "ECB") { DES.Mode = CipherMode.ECB; }
            //Create a file stream to read the encrypted file back.
            FileStream fsread = new FileStream(sInputFilename,
               FileMode.Open,
               FileAccess.Read);
            //Create a DES decryptor from the DES instance.
            ICryptoTransform desdecrypt = DES.CreateDecryptor();
            //Create crypto stream set to read and do a 
            //DES decryption transform on incoming bytes.
            CryptoStream cryptostreamDecr = new CryptoStream(fsread,
               desdecrypt,
               CryptoStreamMode.Read);
            //Print the contents of the decrypted file.

            //FileStream printDecrypted;


            //printDecrypted = new FileStream(@"decrypted.txt", FileMode.Append);
            //



            //printDecrypted.Write(cryptostreamDecr, 0, cryptostreamDecr.Length);

            string path = @"decrypted.txt";
            using (FileStream fs = File.Create(path))
            {
                string toPrint = new StreamReader(cryptostreamDecr).ReadToEnd();
                byte[] info = new UTF8Encoding(true).GetBytes(toPrint);
                fs.Write(info, 0, info.Length);
            }

            //StreamWriter fsDecrypted = new StreamWriter(sOutputFilename);
            //fsDecrypted.Write(new StreamReader(cryptostreamDecr).ReadToEnd());
            //fsDecrypted.Flush();
            //fsDecrypted.Close();
        }


        private void butSifruoti_Click_1(object sender, EventArgs e)
        {
            // Must be 64 bits, 8 bytes.
            // Distribute this key to the user who will decrypt this file.
            string sSecretKey;

            sSecretKey = GenerateKey();

            string mode = comboBox1.SelectedItem.ToString();

            // For additional security Pin the key.
            GCHandle gch = GCHandle.Alloc(sSecretKey, GCHandleType.Pinned);

            EncryptFile(@"tekstas.txt", @"encrypted.txt", sSecretKey, mode);

            boxKeyToDistribute.Text = sSecretKey.ToString();
            // Remove the Key from memory. 
            ZeroMemory(gch.AddrOfPinnedObject(), sSecretKey.Length * 2);
            gch.Free();



        }

        private void button1_Click(object sender, EventArgs e)
        {
            string mode = comboBox1.SelectedItem.ToString();
            string key = boxKeyToDistribute.Text.ToString();
            DecryptFile(@"encrypted.txt", @"decrypted.txt", key, mode);
        }

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            butSifruoti.Enabled = true;
            button1.Enabled = true;
        }
    }
}
