---
---
# Metadata Update Tool  
  
When you need to update OPS metadata across a number of Markdown files in an OPS content repo, such as we use for docs.microsoft.com content, you can use the Metadata Update Tool. The Metadata Update Tool consists of a couple of command line tools, one that extracts metadata from Markdown files, and another that applies metadata changes.  
  
The general process is:  
  
1. Create a GitHub branch for the metadata work. This keeps the metadata updates separated from other changes.  
2. Use mdextract.exe to generate an Excel spreadsheet (a tab-delimited text file) containing the metadata for the files you are working with.  
3. Use Excel to perform metadata updates.  
4. Apply those updates using the mdapply.exe tool.  
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
  
```cmd  
Usage: mdextract.exe [--path <path>] [--recurse] [--file <file>] [--output <name>]

Options:
  --path <path>, -p <path>    Absolute or relative path to search [default: .\]
  --recurse -r                Search subdirectories
  --file <file>, -f <file>    Filename to match [default: *.md]
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
  
The action field controls the behavior of the mdapply tool. By default, the mdextract tool sets this field to IGNORE, or empty, which specifies no change. Case is not significant in the action field.  
  
The action keywords and synonyms, and their meanings, are:  
  
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
  
Use the mdapply.exe tool to read in a spreadsheet and perform metadata update operations to a set of Markdown files. The spreadsheet that is read in is typically generated by mdextract.exe then edited in Excel, but you can also create your own spreadsheets that describe the metadata changes you want to make and save them as text files.  
  
Mdapply.exe is a command-line tool that reads in a tab-formatted command file, and applies the actions specified to the metadata of the specified files. The files are rewritten with the changes if any changes are made.  
  
The tool takes options to specify:  
  * Whether to force write of all collections as bracketed or dash delimited. Default: follow format field.  
  * A suffix to apply to written files, instead of overwriting the existing files. Default: overwrite existing.  
  * The name of the reporting output file, if any. Default: write to stdout.  
  
```cmd  
Usage: mdapply.exe <file> [--bracket | --dash] [--suffix <ext>]

Options:
  --bracket -b                Force multivalue tags into bracketed lists
  --dash -d                   Force multivalue tags into dash lists
  --suffix <ext>, -s <ext>    Add suffix extension to files changed
  --help -h -?                Show this usage statement
```  
  
The mdapply tool reads in a tab-delimited file that contains rows that have specific files, actions, metadata tags, values, and format info. Then it applies the specified action for each specified file, metadata tag and value collection. Finally, it writes out the updated file.  
  
The tool follows this general algorithm:  
For every action row, open the specified file if not already open; write and close any other file if open. 
If not already parsed,  
  * parse the YML content of the file in the same format as mdexport.  
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

