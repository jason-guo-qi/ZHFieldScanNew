using System.Windows;

namespace FieldScanNew.Views
{
    public partial class DialogWindow : Window
    {
        public DialogWindow()
        {
            InitializeComponent();
            Owner = System.Windows.Application.Current.MainWindow;
        }
    }
}