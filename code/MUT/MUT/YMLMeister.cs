//#define CURIOUS

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;




namespace MUT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;


    public class YMLMeister
    {
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
            int metadataEndPos = filedata.IndexOf("---", 4);
            return filedata.IndexOf(tag, 4, metadataEndPos - 4);
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
            var m = Regex.Match(textToSearch, @"(^[A-Za-z\._]+: )|(---$)", RegexOptions.Multiline);
            
            int ret = m.Index;
            return ret + lineEnd - 1;
            
        }

        /// <summary>
        ///     Gets the tag itself, and its value
        /// </summary>
        /// <param name="filedata"></param>
        /// <param name="tag"></param>
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
            if (Regex.IsMatch(nextLine, @"^[A_Za-z0-9\._-]+:"))
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
            if (Regex.IsMatch(nextLine, @"^[A_Za-z0-9\._-]+:"))
            {
                // tag is a single value 
                return false;
            }

            return true;
        }

#if NEVER
        /// <summary>
        ///     Reads a YML block into a dictionary. For use in mdextract. Compresses multi-line
        ///     values into a single comma-separated string.
        /// </summary>
        /// <param name="yml"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseYML(string yml)
        {
            var d = new Dictionary<string, string>();
            var lines = yml.Split('\n');

            // Theoretically matches only keys, not values. Needs good tests.
            Regex rgx = new Regex(@"^[A-Za-z\._]+:");

            // Store current key for cases where we need to iterate over multiline values.
            // Possibly not needed.
            string currentKey = "";

            // For use in multiline values. All multiline values get enclosed in brackets, even if there is only
            // one value present.
            StringBuilder currentVal = new StringBuilder("{");

            foreach (var v in lines)
            {
                if (rgx.IsMatch(v)) // Are we on a new key, or a new value in a multiline value list?
                {
                    // we are on a new key, but have we just finshed appending a bunch of multiline vals
                    // that now need to be associated with the previous key in the dictionary?
                    if (currentVal.Length > 1)
                    {
                        currentVal.Append("}");
                        // currentKey is what we stored when we started a multiline parse.
                        // now we're ready to update the value
                        d[currentKey] = currentVal.ToString().Replace("\"- ", "\", ").Replace("{-", "{");

                        // reset the stringbuilder
                        currentVal.Clear();
                        currentVal.Append("{");
                    }

                    // We are on a key, so split into key - value at the colon
                    var pair = v.Split(':');
                    string str;
                    bool b = d.TryGetValue(pair[0], out str);
                    if (!b)
                    {
                        // add KV pair to dicctionary removing trailing or leading whitespace
                        d.Add(pair[0].Trim(), pair[1].Trim());
                        currentKey = pair[0].Trim(); // store in case we are about to parse a multiline value
                    }
                }
                else
                {
                    // we are on a multiline value, not a key
                    int beg = v.IndexOf(" - ");
                    // hacky sanity check, not very robust

                    if (beg >= 0 && beg < 5)
                    {
                        // add this into the string that we are building up
                        // for currentKey
                        currentVal.Append(v.Substring(beg).Trim());
                    }
                }
            }
            // we are  done looping, but if we have a string stored in currentVal
            // we need to add it to the dictionary. This happens when last key has multiline vals.
            if (currentVal.Length > 1)
            {
                currentVal.Append("}");
                d[currentKey] = currentVal.ToString().Trim().Replace("\"- ", "\", ").Replace("{-", "{");
            }
            return d;
        }
#endif
        public static List<Tag> ParseYML2(string filedata)
        {
            var yml = GetYmlBlock(filedata);
#if CURIOUS
            Console.WriteLine("vvvvvvvvvvvvvvv YML vvvvvvvvvvvvvvvvvvv");
            Console.WriteLine(yml);
            Console.WriteLine("^^^^^^^^^^^^^^^ YML ^^^^^^^^^^^^^^^^^^^");
#endif
            var tags = GetAllTags(yml);
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
        /// <param name="yml"></param>
        /// <returns></returns>
        public static List<string> GetAllTags(string yml)
        {
            var matches = Regex.Matches(yml, @"^[A-Za-z0-9\._]+:", RegexOptions.Multiline);
            List<string> tags = new List<string>();
            foreach (Match m in matches)
            {
                string tagVal = GetTagAndValue(yml, m.Index);
                tags.Add(tagVal);
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
            return filedata.Substring(0, filedata.IndexOf("---", 4) + 3);
        }


        #endregion

#region CRUD operations

#if false
        public static string DeleteTagAndValue(string file, string tag)
        {
            var pre = GetPrefix(file, tag);
            var suf = GetSuffix(file, tag);
            StringBuilder sb = new StringBuilder(pre);
            sb.Append(suf);
            return sb.ToString();
        }

        public static string ReplaceSingleValue(string file, string tag, string newVal)
        {
            var pre = GetPrefix(file, tag);
            var suf = GetSuffix(file, tag);
            var old = GetTagAndValue(file, tag);
            var parts = old.Split(':');
            StringBuilder sb = new StringBuilder(pre);
            sb.Append(parts[0]).Append(": ").Append(newVal)
                .Append("\r\n").Append(suf);
            return sb.ToString();
        }

        /// <summary>
        /// Adds a new tag and value to the metadata section. 
        /// TODO--Place tag is proper ordered position in the metadatablock.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="tagName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CreateTag(string file, string tagName, string value)
        {
            // skip over opening "---" and find the next one
            int metadataEndPos = file.IndexOf("---", 4);

            string pre = file.Substring(0, metadataEndPos);
            string suf = file.Substring(metadataEndPos, file.Length - metadataEndPos);
            StringBuilder sb = new StringBuilder(pre);
            sb.Append(tagName).Append(": ").Append(value);
                sb.Append("\r\n").Append(suf);
            return sb.ToString();
        }


        public static string AddValueToMultiTag(string file, string tagName, string newVal)
        {
            var str = GetTagAndValue(file, tagName);
            var tag = new Tag(str);

            // Do not add a value if it already exists!
            if (!tag.Values.Contains(newVal))
            {
                tag.Values.Add(newVal);
            }
            else
            {
                //TODO how to issue warnings? Log file?
                Console.WriteLine("Warning: " + tag.name_ +
                    "in" + GetValue(file, "title") + "already has a value " + newVal);
            }
            tag.Values.Sort();
            return tag.ToString();
        }

        /// <summary>
        ///  Deletes one value from a multi-value tag. This could be done more efficiently
        ///  by simple string replacement, but keeping this version for now
        ///  for the sake of consistency and in case the structured approach proves useful for
        ///  other more complex operations we might want to do later.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="tagName"></param>
        /// <param name="val"></param>
        /// <returns>A string that represents the new tag.</returns>
        public static string DeleteValueFromMultiTag(string file, string tagName, string val)
        {
            var str = GetTagAndValue(file, tagName);
            var tag = new Tag(str);

            // Can't delete a value if it doesn't exist!
            if (tag.Values.Contains(val))
            {
                tag.Values.Remove(val);
                tag.Values.Sort();
            }
            else
            {
                //TODO how to issue warnings? Log file?
                Console.WriteLine("Warning. Nothing to delete: " + tag.name_ +
                    "in" + GetValue(file, "title") + "does not contain a value " + val);
            }

            return tag.ToString();
        }
#endif
#endregion
    }
}

