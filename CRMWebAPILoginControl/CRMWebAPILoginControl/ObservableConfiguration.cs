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
using Microsoft.Crm.Sdk.Samples.HelperCode;
using System.ComponentModel;


namespace CRMWebAPILoginControl
{
   public class ObservableConfiguration : Configuration, INotifyPropertyChanged 
    {
        public ObservableConfiguration() { }
        public string OServiceUrl {
            get { return ServiceUrl; }
            set
            {
                if (ServiceUrl != value)
                {
                    if (!value.EndsWith("/"))
                    {
                        value = value + "/";
                    }
                    base.ServiceUrl = value;
                    RaisePropertyChanged("OServiceUrl");
                }
            }
        }

        public string OUsername
        {
            get {
                return Username;
            }
            set
            {
                if (Username != value)
                {
                    Username = value;
                    RaisePropertyChanged("OUsername");
                }
            }
        }

        public string ODomain
        {
            get { return base.Domain; }
            set
            {
                if (Domain != value)
                {
                    Domain = value;
                    RaisePropertyChanged("ODomain");
                }
            }
        }
     
        private PropertyChangedEventHandler propertyChanged;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add
            {
                propertyChanged += value;
            }

            remove
            {
                propertyChanged -= value;
            }
        }
        private void RaisePropertyChanged(string propertyName)
        {
            propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


    }


}
