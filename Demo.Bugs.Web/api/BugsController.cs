using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Demo.Bugs.Web.Hubs;
using Demo.Bugs.Web.Model;
using Microsoft.AspNet.SignalR;

namespace Demo.Bugs.Web.api
{
    public class BugsController : ApiController
    {
        IBugsRepository _bugsRepository = new BugsRepository();
        private IHubContext _hub;

        public BugsController()
        {
            _hub = GlobalHost.ConnectionManager.GetHubContext<BugHub>();
        }

        public IEnumerable<Bug> Get()
        {
            return _bugsRepository.GetBugs();
        }

        // Web API expects primitives coming from the request body to have no key value (e.g. '') - they should be encoded, then as '=value'

        [Route("api/bugs/backlog")]
        public Bug PostToBacklog([FromBody] int id)
        {
            var bug = _bugsRepository.GetBugs().First(b=>b.id==id);
            bug.state = "backlog";

            _hub.Clients.All.moved(bug);

            return bug;
        }

        [Route("api/bugs/working")]
        public Bug PostToWorking([FromBody] int id)
        {
            var bug = _bugsRepository.GetBugs().First(b => b.id == id);
            bug.state = "working";

            _hub.Clients.All.moved(bug);

            return bug;
        }

        [Route("api/bugs/done")]
        public Bug PostToDone([FromBody] int id)
        {
            var bug = _bugsRepository.GetBugs().First(b => b.id == id);
            bug.state = "done";

            _hub.Clients.All.moved(bug);

            return bug;
        }
    }
}