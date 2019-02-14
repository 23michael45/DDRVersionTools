using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DDRVersionTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args[0] == "compile-time")
            {
                CompileTime(args);
            }
            else if (args[0] == "download-recent")
            {
                DownloadRecent(args);
            }
            else if (args[0] == "download-file")
            {
                DownloadRecent(args);
            }
        }

        //DDRVersionTools.exe compile-time Version.h 2
        static void CompileTime(string[] args)
        {
            string filename = args[1];
            int linenum = Convert.ToInt32(args[2]);
            VersionWriter vw = new VersionWriter();
            vw.WriteTime(filename, linenum);

        }

        //DDRVersionTools download-recent http://111.230.250.213:8000/Distribution/ DDR_LocalServer.exe Debug
        static void DownloadRecent(string[] args)
        {
            string url = args[1];
            string filename = args[2];
            string mode = args[3];
            HttpDownloader httpDownloader = new HttpDownloader();
            httpDownloader.DownloadRecent(url, filename,mode);
        }
        //DDRVersionTools download-file http://111.230.250.213:8000/Distribution/Debug/2019-2-13/DDR_LocalServer.exe
        static void DownloadFile(string[] args)
        {
            string url = args[1];
            string filename = Path.GetFileName(url);
            HttpDownloader httpDownloader = new HttpDownloader();
            httpDownloader.DownloadFile(url, filename);
        }
    }
}
