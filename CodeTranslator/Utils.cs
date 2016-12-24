using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace CodeTranslator
{
    public static class Utils
    {
        public static void DeselectText()
        {
            if (MainWindow.LastErrorStart != null)
            {
                MainWindow.Textbox.Selection.Select(MainWindow.LastErrorStart, MainWindow.LastErrorEnd);
                MainWindow.Textbox.Selection.ClearAllProperties();
            }
        }

        public static void SelectText(int line, string text)
        {
            if (MainWindow.Textbox == null) return;
            
            var codeboxText =
                new TextRange(MainWindow.Textbox.Document.ContentStart, MainWindow.Textbox.Document.ContentEnd)
                .Text.Split('\n');

            var index = codeboxText[line - 1].IndexOf(text) + 2;
            for (var i = 0; i < line - 1; i++)
                index += codeboxText[i].Length + 3;

            MainWindow.LastErrorStart = MainWindow.Textbox.Document.ContentStart
                .GetPositionAtOffset(index);
            MainWindow.LastErrorEnd = MainWindow.Textbox.Document.ContentStart
                .GetPositionAtOffset(index + text.Length);

            MainWindow.Textbox.Selection.Select(MainWindow.LastErrorStart, MainWindow.LastErrorEnd);
            MainWindow.Textbox.Selection.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Blue));
            MainWindow.Textbox.Selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
        }
    }
}
