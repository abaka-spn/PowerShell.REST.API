# PowerShell.REST.API

Turn any PowerShell script into a HTTP REST API!

### Builds

* 1.2.1804.1801


## The goal of this project

I happy to publish my first project !

This project is inspired of DynamicPowerShellApi projet but I want to add the following features:
* Take in charge Custom PowerShell module: `Done`.
* Expose a OpenAPI specification from Script or Module: `Done`.
* Define Rest Method for each WebMethod in configuration file: `Done`.
* Define Rest Location for each parameters in configuration file: `Done`.
* Each parameters definition will be fetched from scripts or commands: `Done`.
* Add Swagger UI for graphical help: `Done`.
* Add ApiKey authentication: `Done`.
* Transfer user identity to the Powershell command : `Done`.
* Take in charge Custum Powershell class as parameter: `Done`
* Take in charge versionng of API : `In future`


## Overview
*It is near same as orignal project but the behavior is not same*

HTTP service written in C#.NET using the Microsoft OWIN libraries.

The concept is to take an existing PowerShell **script** or **command** in PowerShell module, with parameters and expose it as a HTTP/REST method.

The web service configures the web methods as boot based on the configuration of the script repository and generate the OpenAPI specfication file.

It hosts a PowerShell runspace pool to load, run and check PowerShell scripts, capturing errors and stack traces to the logs
and parsing complex PowerShell response objects back into JSON.

It also supports async jobs to be run as separate threads, with the job results to be stored on disk.

A OpenAPI specification file is available at **/api/server/spec**.

A swagger UI is available at **/help**

## How it works
*This project has been restructured but it is near same as orignal*

This project implements a OWIN WebAPI HTTP service with a single "generic" API controller. The API controller consults the configuration collection of the endpoint to identify which PowerShell script needs to be run for each HTTP request.
It hosts a PowerShell session to import and run the script, whilst monitoring the script process for faults. It is piped through [ConvertTo-Json] and the response to a temporary JObject and then returns the response data (it is a difference with original project).

The PowerShell scripts can be called by GET, POST, DELETE or PUT method. It mus be defined in the configuration file

The parameters can be passed by PATH, QUERYSTRING, BODY, HEADER or VALUE defined in the configuration file (Default: Body).

When the application start, all PowerShell commands are analyze by reflexion and a OpenAPI specification file are generated.

## Running
### run on the command line
```cmd
PowerShellRestApi.Host.exe --console
```
### run as a service
```cmd
PowerShellRestApi.Host.exe --service
```
### install the service
```cmd
PowerShellRestApi.Host.exe --install-service --service-user "UserABC" --service-password "Password123"
```
You need to authorize the 

## Configuration

### The main service configuration file
The file **PowerShellRestApi.Host.exe.config** is the main configuration file. It contains the setup for security, logging and the methods themselves.

```xml
<WebApiConfiguration HostAddress="http://localhost:9000"
                     Version="1.0.0.0"
                     Title="Sample PowerShell Command Rest API">
		<Jobs JobStorePath="c:\temp\" />
		<Authentication Enabled="false" StoreName="My" StoreLocation="LocalMachine" Thumbprint="E6B6364C75ED8B6495A42D543AC728B4C2263082" Audience="http://aperture.identity/connectors" />
	    <ApiKeyAuthentication Enabled="true" Header="X-API-KEY" HeaderKey="" />
		<Users>
		   <User Name="SeB" ApiKey="123" IpAddresses="10.1.53.10,10.1.52.10" Roles="admins" />
		   <User Name="SeB2" ApiKey="1234" IpAddresses="" Roles="admins,OSInstallRW" />
		</Users>

		<Apis>
		<!-- Using Powershell module -->
		    <WebApi Name="Demo" Module="DemoAnimal.psm1">
		      <WebMethods>
				<!-- Published as PUT on http://localhost:9000/Demo/Animal --> 
		        <WebMethod Name="Put-Animal" Command="Add-Animal" />
				<!-- Published as POST on http://localhost:9000/Demo/Animal --> 
		        <WebMethod Name="Post-Animal" Command="New-Animal"  />
   				<!-- Published as GET on http://localhost:9000/Demo/Animal --> 
		        <WebMethod Name="Get-Animal" Command="Get-Animal"  Roles="admins,users" />
		        <WebMethod Name="Get-Animal2" Command="Get-Animal" Roles="users" Users="SeB" />
		        <WebMethod Name="Get-Animal3" Command="Get-Animal"  />
		        <WebMethod Name="Get-Info" Command="Get-Info" AsJob="false" ParameterForUserName="User" ParameterForUserRoles="Roles" ParameterForUserClaims="" />
		      </WebMethods>
		    </WebApi>	
		    
		    <!-- Using Powershell module -->
		    <WebApi Name="Example">
		      <WebMethods>
		        <WebMethod Name="GET-Message" AsJob="false" PowerShellPath="Example.ps1">
		          <Parameters>
		            <Parameter Name="message1" Location="Query" />
		            <Parameter Name="message2" Location="Path" />
		            <Parameter Name="message3" Location="Header" />
		          </Parameters>
		        </WebMethod>
		        <WebMethod Name="Post-Message" AsJob="false" PowerShellPath="Example.ps1">
		          <Parameters>
		            <Parameter Name="message1" Location="Query" />
		            <Parameter Name="message2" Location="Path" />
		            <Parameter Name="message3" Location="Query" />
		          </Parameters>
		        </WebMethod>
		        <WebMethod Name="Put-Message" AsJob="true" PowerShellPath="Example.ps1">
		          <Parameters>
		            <Parameter Name="message1" Location="Query" />
		          </Parameters>
		        </WebMethod>
		        <WebMethod Name="Delete-Message" AsJob="true" PowerShellPath="Example.ps1">
		          <Parameters>
		            <Parameter Name="message1" Location="Query" />
		          </Parameters>
		        </WebMethod>
		      </WebMethods>
		    </WebApi>
		</Apis>
	</WebApiConfiguration>
```

