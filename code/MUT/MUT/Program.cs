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

#if DEBUG
            var mydocs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string fileName = "MUT-log-" + DateTime.Now.ToFileTime() + ".txt";
            string LogFileName = System.IO.Path.Combine(mydocs, fileName);
            YMLMeister.PrintLog(LogFileName);
#endif

            Console.WriteLine("Output was successfully written to {0}.", cb.OutputFile);

            // debug trap so you can see it at work; remove from production
            Console.WriteLine("Press any key to continue... ... ...");
            Console.ReadLine();
        }

        // ParseXML
    }

}
