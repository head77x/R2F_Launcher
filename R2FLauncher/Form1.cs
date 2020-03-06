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
using LitJson;

namespace R2FLauncher
{
    public partial class Form1 : Form
    {
        String BundleURL = "http://www.r2fantasy.com";
        String MainURL = "http://r2f.netmego.com:8812";

        String encrypt_string = "GodAlwaysLovesUs!ThanksToGod!";

        WebClient DownloadMan = new WebClient();

        UInt32 game_version = 0;

        bool NeedToUpdate = false;

        int downloadcounter = 0;
        int downloaded = 0;

        Dictionary<String, UInt32> fileversions;
        Dictionary<String, UInt32> noneedupdate;
        String versionStr;

        bool MainFileNeedToUpdate = true;
        int MainFileLoadCounter = 0;
        bool MainResourceNeedToUpdate = true;
        int MainResourceLoadCounter = 0;

        public Form1()
        {
            InitializeComponent();

            fileversions = new Dictionary<string, uint>();
            noneedupdate = new Dictionary<string, uint>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            button1.Visible = false;

            versionStr = "";

            try
            {
                versionStr = File.ReadAllText("HolyVersion.bin");
            }
            catch
            {
            }

            // 로컬 파일 버전들 얻어오기
            if ( versionStr.Length > 0 )
            {
                char[] spliter = { ',' };
                String[] versions = versionStr.Split( spliter, StringSplitOptions.RemoveEmptyEntries );

                for ( int i = 0; i < versions.Length; i+=2 )
                {
                    fileversions.Add(versions[i], UInt32.Parse(versions[i + 1]));
                }
            }

            // 원격 파일 버전들 얻어오기
            Downloader man = Downloader.instance;

            WebSlaveParam versioninfo = new WebSlaveParam();
            versioninfo.url = MainURL + "/php/bundlev.php";
            versioninfo.download_filename = "version";
            versioninfo.whattype = WebRequestType.StringReturn;
            versioninfo.return_object = this;
            man.CallWeb(versioninfo);

            timer1.Start();
        }

        void StartGame()
        {
            button2.Visible = false;

            bool ok_or_not = false;

            if ( DownloadAndDecryption( BundleURL + "/Bundles/Patch/Managed/Assembly-CSharp.unity3d", "./R2Fantasy_Data/Managed/Assembly-CSharp.dll" ) )
                if (DownloadAndDecryption(BundleURL + "/Bundles/Patch/Managed/Assembly-CSharp-firstpass.unity3d", "./R2Fantasy_Data/Managed/Assembly-CSharp-firstpass.dll"))
                    if (DownloadAndDecryption(BundleURL + "/Bundles/Patch/Managed/Assembly-UnityScript.unity3d", "./R2Fantasy_Data/Managed/Assembly-UnityScript.dll"))
                        if (DownloadAndDecryption(BundleURL + "/Bundles/Patch/Managed/Assembly-UnityScript-firstpass.unity3d", "./R2Fantasy_Data/Managed/Assembly-UnityScript-firstpass.dll"))
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
                //startInfo.Arguments = "-launchfromlauncher";   // 한글 버전
                startInfo.Arguments = "-launchfromlauncher -chinese"; // 중국 버전
                Process.Start(startInfo);
                Application.Exit();
            }
        }

