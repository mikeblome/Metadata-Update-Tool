using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace MUT
{
    public class CommandBuilder
    {
        StringBuilder sb;

        public static readonly string Header = "FILENAME\tACTION\tTAG\tVALUE\tFORMAT";

        public string outputFile { get; private set; }

        public CommandBuilder(string filename)
        {
            this.outputFile = filename;
            this.sb = new StringBuilder(Header);
            this.sb.Append(Environment.NewLine);
        }

        public bool ParseFile(string filename)
        {
            string filedata = File.ReadAllText(filename);

            // Get YML

            var beg = filedata.IndexOf("---") + 3;
            var end = filedata.IndexOf("---", beg); // We assume "---" never appears inside a tag

            if ((beg == -1) || (end == -1))
            {
                Console.WriteLine("CommandBuilder: could not find metadata section in {0}", filename);

                return false;
            }

            // Parse YML into a list of Tag objects
            var tagList = YMLMeister.ParseYML2(filedata);

            // Append each tag's data to the output string
            foreach (var tag in tagList)
            {
                sb.Append(filename);
                sb.Append("\t");
                sb.Append("IGNORE");
                sb.Append("\t");
                sb.Append(tag.TagName);
                sb.Append("\t");
                sb.Append(tag.TagValuesBracketedOrSingle());
                sb.Append("\t");
                sb.Append(tag.TagFormatString);
                sb.Append(Environment.NewLine);
            }

            return true;
        }

        public void WriteFile(bool toOutputFile)
        {
            if (toOutputFile && !String.IsNullOrEmpty(outputFile))
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

    }
}
