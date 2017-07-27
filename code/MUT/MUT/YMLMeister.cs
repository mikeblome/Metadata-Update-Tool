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

    public class Tag
    {
        public string Name;
        public List<string> Values;
        public string Format;
        public Tag(string name, List<string> vals, string fmt)
        {
            Name = name;
            Values = vals;
            Format = fmt;
        }
    }
    public class YMLMeister
    {
        #region reading values
        /// <summary>
        ///     Gets the starting position of the specified tag.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public int GetTagStartPos(string file, string tag)
        {
            // Get the upper bound of metadata block to 
            // prevent searches over entire content and the 
            // false hits that could generate
            int metadataEndPos = file.IndexOf("---", 4);
            return file.IndexOf(tag, 4, metadataEndPos - 4);
        }

        /// <summary>
        /// End of tag is tricky. what if it is a multi-line tag?
        /// We can't search for the next \n. We have to search for 
        /// the next tag within the metadata block. If there is none, then
        /// current tag is the last one and endPos is the last character before the "\r\n---"
        /// </summary>
        /// <param name="file"></param>
        /// <param name="startPos"></param>
        /// <returns></returns>
        public int GetTagValueEndPos(string file, int startPos)
        {
            // Get the upper bound of metadata block to 
            // prevent searches over entire content and the 
            // false hits that could generate
            int lineEnd = lineEnd = file.IndexOf("\n", startPos);
            if (!IsMultilineValue( file, startPos))
            {
                return lineEnd + 1; // include the last \n
            }

            // look for next tag, or end of metadata block.
            // we search a substring, because Match method
            // doeesn't allow us to specify a startingPos for the search.
            // the substring's zero element is really lineEnd
            // in the original string. subtract 1 to backtrack over the \n
            int ret = Regex.Match(file.Substring(lineEnd), @"([A-Za-z\._]+:)|(---)").Index;
            return ret + lineEnd - 1;
            
        }

        /// <summary>
        ///     Gets the tag itself, and its value
        /// </summary>
        /// <param name="file"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public string GetTagAndValue(string file, int beg)
        {
            int end = GetTagValueEndPos( file, beg);
            return file.Substring(beg, end - beg);
        }


        /// <summary>
        ///     Gets the tag itself, and its value
        /// </summary>
        /// <param name="file"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public string GetTagAndValue(string file, string tag)
        {
            int beg = GetTagStartPos( file, tag);
            int end = GetTagValueEndPos( file, beg);
            return file.Substring(beg, end - beg);
        }

        /// <summary>
        /// Gets only the value of a specified tag, whether single line or multi-line
        /// </summary>
        /// <param name="file"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public string GetValue(string file, string tag)
        {
            int beg = GetTagStartPos( file, tag);
            //find the ":" and go past it
            int begValue = file.IndexOf(':', beg) + 1;
            int end = GetTagValueEndPos( file, beg);
            return file.Substring(begValue, end - begValue).Trim();
        }

        /// <summary>
        ///   Gets the content up until the beginning of the line
        ///   on which the tag occurs. Default beginning is from beg of file.
        ///   Keeping the startPos param for now in case we would want to do
        ///   multiple changes in one pass. in that case the prefix would be the
        ///   range from the end of hte last line that was modified to the beginning
        ///   of the next line to be modified.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="tag"></param>
        /// <param name="startPos"></param>
        /// <returns></returns>
        public string GetPrefix(string file, string tag, int startPos = 0)
        {
            int tagPos = GetTagStartPos( file, tag);
            return file.Substring(startPos, tagPos - startPos);
        }

        /// <summary>
        /// Gets the substring from the end of the tag's value to the end of the entire file. 
        /// Append this when rebuilding the file string after making changes to a tag.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="tag"></param>
        /// <returns></returns>
        public string GetSuffix(string file, string tag)
        {
            int lineStart = GetTagStartPos( file, tag);
            int lineEnd = GetTagValueEndPos( file, lineStart);
            return file.Substring(lineEnd, file.Length - lineEnd);
        }

        /// <summary>
        /// Tells whether a value is (a) single val on the same line as the tag
        /// (b) multi-val on same line or (c) multi-val (dash format) on multi-lines following
        /// tag line.
        /// </summary>
        /// <param name="file"></param>
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
        public bool IsMultilineValue(string file, string tag)
        {
            string temp = tag + ":";
            int start = file.IndexOf(temp);
            int end = file.IndexOf("\n", start);
            var line = file.Substring(start, end - start);

            if (!line.Contains(":"))
            {
                Console.WriteLine("expected a : in metadata line");
                throw new Exception(); // TODO decide on error policy
            }

            // yes, then sanity check: is the next line a new tag?
            string nextLine = file.Substring(end + 1, file.IndexOf("\n", end + 1));
            if (Regex.IsMatch(nextLine, @"^[A_Za-z0-9\._-]+:"))
            {
                // tag is a single value 
                return false;
            }

            return true;
        }

        public bool IsMultilineValue(string file, int start)
        {

            int end = file.IndexOf("\n", start);
            var line = file.Substring(start, end - start);

            if (!line.Contains(":"))
            {
                Console.WriteLine("expected a : in metadata line");
                throw new Exception(); // TODO decide on error policy
            }

            // yes, then sanity check: is the next line a new tag?
            int idx = file.IndexOf("\n", end + 1);
            if (idx == -1)
            {
                return false; // we must be at the end of the block
            }
            string nextLine = file.Substring(end + 1, idx - end);
            if (Regex.IsMatch(nextLine, @"^[A_Za-z0-9\._-]+:"))
            {
                // tag is a single value 
                return false;
            }

            return true;
        }

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

        public List<Tag> ParseYML2(string file)
        {
            var yml = GetYmlBlock( file);
            var tags = GetAllTags(yml);
            var tagList = new List<Tag>();
            foreach (var t in tags)
            {
                tagList.Add(MakeTagFromString(t));
            }
            return tagList;
        }

        public Tag MakeTagFromString(string tagAndVal)
        {
          //  string s = m.Value;
            var parts = tagAndVal.Split('\n');
            var tagLine = parts[0].Split(':');
            string tagName = tagLine[0];
            string tagVal = tagLine[1].Trim();
            string format = "single";
            List<string> vals = new List<string>();
            if (tagVal.Length > 0)
            {
                if (tagVal.StartsWith("[") && tagVal.EndsWith("]") && tagVal.Length > 2)
                {
                    // multi values in a single comma-separated string
                    format = "bracket";
                    string temp = tagVal.TrimStart('[');
                    temp = temp.TrimEnd(']');
                    var valParts = temp.Split(',');
                    foreach (var p in valParts)
                    {
                        vals.Add(p);
                    }
                }
                else
                    vals.Add(tagVal);
            }
            else
            {
                format = "dash";
                //dash formatted each val on separate line
                // start on 1 because line zero was the tag itself
                for (int i = 1; i < parts.Length; ++i)
                {
                    var s = parts[i].Replace("  - ", "");
                    vals.Add(s.Trim());
                }
            }
            return new Tag(tagName, vals, format);
        }

        public string MakeStringFromTag(Tag tag)
        {
            string fmt = tag.Format;
            // TODO were to do this validation? In Tag ctor?
            if (String.CompareOrdinal("single", fmt.ToLower()) == 0 && tag.Values.Count > 1)
            {
                fmt = "bracket";
                Console.WriteLine("Warning: format was single but multiple values were found. " +
                           "Changing to format to bracket. All values are written to tag.");
            }

            // Init sb with tag name
            StringBuilder sb = new StringBuilder(tag.Name);
            sb.Append(": ");
            if (String.CompareOrdinal("single", fmt.ToLower()) == 0)
            {
                sb.Append(tag.Values[0]).Append(Environment.NewLine);
            }
            else if (String.CompareOrdinal("dash", fmt.ToLower()) == 0)
            {
                sb.Append(Environment.NewLine); //tag is on its own line in dash format
                foreach (var val in tag.Values)
                {
                    sb.Append("  - ").Append(val).Append(Environment.NewLine);
                }

            }
            else if (String.CompareOrdinal("bracket", fmt.ToLower()) == 0)
            {
                sb.Append("[");
                foreach (var val in tag.Values)
                {
                    sb.Append(val).Append(", ");
                }
                sb.Remove(sb.Length - 3, 2); //remove last comma
                sb.Append("]").Append(Environment.NewLine);
            }
            else
            {
                throw new ArgumentException("format must be single, bracket or dash");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets all tags and their values in the yml block.
        /// </summary>
        /// <param name="yml"></param>
        /// <returns></returns>
        public List<string> GetAllTags(string yml)
        {
            
             var matches = Regex.Matches(yml, @"[A-Za-z\._]+:", RegexOptions.Multiline);
            List<string> tags = new List<string>();
            foreach (Match m in matches)
            {
                tags.Add(GetTagAndValue( yml, m.Index));
            }

            return tags;
         }

        /// <summary>
        /// Gets a yml block from file including opening and closing "---" markers
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public string GetYmlBlock(string file)
        {
            return file.Substring(0, file.IndexOf("---", 4) + 3);
        }


        #endregion

        #region CRUD operations

        public string DeleteTagAndValue(string file, string tag)
        {
            var pre = GetPrefix( file, tag);
            var suf = GetSuffix( file, tag);
            StringBuilder sb = new StringBuilder(pre);
            sb.Append(suf);
            return sb.ToString();
        }

        public string ReplaceSingleValue(string file, string tag, string newVal)
        {
            var pre = GetPrefix( file, tag);
            var suf = GetSuffix( file, tag);
            var old = GetTagAndValue( file, tag);
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
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string CreateTag(string file, string tag, string value)
        {
            // skip over opening "---" and find the next one
            int metadataEndPos = file.IndexOf("---", 4);

            string pre = file.Substring(0, metadataEndPos);
            string suf = file.Substring(metadataEndPos, file.Length - metadataEndPos);
            StringBuilder sb = new StringBuilder(pre);
            sb.Append(tag).Append(": ").Append(value);
                sb.Append("\r\n").Append(suf);
            return sb.ToString();
        }


        public string AddValueToMultiTag(string file, string tag, string newVal)
        {
            var str = GetTagAndValue( file, tag);
            var tagStruct = MakeTagFromString(str);
            tagStruct.Values.Add(newVal);
            tagStruct.Values.Sort();
            return MakeStringFromTag(tagStruct);
        }
        #endregion
    }
}

