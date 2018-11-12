using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeFactoryWorker
{
    public static class Logger
    {
        public static void log(string msg)
        {
            string filepath = ".\\WorkerLog.txt";
            string entry = "[" + DateTime.Now.ToString() + "] "+ msg;
            using (StreamWriter sw = new StreamWriter(filepath, true))
            {
                sw.WriteLine(entry);
                sw.Close();
            }
            Console.WriteLine(entry);
        }
    }
}
