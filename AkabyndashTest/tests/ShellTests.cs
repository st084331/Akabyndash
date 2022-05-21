using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using MyPowerShell;
using NUnit.Framework;

namespace AkabyndashTest;

public class ShellTests
{
    [Test]
    public void InitTest()
    {
        Shell testShell = new Shell();
        Dictionary<string, string> testDict = new Dictionary<string, string>();
        testDict.Add("$?", "0");
        Assert.AreEqual(testDict, testShell.getMemory());
        StringAssert.IsMatch(testShell.getMemory()["$?"], "0");
        Assert.AreEqual(testShell.getLastCommandProgress(), 0);
        Assert.AreEqual(testShell.getPolling(), true);
    }


    [Test]
    public void lastCommandUpdateTest()
    {
        Shell testShell = new Shell();
        testShell.lastCommandUpdate(-1);
        StringAssert.IsMatch(testShell.getMemory()["$?"], "-1");
        Assert.AreEqual(testShell.getLastCommandProgress(), -1);
    }

    [Test]
    public void addToMemoryOrUpdateTest1()
    {
        Shell testShell = new Shell();
        Dictionary<string, string> testDict = new Dictionary<string, string>();
        testDict.Add("$?", "0");
        testShell.addToMemoryOrUpdate("$f", "hi");
        testDict.Add("$f", "hi");
        Assert.AreEqual(testDict, testShell.getMemory());
    }

    [Test]
    public void addToMemoryOrUpdateTest2()
    {
        Shell testShell = new Shell();
        Dictionary<string, string> testDict = new Dictionary<string, string>();
        testDict.Add("$?", "0");
        testShell.addToMemoryOrUpdate("$f", "hi");
        testDict.Add("$f", "bye");
        testShell.addToMemoryOrUpdate("$f", "bye");
        Assert.AreEqual(testDict, testShell.getMemory());
    }

    [Test]
    public void wcCommandTest1()
    {
        Shell testShell = new Shell();
        File.WriteAllText(Directory.GetCurrentDirectory() + "/wcCommandTest.txt", "first   second   third\ntwo");
        int[] res = new[] {2, 4, File.ReadAllBytes(Directory.GetCurrentDirectory() + "/wcCommandTest.txt").Length};
        Assert.AreEqual(res, testShell.wcCommand(Directory.GetCurrentDirectory() + "/wcCommandTest.txt"));
    }

    [Test]
    public void wcCommandTest2()
    {
        Shell testShell = new Shell();
        int[] res = new[] {-1, -1, -1};
        Assert.AreEqual(res, testShell.wcCommand("ThisFileIsNotExist"));
    }

    [Test]
    public void pwdCommandTest()
    {
        Shell testShell = new Shell();
        StringAssert.IsMatch(Directory.GetCurrentDirectory(), testShell.pwdCommand());
    }

    [Test]
    public void catCommandTest()
    {
        File.WriteAllText(Directory.GetCurrentDirectory() + "/catCommandTest.txt", "This is cat test!");
        Shell testShell = new Shell();
        Assert.AreEqual(testShell.catCommand(Directory.GetCurrentDirectory() + "/catCommandTest.txt"),
            "This is cat test!");
    }

    [Test]
    public void InputToArgsTest1()
    {
        StringAssert.IsMatch(string.Empty, new Shell().InputToArgs("ThisFileIsNotExist"));
    }

    [Test]
    public void InputToArgsTest2()
    {
        File.WriteAllText(Directory.GetCurrentDirectory() + "/InputToArgsTest2.txt", "1 2 3");
        StringAssert.IsMatch("1 2 3",
            new Shell().InputToArgs(Directory.GetCurrentDirectory() + "/InputToArgsTest2.txt"));
    }

    [Test]
    public void echoCommandTest1()
    {
        Shell testShell = new Shell();
        string[] args = new[] {"My", "name", "is", "Akabyndash"};
        StringAssert.IsMatch("My name is Akabyndash", testShell.echoCommand(args));
    }

