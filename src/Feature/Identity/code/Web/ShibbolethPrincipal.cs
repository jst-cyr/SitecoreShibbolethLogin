using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Sitecore.Configuration;

namespace Sitecore.Feature.Identity.Shibboleth.Web
{
	/// <summary>
	/// This sample ShibbolethPrincipal provides the basic wrapper around the header values
	/// </summary>
	public class ShibbolethPrincipal : GenericPrincipal
	{
		/// <summary>
		/// Key used to extract the user identifier from the header
		/// </summary>
		public string HeaderUserIdKey {
			get { return Settings.GetSetting("Sitecore.Feature.Identity.Shibboleth.HeaderIdentityKey", "uid"); }
		}

		/// <summary>
		/// Key used to extract the user email from the header
		/// </summary>
		public string HeaderEmailKey {
			get { return Settings.GetSetting("Sitecore.Feature.Identity.Shibboleth.HeaderEmailKey", "email"); }
		}

		/// <summary>
		/// Key used to extract the first name from the header
		/// </summary>
		public string HeaderFirstNameKey {
			get { return Settings.GetSetting("Sitecore.Feature.Identity.Shibboleth.HeaderFirstNameKey", "givenName"); }
		}

		/// <summary>
		/// Key used to extract the last name from the header
		/// </summary>
		public string HeaderLastNameKey {
			get { return Settings.GetSetting("Sitecore.Feature.Identity.Shibboleth.HeaderLastNameKey", "sn"); }
		}

		public ShibbolethPrincipal() : this(new GenericIdentity(GetUserIdentityFromHeaders()))
		{

		}

		/// <summary>
		/// Default constructor that creates the new identity and sets up claim information
		/// </summary>
		public ShibbolethPrincipal(IIdentity identity) : base(identity, GetRolesFromHeader())
		{
			this.InitializeClaims();
		}

		/// <summary>
		/// Utility method to retrieve the username from the headers
		/// </summary>
		/// <returns></returns>
		public static string GetUserIdentityFromHeaders()
		{
			var identityHeaderKey = Settings.GetSetting("Sitecore.Feature.Identity.Shibboleth.HeaderIdentityKey", "uid");
			return HttpContext.Current.Request.Headers[identityHeaderKey];
		}

		/// <summary>
		/// Get the user's roles from the header
		/// </summary>
		/// <returns></returns>
		public static string[] GetRolesFromHeader()
		{
			string[] roles = null;
			string rolesheader = HttpContext.Current.Request.Headers["affiliation"];
			if (rolesheader != null)
			{
				roles = rolesheader.Split(';');
			}
			return roles;
		}

		/// <summary>
		/// Initialize claims identity information based on header data
		/// </summary>
		public void InitializeClaims()
		{
			//Extract claims data
			var userId = HttpContext.Current.Request.Headers[HeaderUserIdKey];
			var email = HttpContext.Current.Request.Headers[HeaderEmailKey];
			var firstName = HttpContext.Current.Request.Headers[HeaderFirstNameKey];
			var lastName = HttpContext.Current.Request.Headers[HeaderLastNameKey];

			//Log shibboleth data that is being initialized for debugging purposes
			var shibbolethData = String.Format("Shibboleth data: {0}:{1}|{2}:{3}|{4}:{5}|{6}:{7}",
				HeaderUserIdKey, userId,
				HeaderEmailKey, email,
				HeaderFirstNameKey, firstName,
				HeaderLastNameKey, lastName);
			Sitecore.Diagnostics.Log.Debug(shibbolethData, this);

			var claimsIdentity = this.Identity as ClaimsIdentity;
			if (claimsIdentity == null)
				return;

			//Ensure email claim
			if (!String.IsNullOrEmpty(email)) { 
				var emailClaim = claimsIdentity.FindFirst(ClaimTypes.Email);
				if (emailClaim != null)
				{
					claimsIdentity.RemoveClaim(emailClaim);
				}
				claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));
			}

			//Ensure first name
			if (!String.IsNullOrEmpty(firstName)) { 
				var firstNameClaim = claimsIdentity.FindFirst(ClaimTypes.GivenName);
				if (firstNameClaim != null)
				{
					claimsIdentity.RemoveClaim(firstNameClaim);
				}
				claimsIdentity.AddClaim(new Claim(ClaimTypes.GivenName, firstName));
			}

			//Ensure last name
			if (!String.IsNullOrEmpty(lastName)) { 
				var lastNameClaim = claimsIdentity.FindFirst(ClaimTypes.Surname);
				if (lastNameClaim != null)
				{
					claimsIdentity.RemoveClaim(lastNameClaim);
				}
				claimsIdentity.AddClaim(new Claim(ClaimTypes.Surname, lastName));
			}
		}
	}
}