using DocoptNet;
using System;
using System.Collections.Generic;
using System.IO;

namespace MdExtract
{
    public class Options
    {
        public readonly string usage = @"Usage: mdextract.exe [--path <path>] [--recurse] [--tag <tag>] [--file <file>] [--output <name>]

Options:
  --path <path>, -p <path>    Absolute or relative path to search [default: .\]
  --recurse -r                Search subdirectories
  --file <file>, -f <file>    Filename to match [default: *.md]
  --tag <tag>, -t <tag>       single tag value to extract 
  --output <name>, -o <name>  Output file

  --help -h -?                Show this usage statement
";
        public string Path
        {
            get { return (null == arguments["--path"]) ? null : arguments["--path"].ToString(); }
        }
        public SearchOption Recurse
        {
            get { return (arguments["--recurse"].IsTrue) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly; }
        }
        public string File
        {
            get { return (null == arguments["--file"]) ? null : arguments["--file"].ToString(); }
        }

        public string Tag
        {
            get { return (null == arguments["--tag"]) ? null : arguments["--tag"].ToString(); }
        }
        public bool Use_output
        {
            get
            {
                return arguments.ContainsKey("--output") && !String.IsNullOrEmpty(GetOutput());
            }
        }

        public string GetOutput()
        {
            return (null == arguments["--output"]) ? String.Empty : arguments["--output"].ToString();
        }

        public Options(string[] args)
        {
            arguments = new Docopt().Apply(usage, args, version: "mdextract 0.1", exit: false);
        }

        public void PrintOptions()
        {
            // debug output
            Console.WriteLine("Recognized options:");
            foreach (var argument in arguments)
            {
                Console.WriteLine("{0} = {1}", argument.Key, argument.Value);
            }
        }

        public IEnumerable<string> GetFiles()
        {
            return Directory.EnumerateFiles(this.Path, this.File, this.Recurse);
        }

        private IDictionary<string, ValueObject> arguments { get; set; }
    }
}
