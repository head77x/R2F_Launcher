using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Timers;
using LitJson;

namespace R2FLauncher
{
    public enum WebRequestType { StringReturn, FileReturn };


    public class WebSlaveParam
    {
        public String url;
        public WebRequestType whattype;
        public String download_filename;
        public byte[] return_data;
        public String return_string;
        public UInt32 return_value;
        public Form1 return_object;
    }

    public class Downloader
    {
        static Downloader m_instance;
        public static Downloader instance
        {
            get
            {
                if (m_instance == null)
                {
                    m_instance = new Downloader();
                    m_instance.Start();
                }

                return m_instance;
            }
        }


        bool GetReturned = false;
        Queue<WebSlaveParam> WebCallStack;

        WebClient webslave;

        static String passcode = "BrandonIsGenius!";

        bool m_Dirty = true;

        public int LoadPercent = 0;
        public long TotalBytes = 0;
        public long ReceivedBytes = 0;
        public String NowLoadFile = "";

        bool NeedToQuit = false;

        void Start()
        {
            if (!m_Dirty)
                return;

            m_Dirty = false;

            webslave = new WebClient();
            WebCallStack = new Queue<WebSlaveParam>();
            GetReturned = false;

            webslave.DownloadProgressChanged +=     // 다운로드 작업 진행율이 변경될때 발생됩니다.
                new DownloadProgressChangedEventHandler(DownloadClient_DownloadProgressChanged);
            webslave.DownloadStringCompleted +=       // DownloadString 함수 작업이 완료됐을 때 발생됩니다.
                new DownloadStringCompletedEventHandler(DownloadClient_DownloadStringCompleted);
            webslave.DownloadDataCompleted +=         // DownloadData 함수 작업이 완료됐을 때 발생됩니다.
                    new DownloadDataCompletedEventHandler(DownloadClient_DownloadDataCompleted);
        }

        public void Update()
        {
            if (!GetReturned && WebCallStack.Count > 0)
            {
                WebSlaveParam newparam = (WebSlaveParam)WebCallStack.Dequeue();

                switch (newparam.whattype)
                {
                    case WebRequestType.StringReturn:
                        webslave.DownloadStringAsync(new Uri(newparam.url), newparam);
                        break;
                    case WebRequestType.FileReturn:
                        webslave.DownloadDataAsync(new Uri(newparam.url), newparam);
                        break;
                }

                GetReturned = true;
            }
        }

        public void CallWeb(WebSlaveParam param)
        {
            if (m_Dirty)
                Start();

            WebCallStack.Enqueue(param);
        }

        void DownloadClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            // 다운로드 요청 함수에서 마지막 인자로 넘어온 구분 이름을 가져옵니다.
            WebSlaveParam original = (WebSlaveParam)e.UserState;

            NowLoadFile = original.download_filename;
            LoadPercent = e.ProgressPercentage;                   // 비동기 작업의 진행을 나타내는 백분율 값입니다.
            TotalBytes = e.TotalBytesToReceive;  // 다운받아야 할 데이터 길이입니다.
            ReceivedBytes = e.BytesReceived;                   // 현재까지 다운 받은 데이터 길이입니다.
        }
 
        void DownloadClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            WebSlaveParam original = (WebSlaveParam)e.UserState;

            Exception Error = e.Error;

            if (e.Cancelled)
                NeedToQuit = true;

            if (Error != null)
            {
                webslave.DownloadStringAsync(new Uri(original.url), original);
                return;
            }

            original.return_string = e.Result;

            original.return_object.GetReturnFromWeb(original);

            GetReturned = false;
        }
 
        void DownloadClient_DownloadDataCompleted(object sender, DownloadDataCompletedEventArgs e)
        {
            WebSlaveParam original = (WebSlaveParam)e.UserState;

            Exception Error = e.Error;

            if (e.Cancelled)
                NeedToQuit = true;

            if (Error != null)
            {
                webslave.DownloadDataAsync(new Uri(original.url), original);
                return;
            }

            original.return_data = e.Result;

            original.return_object.GetReturnFromWeb(original);

            GetReturned = false;
        }
 
    }
}
