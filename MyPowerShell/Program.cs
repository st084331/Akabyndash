namespace MyPowerShell;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello!\nThis is my own Shell - Akabyndash\n");
        //Запуск
        Shell shell = new Shell();
        shell.ShellPolling();
        Console.WriteLine("Good Bye!\n");
    }
}