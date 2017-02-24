using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

using System.IO;

public enum IOOutcome
{
    None,
    Success,
    Failure,
}

public class ThreadedReadBytes : ThreadedJob
{
    public string path;
    public byte[] data;
    public IOOutcome outcome;

    public System.Exception exception;

    protected override void ThreadFunction()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            data = File.ReadAllBytes(path);
            outcome = IOOutcome.Success;
        }
        catch (System.Exception e)
        {
            exception = e;

            data = null;
            outcome = IOOutcome.Failure;
        }
    }
}

public class ThreadedReadString : ThreadedJob
{
    public string path;
    public string data;
    public IOOutcome outcome;

    public System.Exception exception;

    protected override void ThreadFunction()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            data = File.ReadAllText(path);
            outcome = IOOutcome.Success;
        }
        catch (System.Exception e)
        {
            exception = e;

            data = null;
            outcome = IOOutcome.Failure;
        }
    }
}

public class ThreadedWriteBytes : ThreadedJob
{
    public string path;
    public byte[] data;
    public IOOutcome outcome;

    public System.Exception exception;

    protected override void ThreadFunction()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            File.WriteAllBytes(path, data);

            outcome = IOOutcome.Success;
        }
        catch (System.Exception e)
        {
            exception = e;

            outcome = IOOutcome.Failure;
        }
    }
}

public class ThreadedWriteString : ThreadedJob
{
    public string path;
    public string data;
    public bool append;
    public IOOutcome outcome;

    public System.Exception exception;

    protected override void ThreadFunction()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));

            if (append)
            {
                File.AppendAllText(path, data);
            }
            else
            {
                File.WriteAllText(path, data);
            }

            outcome = IOOutcome.Success;
        }
        catch (System.Exception e)
        {
            exception = e;

            data = null;
            outcome = IOOutcome.Failure;
        }
    }
}

public class ThreadedDelete : ThreadedJob
{
    public string path;
    public IOOutcome outcome;

    public System.Exception exception;

    protected override void ThreadFunction()
    {
        try
        {
            bool directory = (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;

            if (directory)
            {
                Directory.Delete(path, true);
            }
            else
            {
                File.Delete(path);
            }

            outcome = IOOutcome.Success;
        }
        catch (System.Exception e)
        {
            exception = e;

            outcome = IOOutcome.Failure;
        }
    }
}
