using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System.Text;

public class Commands : MonoBehaviour
{

    CommandExecuter commandExecuter;
    string[] args;
    public string homeDirectory;
    public GameObject player;
    private PythonEngine pythonEngine;
    private StreamReader stdinStreamReader;
    private StreamWriter stdoutStreamWriter;
    private System.Action childAbort;

    DirectoryInfo currentDirectory;

    //Dictionary of valid commands
    Dictionary<string, System.Action> commands;
    // Use this for initialization
    void Start()
    {
        if (homeDirectory == null)
        {
            homeDirectory = Application.dataPath;
        }
        currentDirectory = new DirectoryInfo(homeDirectory);

        //Initialize commands
        commands = new Dictionary<string, System.Action>();

        //Fill command dictionary
        commands["ls"] = List;
        commands["pwd"] = PrintWorkingDirectory;
        commands["wc"] = WordCount;
        commands["cat"] = Concatenate;
        commands["cd"] = ChangeDirectory;
        commands["python"] = Python;

        //Get player python engine
        pythonEngine = player.GetComponent<PythonEngine>();
        childAbort = null;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetArgs(string[] argsIn)
    {
        args = argsIn;
    }

    public void SetStdinStream(StreamReader stdin)
    {
        stdinStreamReader = stdin;
    }
    public void SetStdoutStream(StreamWriter stdout)
    {
        stdoutStreamWriter = stdout;
    }

    public string GetCurrentDirectory()
    {
        return currentDirectory.FullName;
    }

    public bool HasCommand(string command)
    {
        return commands.ContainsKey(command);
    }

    public List<string> GetMatchingCommands(string command, bool clip)
    {
        List<string> list = new List<string>();
        foreach (string key in commands.Keys)
        {
            if (key.StartsWith(command) && clip) list.Add(key.Substring(command.Length));
            else if (key.StartsWith(command)) list.Add(key);
        }
        return list;
    }

    public List<string> GetMatchingPaths(string path, bool clip)
    {
        int lastSlash = path.LastIndexOf('/');
        string completedPath = (lastSlash != -1) ? path.Substring(0, lastSlash) : "";
        string uncompletePath = (lastSlash != -1) ? path.Substring(lastSlash) : path;
        List<string> list = new List<string>();

        //Go to so far complete path
        DirectoryInfo listDirectory = new DirectoryInfo(currentDirectory.FullName + "/" + completedPath);

        if (!listDirectory.Exists) return list;
        // Get content
        FileInfo[] fileInfo = listDirectory.GetFiles();
        DirectoryInfo[] directoryInfo = listDirectory.GetDirectories();

        foreach (DirectoryInfo directory in directoryInfo)
        {
            if (directory.Name.StartsWith(uncompletePath) && clip) list.Add(directory.Name.Substring(uncompletePath.Length) + "/");
            else if (directory.Name.StartsWith(uncompletePath)) list.Add(directory.Name + "/");
        }
        foreach (FileInfo file in fileInfo)
        {
            if (file.Name.StartsWith(uncompletePath) && clip) list.Add(file.Name.Substring(uncompletePath.Length));
            else if (file.Name.StartsWith(uncompletePath)) list.Add(file.Name);
        }
        return list;
    }

    public void RunCommand(string command)
    {
        commands[command]();
        childAbort = null;
    }

    public void Abort()
    {
        if (childAbort != null)
        {
            childAbort();
        }
    }

    //Run callback events
    private void AppendOutput(string s)
    {
        if (stdoutStreamWriter != null)
        {
            stdoutStreamWriter.Write(s);
            stdoutStreamWriter.Flush();
        }
    }

    //------------------------------------------------------
    // COMMANDS
    //------------------------------------------------------

    // ls: List contents of folder 
    void List()
    {
        //TODO: handle asterisk recursive search through multiple hits

        // Get path to list
        string path = "";
        if (args.Length > 1)
        {
            path = args[1];
        }

        // Get directory content
        DirectoryInfo listDirectory = currentDirectory;
        if (path != "")
        {
            listDirectory = new DirectoryInfo(currentDirectory.FullName + "/" + path);
        }

        // Return if directory does not exist
        if (!listDirectory.Exists)
        {
            AppendOutput("ls: " + path + ": No such file or directory");
            return;
        }

        // Get content
        FileInfo[] fileInfo = listDirectory.GetFiles();
        DirectoryInfo[] directoryInfo = listDirectory.GetDirectories();

        foreach (DirectoryInfo directory in directoryInfo)
        {
            Debug.Log(directory.Name);
            AppendOutput(directory.Name + "\t");
        }
        foreach (FileInfo file in fileInfo) 
        {
            Debug.Log(file.Name);
            AppendOutput(file.Name + "\t");
        }

        Debug.Log(path);
    }

    // pwd: Get current directory
    void PrintWorkingDirectory()
    {
        AppendOutput(currentDirectory.FullName);
    }
    
    // wc: count characters in input
    //TODO: count lines, words and characters
    //TODO: work with both stdin and files
    void WordCount()
    {
        string s = stdinStreamReader.ReadToEnd();
        stdinStreamReader.Close();
        AppendOutput(s.Length.ToString());
    }

    // cat: concatenate and print (display) the content of files
    void Concatenate()
    {
        StreamReader streamReader;
        if (args.Length < 2)
        {
            streamReader = stdinStreamReader;
        }
        else if (File.Exists(currentDirectory.FullName + "/" + args[1]))
        {
            streamReader = new StreamReader(currentDirectory.FullName + "/" + args[1]);
        }
        else
        {
            AppendOutput("cat: " + args[1] + ": No such file or directory");
            return;
        }
        string s = streamReader.ReadToEnd();
        streamReader.Close();
        AppendOutput(s);
    }

    // cd: change directory
    //TODO: Limit to home directory
    void ChangeDirectory()
    {
        string path = "";
        if (args.Length < 2)
            path = homeDirectory;
        else
            path = currentDirectory.FullName + "/" + args[1];

        DirectoryInfo newDirectory = new DirectoryInfo(path);
        if (newDirectory.Exists)
            currentDirectory = newDirectory;
        else
            AppendOutput("cd: " + args[1] + ": No such file or directory");
    }

    //------------------------------------------------------
    // IRONPYTHON COMMANDS
    //------------------------------------------------------

    // python: run python script
    void Python()
    {
        if (pythonEngine == null)
        {
            AppendOutput("player has no python engine");
            return;
        }
        if (args.Length == 1)
        {
            AppendOutput("python: not yet able to run in console mode, sorry :(");
            return;
        }

        string path = args[1];
        FileInfo fileInfo = new FileInfo(currentDirectory + "/" + path);
        if (!fileInfo.Exists)
        {
            AppendOutput("python: can't open file '" + path + "': No such file or directory");
            return;
        }
        childAbort = pythonEngine.Abort;
        pythonEngine.SetCwd(currentDirectory.FullName);
        pythonEngine.ExecuteFile(path);//fileInfo.FullName);
        pythonEngine.WriteStdIn(stdinStreamReader.ReadToEnd(), true);
        while (pythonEngine.IsRunning())
        {
            if (pythonEngine.StdOutAvailable())
            {
                AppendOutput(pythonEngine.GetLine());
            }
        }

    }
}
