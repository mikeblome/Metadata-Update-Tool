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
        {
            var opts = new Options(args);

            // Get a list of filenames from a folder
            //string[] fileArray = Directory.GetFiles(@"C:\Users\tglee\vstest\docs\install\", "*.md", SearchOption.AllDirectories);
            var fileArray = opts.GetFiles();

            // Write header values for extract file
            StringBuilder sb = new StringBuilder();
            var cb = new CommandBuilder(opts.output);

            //sb.AppendLine("File Name\tAction\tTag\tValues\tFormat");

            // Open a file in the list

            foreach (var filename in fileArray)
            {
                cb.ParseFile(filename);
                /*
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
                    sb.Append(Environment.NewLine);

                }
                */

            }//end foreach

            cb.WriteFile(!opts.use_output);
            /*
            // If an output file was specified,
            if (opts.use_output)
            {
                // write to the specified file
                File.WriteAllText(opts.output, sb.ToString());
            }
            else
            {
                // else write sb to console.
                Console.WriteLine(sb.ToString());
            }
            */
            // debug trap so you can see it at work; remove from production
            Console.WriteLine("Press any key to continue... ... ...");
            Console.ReadLine();
        }

        // ParseXML
    }

}
