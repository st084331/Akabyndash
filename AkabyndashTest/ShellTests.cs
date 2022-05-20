using System;
using System.Collections.Generic;
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
    public void addToMemoryTest()
    {
        
    }
    
}