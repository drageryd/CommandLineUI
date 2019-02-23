using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;
using System.Reflection;
using System.IO;
using System;

public class CommandExecuter : MonoBehaviour {

    Thread myThread;
    bool busy;
    string output;
    Commands commandObject;

    // Use this for initialization
    void Start () {
        busy = false;
        output = "";
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
        return output;
    }

    public void AppendOutput(string s)
    {
        output += s;
    }

    public void SetOutput(string s)
    {
        output = s;
    }

    void Run(string command)
    {
        output = "";
        string[] args = command.Split(' ');
        if (commandObject != null)
        {
            if (commandObject.HasCommand(args[0]))
            {
                commandObject.SetArgs(args);
                commandObject.RunCommand(args[0]);
            }
            else
            {
                output = args[0] + ": command not found";
            }
        }
    }

    void Nothing()
    {
        //Do nothing
    }
}
