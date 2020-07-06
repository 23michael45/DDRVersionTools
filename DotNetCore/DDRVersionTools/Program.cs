using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.Serialization;

using LitJson;
using ConfigurationParser;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DDRVersionTools
{
    public class Program
    {

        static void Main(string[] args)
        {


            //ConsoleWindow.QuickEditMode(false);


            AsyncServer server = new AsyncServer();



            if (args.Length == 0)
            {
                args = new string[1];
                args[0] = "cmdline";
            }


            if (args[0] == "compile-time")
            {
                CompileTime(args);
            }
            else if (args[0] == "compile-time-cs")
            {
                CompileTimeCS(args);
            }
            else if (args[0] == "download-recent")
            {
                DownloadRecent(args);
            }
            else if (args[0] == "download-file")
            {
                DownloadRecent(args);
            }
            else if (args[0] == "upgrade")
            {
                Upgrade("");
            }
            else if (args[0] == "cmdline")
            {
                bool quit = false;
                do
                {
                    Console.WriteLine("\nDDR版本更新工具v1.0");
                    Console.WriteLine("\n----------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("----------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("请输入指令:");
                    Console.WriteLine("   v                                     显示当前版本 最新版本及可升级版本");
                    Console.WriteLine("   u [ver]                               升级到版本ver  ver为空则升级到最新版本");
                    Console.WriteLine("   b                                     查看工具版本");

                    Console.WriteLine("   quit                                  退出");


                    string cmd = Console.ReadLine();
                    if (cmd == "quit")
                    {
                        quit = true;
                    }
                    else if (cmd != null && cmd.StartsWith("v"))
                    {
                        ShowVersion();

                    }
                    else if (cmd != null && cmd.StartsWith("u"))
                    {
                        List<string> cmdargs = cmd.Split(' ').ToList();
                        if (cmdargs.Count < 2)
                        {
                            cmdargs.Add("");
                        }
                        Upgrade(cmdargs[1]);

                    }
                    else if (cmd != null && cmd.StartsWith("b"))
                    {

                        Console.Write("\n当前工具版本:" + Version.BuildTime);
                    }


                    } while (!quit);

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
        static void CompileTimeCS(string[] args)
        {
            string filename = args[1];
            int linenum = Convert.ToInt32(args[2]);
            VersionWriter vw = new VersionWriter();
            vw.WriteTimeCS(filename, linenum);
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

        static void ShowVersion()
        {

            UpgradeHelper helper = new UpgradeHelper();

            string baseVersion;
            string latestVersion;
            string[] vers;
            string currentVersion;
            helper.ShowVersion(out baseVersion,out latestVersion,out vers,out currentVersion);
        }


        static void Upgrade(string version)
        {
            UpgradeHelper helper = new UpgradeHelper();
            helper.Upgrade(version);
           
            
        }





    }
    
}
