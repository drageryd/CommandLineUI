using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class Commands : MonoBehaviour
{

    CommandExecuter commandExecuter;
    string[] args;
    public string homeDirectory;
    DirectoryInfo currentDirectory;

    //Dictionary of valid commands
    Dictionary<string, System.Action> commands;
    // Use this for initialization
    void Start()
    {
        commandExecuter = GetComponent<CommandExecuter>();
        //homeDirectory = Application.dataPath;
        currentDirectory = new DirectoryInfo(homeDirectory);

        //Initialize commands
        commands = new Dictionary<string, System.Action>();

        //Fill command dictionary
        commands["ls"] = List;
        commands["pwd"] = PrintWorkingDirectory;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetArgs(string[] argsIn)
    {
        args = argsIn;
    }

    public bool HasCommand(string command)
    {
        return commands.ContainsKey(command);
    }

    public void RunCommand(string command)
    {
        commands[command]();
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
            commandExecuter.AppendOutput("ls: " + path + ": No such file or directory");
            return;
        }

        // Get content
        FileInfo[] fileInfo = listDirectory.GetFiles();
        DirectoryInfo[] directoryInfo = listDirectory.GetDirectories();

        foreach (DirectoryInfo directory in directoryInfo)
        {
            Debug.Log(directory.Name);
            commandExecuter.AppendOutput(directory.Name + "\t");
        }
        foreach (FileInfo file in fileInfo) 
        {
            Debug.Log(file.Name);
            commandExecuter.AppendOutput(file.Name + "\t");
        }
        //System.IO.Path.GetDirectoryName(Application.ExecutablePath);

        Debug.Log(path);
    }

    // pwd: Get current directory
    void PrintWorkingDirectory()
    {
        commandExecuter.AppendOutput(currentDirectory.FullName);
    }

}
