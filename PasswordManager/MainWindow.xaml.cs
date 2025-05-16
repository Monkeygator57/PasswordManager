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


namespace PasswordManager;

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

    private void InitializeDatabase()
    {
        // Check if the database file exists
        if (!File.Exists(_databasePath))
        {
            // Create the database and the Passwords table
            using var connection = new SqliteConnection($"Data Source={_databasePath}");
            connection.Open();

            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Passwords (
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
        // Generate a random password
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
        if (PasswordsListView.SelectedItem is PasswordEntry entry)
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





