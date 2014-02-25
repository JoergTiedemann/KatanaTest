using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace Demo.Bugs.Web.Hubs
{
    [HubName("bugs")]
    public class BugHub : Hub
    {
    }
}