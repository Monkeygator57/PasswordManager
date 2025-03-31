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

namespace PasswordManager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    // hardcoded for development purposes
    private static readonly byte[] AesKey = Encoding.UTF8.GetBytes("12345678901234567890123456789012"); // 32 bytes
    private static readonly byte[] AesIV = Encoding.UTF8.GetBytes("1234567890123456"); // 16 bytes


    private class PasswordEntry
    {
        public int ID { get; set; }
        public string? Website { get; set; }
        public string? Username { get; set; }
        public string? EncryptedPassword { get; set; }
    }


    public MainWindow()
    {
        InitializeComponent();

        //Call LoadPasswords to update table with passwords and website info
        LoadPasswords();

    }

    private string EncryptString(string plainText)
    {
        using System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create();
        aes.Key = AesKey;
        aes.IV = AesIV;

        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using MemoryStream ms = new();
        using CryptoStream cs = new(ms, encryptor, CryptoStreamMode.Write);
        using (StreamWriter sw = new(cs))
        {
            sw.Write(plainText);
        }
        
        return Convert.ToBase64String(ms.ToArray());
    }

    private string DecryptString(string encryptedText)
    {
        using System.Security.Cryptography.Aes aes = System.Security.Cryptography.Aes.Create();
        aes.Key = AesKey;
        aes.IV = AesIV;

        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        byte[] buffer = Convert.FromBase64String(encryptedText);

        using MemoryStream ms = new(buffer);
        using CryptoStream cs = new(ms, decryptor, CryptoStreamMode.Read);
        using StreamReader sr = new(cs);
        {
            return sr.ReadToEnd();
        }
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {

    }

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

        string encryptedPassword = EncryptString(Password);

        string dbPath = @"C:\Users\monke\source\repos\PasswordManager\Data\PasswordManagerDB.db";

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
        }

        ClearInputs();
        LoadPasswords();
    }


    private void ClearInputs()
    {
        WebsiteTextBox.Text = "";
        UsernameTextBox.Text = "";
        PasswordTextBox.Text = "";
    }

    private void LoadPasswords()
    {
        var passwordEntries = new List<PasswordEntry>();

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
                    passwordEntries.Add(new PasswordEntry
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

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (PasswordsListView.SelectedItem is not PasswordEntry selectedEntry)
        {
            MessageBox.Show("Please select an entry to delete.");
            return;
        }

        string dbPath = @"C:\Users\monke\source\repos\PasswordManager\Data\PasswordManagerDB.db";

        using (var connection = new SqliteConnection($"Data Source={dbPath}"))
        {
            connection.Open();

            string deleteQuery = "DELETE FROM Passwords WHERE ID = @id;";

            using (var command = new SqliteCommand(deleteQuery, connection))
            {
                command.Parameters.AddWithValue("@id", selectedEntry.ID);
                command.ExecuteNonQuery();
            }

            connection.Close();
        }

        LoadPasswords();
        }
    
    }





