﻿#region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using Sitecore.Analytics;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Security.Accounts;
using Sitecore.Security.Authentication;
#endregion

namespace Sitecore.Feature.Identity.Shibboleth.Web
{
	/// <summary>
	/// This Login Helper is originally from the SitecoreFederatedLogin module by Bas Lijten:
	/// https://github.com/BasLijten/SitecoreFederatedLogin
	/// </summary>
	public class LoginHelper
	{
		/// <summary>
		/// Logins the specified user.
		/// </summary>
		/// <param name="user">The user.</param>
		public void Login(IPrincipal principal)
		{

			var identity = principal.Identity as ClaimsIdentity;
			var allowLoginToShell = false;
#if DEBUG
			WriteClaimsInfo(identity);
#endif
			if (!identity.IsAuthenticated)
				return;
			var userName = string.Format("{0}\\{1}", Context.Domain.Name, identity.Name);
			try
			{
				var virtualUser = AuthenticationManager.BuildVirtualUser(userName, true);

				//Add roles
				var roles = Context.Domain.GetRoles();
				if (roles != null)
				{
					var groups = GetGroups(identity);
					foreach (var role in from role in roles
										 let roleName = GetRoleName(role.Name)
										 where groups.Contains(roleName.ToLower()) && !virtualUser.Roles.Contains(role)
										 select role)
					{
						virtualUser.Roles.Add(role);
					}
					foreach (
						var role2 in
							virtualUser.Roles.SelectMany(
								role1 =>
									RolesInRolesManager.GetRolesForRole(role1, true)
										.Where(role2 => !virtualUser.Roles.Contains(role2))))
					{
						virtualUser.Roles.Add(role2);
					}

					// Setting the user to be an admin.
					//TODO = case sensitive
					virtualUser.RuntimeSettings.IsAdministrator =
						groups.Contains(Settings.GetSetting("Sitecore.Feature.Identity.Shibboleth.AdminUserRole", "Sitecore Local Administrators"), StringComparer.OrdinalIgnoreCase);

					if (virtualUser.RuntimeSettings.IsAdministrator)
						allowLoginToShell = true;
				}

				//Extract user details from claims
				var email = GetClaimValue(identity, ClaimTypes.Email);
				var firstName = GetClaimValue(identity, ClaimTypes.GivenName);
				var lastName = GetClaimValue(identity, ClaimTypes.Surname);

				//Update virtual user
				virtualUser.Profile.Email = email;
				virtualUser.Profile.FullName = String.Format("{0} {1}", firstName, lastName);

				AuthenticationManager.Login(virtualUser);
				var tracker = Tracker.Current;
				if (tracker != null)
					tracker.Session.Identify(virtualUser.Identity.Name);
			}
			catch (ArgumentException ex)
			{
				Log.Error("Shibboleth::Login Failed!", ex, this);
			}
		}

		/// <summary>
		/// Gets the group names.
		/// </summary>
		/// <param name="claimsIdentity">The claims identity.</param>
		/// <returns></returns>
		protected virtual IEnumerable<string> GetGroups(ClaimsIdentity claimsIdentity)
		{
			var enumerable =
				claimsIdentity.Claims.Where(
					c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role").ToList();
			var list = new List<string>();
			foreach (
				var str in
					enumerable.Select(claim => claim.Value.ToLower().Replace('-', '_'))
						.Where(str => !list.Contains(str)))
			{
				list.Add(str);
			}
			return list.ToArray();
		}

		/// <summary>
		/// Gets the name of the role.
		/// </summary>
		/// <param name="roleName">Name of the role.</param>
		/// <returns></returns>
		private static string GetRoleName(string roleName)
		{
			if (!roleName.Contains('\\'))
				return roleName;
			return roleName.Split(new[]
			{
				'\\'
			})[1];
		}

		/// <summary>
		/// Extract the claim value from the identity
		/// </summary>
		/// <param name="claimsIdentity">The current identity</param>
		/// <param name="type">The type to extract</param>
		/// <returns>The user's value for that claim</returns>
		protected virtual string GetClaimValue(ClaimsIdentity claimsIdentity, string type)
		{
			var claim = claimsIdentity.FindFirst(type);
			return (claim != null) ? claim.Value : null;
		}
		
		/// <summary>
		/// Writes the claims information.
		/// </summary>
		/// <param name="claimsIdentity">The claims identity.</param>
		private void WriteClaimsInfo(ClaimsIdentity claimsIdentity)
		{
			Log.Debug("Writing Claims Info", this);
			foreach (var claim in claimsIdentity.Claims)
				Log.Debug(string.Format("Claim : {0} , {1}", claim.Type, claim.Value), this);
		}

		/// <summary>
		/// Adds the claims information.
		/// </summary>
		/// <param name="user">The user.</param>
		/// <param name="claimsIdentity">The claims identity.</param>
		public void AddClaimsInfo(User user, ClaimsIdentity claimsIdentity)
		{
		}
	}
}