using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUT
{
    public class MutLog
    {
        private static StringBuilder logBuilder = new StringBuilder();

        public static void AppendInfo(string str)
        {
            logBuilder.AppendLine(String.Format("Info: {0}", str));
        }

        public static void WriteFile(string filename)
        {
            if (logBuilder.Length > 0)
            {
                System.IO.File.WriteAllText(filename, logBuilder.ToString());
            }
            logBuilder.Clear();
        }
    }
}
