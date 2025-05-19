using System.Data;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using PasswordManager.Data;
using PasswordManager.Security;
using PasswordManager.Models;
using System.Collections.Generic;


namespace PasswordManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly PasswordCrypto _crypto;
        private readonly IPasswordRepository _passwordRepository;
        private readonly string _databasePath;

        // Constructor for MainWindow
        public MainWindow(byte[] derivedKey)
        {
            if (derivedKey == null)
            {
                throw new ArgumentNullException("Derived key or IV cannot be null.");
            }

            try
            {
                // Initialize the database if it doesn't exist, necessary for first run
                InitializeComponent();

                // Set up the database path
                string appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PasswordManager"
                );

                //Create the directory if it doesn't exist
                Directory.CreateDirectory(appDataPath);

                // Ensure dir exists
                _databasePath = Path.Combine(appDataPath, "PasswordManagerDB.db");

                // Initialize the database if needed
                InitializeDatabase();

                // Set up the crypto object
                _crypto = new PasswordCrypto(derivedKey);

                // Set up the password repository
                _passwordRepository = new PasswordRepository(_databasePath);

                // Load existing passwords from the database
                LoadPasswords();
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error initializing application: {ex.Message}");
                Application.Current.Shutdown();
            }

        }

        private void InitializeDatabase()
        {
            // Check if the database file exists
            if (!File.Exists(_databasePath))
            {
                // Create the database and the Passwords table
                using var connection = new SqliteConnection($"Data Source={_databasePath}");
                connection.Open();

                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS passwords (
                    ID INTEGER PRIMARY KEY AUTOINCREMENT,
                    Website TEXT NOT NULL,
                    Username TEXT NOT NULL,
                    EncryptedPassword TEXT NOT NULL
                );";

                using var command = new SqliteCommand(createTableQuery, connection);
                command.ExecuteNonQuery();
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string generatedPassword = GenerateSecurePassword(16); // 16 characters
                PasswordTextBox.Text = generatedPassword;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating password: {ex.Message}");
            }
        }

        private string GenerateSecurePassword(int length)
        {
            const string uppercaseChars = "ABCDEFGHJKLMNPQRSTUVWXYZ";  // excluding similar-looking characters
            const string lowercaseChars = "abcdefghijkmnopqrstuvwxyz"; // excluding similar-looking characters
            const string numericChars = "23456789";                     // excluding 0 and 1
            const string specialChars = "!@#$%^&*-+";

            using var rng = RandomNumberGenerator.Create();
            var result = new StringBuilder(length);
            var buffer = new byte[4];

            // Ensure at least one character from each category
            result.Append(GetRandomChar(uppercaseChars, rng, buffer));
            result.Append(GetRandomChar(lowercaseChars, rng, buffer));
            result.Append(GetRandomChar(numericChars, rng, buffer));
            result.Append(GetRandomChar(specialChars, rng, buffer));

            // Fill the rest with a mix of all character types
            string allChars = uppercaseChars + lowercaseChars + numericChars + specialChars;

            for (int i = 4; i < length; i++)
            {
                rng.GetBytes(buffer);
                uint randomValue = BitConverter.ToUInt32(buffer, 0);
                result.Append(allChars[(int)(randomValue % allChars.Length)]);
            }

            // Shuffle the password characters
            return ShuffleString(result.ToString(), rng);
        }

        private char GetRandomChar(string charSet, RandomNumberGenerator rng, byte[] buffer)
        {
            rng.GetBytes(buffer);
            uint randomValue = BitConverter.ToUInt32(buffer, 0);
            return charSet[(int)(randomValue % charSet.Length)];
        }

        private string ShuffleString(string input, RandomNumberGenerator rng)
        {
            char[] array = input.ToCharArray();
            int n = array.Length;
            var buffer = new byte[4];

            while (n > 1)
            {
                rng.GetBytes(buffer);
                uint randomValue = BitConverter.ToUInt32(buffer, 0);
                int k = (int)(randomValue % n);
                n--;
                char temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }

            return new string(array);
        }

        // Button for adding password to database
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            string website = WebsiteTextBox.Text.Trim();
            string username = UsernameTextBox.Text.Trim();
            string password = PasswordTextBox.Text;

            if (string.IsNullOrEmpty(website) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            try
            {
                var entry = new Models.PasswordEntry
                {
                    Website = website,
                    Username = username,
                    EncryptedPassword = _crypto.EncryptPassword(password)
                };

                _passwordRepository.Add(entry);

                ClearInputs();
                LoadPasswords();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding password: {ex.Message}");
            }
        }


        // Method read and loads passwords from database
        private void LoadPasswords()
        {
            try
            {
                var passwordEntries = _passwordRepository.GetAll();
                PasswordsListView.ItemsSource = passwordEntries;
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error loading passwords: {ex.Message}");
            }

        }

        // Button to delete password from database
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (PasswordsListView.SelectedItem is not PasswordEntry selectedEntry)
            {
                MessageBox.Show("Please select an entry to delete.");
                return;
            }

            try
            {
                _passwordRepository.Delete(selectedEntry.ID);
                LoadPasswords();
            }

            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting password: {ex.Message}");
            }
        }

        // Method to view password, decrypts it
        private void ViewPassword_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is PasswordEntry entry)
            {
                try
                {
                    string decryptedPassword = _crypto.DecryptPassword(entry.EncryptedPassword);
                    MessageBox.Show($"Password: {decryptedPassword}", $"Password for {entry.Website}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error decrypting password: {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show("Please select an entry to view.");
            }
        }

        // Button to clear input fields
        private void ClearInputs()
        {
            WebsiteTextBox.Text = "";
            UsernameTextBox.Text = "";
            PasswordTextBox.Text = "";
        }

    }
}



