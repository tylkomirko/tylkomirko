using Mirko_v2.Controls;
using Mirko_v2.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using WykopAPI;
using WykopAPI.JSON;
using WykopAPI.Models;

namespace Mirko_v2.Utils
{
    public class Properties : DependencyObject
    {
        #region HTML
        public static void SetHtml(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlProperty, value);
        }

        public static string GetHtml(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlProperty);
        }

        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.RegisterAttached("Html", typeof(string), typeof(Properties), new PropertyMetadata(null, HtmlChanged));

        private static void HtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var richText = d as RichTextBlock;
            if (richText == null) return;

            //Generate blocks
            string xhtml = e.NewValue as string;
            if (xhtml == null)
            {
                richText.Blocks.Clear();
                return;
            }

            var parsed = WypokHTMLParser.Parse(xhtml);
            var block = GenerateBlock(parsed);

            //Add the blocks to the RichTextBlock
            richText.Blocks.Clear();
            richText.Blocks.Add(block);
        }

        private static Block GenerateBlock(List<ParserToken> parsed)
        {
            Paragraph p = new Paragraph();

            foreach (var item in parsed)
            {
                Inline i = GenerateBlockForNode(item);

                if (i != null)
                    p.Inlines.Add(i);
            }

            return p;
        }

        private static Inline GenerateBlockForNode(ParserToken item, bool insideSpoiler = false)
        {
            switch (item.Type)
            {
                case TokenTypes.TEXT:
                    return GenerateText(item.Value);
                case TokenTypes.STRONG:
                    return GenerateStrong(item.Internal);
                case TokenTypes.EM:
                    return GenerateEm(item.Internal);

                case TokenTypes.HASHTAG:
                    return GenerateHashtag(item.Value, insideSpoiler);
                case TokenTypes.USERPROFILE:
                    return GenerateUserprofile(item.Value, insideSpoiler);
                case TokenTypes.HYPERLINK:
                    return GenerateHyperlink(item);

                case TokenTypes.SPOILER:
                    return GenerateSpoiler(item.Internal);
                case TokenTypes.CODE:
                    return GenerateCode(item.Internal);
                case TokenTypes.CITE:
                    return GenerateCite(item.Internal);

                default:
                    return null;
            }
        }

        private static Inline GenerateCite(List<ParserToken> list)
        {
            Span s = new Span()
            {
                Foreground = Application.Current.Resources["QuoteForeground"] as SolidColorBrush,
                FontStyle = Windows.UI.Text.FontStyle.Italic,
            };

            foreach (var item in list)
            {
                var i = GenerateBlockForNode(item);
                if (i != null)
                    s.Inlines.Add(i);
            }

            return s;
        }

        private static Inline GenerateCode(List<ParserToken> list)
        {
            Span s = new Span()
            {
                Foreground = new SolidColorBrush(Colors.DarkGray),
                FontFamily = new FontFamily("Consolas"),
            };

            foreach (var item in list)
            {
                var i = GenerateBlockForNode(item);
                if (i != null)
                    s.Inlines.Add(i);
            }

            return s;
        }

        private static Inline GenerateSpoiler(List<ParserToken> list)
        {
            Span s = new Span();

            var spoilerContent = new List<Inline>(list.Count-1);
            foreach (var item in list)
            {
                var i = GenerateBlockForNode(item, true);
                if (i != null)
                    spoilerContent.Add(i);
            }

            InlineUIContainer container = new InlineUIContainer();
            var tb = new SpoilerTextBlock()
            {
                HiddenContent = spoilerContent,
            };

            container.Child = tb;

            s.Inlines.Add(container);
            return s;
        }

        private static Inline GenerateHyperlink(ParserToken token)
        {
            Span s = new Span();

            var tb = new HyperlinkTextBlock()
            {
                VisibleText = token.Internal[0].Value,
                URL = token.Value,
            };

            InlineUIContainer container = new InlineUIContainer() { Child = tb };

            s.Inlines.Add(container);
            return s;
        }

        private static Inline GenerateUserprofile(string p, bool insideSpoiler = false)
        {
            Span s = new Span();
            InlineUIContainer container = new InlineUIContainer();

            SolidColorBrush brush = null;
            if (insideSpoiler)
                brush = Application.Current.Resources["UsernameSpoilerForeground"] as SolidColorBrush;
            else
                brush = Application.Current.Resources["UsernameForeground"] as SolidColorBrush;

            TextBlock textBlock = new TextBlock()
            {
                Text = p,
                Foreground = brush,
                IsTapEnabled = true,
            };

            textBlock.Tapped += Userprofile_Tapped;
            container.Child = textBlock;

            s.Inlines.Add(container);
            return s;
        }

        private static Inline GenerateHashtag(string p, bool insideSpoiler = false)
        {
            var s = new Span();
            var container = new InlineUIContainer();

            SolidColorBrush brush = null;
            if(insideSpoiler)
                brush = Application.Current.Resources["HashtagSpoilerForeground"] as SolidColorBrush;
            else
                brush = Application.Current.Resources["HashtagForeground"] as SolidColorBrush;

            var textBlock = new TextBlock()
            {
                Text = p,
                Foreground = brush,
                IsTapEnabled = true
            };

            textBlock.Tapped += Hashtag_Tapped;
            container.Child = textBlock;

            s.Inlines.Add(new Run() { Text = "\u200B" });
            s.Inlines.Add(container);
            return s;
        }

        private static Inline GenerateEm(List<ParserToken> list)
        {
            Span s = new Span()
            {
                FontStyle = FontStyle.Italic,
            };

            foreach (var item in list)
            {
                var i = GenerateBlockForNode(item);
                if (i != null)
                    s.Inlines.Add(i);
            }

            return s;
        }

        private static Inline GenerateStrong(List<ParserToken> list)
        {
            Span s = new Span() 
            {
                FontWeight = FontWeights.Bold,
            };

            foreach (var item in list)
            {
                var i = GenerateBlockForNode(item);
                if (i != null)
                    s.Inlines.Add(i);
            }

            return s;
        }

        private static Inline GenerateText(string p)
        {
            Span s = new Span();
            Run r = new Run() { Text = p };

            s.Inlines.Add(r);
            return s;
        }

        /*
        private static Paragraph GenerateMoreTextInfo()
        {
            Paragraph p = new Paragraph();
            Span s = new Span();
            Run r = new Run()
            {
                Text = "Tap for more text...",
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Gray),
            };
            s.Inlines.Add(r);
            p.Inlines.Add(s);
            return p;
        }
         * */

        static void Userprofile_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            var tb = sender as TextBlock;
            var control = tb.GetAntecedent<Controls.Entry>();
            if (control == null) return;

            var username = tb.Text.Remove(0, 1); // skip '@'
            control.ProfileTapped(username);
        }

        static void Hashtag_Tapped(object sender, TappedRoutedEventArgs e)
        {
            e.Handled = true;

            var tb = sender as TextBlock;
            var control = tb.GetAntecedent<Controls.Entry>();
            if (control == null) return;

            var tag = tb.Text;
            control.HashtagTapped(tag, tb);
        }
        #endregion HTML

        #region Notification text
        public static Notification GetNotification(DependencyObject obj)
        {
            return (Notification)obj.GetValue(NotificationProperty);
        }

        public static void SetNotification(DependencyObject obj, Notification value)
        {
            obj.SetValue(NotificationProperty, value);
        }

        // Using a DependencyProperty as the backing store for Notification.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NotificationProperty =
            DependencyProperty.RegisterAttached("Notification", typeof(Notification), typeof(Properties), new PropertyMetadata(null, NotificationChanged));

        private static void NotificationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RichTextBlock richText = d as RichTextBlock;
            if (richText == null) return;

            var notification = e.NewValue as Notification;
            if (notification == null) return;

            Paragraph paragraph;

            if (notification.Type == NotificationType.EntryDirected || notification.Type == NotificationType.CommentDirected)
                paragraph = entry_comment_directed_parse(notification);
            else if (notification.Type == NotificationType.Observe)
                paragraph = observe_parse(notification);
            else if (notification.Type == NotificationType.Badge)
                paragraph = badge_parse(notification);
            else
                return;

            richText.Blocks.Clear();
            richText.Blocks.Add(paragraph);
        }

        private static Paragraph entry_comment_directed_parse(Notification n)
        {
            var p = new Paragraph();
            var groupConverter = Application.Current.Resources["GroupToColor"] as IValueConverter;
            var sexConverter = Application.Current.Resources["SexToColor"] as IValueConverter;
            var sexToText = Application.Current.Resources["SexToText"] as IValueConverter;

            var username = new Span();
            username.Inlines.Add(new Run()
            {
                Text = sexToText.Convert(n.AuthorSex, null, null, CultureInfo.CurrentCulture.ToString()) as string + " ",
                FontSize = 18.0,
                Foreground = sexConverter.Convert(n.AuthorSex, null, null, CultureInfo.CurrentCulture.ToString()) as SolidColorBrush,
            });

            username.Inlines.Add(new Run() 
            {
                Text = n.AuthorName,
                FontWeight = FontWeights.Bold,
                Foreground = groupConverter.Convert(n.AuthorGroup, null, null, CultureInfo.CurrentCulture.ToString()) as SolidColorBrush,
            });

            var body = new Span();
            body.Inlines.Add(new Run()
            {
                Text = n.AuthorSex == UserSex.Female ? " napisała do Ciebie w " : " napisał do Ciebie w ",
                Foreground = new SolidColorBrush(Colors.Gray),
            });

            var splitters = new string[] { "napisał do Ciebie w komentarzu do", "napisała do Ciebie w komentarzu do", "napisał do Ciebie w", "napisała do Ciebie w" };
            var splitted = n.Text.Split(splitters, StringSplitOptions.RemoveEmptyEntries);
            if (splitted.Count() != 2) return null;
            var txt = splitted[1].Replace(@"""", "").Replace("\n\n", " ").Replace("\n", " ").Trim();

            var rest = new Span();
            var color = n.IsNew ? Colors.White : Colors.Gray;
            rest.Inlines.Add(new Run()
            {
                Text = txt,
                Foreground = new SolidColorBrush(color),
            });

            p.Inlines.Add(username);
            p.Inlines.Add(body);
            p.Inlines.Add(rest);

            return p;
        }

        private static Paragraph observe_parse(Notification n)
        {
            var p = new Paragraph();
            var groupConverter = Application.Current.Resources["GroupToColor"] as IValueConverter;
            var sexConverter = Application.Current.Resources["SexToColor"] as IValueConverter;
            var sexToText = Application.Current.Resources["SexToText"] as IValueConverter;

            var username = new Span();
            username.Inlines.Add(new Run()
            {
                Text = sexToText.Convert(n.AuthorSex, null, null, CultureInfo.CurrentCulture.ToString()) as string + " ",
                FontSize = 18.0,
                Foreground = sexConverter.Convert(n.AuthorSex, null, null, CultureInfo.CurrentCulture.ToString()) as SolidColorBrush,
            });

            username.Inlines.Add(new Run()
            {
                Text = n.AuthorName,
                FontWeight = FontWeights.Bold,
                Foreground = groupConverter.Convert(n.AuthorGroup, null, null, CultureInfo.CurrentCulture.ToString()) as SolidColorBrush,
            });

            var body = new Span();
            body.Inlines.Add(new Run()
            {
                Text = " obserwuje Cię.",
                Foreground = new SolidColorBrush(Colors.Gray),
            });

            p.Inlines.Add(username);
            p.Inlines.Add(body);

            return p;
        }

        private static Paragraph badge_parse(Notification n)
        {
            var p = new Paragraph();

            var index = n.Text.IndexOf(' ');
            var txt = n.Text.Substring(index + 1);

            var body = new Span();
            body.Inlines.Add(new Run()
            {
                Text = txt,
                Foreground = new SolidColorBrush(Colors.Gray),
            });

            p.Inlines.Add(body);

            return p;
        }

        #endregion

        #region Voters
        public static ObservableCollection<string> GetVoters(DependencyObject obj)
        {
            return (ObservableCollection<string>)obj.GetValue(VotersProperty);
        }

        public static void SetVoters(DependencyObject obj, ObservableCollection<string> value)
        {
            obj.SetValue(VotersProperty, value);
        }

        // Using a DependencyProperty as the backing store for Voters.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty VotersProperty =
            DependencyProperty.RegisterAttached("Voters", typeof(ObservableCollection<string>), typeof(Properties), new PropertyMetadata(null, VotersChanged));

        private static void VotersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var votersCollection = e.NewValue as ObservableCollection<string>;
            if (votersCollection == null) return;

            var blocks = (d as RichTextBlock).Blocks;
            blocks.Clear();

            if (votersCollection.Count == 0) return;

            var header = new Paragraph() { LineHeight = 8 };
            header.Inlines.Add(new Run() { Text = "\u2015\u2015", Foreground = new SolidColorBrush(Colors.Gray), FontSize = 13.5 });
            blocks.Add(header);

            var voters = new Paragraph();

            var count = votersCollection.Count;
            for (int i = 0; i < count; i++)
            {
                var voter = votersCollection[i];

                var voterInline = new Run() { Text = voter, Foreground = new SolidColorBrush(Colors.Gray), FontSize = 13.5 };
                voters.Inlines.Add(voterInline);

                if (i < count - 1)
                {
                    var separatorInline = new Run() { Text = ", ", Foreground = new SolidColorBrush(Colors.Gray), FontSize = 13.5 };
                    voters.Inlines.Add(separatorInline);
                }
            }

            blocks.Add(voters);
        }        
        #endregion
    }
}
