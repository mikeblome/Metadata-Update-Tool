using MdExtract;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MdApply
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load options
            var opts = new Options(args);
            var currentFile = "";
            var currentContent = "";
            var currentBody = "";
            var currentTagList = new Tags();
            Command command = null;
            string commandFile = "";

            try
            {
                commandFile = File.ReadAllText(opts.ArgFile);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to read command file {0}", opts.ArgFile);
                Console.WriteLine(e.Message);
                Console.WriteLine("Press any key to continue...");
                Console.ReadLine();
                System.Environment.Exit(1);
            }

            var commandRecords = commandFile.Split('\n');
            foreach (var commandRecord in commandRecords)
            {
                var trimmedCommand = commandRecord.Trim();
                if (!String.IsNullOrEmpty(trimmedCommand) && trimmedCommand != CommandBuilder.Header)
                {
                    try
                    {
                        command = new Command(trimmedCommand, opts.OptUnique);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Unable to read command {0}", trimmedCommand);
                        Console.WriteLine(e.Message);
                        continue;
                    }

                    if (currentFile != "" && currentFile != command.Filename)
                    {
                        WriteCurrentFile(opts, currentFile, currentBody, currentTagList);
                    }

                    if (currentFile != command.Filename)
                    {
                        currentFile = command.Filename;

                        try
                        {
                            currentContent = File.ReadAllText(currentFile);
                        }
                        catch (System.IO.FileNotFoundException)
                        {
                            // Log it?
                            continue;
                        }
                        catch (System.IO.DirectoryNotFoundException)
                        {
                            // Log it?
                            continue;
                        }
                        // param 2 only used in mdextract to specify just get one tag from each file.
                        // For apply we need to get all tags because any unchanged vals need to be written back into file
                        currentTagList = new Tags(YMLMeister.ParseYML2(currentContent, null)); 
                        currentBody = currentContent.Substring(currentContent.IndexOf("---", 4) + 3);
                    }

                    switch (command.TagAction)
                    {
                        case Command.Action.create:
                            AddTagIfNotPresent(currentTagList, command);
                            break;
                        case Command.Action.delete:
                            DeleteTag(currentTagList, command);
                            break;
                        case Command.Action.overwrite:
                            OverwriteIfTagExists(currentTagList, command);
                            break;
                        case Command.Action.require:
                            OverwriteOrAddTag(currentTagList, command);
                            break;
                        case Command.Action.excise:
                            ExciseValues(currentTagList, command);
                            break;
                        case Command.Action.merge_if:
                            MergeValuesIfTagExists(currentTagList, command);
                            break;
                        case Command.Action.merge_add:
                            MergeValuesOrAddTag(currentTagList, command);
                            break;
                        default:
                            Console.WriteLine("Ignoring the {0} tag in {1}",
                                command.TagData.TagName, command.Filename);
                            break;
                    }
                }
            }

            if (command != null && currentFile != "")
            {
                // We're done, write the file out, if there's anything to write.
                WriteCurrentFile(opts, currentFile, currentBody, currentTagList);
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

            Console.WriteLine("Done. Press Enter to close this window.");
            Console.ReadLine();
        }

        private static void AddTagIfNotPresent(Tags currentTagList, Command command)
        {
            if (null == currentTagList.TryGetTag(command.TagData.TagName))
            {
                currentTagList.TagList.Add(command.TagData);
                currentTagList.Changed = true;
                Console.WriteLine("Adding a {0} tag to {1}",
                    command.TagData.TagName, command.Filename);
            }
            else
            {
                Console.WriteLine("Attempted to add a {0} tag that already exists in {1}",
                    command.TagData.TagName, command.Filename);
            }
        }

        private static void DeleteTag(Tags currentTagList, Command command)
        {
            var tagItem = currentTagList.TryGetTag(command.TagData.TagName);
            if (null != tagItem)
            {
                currentTagList.TagList.Remove(tagItem);
                currentTagList.Changed = true;
                Console.WriteLine("Removing a {0} tag from {1}",
                    command.TagData.TagName, command.Filename);
            }
            else
            {
                Console.WriteLine("Attempted to delete a {0} tag that doesn't exist in {1}",
                    command.TagData.TagName, command.Filename);
            }
        }

        private static void OverwriteIfTagExists(Tags currentTagList, Command command)
        {
            var tagItem = currentTagList.TryGetTag(command.TagData.TagName);
            if (null != tagItem)
            {
                tagItem.TagValues = command.TagData.TagValues;
                tagItem.TagFormat = command.TagData.TagFormat;
                currentTagList.Changed = true;
                Console.WriteLine("Overwriting the {0} tag in {1}",
                    command.TagData.TagName, command.Filename);
            }
            else
            {
                Console.WriteLine("Attempted to overwrite a {0} tag that doesn't exist in {1}",
                    command.TagData.TagName, command.Filename);
            }
        }

        private static void OverwriteOrAddTag(Tags currentTagList, Command command)
        {
            var tagItem = currentTagList.TryGetTag(command.TagData.TagName);
            if (null != tagItem)
            {
                tagItem.TagValues = command.TagData.TagValues;
                tagItem.TagFormat = command.TagData.TagFormat;
                currentTagList.Changed = true; // Might actually be the same.
                Console.WriteLine("Replacing the {0} tag in {1}",
                    command.TagData.TagName, command.Filename);
            }
            else
            {
                currentTagList.TagList.Add(command.TagData);
                currentTagList.Changed = true;
                Console.WriteLine("Inserting a {0} tag in {1}",
                    command.TagData.TagName, command.Filename);
            }
        }

        private static void MergeValuesOrAddTag(Tags currentTagList, Command command)
        {
            var tagItem = currentTagList.TryGetTag(command.TagData.TagName);
            if (null != tagItem)
            {
                if (tagItem.TagFormat != command.TagData.TagFormat)
                {
                    tagItem.TagFormat = command.TagData.TagFormat;
                    currentTagList.Changed = true;
                }
                foreach (var val in command.TagData.TagValues)
                {
                    if (!tagItem.TagValues.Contains(val))
                    {
                        tagItem.TagValues.Add(val);
                        currentTagList.Changed = true;
                    }
                }
                Console.WriteLine("Merged to {0} tag in {1}",
                    command.TagData.TagName, command.Filename);
            }
            else
            {
                currentTagList.TagList.Add(command.TagData);
                currentTagList.Changed = true;
                Console.WriteLine("Inserting a merge {0} tag in {1}",
                    command.TagData.TagName, command.Filename);
            }
        }

        private static void MergeValuesIfTagExists(Tags currentTagList, Command command)
        {
            var tagItem = currentTagList.TryGetTag(command.TagData.TagName);
            if (null != tagItem)
            {
                if (tagItem.TagFormat != command.TagData.TagFormat)
                {
                    tagItem.TagFormat = command.TagData.TagFormat;
                    currentTagList.Changed = true;
                }
                foreach (var val in command.TagData.TagValues)
                {
                    if (!tagItem.TagValues.Contains(val))
                    {
                        tagItem.TagValues.Add(val);
                        currentTagList.Changed = true;
                    }
                }
                Console.WriteLine("Merged into {0} tag in {1}",
                    command.TagData.TagName, command.Filename);
            }
            else
            {
                Console.WriteLine("Attempted to merge missing {0} tag in {1}",
                    command.TagData.TagName, command.Filename);
            }
        }

        private static void ExciseValues(Tags currentTagList, Command command)
        {
            var tagItem = currentTagList.TryGetTag(command.TagData.TagName);
            if (null != tagItem)
            {
                var newTagValues = new List<string>();
                if (tagItem.TagFormat != command.TagData.TagFormat)
                {
                    tagItem.TagFormat = command.TagData.TagFormat;
                    currentTagList.Changed = true;
                }
                foreach (var val in tagItem.TagValues)
                {
                    if (!command.TagData.TagValues.Contains(val))
                    {
                        newTagValues.Add(val);
                    }
                    else
                    {
                        currentTagList.Changed = true;
                    }
                }

                if (newTagValues.Count > 0)
                {
                    tagItem.TagValues = newTagValues;
                    Console.WriteLine("Excising from {0} tag in {1}",
                        command.TagData.TagName, command.Filename);
                }
                else
                {
                    currentTagList.TagList.Remove(tagItem);
                    Console.WriteLine("Excised entire {0} tag from {1}",
                         command.TagData.TagName, command.Filename);
                }
            }
            else
            {
                Console.WriteLine("Attempted to excise from missing {0} tag in {1}",
                    command.TagData.TagName, command.Filename);
            }
        }

        private static void WriteCurrentFile(Options opts, string currentFile, string currentBody, Tags currentTagList)
        {
            // We're done with the file;
            // write the file out, if there are any changes to write.
            var newContent = new StringBuilder();
            newContent.AppendLine("---");
            if (currentTagList.Count > 0)
            {
                foreach (var currentTag in currentTagList.TagList)
                {
                    if (opts.OptBracket && currentTag.TagFormat == Tag.TagFormatType.dash)
                    {
                        currentTag.TagFormat = Tag.TagFormatType.bracket;
                        currentTagList.Changed = true;
                    }
                    else if (opts.OptDash && currentTag.TagFormat == Tag.TagFormatType.bracket)
                    {
                        currentTag.TagFormat = Tag.TagFormatType.dash;
                        currentTagList.Changed = true;
                    }
                    var tagOutput = currentTag.ToString();
                    newContent.Append(tagOutput);
                }
            }

            newContent.Append("---");
            newContent.Append(currentBody);

            if (currentTagList.Changed)
            {
                try
                {
                    File.WriteAllText(currentFile + opts.OptSuffix, newContent.ToString());
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine("Changed but skipping because file not found: {0}", currentFile);
                }
                catch (DirectoryNotFoundException)
                {
                    Console.WriteLine("Changed but skipping because folder not found: {0}", currentFile);
                }
            }
        }
    }
}
