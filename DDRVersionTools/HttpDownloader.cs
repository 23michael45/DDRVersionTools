using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using LitJson;
using TinyWeb;

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



    public class AsyncServer
    {
        public static AsyncServer Instance;
        public class VersionJson
        {
            public string baseVersion { get; set; }
            public string latestVersion { get; set; }
            public string[] vers { get; set; }
            public string currentVersion { get; set; }
        }

        private static Mutex ProgressMutex = new Mutex();
        public class ProgressJson
        {

            public string ProgressName { get; set; }
            public double Progress { get; set; }
        }
        ProgressJson currentProgress = new ProgressJson();

        public void SetProgress(string name,double value)
        {
            ProgressMutex.WaitOne();
            currentProgress.ProgressName = name;
            currentProgress.Progress = value;
            ProgressMutex.ReleaseMutex();
        }


        HttpListener listener;
        bool working = true;
        public AsyncServer()
        {
            Instance = this;
            Thread thread1 = new Thread(ThreadFunc);
            thread1.Start();

            AsyncServer.Instance.SetProgress("Idle", 0);
            working = true;
        }
        public void Stop()
        {
            working = false;
        }

        void ThreadFunc()
        {
            //listener = new HttpListener();

            ////listener.Prefixes.Add("http://+:8081/");
            //listener.Prefixes.Add("http://localhost:8081/");
            //listener.Prefixes.Add("http://127.0.0.1:8081/");

            //listener.Start();
            //while (working)
            //{
            //    try
            //    {
            //        var context = listener.GetContext();
            //        ThreadPool.QueueUserWorkItem(o => HandleRequest(context));
            //    }
            //    catch (Exception)
            //    {
            //        // Ignored for this example
            //    }
            //}

            //listener.Close();



            //TinyWeb.WebServer webServer = new TinyWeb.WebServer();

            //webServer.EndPoint = new IPEndPoint(0, 8081);
            //webServer.ProcessRequest += new TinyWeb.ProcessRequestEventHandler(this.webServer_ProcessRequest);
            //webServer.IsStarted = true;


            using (var server = new NHttp.HttpServer())
            {
                server.RequestReceived += (s, e) =>
                {
                    // The response must be written to e.Response.OutputStream.
                    // When writing text, a StreamWriter can be used.
                    string cmd = e.Request.RawUrl;


                    using (System.IO.BinaryWriter writer = new BinaryWriter(e.Response.OutputStream))
                    {

                        if (cmd == "/ver")
                        {
                            UpgradeHelper helper = new UpgradeHelper();
                            string baseVersion;
                            string latestVersion;
                            string[] vers;
                            string currentVersion;
                            helper.ShowVersion(out baseVersion, out latestVersion, out vers, out currentVersion);


                            VersionJson json = new VersionJson();
                            json.baseVersion = baseVersion;
                            json.latestVersion = latestVersion;
                            json.vers = vers;
                            json.currentVersion = currentVersion;

                            string jsonString;
                            jsonString = JsonMapper.ToJson(json);

                            var data = Encoding.UTF8.GetBytes(jsonString);
                            writer.Write(data, 0, data.Length);
                        }
                        else if (cmd == "/upgrade")
                        {
                            Thread thread1 = new Thread(() =>
                            {
                                UpgradeHelper helper = new UpgradeHelper();
                                helper.Upgrade("");
                            });
                            thread1.Start();

                            var data = Encoding.UTF8.GetBytes("Launched");
                            writer.Write(data, 0, data.Length);
                        }
                        else if (cmd == "/progress")
                        {
                            string jsonString;
                            jsonString = JsonMapper.ToJson(currentProgress);
                            var data = Encoding.UTF8.GetBytes(jsonString);
                            writer.Write(data, 0, data.Length);
                        }

                    }
                };

                server.EndPoint = new IPEndPoint(IPAddress.Loopback, 8081);

                server.Start();
                Console.ReadKey();
            }
        }
        void webServer_ProcessRequest(object sender, ProcessRequestEventArgs args)
        {
            string cmd = args.Request.Path;
            if (cmd == "/ver")
            {
                UpgradeHelper helper = new UpgradeHelper();
                string baseVersion;
                string latestVersion;
                string[] vers;
                string currentVersion;
                helper.ShowVersion(out baseVersion, out latestVersion, out vers, out currentVersion);




                VersionJson json = new VersionJson();
                json.baseVersion = baseVersion;
                json.latestVersion = latestVersion;
                json.vers = vers;
                json.currentVersion = currentVersion;

                string jsonString;
                jsonString = JsonMapper.ToJson(json);

                var data = Encoding.UTF8.GetBytes(jsonString);
                args.Response.BinaryWrite(data);
            }
            else if (cmd == "/upgrade")
            {
                UpgradeHelper helper = new UpgradeHelper();
                helper.Upgrade("");

                var data = Encoding.UTF8.GetBytes("Launched");
                args.Response.BinaryWrite(data);
            }
            else if (cmd == "/progress")
            {
                string jsonString;
                jsonString = JsonMapper.ToJson(currentProgress);
                var data = Encoding.UTF8.GetBytes(jsonString);
                args.Response.BinaryWrite(data);
            }
            
        }

        private void HandleRequest(object state)
        {
            try
            {
                var context = (HttpListenerContext)state;

                context.Response.StatusCode = 200;
                context.Response.SendChunked = true;

                int totalTime = 0;


                if(context.Request.RawUrl == "/ver")
                {
                    UpgradeHelper helper = new UpgradeHelper();
                    string baseVersion;
                    string latestVersion;
                    string[] vers;
                    string currentVersion;
                    helper.ShowVersion(out baseVersion, out latestVersion, out vers, out currentVersion);




                    VersionJson json = new VersionJson();
                    json.baseVersion = baseVersion;
                    json.latestVersion = latestVersion;
                    json.vers = vers;
                    json.currentVersion = currentVersion;

                    string jsonString;
                    jsonString = JsonMapper.ToJson(json);

                    var data = Encoding.UTF8.GetBytes(jsonString);
                    context.Response.OutputStream.Write(data, 0, data.Length);
                }
                else if (context.Request.RawUrl == "/upgrade")
                {
                    UpgradeHelper helper = new UpgradeHelper();
                    helper.Upgrade("");

                    var data = Encoding.UTF8.GetBytes("Launched");
                    context.Response.OutputStream.Write(data, 0, data.Length);
                }
                else if (context.Request.RawUrl == "/progress")
                {
                    string jsonString;
                    jsonString = JsonMapper.ToJson(currentProgress);
                    var data = Encoding.UTF8.GetBytes(jsonString);
                    context.Response.OutputStream.Write(data, 0, data.Length);
                }

                context.Response.OutputStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                // Client disconnected or some other error - ignored for this example
            }
        }
    }
}
