using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace R2FLauncher
{
    public partial class Form1 : Form
    {
        String encrypt_string = "GodAlwaysLovesUs!ThanksToGod!";

        public Form1()
        {
            InitializeComponent();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            bool ok_or_not = false;

            if ( DownloadAndDecryption( "http://r2f.netmego.com:8812/Bundles/Patch/Managed/Assembly-CSharp.unity3d", "./R2Fantasy_Data/Managed/Assembly-CSharp.dll" ) )
                if ( DownloadAndDecryption( "http://r2f.netmego.com:8812/Bundles/Patch/Managed/Assembly-CSharp-firstpass.unity3d", "./R2Fantasy_Data/Managed/Assembly-CSharp-firstpass.dll" ) )
                    if ( DownloadAndDecryption( "http://r2f.netmego.com:8812/Bundles/Patch/Managed/Assembly-UnityScript.unity3d", "./R2Fantasy_Data/Managed/Assembly-UnityScript.dll" ) )
                        if ( DownloadAndDecryption( "http://r2f.netmego.com:8812/Bundles/Patch/Managed/Assembly-UnityScript-firstpass.unity3d", "./R2Fantasy_Data/Managed/Assembly-UnityScript-firstpass.dll" ) )
                            ok_or_not = true;

            if ( !ok_or_not )
            {
                File.Delete( "./R2Fantasy_Data/Managed/Assembly-CSharp.dll" );
                File.Delete( "./R2Fantasy_Data/Managed/Assembly-CSharp-firstpass.dll" );
                File.Delete( "./R2Fantasy_Data/Managed/Assembly-UnityScript.dll" );
                File.Delete( "./R2Fantasy_Data/Managed/Assembly-UnityScript-firstpass.dll" );
                Application.Exit();
            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(".\\R2Fantasy.exe");
                startInfo.Arguments = "-launchfromlauncher";
                Process.Start(startInfo);
                Application.Exit();
            }

        }

        bool DownloadAndDecryption(String urlpath, String localpath)
        {
            WebClient webClient = new WebClient();
            byte[] received = null;
            try
            {
                received = webClient.DownloadData(urlpath);
            }
            catch(WebException e)
            {
                Console.WriteLine(e.Message);
            }

            if (received == null)
                return false;

            UTF8Encoding encoding = new UTF8Encoding();
            byte[] encryptcode = encoding.GetBytes(encrypt_string);

            for( int i = 0; i < received.Length; i++ )
                received[i] ^= encryptcode[i%encryptcode.Length];

            File.WriteAllBytes(localpath, received);

            return true;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}
