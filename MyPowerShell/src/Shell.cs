using System.Text.RegularExpressions;

namespace MyPowerShell;

public class Shell
{
    //Память реализованна в виде словаря
    private Dictionary<string, string> memory = new Dictionary<string, string>();
    //Запоминаем последнюю комманду независимо от обновления памяти
    private int lastCommandProgress = 0;
    //Переменная для передачи о завершении работы
    private bool polling = true;

    public Dictionary<string, string> getMemory()
    {
        return memory;
    }
    
    public void addToMemoryOrUpdate(string variable, string value)
    {
        //Изменяем значение переменной, если уже созданна
        if (memory.ContainsKey(variable))
        {
            memory[variable] = value;
        }
        //Добавляем переменную в память, если не созданна
        else
        {
            memory.Add(variable, value);
        }
    }

    public int getLastCommandProgress()
    {
        return lastCommandProgress;
    }

    public bool getPolling()
    {
        return polling;
    }
    
    //Запуская оболочку в ее памяти всегда должена быть переменная $? по условия задания 
    public Shell()
    {
        memory.Add("$?", lastCommandProgress.ToString());
    }

    public int ShellPolling()
    {
        string currentLine = default;
        //Запрашиваем от пользователя комманды
        while (polling)
        {
            currentLine = Console.ReadLine();
            //Парсинг строки
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

    
    public void ProcessLine(string line)
    {
        if (line.Length > 0)
        {
            line = Regex.Replace(line, " {2,}", " ").Trim();
            //Разделяем строку на последовательность
            string[] woConnectorsCommands = Regex.Split(line, @"(;)|(&&)|(\|\|)");
            bool skipNext = false;
            foreach (var commandWithRedicters in woConnectorsCommands)
            {
                if (commandWithRedicters == "&&" || commandWithRedicters == "||" || commandWithRedicters == ";")
                {
                    if ((commandWithRedicters == "&&" && lastCommandProgress != 0) ||
                        (commandWithRedicters == "||" && lastCommandProgress == 0))
                    {
                        //Пропускаем следующую команду, она будет защитана как неудачно завершенная
                        lastCommandUpdate(-1);
                        skipNext = true;
                    }
                    else
                    {
                        skipNext = false;
                    }
                    continue;
                }
                else if (!skipNext)
                {
                    //Переменная для указания нужна ли дозапись или перезапись
                    bool writePlus = false;

                    string[] OutputCommands = new string[] { };

                    //Делим элементы предыдущего массива на подзадачи относительно ">>" или ">"
                    if (commandWithRedicters.IndexOf(">>") != -1)
                    {
                        OutputCommands = commandWithRedicters.Split(">>");
                        writePlus = true;
                    }
                    else
                    {
                        OutputCommands = commandWithRedicters.Split(">");
                    }

                    if (OutputCommands.Length <= 2 && OutputCommands.Length > 0)
                    {
                        //Отделяем комманду от ее параметров, которые переданный в файле
                        string[] InputCommands = OutputCommands[0].Split("<");
                        if (InputCommands.Length <= 2 && InputCommands.Length > 0)
                        {
                            //Проверка есть ли какая-нибудь комманда?
                            InputCommands[0] = Regex.Replace(InputCommands[0], " {2,}", " ").Trim();
                            if (InputCommands[0] == String.Empty || InputCommands[0] == " ")
                            {
                                lastCommandUpdate(0);
                                continue;
                            }

                            string outputPath = string.Empty;
                            if (OutputCommands.Length == 2)
                            {
                                //Удаление лишних пробелов
                                OutputCommands[1] = Regex.Replace(OutputCommands[1], " {2,}", " ").Trim();
                                string[] inpt = new[] {OutputCommands[1]};
                                //Если названием файла была переменная, то используем значение переменной
                                string trueOutput = argsParser(inpt)[0];
                                if (trueOutput == string.Empty)
                                {
                                    Console.WriteLine("Empty filename.");
                                    lastCommandUpdate(-1);
                                    continue;
                                }

                                if (!File.Exists(trueOutput))
                                {
                                    try
                                    {
                                        //Создаем файл для записи, если его не было
                                        FileStream fs = File.Create(trueOutput);
                                        fs.Close();
                                    }
                                    catch
                                    {
                                        Console.WriteLine("Incorrect filename.");
                                        lastCommandUpdate(-1);
                                        continue;
                                    }
                                }

                                outputPath = trueOutput;
                            }

                            if (InputCommands.Length == 2)
                            {
                                //Удаление лишних пробелов
                                InputCommands[1] = Regex.Replace(InputCommands[1], " {2,}", " ").Trim();
                                string[] inpt = new[] {InputCommands[1]};
                                //Если названием файла была переменная, то используем значение переменной
                                string trueInput = argsParser(inpt)[0];
                                if (trueInput == string.Empty)
                                {
                                    Console.WriteLine("Empty filename.");
                                    lastCommandUpdate(-1);
                                    continue;
                                }

                                if (File.Exists(trueInput))
                                {
                                    //Добавляем к комманде аргументы
                                    InputCommands[0] += " " + InputToArgs(trueInput);
                                }
                                else
                                {
                                    Console.WriteLine("No such file.");
                                    lastCommandUpdate(-1);
                                    continue;
                                }
                            }

                            var standardOutput = new StreamWriter(Console.OpenStandardOutput());
                            standardOutput.AutoFlush = true;
                            bool newOut = false;
                            if (outputPath != string.Empty && File.Exists(outputPath))
                            {
                                //Если указан файл вывода, то меняем Out
                                StreamWriter sr = new StreamWriter(outputPath, writePlus);
                                Console.SetOut(sr);
                                newOut = true;
                                lastCommandUpdate(ProcessCommand(InputCommands[0], newOut));
                                //Возвращаем консоль к стандартному значению после вывода
                                Console.SetOut(standardOutput);
                                sr.Close();
                            }
                            else
                            {
                                lastCommandUpdate(ProcessCommand(InputCommands[0], newOut));
                            }
                        }
                        else
                        {
                            Console.WriteLine("Unknown expression.");
                            lastCommandUpdate(-1);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Unknown expression.");
                        lastCommandUpdate(-1);
                    }
                }
                else
                {
                    skipNext = false;
                }
            }
        }
    }
    
            
        


    public int lastCommandUpdate(int res)
    {
        //Обновляем значение переменной в памяти и поле класса
        lastCommandProgress = res;
        memory["$?"] = lastCommandProgress.ToString();
        return lastCommandProgress;
    }

    public string[] argsParser(string[] args)
    {
        //набор аргументов преобразуем в набор, где раскрыты все переменные
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

    public int[] wcCommand(string path)
    {
        int[] res = new int[3];
        if (File.Exists(path))
        {
            foreach (string line in File.ReadAllLines(path))
            {
                //Удаление лишних пробелов, так как считаем слова
                res[1] += Regex.Replace(line, " {2,}", " ").Trim().Split(' ').Length;
                res[0]++;
            }
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

    public string executeFile(string path, string[] allArgs, bool newOut)
    {
        string args = string.Empty;
        string verbs = string.Empty;
        for (int i = 1; i < allArgs.Length; i++)
        {
            if (allArgs[i][0] == '-')
            {
                verbs += allArgs[i] + " ";
            }
            else
            {
                args += allArgs[i] + " ";
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
        //Присутствует ли приравнивание
        if (assignmentIndex != token.Length - 1 && assignmentIndex != -1)
        {
            string[] tokens = token.Split('=');
            if (tokens.Length == 2)
            {
                string value = argsParser(tokens)[1];
                string variable = tokens[0];
                try
                {
                    addToMemoryOrUpdate(variable, value);
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

    public string catCommand(string path)
    {
        return File.ReadAllText(path);
    }

    public string echoCommand(string[] args)
    {
        string res = string.Empty;
        foreach (var arg in args)
        {
            res += arg + " ";
        }
        res = res.Trim();
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

    public int ProcessCommand(string command, bool newOut)
    {
        string[] tokens = command.Split(' ');
        if (tokens.Length > 0)
        {
            //Это переменная?
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
                string path= argsParser(args)[0];
                if (File.Exists(path))
                {
                    try
                    {
                        Console.WriteLine(catCommand(path));
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
                        Console.WriteLine(echoCommand(argsParser(args)));


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
                string[] args = {tokens[1]};
                string path = argsParser(args)[0];
                try
                {
                    int[] res = wcCommand(path);
                    if (res[0] != -1)
                    {
                        //строки
                        Console.WriteLine(res[0]);
                        //слова - набор символов, разделенных пробелами
                        Console.WriteLine(res[1]);
                        //байты
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
                string[] arg = {tokens[0]};
                
                string path = argsParser(arg)[0];
                string[] ScriptCommands = File.ReadAllLines(path);
                if (ScriptCommands.Length > 0)
                {
                    //Первая строка скрипта это Akabyndash
                    if (ScriptCommands[0] == "Akabyndash")
                    {
                        return ShellScriptProcess(path);
                    }
                }

                try
                { 
                    //Если не скрипт, то запускаем файл
                    string[] args = argsParser(tokens);
                    executeFile(path, args, newOut);
                    return 0;
                }
                catch
                {
                    Console.WriteLine("Can not execute file.");
                    return -1;
                }
            }
            //end комманда для завершения оболочки
            else if (tokens[0] == "end")
            {
                polling = false;
                return 0;
            }
            else
            {
                Console.WriteLine("Error! Unknown command!");
                return -1;
            }
        }

        return 0;
    }
}