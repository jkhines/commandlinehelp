# commandlinehelp
Command Line Helper

Imported from https://commandlinehelp.codeplex.com/
Original commit date: January 2011


Project Description
CommandLineHelp is a framework for simplifying the automated execution of command-line programs and saving their output.

Overview
CommandLineHelp was written to simplify the act of running multiple statements from the command line and gathering their output. Specifically, this is targeted at automating the execution of commands that need to be wrapped within a batch file. For example, a very simple example of a multi-line set of commands would be:

Example 1

SET HOME=%SystemRoot%
DIR %HOME%


The library supports much more complicated sets of commands, of course, using command line arguments. The Command Line Parser Library is used to parse and save command line arguments for use in your code.

Usage
The library implements the Strategy pattern to simplify the execution of the commands. For example, the Main() method can be written as:

Example 2

var helper = new CommandLineHelper();
helper.SetTool(new MyTool());
helper.Setup(args);
helper.DoWork();


Each set of commands is written as a class (MyTool in Example 2 above) which implements the ICommandLine interface:

Example 3

public interface ICommandLineTool
{
    string GetUsage();
    string GetCommands();
    string GetLogFilePath();
    bool GetAppendToLogFile();
}


Lastly, to create the set of commands in Example 1, you'd implement the following GetCommands() method:

Example 4

public class MyTool : ICommandLineTool
{
    . . .
    public string GetCommands()
    {
        return "SET HOME=%SystemRoot%\r\n" +
            "DIR %HOME%\r\n";
    }
}



Details
This project was written for the .NET Framework 4 Client Profile in Microsoft Visual Studio 2010.

Last edited Jan 18, 2011 at 4:22 PM by jkhines, version 4

