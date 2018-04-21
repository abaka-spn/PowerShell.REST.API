

# PowerShell.REST.API

Turn any PowerShell script into a HTTP REST API!

### Builds

* 1.2.1804.1801


## The goal of this fork

* Take in charge a Custom PowerShell module: Done.
* Expose a OpenAPI specification from Script or Module: Done.
* Define Rest Method for each WebMethod in configuration file: Done.
* Define Rest Location for each parameters in configuration file: Done.
* Each parameters definition will be fetched from scripts or commands: Done.
* Add Swagger UI for graphical help: Done.
* Add ApiKey authentication: Done.
* Transfer user identity to the Powershell command: Done.
* 


## Overview
*It is near same as orignal project*

HTTP service written in C#.NET using the Microsoft OWIN libraries.

The concept is to take an existing PowerShell **script** or **command** in PowerShell module, with parameters and expose it as a HTTP/REST method.

The web service configures the web methods as boot based on the configuration of the script repository and generate the OpenAPI specfication file.

It hosts a PowerShell runspace pool to load, run and check PowerShell scripts, capturing errors and stack traces to the logs
and parsing complex PowerShell response objects back into JSON.

It also supports async jobs to be run as separate threads, with the job results to be stored on disk.

When the application start, all PowerShell commands are analyze by reflexion and a OpenAPI specification file are generated on /api/server/.


## How it works
*It is near same as orignal project*

This project implements a OWIN WebAPI HTTP service with a single "generic" API controller. The API controller consults the configuration collection of the endpoint to identify which PowerShell script needs to be run for each HTTP request.
It hosts a PowerShell session to import and run the script, whilst monitoring the script process for faults. It is piped through [ConvertTo-Json] and the response to a temporary JObject and then returns the response data (it is a difference with original project).

The PowerShell scripts can be called by GET, POST, DELETE or PUT method. It mus be defined in the configuration file

The parameters can be passed by PATH, QUERYSTRING, BODY, HEADER or VALUE defined in the configuration file (Default: Body).

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

.PARAMETER MESSAGE2
	Message 2 description
If you wanted to offer a script HTTP GET:/foo/bar, POST:/foo/baz

First, add a WebApi element with the name __foo__

```xml
    <Apis>
        ...
		<WebApi Name="foo">
```

Then for your script, bar.ps1, by convention each script should pipe the return object through [ConvertTo-Json](https://technet.microsoft.com/en-us/library/hh849922.aspx), this is because PowerShell's dynamic objects can contain circular references and cause JSON convertors to crash.

Take your named parameters, for example `$message` and do something with them, in this example


```powershell
<#
.SYNOPSIS
	This script is PowerShell Script Sample for Rest Api exposition.
.DESCRIPTION
    This script content differents types arguments with attributes validator.
.NOTES
    Additional Notes, eg
    File Name  : Example.ps1
    Author     : Sébastien Pichon - sebastien.pichon@adeo.com
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
	#Message 1 ok?
	[string]$Message1 = "def1",
	[string]$Message2 = "def2",
	[string]$Message3 = "def3",
	[string]$Message4 = "def4"
	)

#Sleep -s 10

@{
	forlder = Get-Item -Path . | Select-Object name;
	path = $PWD.path;
	msg1 = $Message1;
	msg2 = $Message2;
	msg3 = $Message3;
	msg4 = $Message4
}

```

Now, add the method to the configuration file by adding an `WebMethod` Element

* `Name` The name of the method, which matches the URL pattern /{WebApi:Name}/{WebMethod:Name}?params
* `AsJob` Whether to run the script synchronously (__false__) or async (__true__)
* `PowerShellPath` The script path relative to the ScriptRepository directory.

Then, add a `Parameter` Element to the `Parameters` collection for each parameter, either POST or GET.

```xml
    <Apis>
        ...
		<WebApi Name="foo">
            <WebMethods>
                <WebMethod Name="bar" AsJob="false" PowerShellPath="bar.ps1">
                    <Parameters>
                        <Parameter Name="message" Type="string" />
                    </Parameters>
                </WebMethod>
            </WebMethods>
```

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

By default, authentication is __disabled__ for testing purposes. The primary authentication option is JSON Web-Token (JWT)

You can re-enable it by setting Enabled to __"true"__ then configure the following values to enable JWT auth.

* `StoreName` - The Windows certificate store name, e.g. My, Root, Trust
* `StoreLocation` - The location store, e.g. LocalMachine, CurrentUser
* `Thumbprint` - The thumbprint of the SSL certificate
* `Audience` - The expected JWT audience for inbound tokens [Help](https://tools.ietf.org/html/rfc7519#section-4.1.3)

```xml
<Authentication Enabled="false" StoreName="My" StoreLocation="LocalMachine" Thumbprint="E6B6364C75ED8B6495A42D543AC728B4C2263082" Audience="http://dimensiondata.com/auth/connectors" />
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

By default, any terminal errors in your powershell script will cause the HTTP response code to be HTTP500,

You will get the following response from the API

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
<!--stackedit_data:
eyJoaXN0b3J5IjpbMzQ0MTYwMjMwXX0=
-->