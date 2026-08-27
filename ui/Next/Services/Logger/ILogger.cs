using System.Collections.Generic;

namespace Next.Services;

public interface ILogger
{
    public void WriteLine(string value);
    // public IEnumerable<string> GetLines();
}