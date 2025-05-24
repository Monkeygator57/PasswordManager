using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using PasswordManager.Security;

namespace PasswordManager
{
    public partial class LoginWindow : Window
    {
        private readonly KeyGenerator _keyGenerator;
        private byte[]? _derivedKey = null;
        private byte[]? _derivedIv = null;

        public event EventHandler<byte[]>? LoginSucceeded;

        public bool IsFirstRun{ get; private set; }

        public byte[]? DerivedKey => _derivedKey;
        public byte[]? DerivedIv => _derivedIv;

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

            // Set focus to the password box
            Loaded += (s, e) => PasswordBox.Focus();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            PerformLogin();
        }

        private void PasswordBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                PerformLogin();
            }
        }

        private void PerformLogin()
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

                        LoginSucceeded?.Invoke(this, _derivedKey);

                        this.Close();
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

                    LoginSucceeded?.Invoke(this, _derivedKey);

                    this.Close();
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
