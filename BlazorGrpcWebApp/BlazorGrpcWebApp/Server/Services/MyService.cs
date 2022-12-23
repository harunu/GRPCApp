using BlazorGrpcWebApp.Shared;
using BlazorGrpcWebApp.Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorGrpcWebApp.Server.Services;

public class MyService : IMyService
{
	public Task<MyServiceResult> DoSomething(MyServiceRequest request)
	{
		return Task.FromResult(new MyServiceResult()
		{
			NewText = request.Text + " from server",
			NewValue = request.Value + 1
		});
	}
}

