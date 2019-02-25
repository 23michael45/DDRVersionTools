﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Runtime.Serialization;

using LitJson;
using ConfigurationParser;

namespace DDRVersionTools
{
    public class Program
    {
        public static string m_HttpAddr;
        public static string m_AppName;
        public static string m_DebugMode;

        public static string m_LargeFileName;
        public static string m_LargeFilePath;
        public static string m_SeqFilePath;

        static void Main(string[] args)
        {
            var parser = new Parser("Config.dat");
            string heading = "下载设置";
            m_HttpAddr = parser.GetString(heading, "服务器地址");
            m_HttpAddr = m_HttpAddr.TrimEnd('/');
            m_AppName = parser.GetString(heading, "程序名");
            m_DebugMode = parser.GetString(heading, "调试模式");

            m_LargeFileName = parser.GetString(heading, "大文件表文件");
            m_LargeFilePath = parser.GetString(heading, "大文件路径");
            m_SeqFilePath = parser.GetString(heading, "流程文件路径");



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
            else if (args[0] == "cmdline")
            {
                bool quit = false;
                do
                {
                    Console.WriteLine("\n----------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("----------------------------------------------------------------------------------------------------------------------");
                    Console.WriteLine("请输入指令:");
                    Console.WriteLine("   version                               显示当前版本 最新版本及可升级版本 addr为服务器地址 mode为副目录地址");
                    Console.WriteLine("   upgrade [v]                           升级到版本v");

                    Console.WriteLine("   quit                                  退出");


                    string cmd = Console.ReadLine();
                    if (cmd == "quit")
                    {
                        quit = true;
                    }
                    else if (cmd.StartsWith("version"))
                    {
                        ShowVersion();

                    }
                    else if (cmd.StartsWith("upgrade"))
                    {
                        List<string> cmdargs = cmd.Split(' ').ToList();
                        if (cmdargs.Count < 2)
                        {
                            cmdargs.Add("Base");
                        }
                        Upgrade(cmdargs[1]);

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
            string url = m_HttpAddr + "/" + m_AppName + "/" + m_DebugMode + @".txt";
            using (var client = new WebClient())
            {
                string path = client.DownloadString(url);
                string[] vers = path.Split('\n');

                int i = 0;
                Console.Write("\n当前可用版本");

                for (i = 0; i < vers.Length;i++)
                {
                    vers[i] = vers[i].Trim();
                    Console.Write("\n     " + vers[i]);
                }

                string baseVersion = vers[0];
                string currentVersion = vers[vers.Length - 1];


                Console.Write("\n基础版本:" + baseVersion);
                Console.Write("\n最新版本:" + currentVersion);

            }
        }

        static  void CheckMd5Dwonload(string url,string fileName,string relativeDir)
        {
            fileName = fileName.TrimStart('/');
            relativeDir = relativeDir.Trim('/');
            using (var client = new WebClient())
            {
                string fullPath = url + "/" + relativeDir + "/" + fileName;
                string localFullPath = relativeDir + "/" + fileName;
                localFullPath = localFullPath.Replace("\\", "/");
                localFullPath = localFullPath.Replace("//","/");
                localFullPath = localFullPath.TrimStart('/');
                localFullPath = localFullPath.TrimStart('\\');

                string localMd5 = GetMD5HashFromFile(localFullPath);
                string remoteMd5 = client.DownloadString(fullPath + "?md5");

                if (localMd5 == remoteMd5)
                {
                    Console.WriteLine(string.Format("Files Md5 are Same : {0}", localFullPath));
                }
                else
                {

                    string saveFilePath = relativeDir + "/" + Path.GetFileName(fullPath);
                    saveFilePath = saveFilePath.TrimStart('/');
                    saveFilePath = saveFilePath.TrimStart('\\');
                    HttpDownloader httpDownloader = new HttpDownloader();
                    httpDownloader.DownloadFile(fullPath, saveFilePath);
                }
            }
        }

        static void Upgrade(string version)
        {
            try
            {

                string url = m_HttpAddr + "/" + m_AppName + "/" + m_DebugMode + "/" + version;


                string jfilelist;
                using (var client = new WebClient())
                {
                    jfilelist = client.DownloadString(url);
                }

                List<List<string>> fileList = JsonMapper.ToObject<List<List<string>>>(jfilelist);

                foreach (var ls in fileList)
                {
                    string fileName = ls[0];
                    string relativeDir = ls[1];

                    CheckMd5Dwonload(url, fileName, relativeDir);

                }


                if (File.Exists(m_LargeFileName))
                {

                    string largeFileTxt = File.ReadAllText(m_LargeFileName);
                    string[] largeFiles = largeFileTxt.Split('\n');
                    for (int i = 0; i < largeFiles.Length; i++)
                    {
                        var temp = largeFiles[i].Trim();
                        temp = temp.Replace("\\", "/");
                        temp = temp.Replace("//", "/");
                        temp = temp.Replace(m_DebugMode + "/" + m_LargeFilePath, "");
                        largeFiles[i] = temp;
                        Console.WriteLine(largeFiles[i]);


                        string fileName =  Path.GetFileName(largeFiles[i]);
                        string relativeDir = largeFiles[i].Replace(fileName, "");


                        string largeFileUrl = m_HttpAddr + "/" + m_AppName + "/" + m_DebugMode + "/" + m_LargeFilePath;

                        CheckMd5Dwonload(largeFileUrl, fileName, relativeDir);
                    }

                }


            }
            catch (Exception e)
            {
                Console.Write(e.Message);
            }


            
        }





        /// <summary>
        /// 获取文件MD5值
        /// </summary>
        /// <param name="fileName">文件绝对路径</param>
        /// <returns>MD5值</returns>
        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {



                    FileStream file = new FileStream(fileName, FileMode.Open);
                    System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
                    byte[] retVal = md5.ComputeHash(file);
                    file.Close();

                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < retVal.Length; i++)
                    {
                        sb.Append(retVal[i].ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }

            return "";
        }
    }
    
}
