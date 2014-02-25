using System.Collections.Generic;

namespace Demo.Bugs.Web.Model
{
    public interface IBugsRepository
    {
        IEnumerable<Bug> GetBugs();
    }
}