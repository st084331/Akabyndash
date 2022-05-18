using System.Text.RegularExpressions;

namespace MyPowerShell;

public class Shell
{
    private Dictionary<string, string> memory = new Dictionary<string, string>();
    private int lastCommandProgress = 0;

    public Shell()
    {
        memory.Add("$?", lastCommandProgress.ToString());
    }

    public int ShellPolling()
    {
        string currentLine = default;
        while (currentLine != "end")
        {
            currentLine = Console.ReadLine();
            ProcessLine(currentLine);
        }

        return lastCommandProgress;
    }

    public int ShellScriptProcess(string path)
    {
        try
        {
            string[] ScriptCommands = File.ReadAllLines(path);
            Shell ScriptShell = new Shell();
            for (int i = 1; i < ScriptCommands.Length; i++)
            {
                ScriptShell.ProcessLine(ScriptCommands[i]);
            }

            return ScriptShell.lastCommandProgress;
        }
        catch
        {
            return -1;
        }
    }

    public int lastCommandUpdate(int res)
    {
        lastCommandProgress = res;
        memory["$?"] = lastCommandProgress.ToString();
        return lastCommandProgress;
    }

    public string[] argsParser(string[] args)
    {
        string[] resArgs = new string[args.Length];
        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];
            resArgs[i] = arg;
            if (arg.Length > 1)
            {
                if (arg.IndexOf('$') == 0)
                {
                    if (memory.ContainsKey(arg))
                    {
                        resArgs[i] = memory[arg];
                    }
                }
            }
        }

        return resArgs;
    }

    public int[] wcCommand(string[] args)
    {
        int[] res = new int[3];
        string path = argsParser(args)[0];
        if (File.Exists(path))
        {
            int sum = 0;
            foreach (var line in File.ReadAllLines(path))
            {
                sum += line.Split(' ').Length;
            }

            res[0] = File.ReadAllLines(path).Length;
            res[1] = sum;
            res[2] = File.ReadAllBytes(path).Length;
        }
        else
        {
            res[0] = -1;
            res[1] = -1;
            res[2] = -1;
        }

        return res;
    }

    public string executeFile(string path, string[] tokens, bool newOut)
    {
        string[] trueTokens = argsParser(tokens);
        string args = string.Empty;
        string verbs = string.Empty;

        for (int i = 1; i < tokens.Length; i++)
        {
            if (tokens[i][0] == '-')
            {
                verbs += tokens[i] + " ";
            }
            else
            {
                args += tokens[i] + " ";
            }
        }
        
        using (System.Diagnostics.Process process = new System.Diagnostics.Process())
        {
            process.StartInfo.FileName = path;
            process.StartInfo.Arguments = args;
            process.StartInfo.Verb = verbs;
            process.StartInfo.UseShellExecute = !newOut;

            process.StartInfo.RedirectStandardOutput = newOut;
            process.StartInfo.RedirectStandardError = newOut;
            process.Start();
            if (newOut)
            {
                StreamReader reader = process.StandardOutput;
                string output = reader.ReadToEnd();
                StreamReader readerError = process.StandardError;
                string outputError = readerError.ReadToEnd();
                Console.WriteLine(output + outputError);
            }

            process.WaitForExit();
            return process.ExitCode.ToString();
        }
    }

    public int moneyCommand(string token)
    {
        int assignmentIndex = token.IndexOf('=');
        if (assignmentIndex != token.Length - 1 && assignmentIndex != -1)
        {
            string[] args = argsParser(token.Split('='));
            if (args.Length == 2)
            {
                try
                {
                    if (memory.ContainsKey(args[0]))
                    {
                        memory[args[0]] = args[1];
                    }
                    else
                    {
                        memory.Add(args[0], args[1]);
                    }

                    return 0;
                }
                catch
                {
                    return -1;
                }
            }
        }
        else
        {
            Console.WriteLine("Error! Unknown command!");
            return -1;
        }

        return -1;
    }

    public string pwdCommand()
    {
        return Directory.GetCurrentDirectory();
    }

    public string catCommand(string[] args)
    {
        string path = argsParser(args)[0];
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        else
        {
            return "No such file.";
        }
    }

    public string echoCommand(string[] args)
    {
        string[] trueArgs = argsParser(args);
        string res = "";
        if (trueArgs.Length > 0)
        {
            if (trueArgs.Length > 1)
            {
                for (int i = 0; i < args.Length - 1; i++)
                {
                    res += trueArgs[i] + " ";
                }
            }

            res += trueArgs[trueArgs.Length - 1];
        }

        return res;
    }

    public string InputToArgs(string input)
    {
        if (File.Exists(input))
        {
            return File.ReadAllText(input);
        }
        else
        {
            return string.Empty;
        }
    }

    public void ProcessLine(string line)
    {
        if (line.Length > 0)
        {
            string[] BigCommands = line.Split(';');
            foreach (var BigCommand in BigCommands)
            {
                int locRes = 10;
                string[] AndCommands = BigCommand.Split("&&");
                foreach (var AndCommand in AndCommands)
                {
                    if (locRes == 0 || locRes == 10)
                    {
                        string[] OrCommands = AndCommand.Split("||");
                        foreach (var OrCommand in OrCommands)
                        {
                            if (locRes != 0 || locRes == 10)
                            {
                                string[] InputCommands = OrCommand.Split("<");
                                if (InputCommands.Length <= 2)
                                {
                                    if (InputCommands.Length == 2)
                                    {
                                        InputCommands[1] = Regex.Replace(InputCommands[1], " {2,}", " ").Trim();
                                        string[] inpt = new[] {InputCommands[1]};
                                        string trueInput = argsParser(inpt)[0];
                                        if (trueInput == string.Empty)
                                        {
                                            Console.WriteLine("Empty filename.");
                                            locRes = lastCommandUpdate(-1);
                                            continue;
                                        }

                                        if (File.Exists(trueInput))
                                        {
                                            InputCommands[0] += " " + InputToArgs(trueInput);
                                        }
                                        else
                                        {
                                            Console.WriteLine("No such file.");
                                            locRes = lastCommandUpdate(-1);
                                            continue;
                                        }
                                    }

                                    bool writePlus = false;
                                    string[] OutputCommands = new string[] { };
                                    if (InputCommands[0].IndexOf(">>") != -1)
                                    {
                                        OutputCommands = InputCommands[0].Split(">>");
                                        writePlus = true;
                                    }
                                    else
                                    {
                                        OutputCommands = InputCommands[0].Split(">");
                                    }

                                    if (OutputCommands.Length <= 2)
                                    {
                                        string outputPath = string.Empty;
                                        if (OutputCommands.Length == 2)
                                        {
                                            OutputCommands[1] = Regex.Replace(OutputCommands[1], " {2,}", " ").Trim();
                                            string[] inpt = new[] {OutputCommands[1]};
                                            string trueOutput = argsParser(inpt)[0];
                                            if (trueOutput == string.Empty)
                                            {
                                                Console.WriteLine("Empty filename.");
                                                locRes = lastCommandUpdate(-1);
                                                continue;
                                            }

                                            if (!File.Exists(trueOutput))
                                            {
                                                try
                                                {
                                                    FileStream fs = File.Create(trueOutput);
                                                    fs.Close();
                                                }
                                                catch
                                                {
                                                    Console.WriteLine("Incorrect filename.");
                                                    locRes = lastCommandUpdate(-1);
                                                    continue;
                                                }
                                            }

                                            outputPath = trueOutput;
                                        }
                                        var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                                        standardOutput.AutoFlush = true;
                                        bool newOut = false;
                                        if (outputPath != string.Empty && File.Exists(outputPath))
                                        {
                                            StreamWriter sr = new StreamWriter(outputPath, writePlus);
                                            Console.SetOut(sr);
                                            newOut = true;
                                            locRes = lastCommandUpdate(ProcessCommand(OutputCommands[0], newOut));
                                            Console.SetOut(standardOutput);
                                            sr.Close();
                                        }
                                        else
                                        {
                                            locRes = lastCommandUpdate(ProcessCommand(OutputCommands[0], newOut));
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("Unknown expression.");
                                        locRes = lastCommandUpdate(-1);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Unknown expression.");
                                    locRes = lastCommandUpdate(-1);
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public int ProcessCommand(string command, bool newOut)
    {
        if (command.Length > 0)
        {
            command = Regex.Replace(command, " {2,}", " ").Trim();
            if (command == " " || command.Length < 1)
            {
                return 0;
            }

            string[] tokens = command.Split(' ');
            if (tokens.Length > 0)
            {
                if (tokens[0][0] == '$' && tokens.Length == 1)
                {
                    return moneyCommand(tokens[0]);
                }
                else if (tokens[0] == "true" && tokens.Length == 1)
                {
                    return 0;
                }

                else if (tokens[0] == "false" && tokens.Length == 1)
                {
                    return -1;
                }

                else if (tokens[0] == "pwd")
                {
                    if (tokens.Length > 1)
                    {
                        Console.WriteLine("Warning! Command pwd requires no arguments.");
                    }

                    try
                    {
                        Console.WriteLine(pwdCommand());


                        return 0;
                    }
                    catch
                    {
                        Console.WriteLine("Can not execute this command.");
                        return -1;
                    }
                }
                else if (tokens[0] == "cat" && tokens.Length >= 2)
                {
                    if (tokens.Length > 2)
                    {
                        Console.WriteLine("Warning! Command cat requires only one argument.");
                    }

                    string[] args = {tokens[1]};
                    if (File.Exists(argsParser(args)[0]))
                    {
                        try
                        {
                            Console.WriteLine(catCommand(args));


                            return 0;
                        }
                        catch
                        {
                            Console.WriteLine("Can not execute this command.");
                            return -1;
                        }
                    }
                    else
                    {
                        Console.WriteLine("No such file.");
                        return -1;
                    }
                }
                else if (tokens[0] == "echo")
                {
                    string[] args = new string[tokens.Length - 1];
                    for (int i = 0; i < args.Length; i++)
                    {
                        args[i] = tokens[i + 1];
                    }

                    if (args.Length > 0)
                    {
                        try
                        {
                            Console.WriteLine(echoCommand(args));


                            return 0;
                        }
                        catch
                        {
                            Console.WriteLine("Can not execute this command.");
                            return -1;
                        }
                    }
                    else
                    {
                        return 0;
                    }
                }
                else if (tokens[0] == "wc" && tokens.Length > 1)
                {
                    string[] args = new[] {tokens[1]};
                    try
                    {
                        int[] res = wcCommand(args);
                        if (res[0] != -1)
                        {
                            Console.WriteLine(res[0]);
                            Console.WriteLine(res[1]);
                            Console.WriteLine(res[2]);

                            return 0;
                        }
                        else
                        {
                            Console.WriteLine("No such file.");
                            return -1;
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Can not execute this command.");
                        return -1;
                    }
                }
                else if (File.Exists(argsParser(tokens)[0]))
                {
                    string path = argsParser(tokens)[0];
                    string[] ScriptCommands = File.ReadAllLines(path);
                    if (ScriptCommands.Length > 0)
                    {
                        if (ScriptCommands[0] == "Akabyndash")
                        {
                            return ShellScriptProcess(path);
                        }
                    }

                    try
                    {
                        Console.WriteLine("The process exited, returning " + executeFile(path, tokens, newOut));
                        return 0;
                    }
                    catch
                    {
                        Console.WriteLine("Can not execute file.");
                        return -1;
                    }
                }
                else if (tokens[0] != "end")
                {
                    Console.WriteLine("Error! Unknown command!");
                    return -1;
                }
            }
        }

        return 0;
    }
}