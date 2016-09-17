/*
The MIT License (MIT)

Copyright (c) 2016 Jim Daly

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using CRMWebAPILoginControl;
using Microsoft.Crm.Sdk.Samples.HelperCode;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TestProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableConfiguration config { get; set; }
        private Authentication auth = null;


        public MainWindow()
        {
            InitializeComponent();

            config = new ObservableConfiguration();

            //TODO: To connect to CRM Online you must add your application ClientId and redirect URL here:

            //config.ClientId = "########-####-####-####-############";
            //config.RedirectUrl = "https://yourDomain/yourApp";


            //For testing: 
            //Setting your credentials here will populate the dialog

            //On-premise:
            //config.OServiceUrl = "http://yourCRMServer/yourOrg/";
            //config.OUsername = "administrator";
            //SecureString ss = new SecureString();
            //"yourPassword".ToCharArray().ToList().ForEach(p => ss.AppendChar(p));
            //config.ODomain = "~";

            //online
            config.OServiceUrl = "https://yourOrg.crm.dynamics.com/";
            config.OUsername = "you@yourOrg.onmicrosoft.com";
            SecureString ss = new SecureString();
            "yourPassword".ToCharArray().ToList().ForEach(p => ss.AppendChar(p));


            config.Password = ss;

        }

        private async void connectButton_Click(object sender, RoutedEventArgs e)
        {
            //Clear any message text
            await Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() => Message.Text = ""));

            Window loginWindow = new Window
            {

                Title = "Login to CRM",
                Content = new LoginControl(),
                DataContext = config,
                Height = 275,
                Width = 400
            };

            loginWindow.ShowDialog();



            if (loginWindow.DialogResult.HasValue && loginWindow.DialogResult.Value.Equals(true))
            {

                string authority = await Authentication.DiscoverAuthorityAsync(config.ServiceUrl);

                auth = new Authentication(config, authority);
                //You are connected..

                await ShowUserId();
            }
        }


        /// <summary>
        /// Example method that retrieves the user's Id value
        /// </summary>
        /// <returns></returns>
        private async Task ShowUserId()
        {
            using (HttpClient client = getHttpClient())
            {
                HttpRequestMessage WhoAmIRequest = new HttpRequestMessage(HttpMethod.Get, "WhoAmI()");


                try
                {
                    HttpResponseMessage WhoAmIResponse = await client.SendAsync(WhoAmIRequest);


                    if (WhoAmIResponse.StatusCode == HttpStatusCode.OK)
                    {
                        JObject response = JsonConvert.DeserializeObject<JObject>(await WhoAmIResponse.Content.ReadAsStringAsync());

                        await Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            new Action(() => userId.Text = string.Format("Connected to {0} with userid: '{1}'.", config.ServiceUrl, (string)response.GetValue("UserId"))));

                    }
                    else
                    {

                        CrmHttpResponseException ex = new CrmHttpResponseException(WhoAmIResponse.Content);


                        await Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                            new Action(() => Message.Text = ex.Message));
                    }
                }
                catch (Exception ex)
                {

                    await Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                           new Action(() => Message.Text = ex.Message));
                }


            }
        }

        /// <summary>
        /// Method to get a configured HttpClient using the helper Authentication and Configuration classes
        /// </summary>
        /// <returns></returns>
        public HttpClient getHttpClient()
        {

            HttpClient httpClient = new HttpClient(auth.ClientHandler, true);
            httpClient.BaseAddress = new Uri(config.ServiceUrl + "api/data/v8.1/");
            httpClient.Timeout = new TimeSpan(0, 2, 0);
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            return httpClient;
        }
    }
}
