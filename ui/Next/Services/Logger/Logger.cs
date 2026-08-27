using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Next.Services;


public class Logger : ILogger
{
    public List<string> Logs {get; private set;} = [];

    // public void CreateFile()
    // {
    //     if (!File.Exists(Path))
    //         File.Create(Path);
    //     WriteLine("Log initialized");
    // }

    public void WriteLine(string value)
        => Logs.Add(value);

    // public IEnumerable<string> GetLines()
    //     => File.ReadLines(Path);
}