    [Test]
    public void echoCommandTest2()
    {
        Shell testShell = new Shell();
        string[] args = new string[] { };
        StringAssert.IsMatch(string.Empty, testShell.echoCommand(args));
    }

    [Test]
    public void executeFileTest()
    {
        Shell testShell = new Shell();
        File.WriteAllText(Directory.GetCurrentDirectory() + "/executeFileTest.txt", string.Empty);
        Assert.AreEqual("0",
            testShell.executeFile(Directory.GetCurrentDirectory() + "/executeFileTest.txt", new string[] { }, false));
    }

    [Test]
    public void argsParserTest()
    {
        Shell testShell = new Shell();
        string[] args = new[] {"$f", "jija", "$?", "fork"};
        string[] trueArgs = new[] {"hi", "jija", "0", "fork"};
        testShell.addToMemoryOrUpdate("$f", "hi");
        Assert.AreEqual(testShell.argsParser(args), trueArgs);
    }

    [Test]
    public void pwdProcessTest()
    {
        Shell testShell = new Shell();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        testShell.ProcessCommand("pwd", false);
        var output = stringWriter.ToString();
        Assert.AreEqual(output, Directory.GetCurrentDirectory() + "\n");
    }

    [Test]
    public void trueProcessTest()
    {
        Shell testShell = new Shell();
        Assert.AreEqual(testShell.ProcessCommand("true", false), 0);
    }

    [Test]
    public void falseProcessTest()
    {
        Shell testShell = new Shell();
        Assert.AreEqual(testShell.ProcessCommand("false", false), -1);
    }

    [Test]
    public void moneyProcessTest1()
    {
        Shell testShell = new Shell();
        testShell.ProcessCommand("$f=hi", false);
        Dictionary<string, string> testDict = new Dictionary<string, string>();
        testDict.Add("$?", "0");
        testDict.Add("$f", "hi");
        Assert.AreEqual(testDict, testShell.getMemory());
    }

    [Test]
    public void moneyProcessTest2()
    {
        Shell testShell = new Shell();
        testShell.ProcessCommand("$f=", false);
        Dictionary<string, string> testDict = new Dictionary<string, string>();
        testDict.Add("$?", "0");
        Assert.AreEqual(testDict, testShell.getMemory());
    }

    [Test]
    public void wcProcessTest1()
    {
        Shell testShell = new Shell();
        Assert.AreEqual(-1, testShell.ProcessCommand("wc ThisFileIsNotExist", false));
    }

    [Test]
    public void wcProcessTest2()
    {
        Shell testShell = new Shell();
        File.WriteAllText(Directory.GetCurrentDirectory() + "/wcProcessTest2.txt", string.Empty);
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        testShell.ProcessCommand("wc " + Directory.GetCurrentDirectory() + "/wcProcessTest2.txt", false);
        var output = stringWriter.ToString();
        Assert.AreEqual(output, "0\n0\n0\n");
    }

    [Test]
    public void catProcessTest1()
    {
        Shell testShell = new Shell();
        Assert.AreEqual(-1, testShell.ProcessCommand("cat ThisFileIsNotExist", false));
    }

    [Test]
    public void catProcessTest2()
    {
        Shell testShell = new Shell();
        File.WriteAllText(Directory.GetCurrentDirectory() + "/catProcessTest2.txt", string.Empty);
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        testShell.ProcessCommand("cat " + Directory.GetCurrentDirectory() + "/catProcessTest2.txt", false);
        var output = stringWriter.ToString();
        Assert.AreEqual("\n", output);
    }
    
    [Test]
    public void echoProcessTest()
    {
        Shell testShell = new Shell();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        testShell.ProcessCommand("echo Hi!", false);
        var output = stringWriter.ToString();
        Assert.AreEqual(output, "Hi!\n"); }

    [Test]
    public void endProcessTest()
    {
        Shell testShell = new Shell();
        testShell.ProcessCommand("end", false);
        Assert.AreEqual(testShell.getPolling(), false);
    }

