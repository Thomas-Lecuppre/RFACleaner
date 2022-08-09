using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RFACleaner
{
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // Application is running
            // Process command line args
            string folderPath = null;

            for (int i = 0; i != e.Args.Length; ++i)
            {
                if (Directory.Exists(e.Args[i]))
                {
                    folderPath = e.Args[i];
                }
            }

            MainWindow mainWindow = null;
            if (folderPath != null)
            {
                mainWindow = new MainWindow(folderPath);
            }
            else
            {
                mainWindow = new MainWindow();
            }

            mainWindow.Show();
        }
    }
}
