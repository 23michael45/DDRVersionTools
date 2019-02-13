using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDRVersionTools
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args[0] == "compile-time")
            {
                string filename = args[1];
                int linenum = Convert.ToInt32(args[2]);
                VersionWriter vw = new VersionWriter();
                vw.WriteTime(filename, linenum);
            }
        }
    }
}
