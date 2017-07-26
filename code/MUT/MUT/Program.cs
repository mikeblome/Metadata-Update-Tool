using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MUT
{

    class Program
    {
        static void Main(string[] args)
        {   // Get a list of filenames from a folder
            string[] fileArray = Directory.GetFiles(@"C:\Users\tglee\vstest\docs\install\", "*.md", SearchOption.AllDirectories);

            // Write header values for extract file
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("File Name\tAction\tTag\tValues\tFormat");

            // Open a file in the list

            foreach (var filename in fileArray)
            {

                string content = File.ReadAllText(filename);

                // Get YML

                var beg = content.IndexOf("---") + 3;
                var end = content.IndexOf("---", beg);
                if ((beg == -1) || (end == -1))
                {
                    Console.WriteLine("error in metadata section in " + filename);

                    continue; // uncomment this when the code is actually inside a loop.
                }

                // Parse YML by looping over the lines
                var dict = ParseYML(content.Substring(beg, end - beg));
                ////   Console.WriteLine(s.Substring(beg, end - beg));
                foreach (var v in dict)
                {
                    sb.Append(filename);
                    sb.Append("\t");
                    sb.Append("Action");
                    sb.Append("\t");
                    sb.Append(v.Key);
                    sb.Append("\t");
                    sb.Append(v.Value);
                    sb.Append("\t");
                    sb.Append("Format");
                    sb.Append(Environment.NewLine);


                    Console.WriteLine(v.Key + "    " + v.Value);
                }


                // write to sb
            }//end foreach

            File.WriteAllText(@"c:\temp\MyTest.txt", sb.ToString());
            Console.ReadLine();
        }

        // ParseXML

        static Dictionary<string, string> ParseYML(string yml)
        {
            var d = new Dictionary<string, string>();
            var lines = yml.Split('\n');
            Regex rgx = new Regex(@"[A-Za-z\._]+:");
            string currentKey = "";
            StringBuilder currentVal = new StringBuilder("{");

            bool inMultiline = false;
            foreach (var v in lines)
            {
                if (rgx.IsMatch(v))
                {
                    inMultiline = false;
                    if (currentVal.Length > 1)
                    {
                        currentVal.Append("}");
                        d[currentKey] = currentVal.ToString().Replace("\"- ", "\", ").Replace("{-", "{");
                        currentVal.Clear();
                        currentVal.Append("{");
                    }

                    var pair = v.Split(':');
                    string str;
                    bool b = d.TryGetValue(pair[0], out str);
                    if (!b)
                    {
                        d.Add(pair[0].Trim(), pair[1].Trim());
                        currentKey = pair[0].Trim();
                    }
                }
                else
                {
                    int beg = v.IndexOf(" - ");
                    if (beg >= 0 && beg < 5)
                    {
                        inMultiline = true;
                        currentVal.Append(v.Substring(beg).Trim());
                    }
                }
            }
            if (currentVal.Length > 1)
            {
                currentVal.Append("}");
                d[currentKey] = currentVal.ToString().Trim().Replace("\"- ", "\", ").Replace("{-", "{"); ;
            }
            return d;
        }
    }

}
