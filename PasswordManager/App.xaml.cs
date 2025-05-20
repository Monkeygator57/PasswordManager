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

        /*try
        {
            base.OnStartup(e);

            // Generate a temp key just for testing
            byte[] tempKey = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(tempKey);
            }

            // Skip login window and go straight to main window
            MainWindow mainWindow = new MainWindow(tempKey);
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred during startup: {ex.Message}");
            Shutdown();
        }
        try
        {
            base.OnStartup(e);

            // Show login window first
            var loginWindow = new LoginWindow();

            bool? result = loginWindow.ShowDialog();
            if (result == true)
            {
                if (loginWindow.DerivedKey == null)
                {
                    MessageBox.Show("Failed to generate keys. Exiting application.");
                    Shutdown();
                    return;
                }

                try
                {
                    MainWindow mainWindow = new MainWindow(loginWindow.DerivedKey);
                    mainWindow.Show();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"An error occurred while initializing the main window: {ex.Message}");
                    Shutdown();
                    return;
                }
                // If it's the first run, show the main window after setting the master password
                //MainWindow mainWindow = new MainWindow(loginWindow.DerivedKey);
                //mainWindow.Show();
            }

            else
            {
                Shutdown();
            }
        }

        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred during startup: {ex.Message}");
            Shutdown();
        }*/
    }
}

