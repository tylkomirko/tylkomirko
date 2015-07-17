using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace Mirko_v2.Controls
{
    public class AutoCompletingTextBox : TextBox
    {
        private bool HashtagDetected = false;
        
        public Popup SuggestionsPopup
        {
            get { return (Popup)GetValue(SuggestionsPopupProperty); }
            set { SetValue(SuggestionsPopupProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SuggestionsPopup.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuggestionsPopupProperty =
            DependencyProperty.Register("SuggestionsPopup", typeof(Popup), typeof(AutoCompletingTextBox), new PropertyMetadata(null, SuggestionsPopupChanged));

        private static void SuggestionsPopupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = d as AutoCompletingTextBox;

            c.SuggestionsPopup.Width = Window.Current.Bounds.Width;
            var content = c.SuggestionsPopup.Child as SuggestionsPopupContent;
            content.Width = Window.Current.Bounds.Width;

            var border = content.Content as Border;
            var lv = border.Child as ListView;
            lv.ItemClick += (s, args) =>
            {
                var hashtag = args.ClickedItem as string;
                c.ReplaceWordAtPointer(hashtag);
            };
        }


        public AutoCompletingTextBox()
        {
            Windows.UI.ViewManagement.InputPane.GetForCurrentView().Showing += (s, args) =>
            {
                if (SuggestionsPopup != null && SuggestionsPopup.VerticalOffset == 0)
                {
                    var screen = Window.Current.Bounds;
                    var app = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().VisibleBounds;

                    var appBarHeight = screen.Bottom - app.Bottom;
                    var statusBarHeight = app.Top - screen.Top;

                    SuggestionsPopup.VerticalOffset = screen.Height - args.OccludedRect.Height - statusBarHeight - appBarHeight;
                }
            };

            Windows.UI.ViewManagement.InputPane.GetForCurrentView().Hiding += (s, args) =>
            {
                if (SuggestionsPopup != null && SuggestionsPopup.IsOpen)
                    SuggestionsPopup.IsOpen = false;
            };

            base.TextChanged += AutoCompletingTextBox_TextChanged;
        }

        private void AutoCompletingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // extract word around SelectionStart
            string currentWord = GetWordAtPointer();

            Debug.WriteLine("currentWord: " + currentWord);

            if(currentWord.StartsWith("#"))
            {
                HashtagDetected = true;
                this.IsTextPredictionEnabled = false;
                SuggestionsPopup.IsOpen = true;

                this.IsEnabled = false;
                this.IsEnabled = true;
                this.Focus(FocusState.Programmatic);
            }
            else if(currentWord.StartsWith("") && HashtagDetected)
            {
                this.IsTextPredictionEnabled = true;
                SuggestionsPopup.IsOpen = false;
                HashtagDetected = false;

                this.IsEnabled = false;
                this.IsEnabled = true;
                this.Focus(FocusState.Programmatic);
            }
        }

        private string GetWordCharactersBefore()
        {
            var backwards = Text.Substring(0, SelectionStart);
            var wordCharactersBeforePointer = new string(backwards.Reverse().TakeWhile(c => !char.IsSeparator(c) && (c == '#' || !char.IsPunctuation(c))).Reverse().ToArray());

            return wordCharactersBeforePointer;
        }

        private string GetWordCharactersAfter()
        {
            var fowards = Text.Substring(SelectionStart, Text.Length - SelectionStart);
            var wordCharactersAfterPointer = new string(fowards.TakeWhile(c => !char.IsSeparator(c) && (c == '#' || !char.IsPunctuation(c))).ToArray());

            return wordCharactersAfterPointer;
        }

        private string GetWordAtPointer()
        {
            return string.Join(string.Empty, GetWordCharactersBefore(), GetWordCharactersAfter());
        }
        
        private void ReplaceWordAtPointer(string replacementWord)
        {
            var newText = this.Text;
            var before = GetWordCharactersBefore();
            var after = GetWordCharactersAfter();
            var idx = SelectionStart;

            // first, we have to remove current word.
            newText = newText.Remove(SelectionStart - before.Length, before.Length);
            if(after.Length > 0)
                newText = newText.Remove(SelectionStart, after.Length);

            newText = newText.Insert(SelectionStart - before.Length, replacementWord);

            this.Text = newText;
            this.SelectionStart = idx + replacementWord.Length - after.Length;
        }
    }
}
