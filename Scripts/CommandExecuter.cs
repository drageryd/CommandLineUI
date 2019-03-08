using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;
using System.Reflection;
using System.IO;
using System.Text;
using System;
using CommandLine;

public class CommandExecuter : MonoBehaviour {

    Thread myThread;
    bool busy;
    //string output;
    Commands commandObject;
    Stream stdinStream;
    Stream stdpipeStream;
    StreamStdio stdoutStream;

    // Use this for initialization
    void Start () {
        busy = false;
        //output = "";
        myThread = new Thread(Nothing);
        commandObject = GetComponent<Commands>();
    }
	
	// Update is called once per frame
    void Update () {
        busy = myThread.IsAlive;

        if (myThread.IsAlive)
        {
            //Do stuff
        }
	}

    public bool Busy () {
        return busy;
    }

    public void ExecuteCommand(string command)
    {
        myThread = new Thread(() => Run(command));
        myThread.Start();
    }

    public string GetOutput()
    {
        if (stdoutStream != null)
        {
            return stdoutStream.GetText();
        }
        return "";
    }

    void Run(string command)
    {
        stdinStream = null;
        stdpipeStream = null;
        stdoutStream = null;
        StreamWriter stdpipeStreamWriter = null;
        StreamReader stdinStreamReader = null;

        //Piping
        string[] pipe = command.Split('|');
        for (int i = 0; i < pipe.Length; i++)
        {
            //Leading and trailing spaces
            string pipeCommand = pipe[i].TrimStart(' ').TrimEnd(' ');

            //TODO: Look for redirects here
            if (i == 0) 
            {
                stdinStream = new StreamStdio();
                stdinStream.Close();
            }
            else stdinStream = stdpipeStream;
            stdpipeStream = new StreamStdio();
            if (i == pipe.Length - 1) stdoutStream = (StreamStdio)stdpipeStream;

            //Look for redirect
            pipeCommand = FindRedirectOut(pipeCommand, ref stdpipeStreamWriter);
            pipeCommand = FindRedirectIn(pipeCommand, ref stdinStreamReader);

            Debug.Log(pipeCommand);

            string[] args = pipeCommand.Split(' ');
            if (commandObject != null)
            {
                if (commandObject.HasCommand(args[0]))
                {
                    commandObject.SetStdinStream(stdinStreamReader);
                    commandObject.SetStdoutStream(stdpipeStreamWriter);
                    commandObject.SetArgs(args);
                    commandObject.RunCommand(args[0]);
                    stdpipeStreamWriter.Close();
                    stdpipeStream.Close();
                    stdinStreamReader.Close();
                }
                else
                {
                    StreamWrite(stdpipeStream, args[0] + ": command not found");
                }
            }
        }
    }

    public void Abort()
    {
        if (myThread.IsAlive)
        {
            //Kills potential child threads
            commandObject.Abort();
            myThread.Abort();
            StreamWrite(stdpipeStream, "Interrupt");
            stdinStream.Close();
            stdpipeStream.Close();
            stdoutStream.Close();
        }
    }

    string FindRedirectOut(string command, ref StreamWriter streamWriter)
    {
        int redirect = command.IndexOf('>');
        if (redirect != -1)
        {
            int start = redirect + 1;
            while (start < command.Length && command[start] == ' ') start++;
            if (start >= command.Length)
            {
                StreamWrite(stdpipeStream, "syntax error near unexpected token newline");
                return "";
            }
            int end = start;
            while (end < command.Length && command[end] != ' ' && command[end] != '\n') end++;
            string path = command.Substring(start, end-start);
            streamWriter = new StreamWriter(commandObject.GetCurrentDirectory() + "/" + path);
            command = command.Remove(redirect, end-redirect).TrimEnd(' ');
        }
        else
        {
            streamWriter = new StreamWriter(stdpipeStream);
        }
        return command;
    }

    string FindRedirectIn(string command, ref StreamReader streamReader)
    {
        int redirect = command.IndexOf('<');
        if (redirect != -1)
        {
            int start = redirect + 1;
            while (start < command.Length && command[start] == ' ') start++;
            if (start >= command.Length)
            {
                StreamWrite(stdpipeStream, "syntax error near unexpected token newline");
                return "";
            }
            int end = start;
            while (end < command.Length && command[end] != ' ' && command[end] != '\n') end++;
            string path = command.Substring(start, end-start);
            streamReader = new StreamReader(commandObject.GetCurrentDirectory() + "/" + path);
            command = command.Remove(redirect, end-redirect).TrimEnd(' ');
        }
        else
        {
            streamReader = new StreamReader(stdinStream);
        }
        return command;
    }

    void StreamWrite(Stream stream, string text)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(text);
        stream.Write(bytes, 0, bytes.Length);
    }

    //List possible commands
    public string ListCompleteCommand(string command)
    {
        List<string> list = commandObject.GetMatchingCommands(command, false);
        return String.Join("\t", list.ToArray());
    }

    //List possible path
    public string ListCompletePath(string path)
    {
        List<string> list = commandObject.GetMatchingPaths(path, false);
        return String.Join("\t", list.ToArray());
    }

    //Complete command
    public string CompleteCommand(string command)
    {
        List<string> list = commandObject.GetMatchingCommands(command, true);
        //If only one possible
        if (list.Count == 1) return list[0] + " ";
        //Autofill all matching starting characters
        else return GetCommonChars(list);
    }

    //Complete path
    public string CompletePath(string path)
    {
        List<string> list = commandObject.GetMatchingPaths(path, true);
        if (list.Count == 1) return list[0] + " ";
        //Autofill all matching starting characters
        else return GetCommonChars(list);
    }

    string GetCommonChars(List<string> list)
    {
        //No matches
        if (list.Count == 0) return "";
        //Length of shortest word in list
        string same = "";
        int l = list[0].Length;
        foreach(string c in list) l = (c.Length < l) ? c.Length : l;
        //If all characters match, append to completion
        for (int i = 0; i < l; i++)
        {
            foreach (string s in list)
            {
                //If not match, return all common
                if (s[i] != list[0][i]) return same;
            }
            //If loop completes all characters are equal
            same += list[0][i];
        }
        //If this loop completes all string matches shortest word
        return same;
    }

    void Nothing()
    {
        //Do nothing
    }
}
