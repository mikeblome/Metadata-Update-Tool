using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MUT
{
    class CommandBuilder
    {
        StringBuilder sb;

        public string outputFile { get; private set; }

        public CommandBuilder(string filename)
        {
            this.outputFile = filename;
            this.sb = new StringBuilder("Filename\tAction\tTag\tValue\tFormat\r\n");
        }

        public bool ParseFile(string filename)
        {
            string content = File.ReadAllText(filename);

            // Get YML

            var beg = content.IndexOf("---") + 3;
            var end = content.IndexOf("---", beg); // We assume "---" never appears inside a tag
            if ((beg == -1) || (end == -1))
            {
                Console.WriteLine("CommandBuilder: could not find metadata section in {0}", filename);

                return false;
            }

            // Parse YML by looping over the lines
            var dict = ParseYML(content.Substring(beg, end - beg));
            ////   Console.WriteLine(s.Substring(beg, end - beg));
            foreach (var v in dict)
            {
                sb.Append(filename);
                sb.Append("\t");
                sb.Append("IGNORE");
                sb.Append("\t");
                sb.Append(v.Key);
                sb.Append("\t");
                sb.Append(v.Value);
                sb.Append(Environment.NewLine);


                // Console.WriteLine(v.Key + "    " + v.Value);
            }

            return true;
        }

        public void WriteFile(bool toStdOut)
        {
            if (!toStdOut)
            {
                // write to the specified file
                File.WriteAllText(outputFile, sb.ToString());
            }
            else
            {
                // else write sb to console.
                Console.WriteLine(sb.ToString());
            }

        }


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

                // Append format here, since this is where we know it.
                if (inMultiline)
                {
                    currentVal.Append("\tDash");
                }
                else
                {
                    currentVal.Append("\t");
                }

                d[currentKey] = currentVal.ToString().Trim().Replace("\"- ", "\", ").Replace("{-", "{"); ;
            }
            return d;
        }

    }
}