        void VersionCheck(String versions)
        {
            JsonData jData = JsonMapper.ToObject(versions);

            JsonData versioninfo = jData["Bundles"];
            Downloader man = Downloader.instance;

            downloadcounter = 0;

            for (int i = 0; i < versioninfo.Count; i++)
            {
                JsonData fileinfo = versioninfo[i];
                String filename = fileinfo["name"].ToString();
                UInt32 fileversion = UInt32.Parse(fileinfo["version"].ToString());

                // 기존에 없는 파일 정보 또는 버전이 다르면 다운 받기
                if (!fileversions.ContainsKey(filename) || fileversions[filename] != fileversion)
                {
                    if (filename == "R2FVersion") // 메인 파일들 다운로드
                    {
                        WebSlaveParam maindata = new WebSlaveParam();
                        maindata.url = BundleURL + "/Bundles/Patch/mainData.unity3d";
                        maindata.download_filename = "./R2Fantasy_Data/mainData";
                        maindata.return_value = fileversion;
                        maindata.whattype = WebRequestType.FileReturn;
                        maindata.return_object = this;
                        man.CallWeb(maindata);
                        downloadcounter++;

                        WebSlaveParam maindata2 = new WebSlaveParam();
                        maindata2.url = BundleURL + "/Bundles/Patch/PlayerConnectionConfigFile.unity3d";
                        maindata2.download_filename = "./R2Fantasy_Data/PlayerConnectionConfigFile";
                        maindata2.return_value = fileversion;
                        maindata2.whattype = WebRequestType.FileReturn;
                        maindata2.return_object = this;
                        man.CallWeb(maindata2);
                        downloadcounter++;

                        WebSlaveParam maindata4 = new WebSlaveParam();
                        maindata4.url = BundleURL + "/Bundles/Patch/resources.unity3d";
                        maindata4.download_filename = "./R2Fantasy_Data/resources.assets";
                        maindata4.return_value = fileversion;
                        maindata4.whattype = WebRequestType.FileReturn;
                        maindata4.return_object = this;
                        man.CallWeb(maindata4);
                        downloadcounter++;

                        WebSlaveParam maindata3 = new WebSlaveParam();
                        maindata3.url = BundleURL + "/Bundles/Patch/sharedassets0.unity3d";
                        maindata3.download_filename = "./R2Fantasy_Data/sharedassets0.assets";
                        maindata3.return_value = fileversion;
                        maindata3.whattype = WebRequestType.FileReturn;
                        maindata3.return_object = this;
                        man.CallWeb(maindata3);
                        downloadcounter++;

                        MainFileNeedToUpdate = true;
                        MainFileLoadCounter = 0;
                    }
                    else
                    {
                        WebSlaveParam bundledatas = new WebSlaveParam();
                        bundledatas.url = BundleURL + "/Bundles/" + filename + ".unity3d";
                        bundledatas.download_filename = filename;
                        bundledatas.return_value = fileversion;
                        bundledatas.whattype = WebRequestType.FileReturn;
                        bundledatas.return_object = this;
                        man.CallWeb(bundledatas);
                        downloadcounter++;

                        if (filename == "Game")
                            MainResourceNeedToUpdate = true;
                    }
                }
                else
                {
                    if (filename == "R2FVersion")
                        MainFileNeedToUpdate = false;

                    if (filename == "Game")
                        MainResourceNeedToUpdate = false;

                    noneedupdate[filename] = fileversion;
                }
            }

            SaveHolyVersion();

            // 다운 받을 파일이 없으면 그냥 시작 
            if ( downloadcounter == 0 )
                StartGame();
        }

        void SaveHolyVersion()
        {
            String savedata = "";
            foreach (String name in noneedupdate.Keys)
                savedata += name + "," + noneedupdate[name].ToString() + ",";

            File.WriteAllText("HolyVersion.bin", savedata);
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

        /// <summary>
        /// 웹에서부터 결과 도착함
        /// </summary>
        /// <param name="result">결과값들</param>
        public void GetReturnFromWeb(WebSlaveParam result)
        {
            if (result.whattype == WebRequestType.StringReturn)
            {
                VersionCheck(result.return_string);
            }
            else
            {
                switch (result.download_filename)
                {
                    case "./R2Fantasy_Data/mainData":
                    case "./R2Fantasy_Data/PlayerConnectionConfigFile":
                    case "./R2Fantasy_Data/resources.assets":
                    case "./R2Fantasy_Data/sharedassets0.assets":
                        File.WriteAllBytes(result.download_filename, result.return_data);
                        MainFileLoadCounter++;

                        if (MainFileLoadCounter >= 4)
                        {
                            fileversions["R2FVersion"] = result.return_value;
                            noneedupdate["R2FVersion"] = result.return_value;
                            MainFileNeedToUpdate = false;
                        }
                        break;
                    case "Game":
                        File.WriteAllBytes("./Bundle/" + result.download_filename + ".unity3d", result.return_data);
                        MainResourceLoadCounter++;
                        fileversions[result.download_filename] = result.return_value;
                        noneedupdate[result.download_filename] = result.return_value;
                        MainResourceNeedToUpdate = false;
                        break;
                    default:
                        File.WriteAllBytes("./Bundle/" + result.download_filename + ".unity3d", result.return_data);
                        fileversions[result.download_filename] = result.return_value;
                        noneedupdate[result.download_filename] = result.return_value;
                        break;
                }

                downloaded++;

                SaveHolyVersion();


                // 다운로드 완료됨
                if (downloaded == downloadcounter)
                {
                    StartGame();
                }
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Downloader man = Downloader.instance;

            man.Update();

            if (man.NowLoadFile == "version" || man.NowLoadFile.Length < 1)
            {
                label1.Text = "Check Version";
                label2.Text = "";
            }
            else
            {
                label1.Text = man.ReceivedBytes.ToString() + "/" + man.TotalBytes.ToString();
                label2.Text = downloaded.ToString() + "/" + downloadcounter.ToString() + " files updated";
            }

            if ( man.TotalBytes > 0 )
                progressBar1.Value = (int)(man.ReceivedBytes * 100 / man.TotalBytes);

            if ( downloadcounter > 0 )
                progressBar2.Value = downloaded * 100 / downloadcounter;

            if (!MainFileNeedToUpdate && !MainResourceNeedToUpdate)
                button3.Visible = true;
            else
                button3.Visible = false;
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            button3.Visible = false;

            StartGame();
        }
    }
}
