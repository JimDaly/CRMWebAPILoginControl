﻿// =====================================================================
//  This file is part of the Microsoft Dynamics CRM SDK code samples.
//
//  Copyright (C) Microsoft Corporation.  All rights reserved.
//
//  This source code is intended only as a supplement to Microsoft
//  Development Tools and/or on-line documentation.  See these other
//  materials for detailed information regarding Microsoft code samples.
//
//  THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
//  KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
//  IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
//  PARTICULAR PURPOSE.
// =====================================================================
//<snippetAuthentication>
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading.Tasks;

namespace Microsoft.Crm.Sdk.Samples.HelperCode
{
    /// <summary>
    /// Manages user authentication with the Dynamics CRM Web API (OData v4) services. This class uses Microsoft Azure
    /// Active Directory Authentication Library (ADAL) to handle the OAuth 2.0 protocol. 
    /// </summary>
    public class Authentication
    {
        private Configuration _config = null;
        private HttpMessageHandler _clientHandler = null;
        private AuthenticationContext _context = null;
        private string _authority = null;

        #region Constructors
        /// <summary>
        /// Base constructor.
        /// </summary>
        public Authentication() { }

        /// <summary>
        /// Establishes an authentication session for the service.
        /// </summary>
        /// <param name="config">A populated configuration object.</param>
        public Authentication(Configuration config)
            : base()
        {
            if (config == null)
                throw new Exception("Configuration cannot be null.");

            _config = config;

            SetClientHandler();
        }

        /// <summary>
        /// Custom constructor that allows adding an authority determined asynchronously before 
        /// instantiating the Authentication class.
        /// </summary>
        /// <remarks>For a WPF application, first call DiscoverAuthorityAsync(), and then call this
        /// constructor passing in the authority value.</remarks>
        /// <param name="config">A populated configuration object.</param>
        /// <param name="authority">The URL of the authority.</param>
        public Authentication(Configuration config, string authority)
            : base()
        {
            if (config == null)
                throw new Exception("Configuration cannot be null.");

            _config = config;
            Authority = authority;

            SetClientHandler();
        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// The authentication context.
        /// </summary>
        public AuthenticationContext Context
        {
            get
            { return _context; }

            set
            { _context = value; }
        }

        /// <summary>
        /// The HTTP client message handler.
        /// </summary>
        public HttpMessageHandler ClientHandler
        {
            get
            { return _clientHandler; }

            set
            { _clientHandler = value; }
        }


        /// <summary>
        /// The URL of the authority to be used for authentication.
        /// </summary>
        public string Authority
        {
            get
            {
                if (_authority == null)
                    _authority = DiscoverAuthority(_config.ServiceUrl);

                return _authority;
            }

            set { _authority = value; }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Returns the authentication result for the configured authentication context.
        /// </summary>
        /// <returns>The refreshed access token.</returns>
        /// <remarks>Refresh the access token before every service call to avoid having to manage token expiration.</remarks>
        public AuthenticationResult AcquireToken()
        {
            //Not trying to pass the password through without the AD popup, just fill out everything else.
                       
            return _context.AcquireTokenAsync(
                _config.ServiceUrl,
                _config.ClientId,
                new Uri(_config.RedirectUrl),
                new PlatformParameters(PromptBehavior.Auto),
                new UserIdentifier(
                    _config.Username,
                    UserIdentifierType.RequiredDisplayableId)
                ).Result;
        }
        //TODO: What happens when the token expires? Will the user be prompted again?  - Not acceptable.
        //See http://stackoverflow.com/questions/37384660/adal-authentication-without-dialog-box-prompt
        // Possible to make the Configuration class inherit from TokenCache rather than the FileCache used in the sample?


        /// <summary>
        /// Discover the authentication authority.
        /// </summary>
        /// <returns>The URL of the authentication authority on the specified endpoint address, or an empty string
        /// if the authority cannot be discovered.</returns>
        public static string DiscoverAuthority(string serviceUrl)
        {
            try
            {
                AuthenticationParameters ap = AuthenticationParameters.CreateFromResourceUrlAsync(
                    new Uri(serviceUrl + "api/data/")).Result;

                return ap.Authority;
            }
            catch (HttpRequestException e)
            {
                throw new Exception("An HTTP request exception occurred during authority discovery.", e);
            }
            catch (Exception e)
            {
                // This exception ocurrs when the service is not configured for OAuth.
                if (e.HResult == -2146233088)
                {
                    return String.Empty;
                }
                else
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// Discover the authentication authority asynchronously.
        /// </summary>
        /// <param name="serviceUrl">The specified endpoint address</param>
        /// <returns>The URL of the authentication authority on the specified endpoint address, or an empty string
        /// if the authority cannot be discovered.</returns>
        public static async Task<string> DiscoverAuthorityAsync(string serviceUrl)
        {
            try
            {
                AuthenticationParameters ap = await AuthenticationParameters.CreateFromResourceUrlAsync(
                    new Uri(serviceUrl + "api/data/"));

                return ap.Authority;
            }
            catch (HttpRequestException e)
            {
                throw new Exception("An HTTP request exception occurred during authority discovery.", e);
            }
            catch (Exception e)
            {
                // These exceptions ocurr when the service is not configured for OAuth.

                // -2147024809 message: Invalid authenticate header format Parameter name: authenticateHeader
                if (e.HResult == -2146233088 || e.HResult == -2147024809)
                {
                    return String.Empty;
                }
                else
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// Sets the client message handler as appropriate for the type of authentication
        /// in use on the web service endpoint.
        /// </summary>
        private void SetClientHandler()
        {
            // Check the Authority to determine if OAuth authentication is used.
            if (string.IsNullOrEmpty(Authority))
            {
                if (_config.Username != String.Empty)
                {
                    _clientHandler = new HttpClientHandler()
                    { Credentials = new NetworkCredential(_config.Username, _config.Password, _config.Domain) };
                }
                else
                // No username is provided, so try to use the default domain credentials.
                {
                    _clientHandler = new HttpClientHandler()
                    { UseDefaultCredentials = true };
                }
            }
            else
            {
                _clientHandler = new OAuthMessageHandler(this, new HttpClientHandler());
                _context = new AuthenticationContext(Authority, false);
            }
        }
        #endregion Methods

        /// <summary>
        /// Custom HTTP client handler that adds the Authorization header to message requests. This
        /// is required for IFD and Online deployments.
        /// </summary>
        class OAuthMessageHandler : DelegatingHandler
        {
            Authentication _auth = null;

            public OAuthMessageHandler(Authentication auth, HttpMessageHandler innerHandler)
                : base(innerHandler)
            {
                _auth = auth;
            }

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
            {
                // It is a best practice to refresh the access token before every message request is sent. Doing so
                // avoids having to check the expiration date/time of the token. This operation is quick.
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _auth.AcquireToken().AccessToken);

                return base.SendAsync(request, cancellationToken);
            }
        }
    }
}
//</snippetAuthentication>
