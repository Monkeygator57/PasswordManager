using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Security.Cryptography;
using System.Windows;

namespace PasswordManager;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{

    protected override void OnStartup(StartupEventArgs e)
    {
        try
        {
            base.OnStartup(e);

            // Create and show login window
            var loginWindow = new LoginWindow();

            loginWindow.LoginSucceeded += (sender, key) => {
                // Only create the main window in this event handler
                Dispatcher.Invoke(() => {
                    try
                    {
                        var mainWindow = new MainWindow(key);
                        mainWindow.Show();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error creating main window: {ex.Message}");
                        Shutdown();
                    }
                });
            };

            // Show login without using ShowDialog()
            loginWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error: {ex.Message}");
            Shutdown();
        }
    }
}

