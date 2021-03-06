[![Build Status](https://drone.io/github.com/sergeyt/Owin.Routing/status.png)](https://drone.io/github.com/sergeyt/Owin.Routing/latest)
[![Build status](https://ci.appveyor.com/api/projects/status/157su7epxuv23rxj)](https://ci.appveyor.com/project/sergeyt/owin-routing)
[![NuGet Version](http://img.shields.io/nuget/v/Owin.Routing.svg?style=flat)](https://www.nuget.org/packages/Owin.Routing/)
[![NuGet Downloads](http://img.shields.io/nuget/dt/Owin.Routing.svg?style=flat)](https://www.nuget.org/packages/Owin.Routing/)

# Owin.Routing

.NET library with simple routing API inspired by [express.js](http://expressjs.com/)
for [Katana](https://katanaproject.codeplex.com/) applications.

Owin.Routing is now based on ASP.NET System.Web.Routing.

## API

### IAppBuilder Extensions

* `RouteBuilder Route(this IAppBuilder app, string url)` - injects route to app pipeline.

### RouteBuilder class

This class providers fluent API to handlers for specific HTTP verbs (GET, POST, etc).

RouteBuilder methods:

`using HandlerFunc = Func<IOwinContext, Task>`

* `RouteBuilder Get(HandlerFunc handler)` - set GET handler.
* `RouteBuilder Post(HandlerFunc handler)` - set POST handler.
* `RouteBuilder Put(HandlerFunc handler)` - set PUT handler.
* `RouteBuilder Patch(HandlerFunc handler)` - set PATCH handler.
* `RouteBuilder Delete(HandlerFunc handler)` - set DELETE handler.

## Sample code

Below is block of test code.

```c#
[TestCase("/docs/reports", Result = "reports")]
[TestCase("/docs/reports/1", Result = "reports[1]")]
public async Task<string> Test(string path)
{
	using (var server = TestServer.Create(app =>
	{
		app.Route("docs/{collection}")
			.Get(async ctx =>
			{
				var name = ctx.GetRouteValue<string>("collection");
				await ctx.Response.WriteAsync(name);
			});

		app.Route("docs/{collection}/{id}")
			.Get(async ctx =>
			{
				var col = ctx.GetRouteValue<string>("collection");
				var id = ctx.GetRouteValue<string>("id");
				await ctx.Response.WriteAsync(string.Format("{0}[{1}]", col, id));
			});
	}))
	{
		var response = await server.HttpClient.GetAsync(path);
		var s = await response.Content.ReadAsStringAsync();
		return s;
	}
}
```
