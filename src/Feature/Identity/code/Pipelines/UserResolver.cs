using System;
using Sitecore.Pipelines.HttpRequest;
using Sitecore.Feature.Identity.Shibboleth.Web;

namespace Sitecore.Feature.Identity.Shibboleth.Pipelines.HttpRequest
{
	/// <summary>
	/// Checks to see if the Shibboleth header data is available and logs a user in if necessary
	/// </summary>
	public class UserResolver : HttpRequestProcessor
	{
		public override void Process(HttpRequestArgs args)
		{
			var sitecoreUserLoggedIn = Context.IsLoggedIn;
			var shibbolethContext = new ShibbolethContext();

			//Scenario 1: User is not logged in but shibboleth headers are available. Log the user in.
			if (!sitecoreUserLoggedIn && shibbolethContext.IsAvailable())
			{
				var principal = shibbolethContext.GetPrincipal();
				var loginHelper = new LoginHelper();
				loginHelper.Login(principal);
			}
			//Scenario 2: User is logged in, but headers don't match current context user. Log the user in as the header user.
			if (sitecoreUserLoggedIn && shibbolethContext.IsAvailable() && !shibbolethContext.Matches(Context.User))
			{
				var principal = shibbolethContext.GetPrincipal();
				var loginHelper = new LoginHelper();
				loginHelper.Login(principal);
			}

		}
	}
}