using MvvmCross.Platforms.Wpf.Presenters.Attributes;
using MvvmCross.Platforms.Wpf.Views;
using MvvmCross.ViewModels;
using MvxStarter.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Windows.Controls.Primitives;
using MvxStarter.Core.Models;

namespace MvxStarter.Wpf.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [MvxContentPresentation]
    [MvxViewFor(typeof(SearchPanelViewModel))]
    public partial class MainWindowView : MvxWpfView
    {
        public MainWindowView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog= new FolderBrowserDialog();
            if(folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                DirectoryPath.Text = folderBrowserDialog.SelectedPath;
                DirectoryPath.GetBindingExpression(System.Windows.Controls.TextBox.TextProperty)
                   .UpdateSource();
            }
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((FrameworkElement)e.OriginalSource).DataContext as FileModel; //<< your class name here

            if (item != null)
            {
                System.Diagnostics.Process.Start("explorer", System.IO.Path.GetDirectoryName(item.Name));
            }
        }
    }
}
