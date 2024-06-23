
using System.CommandLine;
using System.Diagnostics.SymbolStore;
using System.Reflection.Metadata;

using System.Collections;



//משתנים שהפעולה יכולה לקבל
var bundleOption1 = new Option<FileInfo>("--output", "File path and name");
var bundleOption2 = new Option<string>("--language", "List of language");
var bundleOption3 = new Option<bool>("--note", "source");
var bundleOption4 = new Option<bool>("--sort", "");
var bundleOption5 = new Option<string>("--auther", "File name");
var bundleOption6 = new Option<bool>("--delempty", "delete empty line");

var bundleCommand = new Command("bundle", "Bundle code filed to a single file");

//הוספת המשתנים לפעולה
bundleCommand.AddOption(bundleOption1);
bundleCommand.AddOption(bundleOption2);
bundleCommand.AddOption(bundleOption3);
bundleCommand.AddOption(bundleOption4);
bundleCommand.AddOption(bundleOption5);
bundleCommand.AddOption(bundleOption6);

//מערך לסיומות האפשריות
string[] extension = { "css", "js", "html", "py", "java", "cs", "sql" };

//פונקציה שמוחקת את השורות הריקות מהקובץ
void delEmptyLine(string file)
{
    var lines = File.ReadAllLines(file).Where(arg => !string.IsNullOrWhiteSpace(arg));
    File.WriteAllLines(file, lines);
}

//מחרוזת של כל התוכן שיהיה בקובץ החדש
string s = "";
//פונקציה שמטפלת בתקיה
void folder(string pathIn, bool sort, bool delEmpty)
{
    try
    {
        //מערך של כל הקבצים
        string[] files = Directory.GetFiles(pathIn);

        //מערך שמכיל את כל הקבצים עם הסיומות המבוקשות
        List<string> newFiles = new List<string>();

        foreach (string file in files)
        {
            string ex = file.Substring(file.IndexOf('.') + 1);
            //בדיקה האם הקובץ המבוקש מכיל סיומת מתאימה
            for (int i = 0; i < extension.Length; i++)
                if (ex.CompareTo(extension[i]) == 0)
                {
                    newFiles.Add(file);
                    break;
                }
        }
        //sort
        if (sort)
            newFiles.Sort(((q, p) => q.Substring(q.LastIndexOf('.') + 1).CompareTo(p.Substring(p.LastIndexOf('.') + 1))));
        else
            newFiles.Sort();

        if (newFiles.Count() > 0)
        {
            //כותרת של שם התקיה
            s += "//folder: " + pathIn.Substring(pathIn.LastIndexOf('\\') + 1) + "\n";
            foreach (string file in newFiles)
            {
                if (delEmpty)
                    delEmptyLine(file);
                s += "//file: " + file.Substring(file.LastIndexOf('\\') + 1) + "\n";
                s += File.ReadAllText(file);
            }
        }

        //מערך של כל התקיות
        string[] folders = Directory.GetDirectories(pathIn);
        //מעבר על כל תקיה וטיפול בכל קובץ
        for (int i = 0; i < folders.Length; i++)
            folder(folders[i], sort, delEmpty);
    }
    catch 
    {
        Console.WriteLine("error: the file or argument is wrong");
    }
}

//bundle
bundleCommand.SetHandler((output, language, note, sort, auther, delempty) =>
{
    if (auther != null)
        s += "©: " + auther + "\n";
    if (note)
        s += "sourse: " + output.FullName + "\n";
    try {
        //הצבה במערך הסיומות את הסיומות הרצויות
        if (language != "all")
        {
            string[] l = language.Split(' ');
            for (int j = 0; j < l.Length; j++)
            {
                int i;
                for (i = 0; i < extension.Length && extension[i] != l[j]; i++) ;
                if (i == 7)
                    l[j] = null;
            }
            extension = l;
        }

        folder(Environment.CurrentDirectory, sort, delempty);
        File.WriteAllText(output.FullName, s);
        Console.WriteLine("file was created");
    }
    catch (DirectoryNotFoundException e)
    {
        Console.WriteLine("error: File path is invalied");
    }
    catch
    {
        Console.WriteLine("error: failed");
    }
}, bundleOption1, bundleOption2, bundleOption3, bundleOption4, bundleOption5, bundleOption6);

var rootCommand = new RootCommand("Root command for File Bundler CLI");
rootCommand.AddCommand(bundleCommand);

var rspCommand = new Command("rsp", "Creat response files");

rspCommand.SetHandler(() =>
{
    try
    {
        string s1 = "bundle";

        Console.WriteLine("Enter a path to the new file ");
        string output = Console.ReadLine();
        while (output == "")
        {
            Console.WriteLine("Enter a path to the new file ");
            output = Console.ReadLine();
        }
        s1 += " --output " + output; 
        Console.WriteLine("Enter extensions you want to put in the file, you can put all");
        string language = Console.ReadLine();
        while (language == "" || language[0] != '"' || language[language.Length - 1] != '"')
        {
            if (language == "")
                Console.WriteLine("Enter extensions you want to put in the file, you can put all");
            else
                Console.WriteLine("input with \"\"");
            language = Console.ReadLine();
        }
        s1 += " --language " + language;
        Console.WriteLine("Do you want to sort by code type(the default is to sort by ABC)?(y/n)");
        if (Console.ReadLine() == "y")
            s1 += " --sort " + "true";
        else
            s1 += " --sort " + "false";
        Console.WriteLine("Enter the name of the creator of the file if you want to note it");
        try
        {
            String auther = Console.ReadLine();
            if (auther != "")
                s1 += " --auther " + auther;
        }
        catch
        {
            Console.WriteLine("error: input \"\" to the auther");
        }

        Console.WriteLine("Do you want to delete empty rows?(y/n)");
        if (Console.ReadLine() == "y")
            s1 += " --delempty " + "true";
        else
            s1 += " --delempty " + "false";
        Console.WriteLine("Do you want to save the file source as a comment?(y/n)");
        if (Console.ReadLine() == "y")
            s1 += " --note " + "true";
        else
            s1 += " --note " + "false";
        try
        {
            File.WriteAllText("file.rsp", s1);
        }
        catch 
        {
            Console.WriteLine("error: the file doesnt open");
        }
        Console.WriteLine("Run the following command: פרויקט1 @file.rsp");
    }
    catch
    {
        Console.WriteLine("error: rsp ");
    }
});


rootCommand.AddCommand(rspCommand);

rootCommand.InvokeAsync(args);