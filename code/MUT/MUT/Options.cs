using System;
using System.Collections.Generic;
using System.IO;
using DocoptNet;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MUT
{
    class Options
    {
        public readonly string usage = @"Usage: mdextract.exe [--path <path>] [--recurse] [--file <file>] [--output <name>]

Options:
  --path <path>, -p <path>    Absolute or relative path to search [default: .\]
  --recurse -r                Search subdirectories
  --file <file>, -f <file>    Filename to match [default: *.md]
  --output <name>, -o <name>  Output file
  --help -h -?                Show this usage statement
";
        public string path
        {
            get { return (null == arguments["--path"]) ? null : arguments["--path"].ToString(); }
        }
        public SearchOption recurse
        {
            get { return (arguments["--recurse"].IsTrue) ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly; }
        }
        public string file
        {
            get { return (null == arguments["--file"]) ? null : arguments["--file"].ToString(); }
        }
        public bool use_output
        {
            get { return arguments.ContainsKey("--output") && !(null == arguments["--output"]) && !("" == arguments["--output"].ToString()); }
        }
        public string output
        {
            get { return (null == arguments["--output"]) ? null : arguments["--output"].ToString(); }
        }

        public Options(string[] args)
        {
            arguments = new Docopt().Apply(usage, args, version: "mdextract 0.1", exit: true);

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
            return Directory.EnumerateFiles(this.path, this.file, this.recurse);
        }

        private IDictionary<string, ValueObject> arguments { get; set; }
    }
}
