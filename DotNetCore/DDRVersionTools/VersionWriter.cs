using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DDRVersionTools
{
    class VersionWriter
    {
        public void WriteTime(string filename,int linenum)
        {
            string[] file = File.ReadAllLines(filename);

            for(int i = 0; i < file.Length;i++)
            {
                if(i == linenum)
                {
                    string line = file[i];
                    DateTime dt = DateTime.Now;
                    line = string.Format("std::string g_BuildTime = \"{0}\";", dt.ToString());
                    file[i] = line;
                }
            }

            File.WriteAllLines(filename, file);
        }

        public void WriteTimeCS(string filename, int linenum)
        {
            string[] file = File.ReadAllLines(filename);

            for (int i = 0; i < file.Length; i++)
            {
                if (i == linenum)
                {
                    string line = file[i];
                    DateTime dt = DateTime.Now;
                    line = string.Format("        public static string BuildTime = \"{0}\";", dt.ToString());
                    file[i] = line;
                }
            }

            File.WriteAllLines(filename, file);
        }
    }
}
