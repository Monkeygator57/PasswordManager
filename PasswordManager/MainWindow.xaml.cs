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
using System.Windows.Shapes;
using Microsoft.Data.Sqlite;
using System.IO;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using PasswordManager.Data;


namespace PasswordManager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // hardcoded for development purposes
    private static readonly byte[] AesKey = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 bytes
    private static readonly byte[] AesIV = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bytes

    private readonly PasswordDecryption _crypto;
    private readonly IPasswordRepository _passwordRepository;
    private const string DatabasePath = @"C:\Users\monke\source\repos\PasswordManager\Data\PasswordManagerDB.db";

    // Constructor for MainWindow
    public MainWindow()
    {
        // Initialize the database if it doesn't exist, necessary for first run
        InitializeComponent();

        //Initialize the encryption service
        _crypto = new PasswordDecryption(AesKey, AesIV);
        // Initialize the password repository
        _passwordRepository = new PasswordRepository(DatabasePath);

        //Call LoadPasswords to update table with passwords and website info
        LoadPasswords();

    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
    }

    // Button for adding password to database
    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        string Website = WebsiteTextBox.Text.Trim();
        string Username = UsernameTextBox.Text.Trim();
        string Password = PasswordTextBox.Text;

        if (string.IsNullOrEmpty(Website) || string.IsNullOrEmpty(Password))
        {
            MessageBox.Show("Please fill in all fields.");
            return;
        }

        var entry = new Models.PasswordEntry
        {
            Website = Website,
            Username = Username,
            EncryptedPassword = _crypto.EncryptString(Password)
        };

        /*string encryptedPassword = _crypto.EncryptString(Password);

        const string dbPath = @"C:\Users\monke\source\repos\PasswordManager\Data\PasswordManagerDB.db";

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            string insertQuery = "INSERT INTO Passwords (Website, Username, EncryptedPassword) VALUES (@website, @username, @password);";

            using (var command = new SqliteCommand(insertQuery, connection))
            {
                command.Parameters.AddWithValue("@website", Website);
                command.Parameters.AddWithValue("@username", Username);
                command.Parameters.AddWithValue("@password", encryptedPassword);

                command.ExecuteNonQuery();
            }

            connection.Close();
        }*/

        _passwordRepository.Add(entry);

        ClearInputs();
        LoadPasswords();
    }


    // Method read and loads passwords from database
    private void LoadPasswords()
    {
        var passwordEntries = new List<Models.PasswordEntry>();

        string dbPath = @"C:\Users\monke\source\repos\PasswordManager\Data\PasswordManagerDB.db";

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            string selectQuery = "SELECT * FROM Passwords;";
            using (var command = new SqliteCommand(selectQuery, connection))
            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    passwordEntries.Add(new Models.PasswordEntry
                    {
                        ID = reader.GetInt32(0),
                        Website = reader.GetString(1),
                        Username = reader.GetString(2),
                        EncryptedPassword = reader.GetString(3)
                    });
                }
            }

            connection.Close();
        }

        PasswordsListView.ItemsSource = passwordEntries;

    }

    // Button to delete password from database
    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (PasswordsListView.SelectedItem is not Models.PasswordEntry selectedEntry)
        {
            MessageBox.Show("Please select an entry to delete.");
            return;
        }

        _passwordRepository.Delete(selectedEntry.ID);
        LoadPasswords();
    }

    // Button to clear input fields
    private void ClearInputs()
    {
        WebsiteTextBox.Text = "";
        UsernameTextBox.Text = "";
        PasswordTextBox.Text = "";
    }

}





