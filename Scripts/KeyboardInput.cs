using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardInput : MonoBehaviour
{

    Canvas canvas;
    Text canvasText;

    bool bash;
    public int bufferSize;
    public char marker;
    bool commandBusy;

    List<Dictionary<string, string>> commands;
    int currentCommand;
    int markerPosition;
    string currentOutput = "";

    CommandExecuter commandExecuter;
    ScrollRect scrollRect;
    bool doubleTab;

    // Use this for initialization
    void Start()
    {
        canvas = GetComponent<Canvas>();
        canvasText = canvas.GetComponentInChildren<Text>();
        commands = new List<Dictionary<string, string>>();
        AddCommand();
        canvasText.text = "$ ";
        commandExecuter = GetComponent<CommandExecuter>();
        scrollRect = canvas.GetComponentInChildren<ScrollRect>();
        doubleTab = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (CommandBusy())
        {
            CommandOutput();
        }
        else if (commandBusy)
        {
            CommandOutput();
            AddCommand();
            commandBusy = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bash = !bash;
            return;
        }

        // Canvas shows terminal if enabled
        if (canvas != null)
        {
            canvas.enabled = bash;
            //Send fitting text to text field
            SendToConsole(commands);
        }

        // Handle keyboard input to console
        if (bash && !CommandBusy())
        {
            // First handle non text keys
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                //Autocomplete (first word is command rest is files)
                string[] args = commands[currentCommand]["command"].Split(' ');
                if (doubleTab && args.Length == 1)
                {
                    //List possible commands
                    //currentOutput = commandExecuter.ListCompleteCommand(args[args.Length-1]);
                    string completion = commandExecuter.ListCompleteCommand(args[args.Length-1]);
                    if (completion != "")
                    {
                        commands[commands.Count - 1]["completions"] +=
                            "$ " + commands[currentCommand]["command"] + "\n" +
                            completion + "\n";
                    }
                }
                else if (doubleTab)
                {
                    //List possible path
                    //currentOutput = commandExecuter.ListCompletePath(args[args.Length-1]);
                    string completion = commandExecuter.ListCompletePath(args[args.Length-1]);
                    if (completion != "")
                    {
                        commands[commands.Count - 1]["completions"] +=
                            "$ " + commands[currentCommand]["command"] + "\n" +
                            completion + "\n";
                    }
                }
                else if (args.Length == 1)
                {
                    //Complete command
                    string completion = commandExecuter.CompleteCommand(args[args.Length-1]);
                    commands[commands.Count - 1]["command"] += completion;
                    doubleTab = completion == "";
                }
                else
                {
                    //Complete path
                    string completion = commandExecuter.CompletePath(args[args.Length-1]);
                    commands[commands.Count - 1]["command"] += completion;
                    doubleTab = completion == "";
                }
                return;
            }
            else if (Input.anyKeyDown) doubleTab = false;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                // Get previous in buffer
                currentCommand = (currentCommand > 0) ? currentCommand - 1 : 0;
                markerPosition = 0;
                return;
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                // Get next in buffer
                currentCommand = (currentCommand < commands.Count - 1) ? currentCommand + 1 : commands.Count - 1;
                markerPosition = 0;
                return;
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                //Move marker
                if (markerPosition > 0)
                {
                    markerPosition -= 1;
                }
                else
                {
                    markerPosition = 0;
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                //Move marker
                if (markerPosition < commands[currentCommand]["command"].Length)
                {
                    markerPosition += 1;
                }
                else
                {
                    markerPosition = commands[currentCommand]["command"].Length;
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                if (commands[currentCommand]["command"].Length - markerPosition > 0)
                {
                    commands[currentCommand]["command"] =
                        commands[currentCommand]["command"].Remove(commands[currentCommand]["command"].Length - markerPosition - 1, 1);
                }
                return;
            }
            if (Input.GetKeyDown(KeyCode.Return))
            {
                if (commands[currentCommand]["command"].TrimEnd(' ') == "") commands[currentCommand]["completions"] += "$\n";
                else Exectute();
                return;
            }

            //TODO: Change to "all but printable"
            if (Input.GetKey(KeyCode.UpArrow) ||
                Input.GetKey(KeyCode.DownArrow) ||
                Input.GetKey(KeyCode.RightArrow) ||
                Input.GetKey(KeyCode.LeftArrow) ||
                Input.GetKey(KeyCode.Backspace) ||
                Input.GetKey(KeyCode.Return)) return;

            // Handle textinput
            foreach (char c in Input.inputString)
            {
                Debug.Log("Key " + c + " pressed");
                commands[currentCommand]["command"] =
                    commands[currentCommand]["command"].Insert(commands[currentCommand]["command"].Length - markerPosition, c.ToString());
            }
        }
        //Handle keyboard input when command is executing
        else if (bash)
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                //Abort execution
                if (Input.GetKeyDown(KeyCode.C)) commandExecuter.Abort();
            }
        }
        else
        {
            // Character control and gameplay here
        }
    }

    void SendToConsole(List<Dictionary<string, string>> commandList)
    {
        string newText = "";

        //Print all but last command history
        int l = commands.Count;
        for (int i = 0; i < l - 1; i++)
        {
            Dictionary<string, string> c = commands[i];
            string s = c["history"] + "\n" + c["output"];
            if (s[s.Length - 1] != '\n') s += "\n";
            newText += s;
        }

        // Also print current command
        newText += commands[commands.Count - 1]["completions"] + "$ ";
        for (int i = 0; i < commands[currentCommand]["command"].Length; i++)
        {
            if (i == commands[currentCommand]["command"].Length - markerPosition &&
                currentOutput == "") newText += marker;
            else newText += commands[currentCommand]["command"][i];
        }
        if (markerPosition == 0 && currentOutput == "") newText += marker;
        newText += "\n" + currentOutput;
        if (newText[newText.Length - 1] != '\n') newText += "\n";
        //newText += "$ " + commands[currentCommand]["command"] + "\n";

        // Print
        if (newText != canvasText.text)
        {
            canvasText.text = newText;
            //Force scroll
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
        }

    }


    void Exectute()
    {
        commandBusy = true;
        if (commandExecuter != null)
        {
            commandExecuter.ExecuteCommand(commands[currentCommand]["command"]);
        }
    }

    void CommandOutput()
    {
        if (commandExecuter != null)
        {
            currentOutput = commandExecuter.GetOutput();
        }
    }

    bool CommandBusy()
    {
        if (commandExecuter != null)
        {
            return commandExecuter.Busy();
        }
        else
        {
            return false;
        }
    }

    // Add command to command buffer
    void AddCommand()
    {
        Debug.Log("Add command");
        // Move command to history
        if (commands.Count > 0)
        {
            //currentOutput = "\n";
            commands[commands.Count - 1]["history"] = commands[commands.Count - 1]["completions"] + "$ " + commands[currentCommand]["command"];
            commands[commands.Count - 1]["command"] = commands[currentCommand]["command"];

            //Check if history also contains completions, if so, dont copy them to command field
            int lastDollar = commands[currentCommand]["history"].LastIndexOf('$');
            if (lastDollar != -1) commands[currentCommand]["command"] = commands[currentCommand]["history"].Substring(lastDollar + 2);
            else commands[currentCommand]["command"] = commands[currentCommand]["history"];
            commands[currentCommand]["completions"] = "";

            //Add previous output to buffer
            commands[commands.Count - 1]["output"] = currentOutput;
            currentOutput = "";
        }

        // Add new command
        commands.Add(new Dictionary<string, string>());
        currentCommand = commands.Count - 1;
        markerPosition = 0;
        commands[currentCommand].Add("completions", "");
        commands[currentCommand].Add("command", "");
        commands[currentCommand].Add("history", "");
        commands[currentCommand].Add("output", "");

        // Remove if buffer overflows
        if (commands.Count > bufferSize)
        {
            commands.RemoveAt(0);
            currentCommand -= 1;
        }
    }

    // Count how many lines a text uses
    int TextRows(string text)
    {
        // Count how many rows
        return text.Split('\n').Length;
    }

    // Break text into multiple rows, based on characters per row
    string TextRowBreak(string text, int charactersPerRow)
    {
        string s = "";
        for (int i = 0; i < text.Length; i++)
        {
            s += text[i];
            if (i != 0 && i % charactersPerRow == 0) s += "\n";
        }
        return s;
    }
}
