# Sitecore Shibboleth Login
Supports logging users into a Sitecore site that is operating as a Shibboleth Service Provider.

FULL README WILL BE COMING.

The following settings are available:

- **Sitecore.Feature.Identity.Shibboleth.HeaderKey:** This should be set to a Request Header sent to the SP which can be used to identify that a Shibboleth session has been engaged.Default value is 'ShibIdentityProvider'
- **IgnoreUrlPrefixes:** This adds "/Shibboleth.sso" to the default Sitecore IgnoreUrlPrefixes. If you have a custom patch for this value, you should make sure that your end-result is that Shibboleth.sso is added to your ignored path.
