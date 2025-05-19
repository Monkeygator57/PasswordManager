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
                /*MainWindow mainWindow = new MainWindow(loginWindow.DerivedKey, loginWindow.DerivedIv);
                mainWindow.Show();*/
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
        }
    }
}

