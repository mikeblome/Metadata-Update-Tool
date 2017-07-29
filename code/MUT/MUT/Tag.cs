namespace MUT
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class Tag
    {
        public enum FormatType { single, dash, bracket }

        // Private fields and properties
        string name_;
        List<string> values_;

        FormatType useFormat{ get; set; }

#region Properties
        public string TagName { get { return name_; } }
        public List<string> TagValues { get { return values_; } set { values_ = value; } }
        public string TagFormat
        {
            get
            {
                switch (useFormat)
                {
                    case FormatType.bracket:
                        return "bracket";
                    case FormatType.dash:
                        return "dash";
                    case FormatType.single:
                        return "single";
                    default:
                        return "";
                }
            }
            set
            {
                if (String.IsNullOrEmpty(value) || String.CompareOrdinal("single", value.ToLower()) == 0)
                    useFormat = FormatType.single;
                else if (String.CompareOrdinal("dash", value.ToLower()) == 0)
                    useFormat = FormatType.dash;
                else if (String.CompareOrdinal("bracket", value.ToLower()) == 0)
                    useFormat = FormatType.bracket;
                else
                {
                    throw new ArgumentException("format must be single, bracket or dash");
                }
            }
        }
#endregion

        public Tag(string name, List<string> vals, string fmt)
        {
            name_ = name;
            values_ = vals;
            TagFormat = fmt;
        }

        /// <summary>
        /// Creates a tag object from spreadsheet columns, in particular,
        /// where multiple values might be contained in a single column.
        /// If values are strings,they must already have quotes around them.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="vals"></param>
        /// <param name="fmt"></param>
        public Tag(string name, string vals, string fmt)
        {
            name_ = name;
            TagFormat = fmt;
            values_ = new List<string>();
            if (vals.Length > 0)
            {
                if (vals.Trim().StartsWith("[") && vals.EndsWith("]") && vals.Length > 2)
                {
                    // multi values in a single comma-separated string
                    string temp = vals.TrimStart('[');
                    temp = temp.TrimEnd(']');
                    var valParts = temp.Split(',');
                    foreach (var p in valParts)
                    {
                        values_.Add(p.Trim());
                    }
                }
                else
                    values_.Add(vals);
            }
        }

        /// <summary>
        /// Creates a tag object given a tag-value substring from the yml.
        /// </summary>
        /// <param name="tagAndVal"></param>
        public Tag(string tagAndVal)
        {
            var parts = tagAndVal.Split('\n');

            int idx = parts[0].IndexOf(':');
            name_ = parts[0].Substring(0, idx);
            string tagVal = parts[0].Substring(idx + 1).Trim();
            useFormat = FormatType.single;
            values_ = new List<string>();
            if (tagVal.Length > 0)
            {
                if (tagVal.Trim().StartsWith("[") && tagVal.EndsWith("]") && tagVal.Length > 2)
                {
                    // multi values in a single comma-separated string
                    useFormat = FormatType.bracket;
                    string temp = tagVal.TrimStart('[');
                    temp = temp.TrimEnd(']');
                    var valParts = temp.Split(',');
                    foreach (var p in valParts)
                    {
                        values_.Add(p.Trim());
                    }
                }
                else
                    values_.Add(tagVal);
            }
            else
            {
                useFormat = FormatType.dash;
                // dash formatted each val on separate line
                // start on 1 because line zero was the tag itself
                for (int i = 1; i < parts.Length; ++i)
                {
                    var s = parts[i].Replace("  - ", "");
                    values_.Add(s.Trim());
                }
            }
        }

        /// <summary>
        /// Format values for YML file output (lists formatted according to TagFormat, hard returns)
        /// </summary>
        /// <returns>The values in a formatted string.</returns>
        public string TagValuesYMLFormatted()
        {
            var sb = new StringBuilder();

            switch (useFormat)
            {
                case FormatType.bracket:
                    FormatBracketed(sb);
                    break;
                case FormatType.dash:
                    foreach (var item in values_)
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append("  - " + item);
                    }
                    break;
                case FormatType.single:
                default:
                    sb.Append(values_[0]);
                    break;
            }
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        /// <summary>
        /// Format values for extracted file output (no hard return, lists bracketed).
        /// </summary>
        /// <returns>The values in a formatted string.</returns>
        public string TagValuesExtractFormatted()
        {
            var sb = new StringBuilder();

            switch (useFormat)
            {
                case FormatType.bracket:
                case FormatType.dash:
                    FormatBracketed(sb);
                    break;
                case FormatType.single:
                default:
                    sb.Append(values_[0]);
                    break;
            }

            return sb.ToString();
        }

        private void FormatBracketed(StringBuilder sb)
        {
            bool addComma = false;
            sb.Append("[");
            foreach (var item in values_)
            {
                if (addComma) { sb.Append(", "); } else { addComma = true; }

                sb.Append(item);
            }
            sb.Append("]");
        }

        /// <summary>
        /// Format entire tag and values for YML output, with hard return.
        /// </summary>
        /// <returns>The YML formatted metadata string.</returns>
        public override string ToString()
        {
            // TODO were to do this validation? In Tag ctor?
            if (useFormat == FormatType.single && values_.Count > 1)
            {
                useFormat = FormatType.bracket;
                Console.WriteLine("Warning: format was single but multiple values were found. " +
                           "Changing format to bracket. All values are written.");
            }

            // Init sb with tag name
            StringBuilder sb = new StringBuilder(name_);
            sb.Append(": ");
            sb.Append(TagValuesYMLFormatted());

            return sb.ToString();
        }

    }

    public class Tags
    {
        List<Tag> tagList_;

        public List<Tag> TagList { get { return this.tagList_;  } }

        public Tags()
        {
            this.tagList_ = new List<Tag>();
        }

        public Tags(List<Tag> taglist)
        {
            this.tagList_ = taglist;
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
        
    }

}
