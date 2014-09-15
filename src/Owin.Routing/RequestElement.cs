namespace Owin.Routing
{
	/// <summary>
	/// Defines elements of HTTP request.
	/// </summary>
	public enum RequestElement
	{
		/// <summary>
		/// Specifies that value should be taken from route data.
		/// </summary>
		Route,

		/// <summary>
		/// Specifies that value should be taken from query string.
		/// </summary>
		Query,

		/// <summary>
		/// Specifies that value should be taken from HTTP request headers.
		/// </summary>
		Header,

		/// <summary>
		/// Specifies that value should be taken from HTTP request JSON body.
		/// </summary>
		Body
	}
}
