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

        //Piping
        string[] pipe = command.Split('|');
        for (int i = 0; i < pipe.Length; i++)
        {
            //Leading and trailing spaces
            string pipeCommand = pipe[i].TrimStart(' ').TrimEnd(' ');

            //TODO: Look for redirects here
            if (i == 0) stdinStream = new StreamStdio();
            else stdinStream = stdpipeStream;
            stdpipeStream = new StreamStdio();
            if (i == pipe.Length - 1) stdoutStream = (StreamStdio)stdpipeStream;

            //Look for redirect
            pipeCommand = FindRedirectOut(pipeCommand, ref stdpipeStreamWriter);

            Debug.Log(pipeCommand);

            string[] args = pipeCommand.Split(' ');
            if (commandObject != null)
            {
                if (commandObject.HasCommand(args[0]))
                {
                    commandObject.SetStdinStream(stdinStream);
                    commandObject.SetStdoutStream(stdpipeStreamWriter);
                    commandObject.SetArgs(args);
                    commandObject.RunCommand(args[0]);
                    stdpipeStreamWriter.Close();
                    stdpipeStream.Close();
                }
                else
                {
                    StreamWrite(stdpipeStream, args[0] + ": command not found");
                }
            }
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

    void StreamWrite(Stream stream, string text)
    {
        byte[] bytes = Encoding.ASCII.GetBytes(text);
        stream.Write(bytes, 0, bytes.Length);
    }

    void Nothing()
    {
        //Do nothing
    }
}
