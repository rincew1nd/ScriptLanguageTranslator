using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using CodeTranslator.Exceptions;

namespace CodeTranslator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static RichTextBox Textbox;
        public static TextPointer LastErrorStart;
        public static TextPointer LastErrorEnd;

        public Translator _translator;
        public ObservableCollection<KeyValuePair<string, double>> Results { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _translator = new Translator();
            Results = new ObservableCollection<KeyValuePair<string, double>>();
            Textbox = CodeBox;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Utils.DeselectText();
            ErrorBox.Content = "";
            var codeBoxText = new TextRange(CodeBox.Document.ContentStart, CodeBox.Document.ContentEnd).Text;
            try { _translator.CheckSyntax(codeBoxText); }
            catch (Exception ex) when (ex is SyntaxException | ex is OredrException)
            {
                ErrorBox.Content = ex.Message;
            }
            if ((string) ErrorBox.Content == "")
            {
                Results.Clear();
                foreach (var kvpair in _translator.GetResult().ToList())
                    Results.Add(kvpair);
                this.DataContext = this;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