    [Test]
    public void unknownProcessTest()
    {
        Shell testShell = new Shell();
        Assert.AreEqual(testShell.ProcessCommand("unknown command", false), -1);
    }

    [Test]
    public void shellScriptProcessTest()
    {
        Shell testShell = new Shell();
        File.WriteAllText(Directory.GetCurrentDirectory() + "/shellScriptProcessTest.txt", "Akabyndash\nfalse");
        Assert.AreEqual(testShell.ShellScriptProcess(Directory.GetCurrentDirectory() + "/shellScriptProcessTest.txt"),
            -1);
    }

    [Test]
    public void scriptCommandProcessTest()
    {
        Shell testShell = new Shell();
        File.WriteAllText(Directory.GetCurrentDirectory() + "/scriptCommandProcessTest.txt", "Akabyndash\nfalse");
        Assert.AreEqual(
            testShell.ProcessCommand(Directory.GetCurrentDirectory() + "/scriptCommandProcessTest.txt", false),
            -1);
    }

    [Test]
    public void executeProcessTest()
    {
        Shell testShell = new Shell();
        File.WriteAllText(Directory.GetCurrentDirectory() + "/executeProcessTest.txt", string.Empty);
        Assert.AreEqual(testShell.ProcessCommand(Directory.GetCurrentDirectory() + "/executeProcessTest.txt", false),
            0);
    }

    [Test]
    public void simpleLineTest()
    {
        Shell testShell = new Shell();
        testShell.ProcessLine(" false ");
        Assert.AreEqual(testShell.getLastCommandProgress(), -1);
    }


    [Test]
    public void semicolonTest()
    {
        Shell testShell = new Shell();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        testShell.ProcessLine("echo Hi! ; false");
        var output = stringWriter.ToString();
        Assert.AreEqual(output, "Hi!\n");
        Assert.AreEqual(testShell.getLastCommandProgress(), -1);
    }
    
    [Test]
    public void twoAndTest()
    {
        Shell testShell = new Shell();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        testShell.ProcessLine("true && echo Hi!");
        var output = stringWriter.ToString();
        Assert.AreEqual(output, "Hi!\n");
    }
    
    [Test]
    public void twoSticksTest()
    {
        Shell testShell = new Shell();
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        testShell.ProcessLine("false || echo Hi!");
        var output = stringWriter.ToString();
        Assert.AreEqual(output, "Hi!\n");
    }
    
    [Test]
    public void InputTest1()
    {
        Shell testShell = new Shell();
        testShell.ProcessLine("echo <");
        Assert.AreEqual(testShell.getLastCommandProgress(), -1);
    }
    
    [Test]
    public void InputTest2()
    {
        Shell testShell = new Shell();
        File.WriteAllText(Directory.GetCurrentDirectory() + "/InputTest2", "Hi!\n");
        var stringWriter = new StringWriter();
        Console.SetOut(stringWriter);
        testShell.ProcessLine("echo < " + Directory.GetCurrentDirectory() + "/InputTest2");
        var output = stringWriter.ToString();
        Assert.AreEqual("Hi!\n", output);
    }

    [Test]
    public void OutputTest1()
    {
        Shell testShell = new Shell();
        testShell.ProcessLine("echo Go! > " + Directory.GetCurrentDirectory() + "/OutputTest1");
        Assert.AreEqual("Go!\n", File.ReadAllText(Directory.GetCurrentDirectory() + "/OutputTest1"));
    }
    
    [Test]
    public void OutputTest2()
    {
        Shell testShell = new Shell();
        File.WriteAllText(Directory.GetCurrentDirectory() + "/OutputTest2", "First Line\n");
        testShell.ProcessLine("echo Go! >> " + Directory.GetCurrentDirectory() + "/OutputTest2");
        Assert.AreEqual("First Line\nGo!\n", File.ReadAllText(Directory.GetCurrentDirectory() + "/OutputTest2"));
    }
}