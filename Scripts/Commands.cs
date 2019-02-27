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
    //private PythonEngine pythonEngine;
    private Stream stdinStream;
    private Stream stdoutStream;

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
        //commands["python"] = Python;

        //Get player python engine
        //pythonEngine = player.GetComponent<PythonEngine>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetArgs(string[] argsIn)
    {
        args = argsIn;
    }

    public void SetStdinStream(Stream stdin)
    {
        stdinStream = stdin;
    }
    public void SetStdoutStream(Stream stdout)
    {
        stdoutStream = stdout;
    }

    public bool HasCommand(string command)
    {
        return commands.ContainsKey(command);
    }

    public void RunCommand(string command)
    {
        commands[command]();
    }

    //Run callback events
    private void AppendOutput(string s)
    {
        if (stdoutStream != null)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(s);
            stdoutStream.Write(bytes, 0, bytes.Length);
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
        byte[] array = new byte[1024];
        int count = stdinStream.Read(array, 0, 1024);
        int sum = 0;
        while (count != -1)
        {
            sum += count;
            string s = "";
            for (int i = 0; i < count; i++)
            {
                s += (char)array[i];
            }
            Debug.Log("Wordcount: " + count);
            count = stdinStream.Read(array, 0, 1024);
        }
        AppendOutput(sum.ToString());
    }

    //------------------------------------------------------
    // IRONPYTHON COMMANDS
    //------------------------------------------------------

    /*
    // python: run python script
    void Python()
    {
        if (pythonEngine == null)
        {
            commandExecuter.AppendOutput("player has no python engine");
            return;
        }
        if (args.Length == 1)
        {
            commandExecuter.AppendOutput("python: not yet able to run in console mode, sorry :(");
            return;
        }

        string path = args[1];
        FileInfo fileInfo = new FileInfo(currentDirectory + "/" + path);
        if (!fileInfo.Exists)
        {
            commandExecuter.AppendOutput("python: can't open file '" + path + "': No such file or directory");
            return;
        }

        pythonEngine.SetCwd(currentDirectory.FullName);
        pythonEngine.ExecuteFile(path);//fileInfo.FullName);
        while (pythonEngine.IsRunning())
        {
            if (pythonEngine.StdOutChanged())
            {
                commandExecuter.SetOutput(pythonEngine.GetStdOut());
            }
        }

    }
    */
}
