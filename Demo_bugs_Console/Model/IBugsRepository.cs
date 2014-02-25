using System.Collections.Generic;

namespace Demo_bugs_Console.Model
{
    public interface IBugsRepository
    {
        IEnumerable<Bug> GetBugs();
    }
}