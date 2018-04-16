using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MdApply
{
    using MdExtract;

    class Command
    {
        public enum Action { ignore, create, overwrite, delete, require, excise, merge_if, merge_add  }

        class CommandMatch
        {
            public Action action;
            public string[] options;
        }

        class CommandSet
        {
            public CommandMatch[] matches;

            public Action FindMatch(string input)
            {
                foreach (CommandMatch m in matches)
                {
                    foreach(string s in m.options)
                    {
                        if (String.CompareOrdinal(s, input.ToLower()) == 0)
                        {
                            return m.action;
                        }
                    }
                }
                // Replace with logging output
                MUT.MutLog.AppendInfo(String.Format("Unexpected action '{0}', treating as ignore.", input));
                return Action.ignore;
            }
        }

        CommandSet commands = new CommandSet
        {
            matches = new CommandMatch[] {
                new CommandMatch { action = Action.ignore, options = new string[] { "ignore", "i" } },
                new CommandMatch { action = Action.create, options = new string[] { "create", "add", "a" } },
                new CommandMatch { action = Action.overwrite, options = new string[] { "overwrite", "x" } },
                new CommandMatch { action = Action.delete, options = new string[] { "delete", "d" } },
                new CommandMatch { action = Action.require, options = new string[] { "require", "force", "obligate", "o" } },
                new CommandMatch { action = Action.excise, options = new string[] { "excise", "partial", "p" } },
                new CommandMatch { action = Action.merge_if, options = new string[] { "merge_if", "merge", "m" } },
                new CommandMatch { action = Action.merge_add, options = new string[] { "merge_add", "update", "u" } },
            }
        };


        public string Filename { get; set; }
        public Action TagAction { get; set; }
        public Tag TagData { get; set; }

        public Command(string commandItem, bool removeDupes)
        {
            var elements = commandItem.Split('\t');
            Filename = elements[0];
            TagAction = ToAction(elements[1]);
            
            TagData = new Tag(elements[2], elements[3], elements[4], removeDupes);
        }

        public Action ToAction(string input)
        {
            var result = Action.ignore;

            if (!String.IsNullOrEmpty(input))
            {
                return commands.FindMatch(input);
            }
            else
            {
                // Replace with logging output
                MUT.MutLog.AppendInfo(String.Format("Unexpected empty action, treating as ignore."));
            }
            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("filename: " + Filename + " ");
            sb.Append("action: " + TagAction.ToString() + " ");
            sb.Append("tag: " + TagData.TagYMLFormatted());
            return sb.ToString();
        }

    }
}
