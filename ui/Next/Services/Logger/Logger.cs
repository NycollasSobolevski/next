using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Next.Services;


public class Logger : ILogger
{
    public List<string> Logs {get; private set;} = [];

    public event Action<string>? ExecuteOnLog;

    public void WriteLine(string value)
    {
        Logs.Add(value);
        ExecuteOnLog?.Invoke(value);
    }
}