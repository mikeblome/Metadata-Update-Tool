//#define CURIOUS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdExtract
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public class YMLMeister
    {
        // NOTE: introducing state here, needed so that we can
        // record a filename that is missing a specified keyword.
        // possibly very useful info for various kinds of audits.
        // Keeping this static for now to avoid having to change 
        // the public interface of this class to all instance methods.
        // if we go multithreaded this needs to be changed
        public static string CurrentFile { get; set; }
        private static StringBuilder logBuilder = new StringBuilder();

        #region reading values
        /// <summary>
        ///     Gets the starting position of the specified tag.
        /// </summary>
        /// <param name="filedata"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static int GetTagStartPos(string filedata, string tag)
        {
            // Get the upper bound of metadata block to 
            // prevent searches over entire content and the 
            // false hits that could generate
            int metadataEndPos = filedata.IndexOf("\n---", 4) + 1;
            return filedata.IndexOf("\n" + tag, 3, metadataEndPos) + 1;
        }

        /// <summary>
        /// End of tag is tricky. what if it is a multi-line tag?
        /// We can't search for the next \n. We have to search for 
        /// the next tag within the metadata block. If there is none, then
        /// current tag is the last one and endPos is the last character before the "\r\n---"
        /// </summary>
        /// <param name="filedata"></param>
        /// <param name="startPos"></param>
        /// <returns></returns>
        public static int GetTagValueEndPos(string filedata, int startPos)
        {
            // Find the next line end
            int lineEnd = filedata.IndexOf("\n", startPos);
            if (!IsMultilineValue(filedata, startPos))
            {
                // tag and value is all one one line
                return lineEnd + 1; // include the last \n
            }

            // look for beginning of next tag, or end of metadata block.
            // everything between lineEnd and the beginning of next tag should
            // be all the values in the current tag.
            // we search a substring, because Match method
            // doeesn't allow us to specify a startingPos for the search.
            // the substring's zero element is really lineEnd
            // in the original string. subtract 1 to backtrack over the \n

            string textToSearch = filedata.Substring(lineEnd);
            var m = Regex.Match(textToSearch, @"(^[\w\.-]+:)|(---$)", RegexOptions.Multiline);

            int ret = m.Index;
            return ret + lineEnd - 1;
        }

        /// <summary>
        ///     Gets the tag itself, and its (possibly multi-line) value
        /// </summary>
        /// <param name="filedata"></param>
        /// <param name="beg"></param>
        /// <returns></returns>
        public static string GetTagAndValue(string filedata, int beg)
        {
            int end = GetTagValueEndPos(filedata, beg);
            return filedata.Substring(beg, end - beg);
        }

        /// <summary>
        ///     Gets the tag itself, and its value
        /// </summary>
        /// <param name="filedata"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string GetTagAndValue(string filedata, string tag)
        {
            int beg = GetTagStartPos(filedata, tag);
            int end = GetTagValueEndPos(filedata, beg);
            return filedata.Substring(beg, end - beg);
        }

        /// <summary>
        /// Gets only the value of a specified tag, whether single line or multi-line
        /// </summary>
        /// <param name="filedata"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string GetValue(string filedata, string tag)
        {
            int beg = GetTagStartPos(filedata, tag);
            //find the ":" and go past it
            int begValue = filedata.IndexOf(':', beg) + 1;
            int end = GetTagValueEndPos(filedata, beg);
            return filedata.Substring(begValue, end - begValue).Trim();
        }

        /// <summary>
        ///   Gets the content up until the beginning of the line
        ///   on which the tag occurs. Default beginning is from beg of file.
        ///   Keeping the startPos param for now in case we would want to do
        ///   multiple changes in one pass. in that case the prefix would be the
        ///   range from the end of hte last line that was modified to the beginning
        ///   of the next line to be modified.
        /// </summary>
        /// <param name="filedata"></param>
        /// <param name="tag"></param>
        /// <param name="startPos"></param>
        /// <returns></returns>
        public static string GetPrefix(string filedata, string tag, int startPos = 0)
        {
            int tagPos = GetTagStartPos(filedata, tag);
            return filedata.Substring(startPos, tagPos - startPos);
        }

        /// <summary>
        /// Gets the substring from the end of the tag's value to the end of the entire file. 
        /// Append this when rebuilding the file string after making changes to a tag.
        /// </summary>
        /// <param name="filedata"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public static string GetSuffix(string filedata, string tag)
        {
            int lineStart = GetTagStartPos(filedata, tag);
            int lineEnd = GetTagValueEndPos(filedata, lineStart);
            return filedata.Substring(lineEnd, filedata.Length - lineEnd);
        }

        /// <summary>
        /// Tells whether a value is (a) single val on the same line as the tag
        /// (b) multi-val on same line or (c) multi-val (dash format) on multi-lines following
        /// tag line.
        /// </summary>
        /// <param name="filedata"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        /// <remarks>            
        /// a tag has multiple values if no value on same line
        /// but one or more indented lines with "  - " that follow it
        /// ---OR---
        /// a bracket enclosed, comma-separated list on same line.
        /// Early out -- most are single line, so if
        /// (a) there is a possibly empty string val, not bracketed, on same line after colon
        /// --AND-- next line is either "---" or a new tag
        /// </remarks>
        public static bool IsMultilineValue(string filedata, string tag)
        {
            string temp = tag + ":";
            int start = filedata.IndexOf(temp);
            int end = filedata.IndexOf("\n", start);
            var line = filedata.Substring(start, end - start);

            if (!line.Contains(":"))
            {
                Console.WriteLine("expected a : in metadata line");
                throw new Exception(); // TODO decide on error policy
            }

            // yes, then sanity check: is the next line a new tag?
            string nextLine = filedata.Substring(end + 1, filedata.IndexOf("\n", end + 1));
            if (Regex.IsMatch(nextLine, @"^[\w\.-]+:"))
            {
                // tag is a single value 
                return false;
            }

            return true;
        }

        public static bool IsMultilineValue(string filedata, int start)
        {
            int end = filedata.IndexOf("\n", start);
            var line = filedata.Substring(start, end - start);

            if (!line.Contains(":"))
            {
                Console.WriteLine("expected a : in metadata line");
                throw new Exception(); // TODO decide on error policy
            }

            // yes, then sanity check: is the next line a new tag?
            int idx = filedata.IndexOf("\n", end + 1);
            if (idx == -1)
            {
                return false; // we must be at the end of the block
            }

            string nextLine = filedata.Substring(end + 1, idx - end);
            if (Regex.IsMatch(nextLine, @"^[\w\.-]+:"))
            {
                // tag is a single value 
                return false;
            }

            return true;
        }

        public static List<Tag> ParseYML2(string filedata, string tagToFind)
        {
            var yml = GetYmlBlock(filedata);
#if CURIOUS
            Console.WriteLine("vvvvvvvvvvvvvvv YML vvvvvvvvvvvvvvvvvvv");
            Console.WriteLine(yml);
            Console.WriteLine("^^^^^^^^^^^^^^^ YML ^^^^^^^^^^^^^^^^^^^");
#endif
            // user can specify in command line whether they just want to
            // get the values for one tag across all documents
            List<string> tags;
            if (!String.IsNullOrEmpty(tagToFind))
            {
                tags = GetOneTag(yml, tagToFind);
            }
            else
            {
                tags = GetAllTags(yml);
            }
#if CURIOUS
            Console.WriteLine("vvvvvvvvvvvvvvv YML reconstituted vvvvvvvvvvvvvvvvvvv");
            foreach (var taggy in tags)
            {
                Console.Write(taggy.ToString());
            }
            Console.WriteLine(yml);
            Console.WriteLine("^^^^^^^^^^^^^^^ YML reconstituted ^^^^^^^^^^^^^^^^^^^");
#endif

            var tagList = new List<Tag>();
            foreach (var t in tags)
            {
                tagList.Add(new Tag(t));
            }
            return tagList;
        }

        /// <summary>
        /// Gets all tags and their values in the yml block.
        /// </summary>
        /// <param name="yml">The entire yaml block, including leading and trailing '---' strings.</param>
        /// <returns></returns>
        public static List<string> GetAllTags(string yml)
        {
            var matches = Regex.Matches(yml, @"^[\w\.-]+:", RegexOptions.Multiline);
            List<string> tags = new List<string>();
            foreach (Match m in matches)
            {
                string tagVal = GetTagAndValue(yml, m.Index);
                tags.Add(tagVal);
            }

            return tags;
        }

        /// <summary>
        /// Gets the tag specified in command line and value in the yml block. we keep the return
        /// value as List,string> to avoid the caller having to deal with two separate return types 
        /// </summary>
        /// <param name="yml">The entire yaml block, including leading and trailing '---' strings.</param>
        /// <returns></returns>
        public static List<string> GetOneTag(string yml, string tag)
        {
            var tag_pos = yml.IndexOf(tag);
            List<string> tags = new List<string>();
            if (tag_pos > 0 && tag_pos < yml.Length)
            {
                string tagVal = GetTagAndValue(yml, tag_pos);
                tags.Add(tagVal);
            }
            else
            {
                //Console.WriteLine("No {0} tag in {1}", tag, CurrentFile);
                logBuilder.AppendLine(String.Format("No {0} tag in {1}", tag, CurrentFile));
            }
            return tags;
        }

        /// <summary>
        /// Gets a yml block from file including opening and closing "---" markers
        /// </summary>
        /// <param name="filedata"></param>
        /// <returns></returns>
        public static string GetYmlBlock(string filedata)
        {
            return filedata.Substring(0, filedata.IndexOf("\n---", 4) + 4);
        }

        public static void PrintLog(string filename)
        {
            System.IO.File.WriteAllText(filename, logBuilder.ToString());
            logBuilder.Clear();
        }
        #endregion

    }
}