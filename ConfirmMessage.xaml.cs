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
using System.Windows.Shapes;

namespace eBayKleinanzeigenTracker
{
    /// <summary>
    /// Interaction logic for ConfirmMessage.xaml
    /// </summary>
    public partial class ConfirmMessage : Window
    {
        public ConfirmMessage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (cbRepeat.IsChecked.Value) {
                Properties.Settings.Default.ShowCloseConfirmDialog = false;
                Properties.Settings.Default.Save();
            }
            Close();
        }
    }
}
