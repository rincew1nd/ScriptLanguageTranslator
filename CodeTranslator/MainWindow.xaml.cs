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

namespace CodeTranslator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Translator _translator;
        public ObservableCollection<KeyValuePair<string, double>> Results { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            _translator = new Translator();
            Results = new ObservableCollection<KeyValuePair<string, double>>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var codeBoxText = new TextRange(CodeBox.Document.ContentStart, CodeBox.Document.ContentEnd).Text;
            ErrorBox.Content = _translator.CheckSyntax(codeBoxText);
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
