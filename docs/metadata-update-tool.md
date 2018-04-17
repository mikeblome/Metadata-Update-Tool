---
---
# Metadata Update Tool

When you need to update or view OPS metadata across a number of Markdown files in an OPS content repo, such as we use for docs.microsoft.com content, you can use the Metadata Update Tool. The Metadata Update Tool consists of two command line tools, one that extracts metadata from Markdown files, and another that applies metadata changes.  An opional PowerShell script can be used to open the output into Excel.

The general process is:

1. Create a GitHub branch in your content repo for the metadata work. This keeps the metadata updates separated from other changes.
2. Use mdextract.exe to generate an Excel spreadsheet (a tab-delimited text file) containing the metadata for the files you are working with.
3. Use Excel or other word processing program to modify the metadata.
4. Pass the modified data file to the mdapply.exe tool to apply the updates.
5. Commit the changes and push them to the remote repo.
6. Create a pull request from the branch you created to the original branch (e.g., master).
7. Scan the diffs to verify that the metadata changes are what you expect.

If you are using VS Code to edit Markdown files, you can run the command line tools from VS Code.

## mdextract.exe

You use the mdextract.exe tool to generate a spreadsheet of the existings metadata tags and values for the specified file set.

The tool reads in a specified file or set of files, and writes the yml header data to a tab-delimited formatted text file.

The tool takes options to specify:
  * a specific file, a list of specific files, or a wildcard match for files to read. Default: *.md - all markdown files in the current directory.
  * a specific path to files. Default: `.` (the current working directory, or CWD).
  * whether to recurse into subdirectories of the path. Default: no recursion.
  * the name of the output file, if any. Default: write to stdout.
  * the name of the tag you are interested in. By default MUT extracts all tags. Use this option to limit output to only the value(s) for a specified tag in each file.

```cmd
Usage: mdextract.exe [--path <path>] [--recurse] [--tag <tag>] [--log <log>] [--file <file>] [--output <name>]

Options:
  --path <path>, -p <path>    Absolute or relative path to search [default: .\]
  --recurse -r                Search subdirectories
  --file <file>, -f <file>    Filename to match [default: *.md]
  --tag <tag>, -t <tag>       Single tag value to extract
  --log <log>, -l <log>       Log data filename to write
  --output <name>, -o <name>  Output file

  --help -h -?                Show this usage statement
```

The mdextract tool builds an internal collection of markdown files by applying the command line options to the specified path or CWD and enumerating the files found that match.

For each file in the collection,
- Open the file and parse the YML header into an ordered list of key-value pairs, where key is a string and value is an ordered list of string.
- For each metadata tag, mdextract outputs the YML into a tab-delimited row containing the filename, metadata action, metadata tag, value collection, and a field that specifies the collection type, either a single item on the same row as the tag, one dash-delimited entry per item starting on the next line, or a bracketed collection of comma-separated items.

The output file begins with a header row, containing the following string with escaped tabs:

`"FILENAME\tACTION\tTAG\tVALUE\tFORMAT"`

Each row in the output contains the following tab-delimited ordered set of fields, where strings are surrounded by double-quotes and collections are separated by commas and surrounded by brackets:

`file_path \t action \t metadata_tag \t value-collection  \t format `

The action field controls the behavior of the mdapply tool. By default, the mdextract tool sets this field to IGNORE, or empty, which specifies no change. Case is not significant in the action field. In practice, the OVERWRITE and DELETE actions seem to be the most useful. The other actions tend to perform modifications that you generally do in Excel. The action keywords and synonyms, and their meanings, are:

|||
|---|---|
| A, add, create | Add tag using specified values only if tag does not already exist; leave an existing tag alone. |
| D, delete | Unconditional delete of tag if it exists. |
| P, partial, excise | Partial delete of only specified values from tag, leaving tag and other values only if other values remain, deleting tag if it is now empty. |
| O, require, force | Unconditional overwrite of values in tag, or add tag using specified values if it does not exist. |
| X, overwrite | Unconditional overwrite of values in tag only if tag exists; do not add tag if it does not exist. |
| M, merge, merge_if | Merge unique specified values into tag, only if tag exists; do not add tag if it does not exist. |
| U, update, merge_add | Merge unique specified values into tag, or add tag using all specified values if it does not exist. |

You can have more than one row that applies to the same metadata tag, for example, where one row uses a P action to delete specific values, and another uses the M action to add specific values, leaving any other values unchanged.

The format field also controls the behavior of the mdapply tool. Case is not significant in this field. By default, this field is empty or “single”, which implies a single value on the same line. An entry of “dash” specifies the value collection is dash-delimited entries on the following lines. An entry in the field of "bracket" specifies the value collection is a bracketed collection of entries on the same line.

## mdapply.exe

