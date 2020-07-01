using ConfigurationParser;
using LitJson;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace DDRVersionTools
{
    public class UpgradeHelper
    {
        public static Parser m_ConfigParser;
        public string m_Heading = "下载设置";
        public string m_HttpAddr;
        public string m_AppName;
        public string m_DebugMode;


        public static string m_LargeFileName;
        public static string m_LargeFilePath;
        public static string m_SeqFilePath;

        public static string m_IgnoreFileName;
        public static string m_IgnoreFilePath;

        public string m_CurrentVersion;
        List<string> m_BaseFileList = new List<string>();
        List<string> m_CurFileList = new List<string>();
        List<string> m_NewverFileList = new List<string>();
        List<string> m_SeqwFileList = new List<string>();
        List<string> m_IgnoreFileList = new List<string>();


        public UpgradeHelper()
        {
            m_ConfigParser = new Parser("Config.dat");
            m_HttpAddr = m_ConfigParser.GetString(m_Heading, "服务器地址");
            m_HttpAddr = m_HttpAddr.TrimEnd('/');
            m_AppName = m_ConfigParser.GetString(m_Heading, "程序名");
            m_DebugMode = m_ConfigParser.GetString(m_Heading, "调试模式");

            m_LargeFileName = m_ConfigParser.GetString(m_Heading, "大文件表文件");
            m_LargeFilePath = m_ConfigParser.GetString(m_Heading, "大文件路径");
            m_SeqFilePath = m_ConfigParser.GetString(m_Heading, "流程文件路径");


            m_IgnoreFileName = m_ConfigParser.GetString(m_Heading, "忽略检查文件表文件");

            m_CurrentVersion = m_ConfigParser.GetString(m_Heading, "当前版本");


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
                if(fileName.Contains("VersionTools"))
                {
                    Console.WriteLine("VersionTools File Check");
                }

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
        static void CheckMd5Dwonload(string url, string fileName, string relativeDir)
        {
            fileName = fileName.TrimStart('/');
            relativeDir = relativeDir.Trim('/');
            using (var client = new WebClient())
            {
                string fullPath = url + "/" + relativeDir + "/" + fileName;
                string localFullPath = relativeDir + "/" + fileName;
                localFullPath = localFullPath.Replace("\\", "/");
                localFullPath = localFullPath.Replace("//", "/");
                localFullPath = localFullPath.TrimStart('/');
                localFullPath = localFullPath.TrimStart('\\');

                string localMd5 = GetMD5HashFromFile(localFullPath);
                string remoteMd5 = client.DownloadString(fullPath + "?md5");

                if (localMd5 == remoteMd5)
                {
                    Console.WriteLine(string.Format("\nFiles Md5 are Same : {0}", localFullPath));
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

        static void CheckExistDwonload(string url, string fileName, string relativeDir)
        {
            fileName = fileName.TrimStart('/');
            relativeDir = relativeDir.Trim('/');
            relativeDir = relativeDir.Trim('\\');
            string fullPath = url + "/" + relativeDir + "/" + fileName;
            string localFullPath = relativeDir + "/" + fileName;
            localFullPath = localFullPath.Replace("\\", "/");
            localFullPath = localFullPath.Replace("//", "/");
            localFullPath = localFullPath.TrimStart('/');
            localFullPath = localFullPath.TrimStart('\\');



            string saveFilePath = relativeDir + "/" + Path.GetFileName(fullPath);
            saveFilePath = localFullPath.Replace("\\", "/");
            saveFilePath = localFullPath.Replace("//", "/");
            saveFilePath = localFullPath.TrimStart('/');
            saveFilePath = localFullPath.TrimStart('\\');
            if (!File.Exists(saveFilePath))
            {

                Console.WriteLine("\n下载缺失文件:" + saveFilePath);
                saveFilePath = saveFilePath.TrimStart('/');
                saveFilePath = saveFilePath.TrimStart('\\');
                HttpDownloader httpDownloader = new HttpDownloader();
                httpDownloader.DownloadFile(fullPath, saveFilePath);
            }
            else
            {

            }


        }

        bool FillCurList()
        {
            try
            {

                string progressName = "\n正在查询本地文件";
                AsyncServer.Instance.SetProgress(progressName, 0);

                Console.WriteLine(progressName);
                string[] currentFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories);

                foreach (string f in currentFiles)
                {
                    m_CurFileList.Add(f.Replace(Directory.GetCurrentDirectory(), "").Trim('\\').Replace("\\", "/").Replace("//", "/").Trim('/'));

                }


                progressName = "\n正在查询基础版本文件";
                AsyncServer.Instance.SetProgress(progressName, 0);
                Console.WriteLine(progressName);

                string url = m_HttpAddr + "/" + m_AppName + "/" + m_DebugMode + "/" + "Base";


                string jfilelist;
                using (var client = new WebClient())
                {
                    jfilelist = client.DownloadString(url);
                }

                List<List<string>> fileList = JsonMapper.ToObject<List<List<string>>>(jfilelist);



                progressName = "\n下载缺失文件";
                AsyncServer.Instance.SetProgress(progressName, 0);

                float i = 0;
                foreach (var ls in fileList)
                {
                    string fileName = ls[0];
                    string relativeDir = ls[1];

                    string f = relativeDir + "/" + fileName;
                    f = f.Trim('\\').Replace("\\", "/").Replace("//", "/").Trim('/');
                    if (m_IgnoreFileList.Contains(f) && File.Exists(f))
                    {
                        continue;
                    }


                    AsyncServer.Instance.SetProgress(progressName, i / fileList.Count);

                    CheckExistDwonload(url, fileName, relativeDir);

                    m_BaseFileList.Add(f);

                    i+=1;
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }

        bool DownloadVersionFiles(string version)
        {
            try
            {
                string progressName = "\n正在下载版本文件";
                AsyncServer.Instance.SetProgress(progressName, 0);

                Console.WriteLine(progressName);
                string url = m_HttpAddr + "/" + m_AppName + "/" + m_DebugMode + "/" + version;



                Console.WriteLine("\n将要升级到版本:" + version);
                string jfilelist;
                using (var client = new WebClient())
                {
                    jfilelist = client.DownloadString(url);
                }

                List<List<string>> fileList = JsonMapper.ToObject<List<List<string>>>(jfilelist);

                float index = 0;
                foreach (var ls in fileList)
                {
                    string fileName = ls[0];
                    string relativeDir = ls[1];

                    string f = relativeDir + "/" + fileName;
                    f = f.Trim('\\').Replace("\\", "/").Replace("//", "/").Trim('/');
                    if (m_IgnoreFileList.Contains(f) && File.Exists(f))
                    {
                        continue;
                    }

                    CheckMd5Dwonload(url, fileName, relativeDir);
                    m_NewverFileList.Add((relativeDir.Trim('\\').Replace("\\", "/") + "/" + fileName).Replace("//", "/").Trim('/'));

                    index++;
                    AsyncServer.Instance.SetProgress(progressName, index / fileList.Count);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("\nDownloadVersionFiles Error:" + e.Message);
                return false;
            }

        }
        void FillIgnoreFiles(ref string version)
        {
            try
            {
                string progressName = "\n正在检查版本信息";
                AsyncServer.Instance.SetProgress(progressName, 0);

                Console.WriteLine(progressName);
                if (string.IsNullOrEmpty(version))
                {

                    string urlVerList = m_HttpAddr + "/" + m_AppName + "/" + m_DebugMode + @".txt";
                    using (var client = new WebClient())
                    {
                        string path = client.DownloadString(urlVerList);
                        string[] vers = path.Split('\n');
                        version = vers[vers.Length - 1];
                    }
                }

                if (string.IsNullOrEmpty(m_IgnoreFileName))
                {
                    return;
                }


                string url = m_HttpAddr + "/" + m_AppName + "/" + m_DebugMode + "/" + version + "/";



                Console.WriteLine("\n正在下载忽略检查列表:" + version + "/" + m_IgnoreFileName);


                CheckMd5Dwonload(url, m_IgnoreFileName, "");


                Console.WriteLine("\n正在读取忽略检查文件列表");
                if (File.Exists(m_IgnoreFileName))
                {

                    string ignoreFileTxt = File.ReadAllText(m_IgnoreFileName);
                    string[] ignoreFiles = ignoreFileTxt.Split('\n');
                    for (int i = 0; i < ignoreFiles.Length; i++)
                    {
                        var temp = ignoreFiles[i].Trim();
                        temp = temp.Replace("\\", "/");
                        temp = temp.Replace("//", "/");
                        temp = temp.Replace(m_DebugMode + "/" + m_LargeFilePath, "");
                        ignoreFiles[i] = temp;
                        Console.WriteLine(ignoreFiles[i]);
                        
                        m_IgnoreFileList.Add((ignoreFiles[i].Trim('\\').Replace("\\", "/")).Replace("//", "/").Trim('/'));
                    }

                }

                
            }
            catch (Exception e)
            {

                Console.WriteLine("\nLoadIgnoreFiles Error:" + e.Message);

                return ;
            }
        }

        bool DownloadLargeFiles()
        {
            try
            {
                if (string.IsNullOrEmpty(m_LargeFileName) || string.IsNullOrEmpty(m_LargeFilePath))
                {
                    return true;
                }

                string progressName = "\n正在下载大容量文件";
                AsyncServer.Instance.SetProgress(progressName, 0);

                Console.WriteLine(progressName);
                if (File.Exists(m_LargeFileName))
                {

                    string largeFileTxt = File.ReadAllText(m_LargeFileName);
                    string[] largeFiles = largeFileTxt.Split('\n');

                    float index = 0;
                    for (int i = 0; i < largeFiles.Length; i++)
                    {
                        var temp = largeFiles[i].Trim();
                        temp = temp.Replace("\\", "/");
                        temp = temp.Replace("//", "/");
                        temp = temp.Replace(m_DebugMode + "/" + m_LargeFilePath, "");
                        largeFiles[i] = temp;
                        Console.WriteLine(largeFiles[i]);


                        string fileName = Path.GetFileName(largeFiles[i]);
                        string relativeDir = largeFiles[i].Replace(fileName, "");


                        string largeFileUrl = m_HttpAddr + "/" + m_AppName + "/" + m_DebugMode + "/" + m_LargeFilePath;

                        CheckMd5Dwonload(largeFileUrl, fileName, relativeDir);
                        m_NewverFileList.Add((relativeDir.Trim('\\').Replace("\\", "/") + "/" + fileName).Replace("//", "/").Trim('/'));


                        index++;
                        AsyncServer.Instance.SetProgress(progressName, index / largeFiles.Length);
                    }

                }


                return true;
            }
            catch (Exception e)
            {

                Console.WriteLine("\nDownloadLargeFiles Error:" + e.Message);

                return false;
            }
        }

        bool DownloadSeqFilesAndRun()
        {

            try
            {
                if (string.IsNullOrEmpty(m_SeqFilePath))
                {
                    return true;
                }

                Console.WriteLine("\n正在下载顺序运行文件并运行");

                int curVer = 0;

                if (m_CurrentVersion.IndexOf('.') != -1)
                {
                    int lastp = m_CurrentVersion.LastIndexOf('.') + 1;
                    string scurVer = m_CurrentVersion.Substring(lastp, m_CurrentVersion.Length - lastp);
                    try
                    {
                        curVer = Convert.ToInt32(scurVer);

                    }
                    catch (Exception e)
                    {

                    }
                }


                string seqUrl = m_HttpAddr + "/" + m_AppName + "/" + m_DebugMode + "/" + m_SeqFilePath;


                string jseqfilelist;
                using (var client = new WebClient())
                {
                    jseqfilelist = client.DownloadString(seqUrl);
                }

                List<List<string>> seqfileList = JsonMapper.ToObject<List<List<string>>>(jseqfilelist);

                foreach (var ls in seqfileList)
                {
                    string fileName = ls[0];
                    string relativeDir = ls[1];



                    CheckMd5Dwonload(seqUrl, fileName, relativeDir);

                    m_SeqwFileList.Add((relativeDir.Trim('\\').Replace("\\", "/") + "/" + fileName).Replace("//", "/").Trim('/'));
  


                    int last_ = fileName.LastIndexOf('_') + 1;
                    string sseqVer = fileName.Substring(last_, fileName.Length - last_ - 4);//-4 is delete ".exe"
                    int seqVer = Convert.ToInt32(sseqVer);

                    if (seqVer > curVer)
                    {
                        string temp = relativeDir.Trim('\\').Trim('/');
                        string exePath = Directory.GetCurrentDirectory() + "/" + temp + "/" + fileName;
                        if (File.Exists(exePath))
                        {
                            var process = Process.Start(exePath);
                            process.WaitForExit();

                            Console.WriteLine("\nRun:" + fileName);
                        }
                        else
                        {
                            Console.WriteLine("\nFile Not Exist:" + exePath);
                        }
                    }



                }

                return true;
            }
            catch (Exception e)
            {

                Console.WriteLine("\nDownloadSeqFilesAndRun Error:" + e.Message);

                return false;
            }

        }

        bool DeleteUnusedFiles()
        {
            try
            {
                Console.WriteLine("\n正在删除多除文件");
                foreach (string curF in m_CurFileList)
                {
                    if (m_BaseFileList.Contains(curF))
                    {
                        continue;
                    }
                    if (m_IgnoreFileList.Contains(curF))
                    {
                        continue;
                    }
                    if (curF.Contains("OneRoute"))
                    {
                        continue;
                    }
                    if (m_SeqwFileList.Contains(curF))
                    {
                        continue;
                    }
                    if (!m_NewverFileList.Contains(curF) && !curF.Contains("DDRVersionTools.exe") && !curF.Contains("Config.dat") && !curF.Contains("DDRVersionTools.pdb"))
                    {


                        File.Delete(curF);

                        Console.WriteLine("\nDelete File:" + curF);
                    }
                }
                return true;
            }
            catch (Exception e)
            {

                return false;
            }

        }

        bool WriteNewVersion(string version)
        {
            try
            {

                Console.WriteLine("\n写入新版本");
                m_ConfigParser.SetString(m_Heading, "当前版本", version);
                return true;
            }
            catch (Exception e)
            {

                return false;

            }
        }


        public void Upgrade(string version)
        {
            try
            {

                FillIgnoreFiles(ref version);


                bool ret = true;
                ret = FillCurList();
                if (!ret) return;


                ret = DownloadVersionFiles(version);
                if (!ret) return;


                ret = DownloadLargeFiles();
                if (!ret) return;
                ret = DownloadSeqFilesAndRun();
                if (!ret) return;
                ret = DeleteUnusedFiles();
                if (!ret) return;
                ret = WriteNewVersion(version);
                if (!ret) return;

                AsyncServer.Instance.SetProgress("Idle", 0);
            }
            catch (Exception e)
            {
                Console.Write(e.Message);

                AsyncServer.Instance.SetProgress("Idle",0);
            }



        }

        public void ShowVersion(out string baseVersion,out string latestVersion,out string[] vers,out string currentVersion)
        {
            string url = m_HttpAddr + "/" + m_AppName + "/" + m_DebugMode + @".txt";
            using (var client = new WebClient())
            {
                string path = client.DownloadString(url);
                vers = path.Split('\n');

                currentVersion = m_CurrentVersion;
                Console.Write("\n当前版本:" + m_CurrentVersion);

                int i = 0;
                Console.Write("\n当前可用版本:");

                for (i = 0; i < vers.Length; i++)
                {
                    vers[i] = vers[i].Trim();
                    Console.Write("\n     " + vers[i]);
                }

                baseVersion = vers[0];
                latestVersion = vers[vers.Length - 1];


                Console.Write("\n基础版本:" + baseVersion);
                Console.Write("\n最新版本:" + latestVersion);

            }
        }
    }
}
