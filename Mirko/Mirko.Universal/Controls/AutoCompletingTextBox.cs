using GalaSoft.MvvmLight.Ioc;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Mirko.Controls
{
    public class AutoCompletingTextBox : TextBox
    {
        private bool HashtagDetected = false;
        private bool AtDetected = false; // '@'
        private static CacheViewModel Cache = null;
        
        public bool AreSuggestionsOpen
        {
            get { return (bool)GetValue(AreSuggestionsOpenProperty); }
            set { SetValue(AreSuggestionsOpenProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AreSuggestionsOpen.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AreSuggestionsOpenProperty =
            DependencyProperty.Register("AreSuggestionsOpen", typeof(bool), typeof(AutoCompletingTextBox), new PropertyMetadata(false));               

        public AutoCompletingTextBox()
        {
            if (Cache == null)
            {
                Cache = SimpleIoc.Default.GetInstance<CacheViewModel>();
                if (Cache.PopularHashtags.Count == 0)
                    Cache.GetPopularHashtags();
                if (Cache.ObservedUsers.Count == 0)
                    Cache.GetObservedUsers();
            }

            base.TextChanged += AutoCompletingTextBox_TextChanged;
        }

        private async void AutoCompletingTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // extract word around SelectionStart
            string currentWord = GetWordAtPointer();

            Debug.WriteLine("currentWord: " + currentWord);

            PopupMenu popup = null;
            if(currentWord.StartsWith("#") || currentWord.StartsWith("@"))
            {
                if (currentWord.StartsWith("#"))
                    HashtagDetected = true;
                else
                    AtDetected = true;

                Cache.GenerateSuggestions(currentWord, HashtagDetected);

                popup = CreatePopup();                
            }
            else if(currentWord.StartsWith("") && (HashtagDetected || AtDetected))
            {
                HashtagDetected = false;
                AtDetected = false;
            }

            if (popup != null)
            {
                try
                {
                    await popup.ShowForSelectionAsync(new Rect(GetPopupLocation(), new Size(0, 0)), Placement.Below);
                } catch (Exception) { }
            }
        }

        private Point GetPopupLocation()
        {
            Rect rect;
            if (SelectionStart == Text.Length)
                rect = GetRectFromCharacterIndex(SelectionStart - 1, true);
            else
                rect = GetRectFromCharacterIndex(SelectionStart, false);

            GeneralTransform buttonTransform = TransformToVisual(null);
            Point point = buttonTransform.TransformPoint(new Point(rect.X, rect.Y));
            point.Y += App.IsMobile ? 30 : 50;
            return point;
        }

        private PopupMenu CreatePopup()
        {
            var pop = new PopupMenu();

            var suggestionsCount = App.IsMobile ? 4 : 6;
            var sugs = Cache.HashtagSuggestions.Take(suggestionsCount);
            foreach (var sug in sugs)
                pop.Commands.Add(new UICommand(sug, (c) => ReplaceWordAtPointer(c.Label + " ")));

            return pop;
        }

        private bool CharPredicate(char c)
        {
            bool isSeparator = char.IsSeparator(c) || c == '\r' || c == '\n';
            bool isPunctuation = c == '#' || c == '@' || !char.IsPunctuation(c);

            return !isSeparator && isPunctuation;
        }

        private string GetWordCharactersBefore()
        {
            var start = this.GetNormalizedSelectionStart();
            var backwards = Text.Substring(0, start);
            var wordCharactersBeforePointer = new string(backwards.Reverse().TakeWhile(c => CharPredicate(c)).Reverse().ToArray());

            return wordCharactersBeforePointer;
        }

        private string GetWordCharactersAfter()
        {
            var start = this.GetNormalizedSelectionStart();
            var fowards = Text.Substring(start, Text.Length - start);
            var wordCharactersAfterPointer = new string(fowards.TakeWhile(c => CharPredicate(c)).ToArray());

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
            var start = this.GetNormalizedSelectionStart();
            var idx = start - before.Length;

            // first, we have to remove current word.
            newText = newText.Remove(idx, before.Length);
            newText = newText.Remove(idx, after.Length);

            newText = newText.Insert(idx, replacementWord);

            try
            {
                this.Text = newText;
                this.SelectionStart = idx + before.Length + replacementWord.Length - after.Length;
            }
            catch (Exception e)
            {
                App.TelemetryClient.TrackException(e);
            }
        }
    }
}
