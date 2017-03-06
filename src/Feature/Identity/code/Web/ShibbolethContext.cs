using System;
using System.Web;
using Sitecore.Configuration;

namespace Sitecore.Feature.Identity.Shibboleth.Web
{
	/// <summary>
	/// This class wraps around the header information from Shibboleth to provide utilities to the rest of the application.
	/// </summary>
	public class ShibbolethContext
	{
		/// <summary>
		/// The header key used to identify that Shibboleth identity information is available.
		/// TODO: Move to configuration
		/// </summary>
		protected virtual string ShibbolethHeaderKey
		{
			get {
				return Settings.GetSetting("Sitecore.Feature.Identity.Shibboleth.HeaderKey", "ShibIdentityProvider");
			}
		}

		/// <summary>
		/// Determines if Shibboleth headers are available in the request
		/// </summary>
		/// <returns></returns>
		public bool IsAvailable()
		{
			var request = HttpContext.Current.Request;
			return request != null && !String.IsNullOrEmpty(request.Headers[this.ShibbolethHeaderKey]);
		}

		/// <summary>
		/// Compares Shibboleth principal against the current user in context
		/// </summary>
		/// <param name="contextUser">The current Sitecore authenticated user</param>
		/// <returns></returns>
		public bool Matches(Sitecore.Security.Accounts.User contextUser)
		{
			//If no Shibboleth context, no way to validate they match
			if (!IsAvailable())
				return false;

			var principal = this.GetPrincipal();
			return principal.Identity.Name.ToLower().Equals(contextUser.LocalName);
		}

		/// <summary>
		/// Returns a claims principal implementation which wraps around the Shibboleth headers
		/// </summary>
		/// <returns></returns>
		public ShibbolethPrincipal GetPrincipal()
		{
			return new ShibbolethPrincipal();
		}

	}
}