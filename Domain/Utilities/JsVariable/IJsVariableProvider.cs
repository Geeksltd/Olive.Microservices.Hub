using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Olive.Microservices.Hub.Domain.Utilities.JsVariable
{
	public interface IJsVariableProvider
	{
		Task<string> Render();
	}
}
