using DocoptNet;
using MdExtract;
using System;
using System.Collections.Generic;

namespace MdApply
{
    public class Options
    {
        public readonly string usage = @"Usage: mdapply.exe <file> [--bracket | --dash] [--suffix <ext>] [ --unique]

Options:
  --bracket -b                Force multivalue tags into bracketed lists
  --dash -d                   Force multivalue tags into dash lists
  --unique -u                 Remove duplicate values in multival tags
  --suffix <ext>, -s <ext>    Add suffix extension to files changed
  --help -h -?                Show this usage statement
";

        public string ArgFile { get { return null == arguments["<file>"] ? null : arguments["<file>"].ToString(); } }
        public bool OptBracket { get { return arguments["--bracket"].IsTrue; } }
        public bool OptDash { get { return arguments["--dash"].IsTrue; } }
        public string OptSuffix { get { return null == arguments["--suffix"] ? "" : arguments["--suffix"].ToString(); } }
        public bool OptUnique { get { return arguments["--unique"].IsTrue; } }

        public Tag.TagFormatType OptFormat
        {
            get
            {
                if (OptBracket)
                {
                    return Tag.TagFormatType.bracket;
                }
                else if (OptDash)
                {
                    return Tag.TagFormatType.dash;
                }
                return Tag.TagFormatType.single;
            }
        }

        public Options(string[] args)
        {
            try
            {
                arguments = new Docopt().Apply(usage, args, version: "mdapply 0.1", exit: true);
                LogOptions();
            }
            catch (DocoptNet.DocoptInputErrorException e)
            {
                Console.WriteLine(e.Message);
                System.Environment.Exit(1);
            }
        }

        public void LogOptions()
        {
            // debug output
            MUT.MutLog.AppendInfo("Recognized options:");
            foreach (var argument in arguments)
            {
                MUT.MutLog.AppendInfo(String.Format("    {0} = {1}", argument.Key, argument.Value));
            }
        }

        private IDictionary<string, ValueObject> arguments { get; set; }
    }
}
