# CRMWebAPILoginControl solution
This Visual Studio solution contains a simple login control which uses a slightly modified version of the Microsoft CRM SDK 
authentication helper classes found on NuGet at
[Microsoft.CrmSdk.WebApi.Samples.HelperCode](http://www.nuget.org/packages/Microsoft.CrmSdk.WebApi.Samples.HelperCode/).

The CRM SDK Helper code assists in creating an application which can authenticate using OAuth 
to CRM Online or using windows credentials to a CRM on-premise deployment. For more information 
about these helper classes, see 
[Use the Microsoft Dynamics CRM Web API Helper Library (C#)](https://msdn.microsoft.com/library/mt770381.aspx).

## Contents
* [Solution Description](#solution-description)
* [Remarks](#remarks)

## Solution Description
This solution has two projects:
<dl>
<dt><strong>CRMWebAPILoginControl</strong></dt>
<dd>A WPF user control which builds a CRMWebAPILoginControl.dll which  is referenced in the 
TestProject WPF application. The dll includes the definition of a defines a 
<strong>CRMWebAPILoginControl.LoginControl</strong> class which displays the UI for users to 
enter their login information.</dd>
<dt><strong>TestProject</strong></dt>
<dd>A simple WPF application project that shows how to reference and use the 
<strong>CRMWebAPILoginControl.LoginControl</strong>.</dd>
</dl>

### CRMWebAPILoginControl
This project contains three files:
<dl>
<dt><strong>CrmWebAPILoginControl.xaml</strong></dt>
<dd>Defines the UI to allow entry of connection information and buttons to login.</dd>
<dt><strong>CrmWebAPILoginControl.xaml.cs</strong></dt>
<dd>The code-behind for CrmWebAPILoginControl.xaml</dd>
<dt><strong>ObservableConfiguration.cs</strong></dt>
<dd>An <strong>ObservableConfiguration</strong> class that inherits from the SDK Helper 
<strong>Configuration</strong> class and implements 
<a href="https://msdn.microsoft.com/en-us/library/system.componentmodel.inotifypropertychanged.aspx">INotifyPropertyChanged</a> 
for the ServiceUrl, UserName, and Domain properties.
The purpose of this class is simply to facilitate binding to the UI for these properties.</dd>
</dl>

> In this project the **Microsoft.Crm.Sdk.Samples.HelperCode.Authentication.AcquireToken** 
method has been modified slightly to catch an error which
> occurs if the organization hasn't granted consent to use the application. Rather than simply 
fail, the error is caught an the user is shown a dialog
> which will allow them to give consent if they have the privileges to do so.

### TestProject
This project contains a reference to the **CRMWebAPILoginControl.dll** built by the 
CRMWebAPILoginControl project which provides 
and is the startup project in the solution.
The default MainWindow.xaml file has been modified to display a **Connect to CRM** button 
which uses **connectButton_Click** as the event handler.

The default MainWindow.xaml.cs code-behind file has been modified to include the following:
#### MainWindow constructor
The constructor initializes an **ObservableConfiguration** property and this is where you 
must specify the **ClientId** and **RedirectUrl** properties
if you need to connect with CRM Online using OAuth. You will need to replace the values for 
these placeholders:
```C#
            //TODO: To connect to CRM Online you must add your application ClientId and redirect URL here:

            config.ClientId = "########-####-####-####-############";
            config.RedirectUrl = "https://yourDomain/yourApp";
```
You will also find that there are some examples to pre-populate values which is useful for 
testing because the control doesn't cache the values you enter.

#### connectButton_Click method
The event handler for the **Connect to CRM** button in the **CrmWebAPILoginControl.xaml**.
The method will launch an instance of the **CRMWebAPILoginControl.LoginControl** as a dialog 
and then process the results using the 
**Microsoft.Crm.Sdk.Samples.HelperCode.Authentication** class to authenticate the user 
appropriately.

#### getHttpClient method
This method returns an 
[HttpClient](https://msdn.microsoft.com/en-us/library/system.net.http.httpclient(v=vs.110).aspx) 
that is ready to use with the CRM Web API after the user is authenticated. It leverages capabilities 
within the SDK Helper **Authentication** and **Configuration** classes.

#### ShowUserId method
This method demonstrates using the HttpClient returned by the **getHttpClient** method to call the 
[WhoAmI Function](https://msdn.microsoft.com/library/mt607925.aspx) to retrieve information 
about the user from their CRM organization.
This information is displayed in the UI to confirm authentication occurred.

## Remarks
This project provides an example of a re-usable control which leverages the helper classes 
provided by the CRM SDK team.
Compared to the 
[XRM tooling common login control](https://msdn.microsoft.com/en-us/library/dn689071.aspx) 
it is lacking the following capabilites:
* Credential storage and retrieval
* Support for multiple languages
* Support for Tracing and logging

But most significantly, it isn't an official release from the CRM product team so it is 
unsupported by Microsoft.

On the other hand, this login control is very simple because it doesn't have to support all the 
robust capabilites of the XRM Tooling APIs, which in turn depends on the SDK assemblies. 
It is possible to leverage the Xrm tooling common login control and allow it to manage 
authentication, but it does so through the constructs which work with the dependent assemblies 
and not with the SDK Web API helper classes. This makes things more complicated because 
you need to keep track of the expiration of the tokens and make a call using the assemblies to 
refresh the token when it is about to expire.

I'm releasing this on GitHub because it may provide a useful example to developers and also because
I am going to use the login control in another project I plan to release soon. I welcome any 
contributions you can make to this project.