using System;

using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MUT;
using System.IO;

namespace mdapply
{
    using MUT;

    class Program
    {
        static void Main(string[] args)
        {
            // Load options
            var opts = new Options(args);

            // For debugging, remove from production
            opts.PrintOptions();

            var y = new YMLMeister();
            var currentFile = "";
            var currentContent = "";
            var currentBody = "";
            var currentTagList = new Tags();
            Command command = null;

            string commandFile = File.ReadAllText(opts.ArgFile);

            var commandRecords = commandFile.Split('\n');
            foreach(var commandRecord in commandRecords)
            {
                var trimmedCommand = commandRecord.Trim();
                if (!String.IsNullOrEmpty(trimmedCommand) && trimmedCommand != CommandBuilder.Header)
                {
                    command = new Command(trimmedCommand);
                    if (currentFile != "" && currentFile != command.filename)
                    {
                        // We're done, write the file out, if there's anything to write.
                        var newContent = new StringBuilder();
                        newContent.AppendLine("---");
                        if (currentTagList.Count > 0)
                        {
                            foreach (var currentTag in currentTagList.TagList)
                            {
                                var tagOutput = currentTag.ToString();
                                newContent.Append(tagOutput);
                            }

                        }
                        newContent.Append(currentBody);

                        File.WriteAllText(currentFile + ".new.md", newContent.ToString());
                    }
                    if (currentFile != command.filename)
                    {
                        currentFile = command.filename;
                        currentContent = File.ReadAllText(currentFile);
                        currentTagList = new Tags(YMLMeister.ParseYML2(currentContent));
                        currentBody = currentContent.Substring(currentContent.IndexOf("---", 4));
                    }
                    switch (command.action)
                    {
                        case Command.Action.create:
                            if (null == currentTagList.TryGetTag(command.tagData.TagName))
                            {
                                currentTagList.TagList.Add(command.tagData);
                                Console.WriteLine("Adding a {0} tag to {1}",
                                    command.tagData.TagName, command.filename);
                            }
                            else
                            {
                                Console.WriteLine("Attempted to add a {0} tag that already exists in {1}", 
                                    command.tagData.TagName, command.filename);
                            }
                            break;
                        case Command.Action.delete:
                            var tagToDelete = currentTagList.TryGetTag(command.tagData.TagName);
                            if (null != tagToDelete)
                            {
                                currentTagList.TagList.Remove(tagToDelete);
                                Console.WriteLine("Removing a {0} tag from {1}",
                                    command.tagData.TagName, command.filename);
                            }
                            else
                            {
                                Console.WriteLine("Attempted to delete a {0} tag that doesn't exist in {1}",
                                    command.tagData.TagName, command.filename);
                            }
                            break;
                        case Command.Action.overwrite:
                            var tagToOverwrite = currentTagList.TryGetTag(command.tagData.TagName);
                            if (null != tagToOverwrite)
                            {
                                tagToOverwrite.TagValues = command.tagData.TagValues;
                                tagToOverwrite.TagFormat = command.tagData.TagFormat;
                                Console.WriteLine("Overwriting the {0} tag in {1}",
                                    command.tagData.TagName, command.filename);
                            }
                            else
                            {
                                Console.WriteLine("Attempted to overwrite a {0} tag that doesn't exist in {1}",
                                    command.tagData.TagName, command.filename);
                            }
                            break;
                        default:
                            Console.WriteLine("Ignoring the {0} tag in {1}",
                                command.tagData.TagName, command.filename);
                            break;
                    }

                }
            }
            if (command != null && currentFile != "")
            {
                // We're done, write the file out, if there's anything to write.
                var newContent = new StringBuilder();
                newContent.AppendLine("---");
                if (currentTagList.Count > 0)
                {
                    foreach (var currentTag in currentTagList.TagList)
                    {
                        var tagOutput = currentTag.ToString();
                        newContent.Append(tagOutput);
                    }

                }
                newContent.Append(currentBody);

                File.WriteAllText(currentFile + ".new.md", newContent.ToString());
            }


            // open commands file or report failure and exit
            // create objects for applies-to file, metadata collection
            // for each line in commands file
            //   load fields from line or report failure and continue
            //   if first line or filename field is not the current applies-to file
            //     if filename field is not the current applies-to file
            //       if there's an updated metadata collection
            //         write the metadata collection to the applies-to file or report failure
            //       close current applies-to file
            //     open new applies-to file or report failure and continue
            //     parse metadata collection from applies-to file or report failure and continue
            //   apply action to metadata collection or report failure and continue
            // if there's an open applies-to file
            //   if there's an updated metadata collection
            //     write the metadata collection to the applies-to file or report failure
            //   close current applies-to file
            // report complete and exit

            // debug trap so you can see it at work; remove from production
            Console.Write("Press any key to continue... ... ...");
            Console.ReadLine();

        }
    }
}