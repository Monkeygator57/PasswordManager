using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using PasswordManager.Security;

namespace PasswordManager
{
    public partial class LoginWindow : Window
    {
        private readonly KeyGenerator _keyGenerator;
        private byte[] _derivedKey;
        private byte[] _derivedIv;

        public bool IsFirstRun{ get; private set; }

        public byte[] DerivedKey => _derivedKey;
        public byte[] DerivedIv => _derivedIv;

        public LoginWindow()
        {
            InitializeComponent();

            // Create Application Directory if it doesn't exist
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "PasswordManager"
            );

            _keyGenerator = new KeyGenerator(appDataPath);

            // Check if the keystore file exists
            IsFirstRun = !_keyGenerator.IsKeyStoreInitialized();

            // Update UI based on whether is first run

            if (IsFirstRun)
            {
                Title = "Set Master Password";
                ConfirmPasswordRow.Visibility = Visibility.Visible;
                LoginButton.Content = "Create";
            }
            else
            {
                Title = "Enter Master Password";
                ConfirmPasswordRow.Visibility = Visibility.Collapsed;
                LoginButton.Content = "Login";
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string password = PasswordBox.Password;
           
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter a password.");
                return;
            }


            // First run - create a new keystore
            if (IsFirstRun)
            {
                // First run - create new keystore
                if (password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Passwords do not match.");
                    return;
                }

                if (password.Length < 8)
                {
                    MessageBox.Show("Master password must be at least 8 characters long.");
                    return;
                }

                if (_keyGenerator.InitializeKeyStore(password))
                {
                    var result = _keyGenerator.GetDerivedKey(password);

                    if (result.Success)
                    {
                        _derivedKey = result.Key;
                        _derivedIv = result.IV;
                        DialogResult = true;
                    }

                    else
                    {
                        MessageBox.Show("Error setting up encryption.");
                    }
                }

                else
                {
                    MessageBox.Show("Failed to initialize key store.");
                }
            }

            else
            {
                // Normal login
                var result = _keyGenerator.GetDerivedKey(password);
                if (result.Success)
                {
                    _derivedKey = result.Key;
                    _derivedIv = result.IV;
                    DialogResult = true;
                }
                else
                {
                    MessageBox.Show("Incorrect password. Please try again.");
                }
            }
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    
    }
    
}
