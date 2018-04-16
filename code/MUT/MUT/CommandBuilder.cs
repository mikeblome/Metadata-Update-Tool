using System;
using System.IO;
using System.Text;

namespace MdExtract
{
    public class CommandBuilder
    {
        StringBuilder sb;

        public static readonly string Header = "FILENAME\tACTION\tTAG\tVALUE\tFORMAT";

        public string OutputFile { get; private set; }


        public string Tag { get; private set; }

        public CommandBuilder(string filename, string tag)
        {
            this.OutputFile = filename;
            this.Tag = tag;
            this.sb = new StringBuilder(Header);
            this.sb.Append(Environment.NewLine);
        }

        public void ParseFile(string filename)
        {
            string filedata = File.ReadAllText(filename);

            // Get YML

            var beg = filedata.IndexOf("---") + 3;
            var end = filedata.IndexOf("---", beg); // We assume "---" never appears inside a tag

            if ((beg == -1) || (end == -1))
            {
                // only warn if lack of tag might mean something
                if (!filename.ToLower().Contains("toc.md"))
                {
                    Console.WriteLine("CommandBuilder: could not find metadata section in {0}", filename);
                }
                return;
            }

            // Parse YML into a list of Tag objects
            YMLMeister.CurrentFile = filename;
            var tagList = YMLMeister.ParseYML2(filedata, this.Tag);

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

            return;
        }

        public void WriteFile(bool toOutputFile)
        {
            if (toOutputFile && !String.IsNullOrEmpty(OutputFile))
            {
                // write to the specified file
                File.WriteAllText(OutputFile, sb.ToString());
            }
            else
            {
                // else write sb to console.
                Console.WriteLine(sb.ToString());
            }
        }
    }
}
