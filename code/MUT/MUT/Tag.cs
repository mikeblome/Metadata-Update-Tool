namespace MdExtract
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using System.Text.RegularExpressions;

    public class Tag
    {
        /// <summary>
        /// Tag collections (sequences, mappings) can be in block style
        /// or flow style. Block style is controlled by indentation,
        /// and sequence elements are denoted by a leading dash-space, while 
        /// mappings are denoted by a colon-space between key and value.
        /// Flow style is controlled by explicit indicators, such
        /// as square brackets containing comma-separated items
        /// to denote a sequence, or a comma-separated list of key:value
        /// pairs within curly braces to denote a mapping.
        /// </summary>
        public enum TagFormatType { single, dash, bracket }


        #region Properties
        public string TagName { get; set; }
        public List<string> TagValues { get; set; }
        public TagFormatType TagFormat { get; set; }

        public string TagFormatString
        {
            get
            {
                switch (TagFormat)
                {
                    case TagFormatType.bracket:
                        return "bracket";
                    case TagFormatType.dash:
                        return "dash";
                    case TagFormatType.single:
                        return "single";
                    default:
                        return "";
                }
            }
            set
            {
                if (String.IsNullOrEmpty(value) || String.CompareOrdinal("single", value.ToLower()) == 0)
                    TagFormat = TagFormatType.single;
                else if (String.CompareOrdinal("dash", value.ToLower()) == 0)
                    TagFormat = TagFormatType.dash;
                else if (String.CompareOrdinal("bracket", value.ToLower()) == 0)
                    TagFormat = TagFormatType.bracket;
                else
                {
                    throw new ArgumentException("format must be <empty>, single, bracket, or dash");
                }
            }
        }
        #endregion


        public Tag(string name, List<string> vals, string fmt, bool removeDupes)
        {
            TagName = name;
            TagValues = vals;
            TagFormatString = fmt;
            TagValidate();
        }

        private void TagValidate()
        {
            // TODO where to do this validation? In Tag ctor?
            if (TagFormat == TagFormatType.single && TagValues.Count > 1)
            {
                TagFormat = TagFormatType.bracket;
                Console.WriteLine("Warning: format was single but multiple values were found. " +
                           "Changing format to bracket.");
            }
        }

        /// <summary>
        /// Creates a tag object from spreadsheet columns, in particular,
        /// where multiple values might be contained in a single column.
        /// If values are strings,they must already have quotes around them.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="vals"></param>
        /// <param name="fmt"></param>
        public Tag(string name, string vals, string fmt, bool removeDupes)
        {
            TagName = name;
            TagValues = ValuesFromString(vals, removeDupes);
            TagFormatString = fmt;
            TagValidate();
        }

        /// <summary>
        /// Creates a tag object given a tag-value substring from the yml.
        /// </summary>
        /// <param name="tagAndVal"></param>
        public Tag(string tagAndVal)
        {
            int idx = tagAndVal.IndexOf(':');
            TagName = tagAndVal.Substring(0, idx).Trim();
            TagValues = ValuesFromString(tagAndVal.Substring(idx + 1));
        }

        public List<string> ValuesFromString(string valPart, bool removeDupes = false)
        {
            var result = new List<string>();
            valPart = valPart.Replace("\r", "");
            var lines = valPart.Split('\n');
            var firstLine = lines[0].Trim();
            // If the first line of the value part is not empty,
            if (firstLine.Length > 0)
            {
                if (firstLine.StartsWith("[") && firstLine.EndsWith("]") && firstLine.Length > 2)
                {
                    // multi values in a single comma-separated string
                    TagFormat = TagFormatType.bracket;
                    string temp = firstLine.TrimStart('[');
                    temp = temp.TrimEnd(']');
                    // match all the pairs of quotes inside temp
                    Regex rgx = new Regex(@""".+?""");
                    var matches = rgx.Matches(temp);
                    if (matches.Count == 0)
                    {
                        MUT.MutLog.AppendInfo(String.Format("    {0} has no matches", temp));
                    }
                    foreach (Match m in matches)
                    {
                        string s = m.Value.Trim();
                        if (String.IsNullOrEmpty(s))
                        {
                            MUT.MutLog.AppendInfo(String.Format("    trimmed value is empty"));
                        }
                        result.Add(s);
                        MUT.MutLog.AppendInfo(String.Format("    trimmed value is {0}", m.Value.Trim()));
                    }
                }
                else
                {
                    TagFormat = TagFormatType.single;
                    result.Add(firstLine);
                }
            }
            else
            {
                TagFormat = TagFormatType.dash;
                // dash formatted each val on separate line
                // start on line 1 because line zero was empty
                for (int i = 1; i < lines.Length; ++i)
                {
                    if (lines[i].Length > 0)
                    {
                        // remove leading dash, trim result
                        var s = lines[i].Substring(lines[i].IndexOf('-') + 1).Trim();
                        result.Add(s);                        
                    }
                }
            }
            if (removeDupes)
            {
                result = result.Distinct().ToList();
            }
            
            return result;
        }

        /// <summary>
        /// Format values for YML file output (lists formatted according to TagFormat, hard returns)
        /// </summary>
        /// <returns>The values in a formatted string.</returns>
        public string TagYMLFormatted()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("{0}: ", TagName);

            switch (TagFormat)
            {
                case TagFormatType.dash:
                    foreach (var item in TagValues)
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append("  - " + item);
                    }
                    break;
                case TagFormatType.bracket:
                case TagFormatType.single:
                default:
                    sb.Append(TagValuesBracketedOrSingle());
                    break;
            }
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        /// <summary>
        /// Format values for extracted file output (no hard return, lists bracketed).
        /// </summary>
        /// <returns>The values in a bracket formatted string or a single unbracketed value.</returns>
        public string TagValuesBracketedOrSingle()
        {
            bool insertComma = false;
            var sb = new StringBuilder();
            switch (TagFormat)
            {
                case TagFormatType.bracket:
                case TagFormatType.dash:
                    sb.Append("[");
                    foreach (var item in TagValues)
                    {
                        if (insertComma) { sb.Append(", "); } else { insertComma = true; }

                        sb.Append(item);
                    }
                    sb.Append("]");
                    break;
                case TagFormatType.single:
                default:
                    if (TagValues.Count > 0)
                    {
                        sb.Append(TagValues[0]);
                    }
                    else
                    {
                        MUT.MutLog.AppendInfo("        Inserting \"\" for missing TagValues[0]");
                        sb.Append("\"\"");
                    }
                    break;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Format entire tag and values for YML output, with hard return.
        /// </summary>
        /// <returns>The YML formatted metadata string.</returns>
        public override string ToString()
        {
            return TagYMLFormatted();
        }

    }

    public class Tags
    {
        List<Tag> tagList_;

        public List<Tag> TagList { get { return this.tagList_; } }

        public Tags()
        {
            this.tagList_ = new List<Tag>();
            this.Changed = false;
        }

        public Tags(List<Tag> taglist)
        {
            this.tagList_ = taglist;
            this.Changed = false;
        }

        public Tag TryGetTag(string tagname)
        {
            foreach (var tag in this.tagList_)
            {
                if (tagname == tag.TagName)
                    return tag;
            }
            return null;
        }

        public int Count { get { return this.tagList_.Count; } }

        public bool Changed { get; set; }

    }

}
