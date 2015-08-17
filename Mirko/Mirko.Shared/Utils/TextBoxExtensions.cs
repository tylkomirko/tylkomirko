using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

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

        // from ContextMenu UWP sample app
        public static Rect GetSelectionRect(this TextBox textbox)
        {
            Rect rectFirst, rectLast;
            if (textbox.SelectionStart == textbox.Text.Length)
            {
                rectFirst = textbox.GetRectFromCharacterIndex(textbox.SelectionStart - 1, true);
            }
            else
            {
                rectFirst = textbox.GetRectFromCharacterIndex(textbox.SelectionStart, false);
            }

            int lastIndex = textbox.SelectionStart + textbox.SelectionLength;
            if (lastIndex == textbox.Text.Length)
            {
                rectLast = textbox.GetRectFromCharacterIndex(lastIndex - 1, true);
            }
            else
            {
                rectLast = textbox.GetRectFromCharacterIndex(lastIndex, false);
            }

            GeneralTransform buttonTransform = textbox.TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point());

            // Make sure that we return a valid rect if selection is on multiple lines
            // and end of the selection is to the left of the start of the selection.
            double x, y, dx, dy;
            y = point.Y + rectFirst.Top;
            dy = rectLast.Bottom - rectFirst.Top;
            if (rectLast.Right > rectFirst.Left)
            {
                x = point.X + rectFirst.Left;
                dx = rectLast.Right - rectFirst.Left;
            }
            else
            {
                x = point.X + rectLast.Right;
                dx = rectFirst.Left - rectLast.Right;
            }

            return new Rect(x, dx, y, dy);
        }
    }
}
