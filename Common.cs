using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UpdateLocalTimeNTP
{
    public class Common
    {
        static object LockObject = new object();
        

        static public bool WriteAndLogThisLine(string message, bool log, bool logAsError, string filename)
        {
            if (string.IsNullOrWhiteSpace(filename) && (logAsError || log))
            {
                throw new InvalidOperationException("Cannot log to a file where the filename was not specified!");
            }
            Console.WriteLine(message);
            lock (LockObject)
            {
                if (log)
                {
                    System.IO.StreamWriter writeHere = System.IO.File.AppendText(filename);
                    writeHere.AutoFlush = true;
                    writeHere.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ">" + message);
                    writeHere.Close();
                }
                if (logAsError)
                {
                    System.IO.StreamWriter writeErrorHere = System.IO.File.AppendText("ERROR-" + filename);
                    writeErrorHere.AutoFlush = true;
                    writeErrorHere.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ">" + message);
                    writeErrorHere.Close();
                }
            }
            return true;
        }
    }
}