### Configuring the HTTP listeners URI

#### Running on a specific IP (IPv4)
```xml
<WebApiConfiguration HostAddress="http://12.32.12.42:9000">
```

#### Running on any IP (IPv4 and IPv6)
```xml
<WebApiConfiguration HostAddress="http://+:9000">
```

## Adding a web API
If you wanted to offer a script HTTP **GET:/api/foo/bar**  

First, add a WebApi element with the name **foo** then configure the following values then configure the following values .
```xml
    <Apis>
        ...
		<WebApi Name="foo">
```
The first part of URI **/api** is static.
The second part of URI is the `Name` of WebApi. eg: **foo**
If you want to use PowerSHell module, add property `Module`

#### WebApi elements
* `Name`: The name of WebApi and the second part of URI. eg /api/**foo**/bar
* `Module`: Name or path of module will be loaded before launch command


```powershell
<#
.SYNOPSIS
	This script is PowerShell Script Sample for Rest Api exposition.
.DESCRIPTION
    This script content differents types arguments with attributes validator.
.NOTES
    Additional Notes, eg
    File Name  : bar.ps1
    Author     : John Doe - xxx@xxx.com
.LINK
    A hyper link, eg
.PARAMETER MESSAGE1
    Message 1 description
.PARAMETER MESSAGE2
	Message 2 description
.PARAMETER MESSAGE2
	Message  description
#>
param ( 
	[string]$Message1 = "def1",
	[string]$Message2 = "def2",
	[string]$Message3 = "def3",
	[string]$Message4 = "def4"
)

#Sleep -s 10

# Return object
@{
	forlder = Get-Item -Path . | Select-Object name;
	path = $PWD.path;
	msg1 = $Message1;
	msg2 = $Message2;
	msg3 = $Message3;
	msg4 = $Message4
}

```

#### WebMethod element
Now, add the method to the configuration file by adding an `WebMethod` Element

* `Name` The name of the method, which matches the URL pattern /{WebApi:Name}/{WebMethod:Name}?params. The name must be started by  HTTP verb separated by -. eg: Get-bar
* `AsJob` Whether to run the script synchronously (__false__) or async (__true__)
* `PowerShellPath` The script path relative to the ScriptRepository directory. 
* `Command` The cmdlet name if you use PowerShell module, 
* `Roles` The list of roles needed for execution
* `ParameterForUserName` If you want to transfer the caller's username to the PowerShell script, set the name of Powershell parameter.
* `ParameterForUserRoles` If you want to transfer the caller's roles to the PowerShell script, set the name of Powershell parameter.
* `ParameterForUserClaims` If you want to transfer the caller's claims to the PowerShell script, set the name of Powershell parameter.

If you want custimze the paramter, add a `Parameter` Element to the `Parameters` collection for each parameter 

```xml
    <Apis>
        ...
		<WebApi Name="foo">
            <WebMethods>
                <WebMethod Name="Get-bar" AsJob="false" PowerShellPath="bar.ps1">
		            <Parameters>
		              <Parameter Name="Message1" Location="Query" />
		            </Parameters>  
				</WebMethod>
            </WebMethods>
```

