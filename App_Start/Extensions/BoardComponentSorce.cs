using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Olive.Microservices.Hub.BoardComponent;

namespace Olive.Microservices.Hub
{
    public class BoardSource : BoardComponentsSource
    {
        public override async Task AddableItem(ClaimsPrincipal user, string id, string type)
        {
        }

        public override async Task Process(ClaimsPrincipal user, string id, string type)
        {
            Add("Logout", "/logout");

        }
    }
}
