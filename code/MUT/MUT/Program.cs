using System;
using System.Text;

namespace MdExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            var opts = new Options(args);

            // Get a list of filenames from options
            var fileArray = opts.GetFiles();

            // Extract YML header values for output
            StringBuilder sb = new StringBuilder();
            var cb = new CommandBuilder(opts.GetOutput(), opts.Tag);

            // Parse each file in the list
            foreach (var filename in fileArray)
            {

                cb.ParseFile(filename);
            }

            // Write results to the output file or stdout
            cb.WriteFile(opts.Use_output);

            if (opts.Use_log)
            {
                MUT.MutLog.WriteFile(opts.Logfile());
            }

            Console.WriteLine("MdExtract: Output was successfully written to {0}.", opts.Use_output ? cb.OutputFile : "stdout");
        }

        // ParseXML
    }

}