#### Parameter elements
* `Name` Name of PowerShell parameter
* `Location` Parameter's location. Must be HTTP Location QUERY, PATH, BODY, HEADER or ConfigFile. Default: QUERY for GET else BODY. 
* `Hidden` **true** if you want hide the paramter in specification OpenApi file. Default: false
* `Value` Value of parameter (if you set it, `Location` is forced to ConfigFile and `Hidden` to true) 
* `IsRequired` Set parameter as Mandatory (if it's defined in PowerShell script, you dont have to define it here)

### Testing your script

Start up the API host from a console

```cmd
.\DynamicPowerShellApi.Host.exe --console
```

![Console](http://s28.postimg.org/4h2oquti5/ps_host_test1.png)

Using a tool like Postman you can check your script output

![Example response](http://s13.postimg.org/wab4f3cbr/ps_host_response.png)

returns
```json
{
  "message": "zab"
}
```

## Authentication 

### Authenticatin with JWT

By default, authentication is __disabled__ for testing purposes. The primary authentication option is JSON Web-Token (JWT)

You can re-enable it by setting Enabled to __"true"__ then configure the following values to enable JWT auth.

* `StoreName` - The Windows certificate store name, e.g. My, Root, Trust
* `StoreLocation` - The location store, e.g. LocalMachine, CurrentUser
* `Thumbprint` - The thumbprint of the SSL certificate
* `Audience` - The expected JWT audience for inbound tokens [Help](https://tools.ietf.org/html/rfc7519#section-4.1.3)

```xml
<Authentication Enabled="false" StoreName="My" StoreLocation="LocalMachine" Thumbprint="E6B6364C75ED8B6495A42D543AC728B4C2263082" Audience="http://dimensiondata.com/auth/connectors" />
```
### Authentication with ApiKey

By default, authentication is __disabled__ for testing purposes. 
You can re-enable it by setting Enabled to __"true"__ to enable ApiKey auth.

By default, it expects a header in the following format:
```
Authentication: ApiKey {key}
```

The format of the expected header containing the API key is completely customisable. 

* `Header` - The name of the HTTP header field. Default : Authentication
* `HeaderKey` - Prefix of the value. Default : ApiKey

Example : 

To use `X-API-KEY` as the field name without prefix for value, configure the following values: 
```xml
<ApiKeyAuthentication Enabled="true" Header="X-API-KEY" HeaderKey="" />
```
Now, add the user to the configuration file by adding an `User` Element in `Users` section

* `ApiKey` String representing the ApiKey *(if you don' have idea, generate new GUID in PowerShell with [guid]::NewGuid())* 
* `Name` The user name 
* `Roles` List of user roles (separated by comma)
* `IpAddresses` List of IP Addresses (separated by comma) where the call can be made.

#### Adding ApiKey

```xml
<Users>
   <User Name="SeB" ApiKey="123" IpAddresses="10.1.53.10,10.1.52.10" Roles="admins" />
   <User Name="SeB2" ApiKey="0bdb7446-0087-4979-b82c-e6c827822eba" IpAddresses="" Roles="admins,OSInstallRW" />
</Users>
```
### Adding another authentication option

If you want to use another authentication option, you can leverage OWIN middleware to plug and play OAuth, certificates or any of the other supported auth methods.

In DynamicPowerShellApi.Owin/Startup.cs replace the existing auth configuration with your choice, e.g. OAuth

```csharp
    // If the config file specifies authentication, load up the certificates and use the JWT middleware.
    if (WebApiConfiguration.Instance.Authentication.Enabled)
    {
        appBuilder.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions()
        {
            AccessTokenFormat = "eg.."
            ...
        });
       
    }
```

## Error Handling

4 error categories are trapped in your Powershell script
* `PermissionDenied`: cause the HTTP response code to be 403 (Forbidden)
* `ObjectNotFound`: cause the HTTP response code to be 404 (NotFound)
* `InvalidArgument`: cause the HTTP response code to be 400 (BadRequest)
* `ResourceExists`: cause the HTTP response code to be 409 (Conflict)

Any other errors cause the HTTP response code to be 500 (InternalServerError)

Add Write-Error command in your powershell script with the appropriate category
```powershell
Write-Error "Group does not exist." -Category ObjectNotFound
```

You will get the following response from the API

For error 400, 403, 404, 409
```json
{
  "Message": "Group does not exist.",
}
```


For error 500
```json
{
  "Message": "Error reading JObject from JsonReader. Current JsonReader item is not an object: String. Path '', line 1, position 5.",
  "Success": false,
  "LogFile": "bar130899475577107290.xml",
  "ActivityId": "bc346446-9964-4ff2-ad45-d7b13efe84b5"
}
```

Also, it will log the error in a `Logs` folder underneath the host directory.

### Example error log
```xml
<?xml version="1.0" encoding="utf-8"?>
<CrashLogEntry xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <ActivityId>bc346446-9964-4ff2-ad45-d7b13efe84b5</ActivityId>
  <LogTime>2015-10-22T11:32:37.710729+11:00</LogTime>
  <RequestUrl>http://localhost:9000/api/foo/bar?message=baz</RequestUrl>
  <RequestAddress />
  <Exceptions>
    <PowerShellException>
      <ScriptName>GenericController.cs</ScriptName>
      <ErrorMessage>Error reading JObject from JsonReader. Current JsonReader item is not an object: String. Path '', line 1, position 5.</ErrorMessage>
      <LineNumber>0</LineNumber>
      <StackTrace>   at Newtonsoft.Json.Linq.JObject.Load(JsonReader reader)
   at Newtonsoft.Json.Linq.JObject.Parse(String json)
   at DynamicPowerShellApi.Controllers.GenericController.&lt;ProcessRequestAsync&gt;d__1f.MoveNext()</StackTrace>
    </PowerShellException>
  </Exceptions>
  <RequestMethod>bar</RequestMethod>
</CrashLogEntry>
```
