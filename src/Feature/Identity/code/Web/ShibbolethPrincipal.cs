using System;
using System.Security.Principal;
using System.Web;

namespace Sitecore.Feature.Identity.Shibboleth.Web
{
	/// <summary>
	/// This sample ShibbolethPrincipal provides the basic wrapper around the header values
	/// </summary>
	public class ShibbolethPrincipal : GenericPrincipal
	{
		public string uid
		{
			get { return HttpContext.Current.Request.Headers["uid"]; }
		}

		public string eppn
		{
			get { return HttpContext.Current.Request.Headers["eppn"]; }
		}

		public string mail
		{
			get { return HttpContext.Current.Request.Headers["mail"]; }
		}

		public ShibbolethPrincipal() : base(new GenericIdentity(GetUserIdentityFromHeaders()), GetRolesFromHeader())
		{
		}

		public static string GetUserIdentityFromHeaders()
		{
			return HttpContext.Current.Request.Headers["uid"];
		}

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
	}
}