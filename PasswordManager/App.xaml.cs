using System.Configuration;
using System.Data;
using System.Windows;
using System;
using System.IO;

namespace PasswordManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Show login window first
        var loginWindow = new LoginWindow();

        if (loginWindow.ShowDialog() == true)
        {
            Console.WriteLine("Login successful.");    

            // If it's the first run, show the main window after setting the master password
            MainWindow mainWindow = new MainWindow(loginWindow.DerivedKey);
            Console.WriteLine("MainWindow created.");
            mainWindow.Show();
            Console.WriteLine("MainWindow shown.");
        }

        else
        {
            Console.WriteLine("Login failed or cancelled.");
            Shutdown();
        }
    }

}