Mdapply.exe is a command-line tool that reads in a tab-formatted text command file in the same format generated by mdextract, and applies the actions specified to the metadata of the specified files. The files are rewritten with the changes if any changes are made.

The tab-delimited format of the mdapply command file is easy to open and edit, or even to generate from scratch, in Excel as a spreadsheet. Excel can then save the file as tab-delimited text. You can also create your own command files that describe the metadata changes you want to make in an ordinary text editor.

By default, mdapply creates a backup of any file that is overwritten by the tool, with a .backup extension.

### mdapply options

The tool takes options to specify:

* Whether to force write of all collections as bracketed or dash delimited. Default: follow format field.
* A suffix to apply to written files, instead of overwriting the existing files. Default: overwrite existing.
* Whether to skip backups of overwritten files
* The name of the reporting output file, if any. Default: write errors to stdout.

```cmd
Usage: mdapply.exe <file> [--bracket | --dash] [--nobackup] [--suffix <ext>] [--log <log>] [ --unique]

Options:
  --bracket -b                Force multivalue tags into bracketed lists
  --dash -d                   Force multivalue tags into dash lists
  --unique -u                 Remove duplicate values in multival tags
  --suffix <ext>, -s <ext>    Add suffix ext to files changed
  --nobackup, -n              Do not create backups of changed files
  --log <log>, -l <log>       Log info to log file
  --help -h -?                Show this usage statement
```

#### --bracket and --dash

The global **--bracket** and **--dash** options to mdapply affect how YML list items are formatted when written out. They override the individual formatting specified by the format field for a particular tag. They're for use when you want to apply a particular list style globally.

Let's say you have a file myexample.md with a header that looks like this:

```md
---
Example.tag: 
  - "item one"
  - "item two"
Another.one: ["a", "b", "c"]
---
```

This turns into a pair of entries in your output from mdextract that looks like this:

```txt
.\myexample.md IGNORE Example.tag   ["item one", "item two"]     dash
.\myexample.md IGNORE Another.one   ["a", "b", "c"]      bracket
```

Then you edit it, and tell it to update the existing tags to have another value in the lists:

```txt
.\myexample.md UPDATE Example.tag   ["item one", "item two", "item 3"]  dash
.\myexample.md UPDATE Another.one   ["a", "b", "c", "d"]  bracket
```

This is how it is formatted when written by mdapply if you don't use **--bracket** or **--dash** to override the existing format field:

```md
---
Example.tag: 
  - "item one"
  - "item two"
  - "item 3"
Another.one: ["a", "b", "c", "d"]
---
```

If you specify **--bracket**, it overrides every format field, and you get it written in bracketed list format:

```md
---
Example.tag: ["item one", "item two", "item 3"]
Another.one: ["a", "b", "c", "d"]
---
```

If you specify **--dash**, then this is your output:

```md
---
Example.tag: 
  - "item one"
  - "item two"
 - "item 3"
Another.one: 
  - "a"
  - "b"
  - "c"
  - "d"
---
```

#### --unique

The **--unique** option is for metadata cleanup. It identifies duplicate values in list entries, and only writes the first value to the output.

#### --suffix

The **--suffix** option lets you create updated files without overwriting the existing files. It adds the specified suffix to the file name when generating output.

For example, if the original file name is original.md, and you've specified the option `--suffix .new` to mdapply, then the output is written to original.md.new instead of overwriting original.md.

#### --nobackup

The **--nobackup** option lets you overwrite existing files without creating a backup. By default, mdapply creates a backup (with a .backup extension) of any file that is overwritten (for example, if you do not specify a suffix option).

For example, if the original file name is original.md, and you've specified the option `--nobackup` to mdapply, then the output is written to original.md and no backup is made at original.md.backup.

### mdapply operation

The mdapply tool reads in a tab-delimited file that contains rows that have specific files, actions, metadata tags, values, and format info. Then it applies the specified action for each specified file, metadata tag and value collection. Finally, it writes out the updated file.

The tool follows this general algorithm:
For every action row, open the specified file if not already open; write and close any other file if open.
If not already parsed,

* Parse the YML content of the file in the same format as mdexport.
* Apply the specified action to the metadata.
* If the action adds a tag, it's added to the end of the YML block, or immediately after the previous tag from the action list if in the same file. (I.e. attempt to preserve ordering.)
* Output a report of every changed tag.


## Scenarios

### How to change one or more metadata values in .md files in a git repo or any other file folder. 

### How to specify files (for example, as belonging to a VS workload) and tag each one with a value. 

### How to add or remove individual values from multi-value metadata tags. 

### How to list all files that contain a specified metadata value. 

### How to filter the files that are touched so that only those files that require changes are opened. 

### How to add a new metadata tag, delete an existing tag, or modify an existing value. 

