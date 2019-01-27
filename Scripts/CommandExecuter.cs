using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Threading;
using System.Reflection;
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

    void Run(string command)
    {
        output = "";
        string[] args = command.Split(' ');
        if (commandObject != null)
        {
            /*
            Type type = commandObject.GetType();
            MethodInfo methodInfo = type.GetMethod(args[0]);
            if (methodInfo != null)
            {
                commandObject.SetArgs(args);
                Debug.Log("Command execute " + string.Join(" - ", args));
                methodInfo.Invoke(commandObject, null);
            }
            */
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
        //int number = (int)Random.Range(1, 5);
        //output = "Hello";
        //Thread.Sleep(1000);
        //output += " World";
        //Thread.Sleep(1000);
        //output += " foo\n";

    }

    void Nothing()
    {
        //Do nothing
    }
}
