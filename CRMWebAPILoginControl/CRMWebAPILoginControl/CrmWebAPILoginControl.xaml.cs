using Microsoft.Crm.Sdk.Samples.HelperCode;
using System;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Controls;


namespace CRMWebAPILoginControl
{
    /// <summary>
    /// Interaction logic for CRMWebAPILoginControl.xaml
    /// </summary>
    public partial class LoginControl : UserControl
    {

        public Configuration config;


        public LoginControl()
        {
            InitializeComponent();

            
        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {

            Window.GetWindow(this).DialogResult = false;
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            ((ObservableConfiguration)DataContext).Password = Password.SecurePassword;

            Window parentWindow = Window.GetWindow(this);
            
            parentWindow.DialogResult = true;
            parentWindow.Close();
        }

        private void setPassword(object sender, RoutedEventArgs e)
        {
            SecureString pw = ((ObservableConfiguration)DataContext).Password;
            if (pw != null && pw.Length > 0)
            {
                Password.Password = SecureStringHelper.ConvertToUnsecureString(pw);
            }
        }


    }

    public static class SecureStringHelper
    {
        //From https://blogs.msdn.microsoft.com/fpintos/2009/06/12/how-to-properly-convert-securestring-to-string/
        public static string ConvertToUnsecureString(SecureString securePassword)
        {
            if (securePassword == null)
                throw new ArgumentNullException("securePassword");

            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(securePassword);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}
