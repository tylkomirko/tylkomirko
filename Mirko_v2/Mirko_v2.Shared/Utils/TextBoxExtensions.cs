using Windows.UI.Xaml.Controls;

namespace Mirko_v2.Utils
{
    // thanks to http://stackoverflow.com/questions/15291600/
    public static class TextBoxExtension
    {
        public static int GetNormalizedSelectionStart(this TextBox textBox)
        {
            int occurences = 0;
            string source = textBox.Text;

            for (var index = 0; index < textBox.SelectionStart + occurences; index++)
            {
                if (source[index] == '\r' && source[index + 1] == '\n')
                    occurences++;
            }
            return textBox.SelectionStart + occurences;
        }
    }
}
