using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
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
using Lemon;
using YouDown.Presentation;

namespace YouDown.Views
{
    [PartCreationPolicy(CreationPolicy.NonShared)]
    [ExportOverlayWindow("Import")]
    public partial class ImportView : OverlayWindow
    {
        public ImportView()
        {
            InitializeComponent();

            var vm = DataContext as ViewModelBase;

            if (vm != null)
                vm.RequestClose += Close;
        }
    }
}
