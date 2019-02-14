using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace DDRVersionTools
{
    class HttpDownloader
    {
        bool bComplete = false;

        //string basePath = "http://111.230.250.213:8000/Distribution/";    mode->"Debug or Release"
        public void DownloadRecent(string basePath,string filename,string mode = "Debug")
        {
            string verPath = GetVersionPath(basePath,mode);
            string url = basePath + verPath + "/" + filename;

            DownloadFile(url, filename);
        }


        //http://111.230.250.213:8000/Distribution/Debug.txt
        public string GetVersionPath(string basePath,string mode)
        {
            using (var client = new WebClient())
            {
                string path = client.DownloadString(basePath + mode + @".txt");
                return path;
            }
        }
    



        public void DownloadFile(string url,string filename)
        {
            using (var client = new WebClient())
            {
                Console.WriteLine(string.Format("Downloading: {0}", url));

                client.DownloadProgressChanged += OnDownloadProgressChanged;
                client.DownloadFileCompleted += OnDownloadFileCompleted;
                client.DownloadFileAsync(new Uri(url), filename);

                while(!bComplete)
                {
                    Thread.Sleep(1000);
                }
            }

        }

        void OnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {



            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());

            double percentage = bytesIn / totalBytes * 100;

            Console.WriteLine(string.Format("Downloaded {0} of {1}  Progress: {2}", e.BytesReceived,e.TotalBytesToReceive, percentage));
        }
        public void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            bComplete = true;
            Console.WriteLine(string.Format("Downloading Complete"));
        }
    }
}
