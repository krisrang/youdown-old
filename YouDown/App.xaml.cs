using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using Lemon;
using MEFedMVVM.ViewModelLocator;
using YouDown.Models;

namespace YouDown
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Analytics.AppStart();

            MainWindow = (Window)Bootstrapper.Initialize();
            MainWindow.Show();
        }
    }
}
