using GalaSoft.MvvmLight.Ioc;
using Mirko_v2.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;

namespace Mirko_v2.Controls
{
    public class AutoCompletingTextBox : TextBox
    {
        private bool HashtagDetected = false;
        private Popup SuggestionsPopup = null;
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
                Cache = SimpleIoc.Default.GetInstance<CacheViewModel>();

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
                {
                    SuggestionsPopup.IsOpen = false;
                    AreSuggestionsOpen = false;
                }
            };

            base.Loaded += (s,e) =>
            {
                var navService = SimpleIoc.Default.GetInstance<GalaSoft.MvvmLight.Views.INavigationService>() 
                    as Mirko_v2.ViewModel.NavigationService;

                SuggestionsPopup = navService.GetPopup();

                SuggestionsPopup.Width = Window.Current.Bounds.Width;
                var content = SuggestionsPopup.Child as SuggestionsPopupContent;
                content.Width = Window.Current.Bounds.Width;

                var border = content.Content as Border;
                var lv = border.Child as ListView;
                lv.ItemClick += (se, args) =>
                {
                    var hashtag = args.ClickedItem as string;
                    ReplaceWordAtPointer(hashtag);
                };
                lv.SetBinding(ListView.ItemsSourceProperty, new Binding()
                {
                    Source = Cache,
                    Path = new PropertyPath("HashtagSuggestions"),
                });
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
                Cache.GenerateSuggestions(currentWord);

                HashtagDetected = true;
                this.IsTextPredictionEnabled = false;
                SuggestionsPopup.IsOpen = true;
                AreSuggestionsOpen = true;

                this.IsEnabled = false;
                this.IsEnabled = true;
                this.Focus(FocusState.Programmatic);
            }
            else if(currentWord.StartsWith("") && HashtagDetected)
            {
                this.IsTextPredictionEnabled = true;
                SuggestionsPopup.IsOpen = false;
                AreSuggestionsOpen = false;
                HashtagDetected = false;

                this.IsEnabled = false;
                this.IsEnabled = true;
                this.Focus(FocusState.Programmatic);
            }
        }

        private bool CharPredicate(char c)
        {
            bool isSeparator = char.IsSeparator(c) || c == '\r' || c == '\n';
            bool isPunctuation = c == '#' || !char.IsPunctuation(c);

            return !isSeparator && isPunctuation;
        }

        private string GetWordCharactersBefore()
        {
            var backwards = Text.Substring(0, SelectionStart);
            var wordCharactersBeforePointer = new string(backwards.Reverse().TakeWhile(c => CharPredicate(c)).Reverse().ToArray());

            return wordCharactersBeforePointer;
        }

        private string GetWordCharactersAfter()
        {
            var fowards = Text.Substring(SelectionStart, Text.Length - SelectionStart);
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
