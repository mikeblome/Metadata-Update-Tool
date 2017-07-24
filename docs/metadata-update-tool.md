---
---
# Metadata Update Tool

When you need to update OPS metadata across a number of Markdown files in an OPS content repo, such as we use for docs.microsoft.com content, you can use the Metadata Update Tool. The Metadata Update Tool consists of a couple of command line tools, one that extracts metadata from Markdown files, and another that applies metadata changes.

The general process is:

1. Create a GitHub branch for the metadata work. This keeps the metadata updates separated from other changes.
1. Use mdextract.exe to generate an Excel spreadsheet (.csv file) containing the metadata for the files you are working with.
1. Use Excel to perform metadata updates.
1. Apply those updates using the mdapply.exe tool. 
1. Commit the changes and push them to the remote repo.
1. Create a pull request from the branch you created to the original branch (e.g., master).
1. Scan the diffs to verify that the metadata changes are what you expect.

If you are using VS Code to edit Markdown files, you can run the command line tools from VS Code.

## mdextract.exe

You use the mdextract.exe tool to generate a spreadsheet of the existings metadata tags and values for the specified file set.

The tool reads in a specified file or set of files, and writes the yml header data to a .csv file. 

The tool takes options to specify  

    * a specific file, a list of specific files, or a wildcard match for files. Default: *.md - all markdown files in the directory. 
    * a specific path to files. Default: . - the current working directory. 
    * whether to recurse into subdirectories of the path. Default: no recursion. 
    * files that have|do not have a specific metadata tag or specific tag-value pair. Default: no match tag specified 
    * the name of the output file, if any. Default: write to stdout. 

``` cmd
mdextract [--help] [--path pathspec] [--recurse] [--file filespec] [--match|--nomatch tag[:"value"]] [--output filename] 
    --help|-h|-?                Output this usage statement and exit.  
    --path|-p pathspec          Path under which to build file collection. Default pathspec if unspecified is CWD. 
                                Pathspec may be absolute ("C:\source\repo\myfork") or relative to CWD (".\reference"). 
    --recurse|-r                Recurse into subdirectories of pathspec. Default is no recursion. 
    --file|-f                   Filespec, possibly including wildcards, to match to build file collection. Default is "*.md". 
    --match|-m tag[:"value"]    Tag and optional value that must be present in a file to be included in the file collection. 
    --nomatch|-n tag[:"value"]  Tag and optional value that must not be present in a file to be included in the file collection. 
    --output|-o filename        Name of formatted output file. May be an absolute filepath, or relative to CWD.  
Default output is to stdout. 
```

The mdextract tool builds an internal collection of markdown files by applying the command line options to the specified path or CWD and enumerating the files found that match. 

For each file in the collection, 
    * Opens the file and parse the YML header into an ordered list of key-value pairs, where key is a string and value is an ordered list of string 
    * If there is no tag match criterion, or if the list matches a tag match criterion, for each metadata tag, mdextract outputs the YML into a tab-delimited row containing the filename, metadata disposition, metadata tag, and value collection. Optionally, mdextract includes a field that specifies the collection type, either a single item on the same row as the tag, one dash-delimited entry per item starting on the next line, or a bracketed collection of comma-separated items. 
 
Each row in the output contains the following ordered set of fields, where strings are surrounded by double-quotes and collections are separated by semicolons: 
 
    file_path \t disposition \t metadata_tag \t value-collection { \t format } 

where \t is the tab character.
 
The disposition field controls the behavior of the mdapply tool. By default, this field is empty, which implies no change. 
Other disposition keywords and their meanings are 
 
|A|Add tag using specified values iff tag does not already exist; leave an existing tag alone. |
|D|Unconditional delete of tag if it exists |
|P|Partial delete of only specified values from tag, leaving tag and other values iff other values remain, deleting tag if it is now empty.| 
| |Unconditional overwrite of values in tag, or add tag using specified values if it does not exist.| 
|I|Unconditional overwrite of values in tag iff tag exists; do not add tag if it does not exist. |
|M|Merge unique specified values into tag, iff tag exists; do not add tag if it does not exist. |
|U|Merge unique specified values into tag, or add tag using all specified values if it does not exist.| 
 
You can have more than one row that applies to the same metadata tag, where one row uses P to delete specific values, and another uses M to add specific values, leaving any other values unchanged. 
 
The format field also controls the behavior of the mdapply tool. By default, this field is empty, which implies a single value on the same line or dash-delimited entries on the following lines. An entry in the field of "C" specifies use of a bracketed collection of entries. 

## mdapply.exe

You use the mdapply.exe tool to read in a spreadsheet and perform metadata update operations to a set of Markdown files. The spreadsheet you read in is typically generated by mdextract.exe, but you can also create your own spreadsheets that describe the metadata changes you want to make.


This is a command-line tool that reads in a formatted CSV file, and applies the content instructions to the metadata of all specified files. 
The tool takes options to specify  
    * whether to require an absolute path, or accept a path relative to the CWD. Default: use only absolute paths. 
    * whether to accept filename wildcards in the input. Default: no wildcards. 
    * whether to recurse into subdirectories of the CWD for filename wildcards. Default: no recursion. 
    * whether to force write of all collections as bracketed or dash delimited. Default: follow CSV format 
    * the name of the reporting output file, if any. Default: write to stdout. 

```cmd
mdapply [--help] file.csv [--cwd-relative] [--wildcards] [--recurse] [--bracket|--dash] [--output filename] 
    --help|-h|-?                Output this usage statement and exit.  
    file.csv                    The CSV file to read for metadata actions to apply. Required. May be an absolute filepath, or relative to CWD. 
    --cwd-relative|-c           Paths in CSV may be interpreted as relative to CWD. Default: all paths must be absolute. 
    --wildcards|-w              Paths in CSV may include wildcards. Default: paths may not include wildcards. 
                                If wildcards are found, apply the metadata action to all matching files. 
    --recurse|-r                Recurse into subdirectories of CWD for wildcard match. Default is no recursion. Has no effect if -c and -w are not specified. 
    --bracket                   Force all value collections to use bracket format, overriding value in CSV. Default: use CSV value. 
    --dash                      Force all value collections to use dash format, overriding value in CSV. Default: use CSV value. 
    --output|-o filename        Name of reporting output file. May be an absolute filepath, or relative to CWD.  
                                Default output is to stdout. 
```

The mdapply tool reads in a CSV file that contains rows that have specific files, metadata tags, values, disposition, and optional format info. 
Then it applies the specified disposition for each specified file, metadata tag and value collection. 
 
For every CSV row, open the specified file if not already open; write and close any other file if open. 
If not already parsed,  
    * parse the YML content of the file in the same format as mdexport. 
    * Apply the specified disposition to the metadata tag.  
    * If the disposition of a tag is to be added or inserted,  
    * Insert at the beginning of the YML block if a file is newly opened, or immediately after the previous tag from the CSV list if in the same file. (I.e. preserve ordering.) 
    * Output a report of every changed tag. 

## Scenarios

### How to change one or more metadata values in .md files in a git repo or any other file folder. 

### How to specify files (for example, as belonging to a VS workload) and tag each one with a value. 

### How to add or remove individual values from multi-value metadata tags. 

### How to list all files that contain a specified metadata value. 

### How to filter the files that are touched so that only those files that require changes are opened. 

### How to add a new metadata tag, delete an existing tag, or modify an existing value. 

