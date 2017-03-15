# Sitecore Shibboleth Login
This module supports logging users into a Sitecore site that is operating as a Shibboleth Service Provider. The virtual user that is authenticated will be initialized with attributes from the Shibboleth IdP to provide username, email address, and a full name based on the user's first name and last name.

The module patches in a processor which runs after the Sitecore UserResolver and looks for Shibboleth headers to indicate that an authentication should occur. If headers are found, a virtual user is logged in.

The following configuration settings are available:

- **Sitecore.Feature.Identity.Shibboleth.HeaderKey:** This should be set to a Request Header sent to the SP which can be used to identify that a Shibboleth session has been engaged.Default value is 'ShibIdentityProvider'
- **Sitecore.Feature.Identity.Shibboleth.HeaderIdentityKey:** This should be set to a Request Header defined in the SP's attribute-map.xml for the user's uid/username attribute. Default value is 'uid'.
- **Sitecore.Feature.Identity.Shibboleth.HeaderEmailKey:** This should be set to a Request Header defined in the SP's attribute-map.xml for the user's email address attribute. Default value is 'email'.
- **Sitecore.Feature.Identity.Shibboleth.HeaderFirstNameKey:** This should be set to a Request Header defined in the SP's attribute-map.xml for the user's first name/given name attribute. Default value is 'givenName'.
- **Sitecore.Feature.Identity.Shibboleth.HeaderLastNameKey:** This should be set to a Request Header defined in the SP's attribute-map.xml for the user's last name/surname attribute. Default value is 'sn'.
- **IgnoreUrlPrefixes:** This adds "/Shibboleth.sso" to the default Sitecore IgnoreUrlPrefixes. If you have a custom patch for this value, you should make sure that your end-result is that Shibboleth.sso is added to your ignored path.

## Configuring a Shibboleth Service Provider for Sitecore
1. **Install Shibboleth.**
   See the official instructions here: https://wiki.shibboleth.net/confluence/display/SHIB2/NativeSPWindowsIIS7Installer
   
2. **Validate IIS installation.**
   1. **Verify ISAPI filter.** Add the filter using the IIS Manager console. At either the top-level or individual Site level, select the "ISAPI Filters" feature; then, add a new filter called Shibboleth and specify the library.
      - Executable path should be: C:\opt\shibboleth-sp\lib64\shibboleth\isapi_shib.dll 
   2. **Verify Handler Mapping.** Under Handler Mappings, check for the .sso extension 
      - Executable path should be: C:\opt\shibboleth-sp\lib64\shibboleth\isapi_shib.dll 
      - Access level: Script 
      - Verbs: All verbs 
   3. **Verify ISAP restrictions.** Under "ISAPI and CGI Restrictions" at the top level. Add the Shibboleth ISAPI Extension to the list of permitted extensions in the list of allowed extensions.  
      - Executable path: C:\opt\shibboleth-sp\lib64\shibboleth\isapi_shib.dll 
      - Description: Shibboleth Web Service Extension 
      - Allowed to execute should be true 
   4. **Restart IIS.** 
      - If any manual changes were needed, restart IIS so it picks them up. 
  
3. **Install your IdP metadata**
   1. Visit your IdP metadata URL and save the content as an XML file on your disk. (e.g. idp-metadata.xml)
   2. Copy the IdP metadata file to your SP installation folder. I prefer to create a metadata subfolder in case I have multiple IdPs I'm switching between. e.g C:\opt\shibboleth-sp\etc\shibboleth\metadata\idp-metadata.xml
  
4. **Configure shibboleth2.xml** (C:\opt\shibboleth-sp\etc\shibboleth\shibboleth2.xml)
   1. ISAPI Site Definition
      - The 'id' attribute is the IIS unique ID for the site. In IIS, click on Advanced Settings and the dialog will show the ID at the top.
      - The 'name' attribute is the name that will be referenced by the Request Map (use your hostname e.g. sitecore-sp.example.org) 
      - Example: `<Site id="2" name="sitecore-sp.example.org"/>`
   2. Request Map
      - You need to define the site that will be protected by Shibboleth and also which areas will force a login. To lock down for the entire site, Shibboleth needs to intercept all the requests, but we want Sitecore to work and let users login with 'sitecore' accounts, so we need exceptions to allow Sitecore to function. Below is an example of a host set to restrict the entire site, but allow exceptions for subfolders. You might have your own subfolders you want to add to the list (maybe an assets folder that has your CSS?)
 
