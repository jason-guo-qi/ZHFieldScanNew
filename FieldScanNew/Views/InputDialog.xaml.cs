using System.Windows;

namespace FieldScanNew.Views
{
    // 确保这是 partial class
    public partial class InputDialog : Window
    {
        public string Answer { get; private set; }

        public InputDialog(string question, string defaultAnswer = "")
        {
            InitializeComponent();
            // 明确指定使用WPF的Application
            Owner = System.Windows.Application.Current.MainWindow;
            QuestionText.Text = question;
            AnswerTextBox.Text = defaultAnswer;
            AnswerTextBox.Focus();
            AnswerTextBox.SelectAll();
            Answer = string.Empty; // 解决CS8618警告
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Answer = AnswerTextBox.Text;
            this.DialogResult = true;
        }
    }
}