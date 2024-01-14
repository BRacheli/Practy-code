using System;
using System.CommandLine;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Channels;

//bundle command:
var bundleCommand = new Command("bundle", "Bundle code files to a single file");

//bundle options:
var outputOption = new Option<FileInfo>(aliases: new string[] { "-o", "--output" }, description: "File path and name");
outputOption.SetDefaultValue(null);
var languageOption = new Option<string>(aliases: new string[] { "-l", "--language" }, description: "Language of code") { IsRequired = true };
var noteOption = new Option<bool?>(aliases: new string[] { "-n", "--note" }, description: "Comment with source code");
var sortOption = new Option<string>(aliases: new string[] { "-s", "--sort" }, description: "Order of copying the code files, according to the alphabet of the file name or to the type of code.");
var removeEmptyLinesOption = new Option<bool?>(aliases: new string[] { "-r", "--remove-empty-lines" }, description: "Remove the empty lines from the source code before copying it into the bundle file.");
var authorOption = new Option<string>(aliases: new string[] { "-a", "--author" }, description: "Write the name of creator of the file");

bundleCommand.AddOption(outputOption);
bundleCommand.AddOption(languageOption);
bundleCommand.AddOption(noteOption);
bundleCommand.AddOption(sortOption);
bundleCommand.AddOption(removeEmptyLinesOption);
bundleCommand.AddOption(authorOption);
//bundle handle
#region bundle command
bundleCommand.SetHandler((output, language, note, sort, removeEmptyLines, author) =>
{
    if (output == null)
    {
        Console.WriteLine("Output option is not provided or is null.");
    }
    try
    {
        DirectoryInfo currentFolder = new DirectoryInfo(Directory.GetCurrentDirectory());
        List<FileInfo> allFiles = new List<FileInfo>();
        List<FileInfo> selectedFiles = new List<FileInfo>();
        List<string> selectedLanguages = new List<string>();
        //all files
        allFiles = currentFolder.GetFiles("*.*", SearchOption.AllDirectories).Where(file =>
        {
            var fileName = file.Directory?.Name?.ToLower();
            return fileName != null && fileName != "bin" && fileName != "debug";

        }).ToList();

        selectedFiles = allFiles;

        if (language != "all" && language != null)
        {
            //selected language
            selectedLanguages = language.Split(',').Select(x => x.Trim().ToLower()).ToList();

            //if entered all in list
            if (selectedLanguages.Contains("all"))
            {
                selectedFiles = allFiles;
            }
            else
            {
                selectedFiles = allFiles.Where(file =>
                {
                    string fileExtension = Path.GetExtension(file.Name).TrimStart('.').ToLower();
                    return selectedLanguages.Contains(fileExtension);

                }).ToList();
            }
            if (selectedFiles.Count == 0)
            {
                Console.WriteLine("No files found for the selected languages.");
                return;
            }
        }

        if (sort != null)
        {
            selectedFiles = SortCodeFiles(selectedFiles, sort);
        }

        string bundleCode = "";

        foreach (var file in selectedFiles)
        {
            if (note == true)
            {
                bundleCode += "file name: " + file.Name + "\n file path: " + file.FullName + "\n";
            }
            var fileCode = File.ReadAllText(file.FullName);
            if (removeEmptyLines == true)
                fileCode = DeleteEmptyLines(fileCode);
            bundleCode += fileCode;
        }
        if (output != null && output.FullName != null)
        {
            if (!string.IsNullOrEmpty(author))
            {
                var authorString = "**************************\n" + "Created by:" + author + "\n**************************";
                File.WriteAllText(output.FullName, authorString + bundleCode);
            }
            else
            {
                File.WriteAllText(output.FullName, bundleCode);
            }
            Console.WriteLine("Code files bundled successfully!");
        }
    }
    catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }

}, outputOption, languageOption, noteOption, sortOption, removeEmptyLinesOption, authorOption);
#endregion

var createRspCommand = new Command("create-rsp", "Create a response file with a prepared command");

var rspFilePathOption = new Option<FileInfo>(aliases: new string[] { "-r", "--rsp-file-path" }, description: "Response file path and name");

createRspCommand.AddOption(rspFilePathOption);

createRspCommand.SetHandler(rspFilePath =>
{
    try
    {
        if (rspFilePath != null && rspFilePath.FullName != null)
        {
            Console.WriteLine("Enter path and name for file");
            var output = Console.ReadLine();

            Console.WriteLine("Enter programming languages or all ");
            var language = Console.ReadLine();

            Console.WriteLine("Include source comment ? (y/n) ");
            var sourceCommentAns = Console.ReadLine();
            bool sourceComment = false;
            if (sourceCommentAns != null && sourceCommentAns.ToLower() == "y")
                sourceComment = true;

            Console.Write("Order the code files (alphabet/type): ");
            var order = Console.ReadLine();

            Console.WriteLine("Remove empty lines ? (y/n) ");
            var removeEmptyLines = Console.ReadLine();
            bool r_m_l = false;
            if (removeEmptyLines != null && removeEmptyLines.ToLower() == "y")
                r_m_l = true;
            Console.WriteLine("Enter author of this code or press enter");
            var author = Console.ReadLine();

            var rspContent = "";
            if (sourceComment == true || r_m_l == true)
            {
                if (sourceComment == true && r_m_l == true)
                {
                    rspContent = $"bundle --output \"{output}\" --language \"{language}\" --note --sort \"{order}\" --remove-empty-lines --author \"{author}\"";
                }
                else
                {
                    if (sourceComment == true)
                    {
                        rspContent = $"bundle --output \"{output}\" --language \"{language}\" --note --sort \"{order}\" --author \"{author}\"";
                    }
                    else
                    {
                        rspContent = $"bundle --output \"{output}\" --language \"{language}\" --sort \"{order}\" --remove-empty-lines --author \"{author}\"";
                    }
                }
            }
            else
            {
                rspContent = $"bundle --output \"{output}\" --language \"{language}\" --sort \"{order}\" --author \"{author}\"";
            }
            File.WriteAllText(rspFilePath.FullName, "fib "+rspContent);
            Console.WriteLine("Created a response file");
        }
        else
        {
            Console.WriteLine("Error: The response file path is not provided.");
        }
    }
    catch (Exception ex) { Console.WriteLine("Error: " + ex.Message); }
}, rspFilePathOption);



string DeleteEmptyLines(string fileCode)
{
    string[] linesCode = fileCode.Split("\n");
    return string.Join("\n", linesCode.Where(line => !string.IsNullOrWhiteSpace(line)).ToString());
}

List<FileInfo> SortCodeFiles(List<FileInfo> selectedFiles, string sortOptionValue)
{
    if (sortOptionValue == "type".ToLower())
        return selectedFiles.OrderBy(f => f.Extension).ToList();
    else
        return selectedFiles.OrderBy(f => f.Name).ToList();
}

var rootCommand = new RootCommand("Root command for file bundle CLI ");
rootCommand.AddCommand(bundleCommand);
rootCommand.AddCommand(createRspCommand);
rootCommand.InvokeAsync(args);

