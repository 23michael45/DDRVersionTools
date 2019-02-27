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
        float progress = 0;

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
                Console.Write(string.Format("\nDownloading: {0} ", url));

                client.DownloadProgressChanged += OnDownloadProgressChanged;
                client.DownloadFileCompleted += OnDownloadFileCompleted;

                CreateDirectoryRecursively(filename);

                progress = 0;
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

            //Console.WriteLine(string.Format("Downloaded {0} of {1}  Progress: {2}", e.BytesReceived,e.TotalBytesToReceive, percentage));
            if(percentage - progress > 5)
            {
                progress += 5;
                Console.Write("#");

            }
        }
        public void OnDownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            bComplete = true;
            if(e.Error != null)
            {

                Console.WriteLine(string.Format("Downloading Error : {0}",e.Error.ToString()) );
            }
            else
            {
                Console.Write(string.Format("Complete"));

            }
        }



        public bool CreateDirectoryRecursively(string path)
        {
            try
            {
                path = path.Replace("/", "\\");

                string[] pathParts = path.Split('\\');
                for (var i = 0; i < pathParts.Length; i++)
                {
                    // Correct part for drive letters
                    if (i == 0 && pathParts[i].Contains(":"))
                    {
                        pathParts[i] = pathParts[i] + "\\";
                    } // Do not try to create last part if it has a period (is probably the file name)
                    else if (i == pathParts.Length - 1 && pathParts[i].Contains("."))
                    {
                        return true;
                    }
                    if (i > 0)
                    {
                        pathParts[i] = Path.Combine(pathParts[i - 1], pathParts[i]);
                    }
                    if (!Directory.Exists(pathParts[i]))
                    {
                        Directory.CreateDirectory(pathParts[i]);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;

            }

        }
    }
}