```xml
   <Host name="sitecore-sp.example.org" authType="shibboleth" requireSession="true"> 
    <!--Exclude the Sitecore default folders so that Sitecore handles those requests--> 
    <Path name="App_Browsers" authType="shibboleth" requireSession="false"/> 
    <Path name="App_Config" authType="shibboleth" requireSession="false"/> 
    <Path name="App_Data" authType="shibboleth" requireSession="false"/> 
    <Path name="Areas" authType="shibboleth" requireSession="false"/> 
    <Path name="layouts" authType="shibboleth" requireSession="false"/> 
    <Path name="sitecore" authType="shibboleth" requireSession="false"/> 
    <Path name="sitecore modules" authType="shibboleth" requireSession="false"/> 
    <Path name="sitecore_files" authType="shibboleth" requireSession="false"/> 
    <Path name="temp" authType="shibboleth" requireSession="false"/> 
    <Path name="upload" authType="shibboleth" requireSession="false"/> 
    <Path name="Views" authType="shibboleth" requireSession="false"/> 
    <Path name="xsl" authType="shibboleth" requireSession="false"/> 
   </Host>
```
   3. ApplicationDefaults entityID
      - Specify the entityID attribute with your hostname to make things easy (e.g. https://sitecore-sp.example.org). This is used to refer to your application between the IdP and the SP.
   4. ApplicationDefaults SSO entityID for IdP
      - The SSO tag inside ApplicationDefaults\Sessions should have the entity ID of IdP you are sending users to for login. (e.g. http://myloginsite.example.org). You can find this entity ID in the metadata file you downloaded earlier.
   5. Configure ACL for 'Status' (for local development)
      - A Local SP doesn't have this issue since you are running your domains on 127.0.0.1 (the default ACL). However, to load the Status page from a server using a domain name we need to put the resolving IP address of the server as a valid ACL. 
      - As an example, let us assume that the IP of our server is 192.168.13.37:
     `<Handler type="Status" Location="/Status" acl="192.168.13.37 127.0.0.1 ::1"/>`
   6. Support contact
      - This isn't required, but you probably want to put your support team email address on the Errors tag. Update the 'supportContact' attribute of the Errors tag with your email address.
   7. IdP metadataprovider
      - Copy the MetaDataProvider example 
      - Comment out the 'MetadataFilter' elements (unless you need them in your scenario)
      - Edit the MetaDataProvider Uri attribute to match to your IdP (e.g https://myloginsite.example.org)
      - Edit the MetadataProvider BackingFilePath attribute. The file system path where you are storing a copy of the IDPs meta data (e.g. C:\opt\shibboleth-sp\etc\shibboleth\metadata\idp-metadata.xml). 
	
 5. **Configure attribute-map.xml** (C:\opt\shibboleth-sp\etc\shibboleth\attribute-map.xml)
    - The SP needs to be configured to accept the attributes passed to it by the IdP so that the headers show up in the Sitecore request. You can leave the defaults, but I recommend putting your customizations at the top.
    - Example:
   ```xml
   	<!-- Custom User Attributes -->
	<Attribute name="urn:oid:0.9.2342.19200300.100.1.1" id="uid"/>
	<Attribute name="urn:mace:dir:attribute-def:uid" id="uid"/>
	<Attribute name="urn:oid:2.5.4.18" id="sid"/>
	<Attribute name="urn:mace:dir:attribute-def:sid" id="sid"/>
	<Attribute name="email" id="email" />
	<Attribute name="urn:mace:dir:attribute-def:email" id="email"/>
	<Attribute name="urn:oid:2.5.4.42" id="givenName"/>
	<Attribute name="urn:mace:dir:attribute-def:givenName" id="givenName"/>
	<Attribute name="urn:oid:2.5.4.4" id="sn"/>
	<Attribute name="urn:mace:dir:attribute-def:sn" id="sn"/>
   ```
6. **Extract the SP metadata**
   - This will be given to the IdP owner and installed so it knows about your SP. 
   - Download the file from your SP at /Shibboleth.sso/Metadata. Example: https://sitecore-sp.example.org/Shibboleth.sso/Metadata 
   - Rename the file to something meaningful like sitecore-sp-metadata.xml 
   - Send the metadata file to the owner of the IdP server so they can add it to their IdP configuration
