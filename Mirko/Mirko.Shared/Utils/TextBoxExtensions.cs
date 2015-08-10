using Windows.UI.Xaml.Controls;

namespace Mirko.Utils
{
    // thanks to http://stackoverflow.com/questions/15291600/
    public static class TextBoxExtension
    {
        public static int GetNormalizedSelectionStart(this TextBox textBox)
        {
            int occurences = 0;
            string source = textBox.Text;
            int maxIndex = source.Length;
            int start = textBox.SelectionStart;

            if (start > source.Length)
                start = source.Length;

            for (var index = 0; index < start + occurences; index++)
            {
                if (index >= source.Length)
                    break;

                if (source[index] == '\r' && source[index + 1] == '\n')
                    occurences++;
            }

            var result = start + occurences;
            if (result > source.Length)
                result = source.Length;

            return result;
        }
    }
}
