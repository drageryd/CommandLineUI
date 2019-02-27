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
        //output = "";
        stdinStream = null;
        stdpipeStream = null;
        stdoutStream = null;

        //Piping
        string[] pipe = command.Split('|');
        for (int i = 0; i < pipe.Length; i++)
        {
            //Leading and trailing spaces
            string c = pipe[i].TrimStart(' ').TrimEnd(' ');

            //TODO: Look for redirects here
            if (i == 0) stdinStream = new StreamStdio();
            else stdinStream = stdpipeStream;
            stdpipeStream = new StreamStdio();
            if (i == pipe.Length - 1) stdoutStream = (StreamStdio)stdpipeStream;

            string[] args = c.Split(' ');
            if (commandObject != null)
            {
                if (commandObject.HasCommand(args[0]))
                {
                    commandObject.SetStdinStream(stdinStream);
                    commandObject.SetStdoutStream(stdpipeStream);
                    commandObject.SetArgs(args);
                    commandObject.RunCommand(args[0]);
                    stdpipeStream.Close();
                }
                else
                {
                    //output = args[0] + ": command not found";
                    string s = args[0] + ": command not found";
                    byte[] bytes = Encoding.ASCII.GetBytes(s);
                    stdpipeStream.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }

    void Nothing()
    {
        //Do nothing
    }
}
