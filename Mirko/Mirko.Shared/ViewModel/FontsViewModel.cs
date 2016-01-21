using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace Mirko.ViewModel
{
    public class FontsViewModel : ViewModelBase
    {
        private static SettingsViewModel Settings = null;

        public FontsViewModel()
        {
            Settings = SimpleIoc.Default.GetInstance<SettingsViewModel>();
            Settings.PropertyChanged += Settings_PropertyChanged;
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "FontScaleFactor")
            {
                base.RaisePropertyChanged("EntryFontSize");
                base.RaisePropertyChanged("AuthorFontSize");
                base.RaisePropertyChanged("AuthorSexFontSize");
                base.RaisePropertyChanged("DateFontSize");
                base.RaisePropertyChanged("VoteFontSize");
                base.RaisePropertyChanged("CommentsBarFontSize");
                base.RaisePropertyChanged("EmbedSymbolFontSize");
                base.RaisePropertyChanged("AttachmentFontSize");
                base.RaisePropertyChanged("PMFontSize");
                base.RaisePropertyChanged("PMDateFontSize");
            }
        }

#if WINDOWS_PHONE_APP
        private const double SystemScaling = 1.0;
#else
        private const double SystemScaling = 0.8;
#endif

        private const double EntryBasicFontSize = 14.0 * SystemScaling;
        private const double AuthorBasicFontSize = 15.0 * SystemScaling;
        private const double AuthorSexBasicFontSize = 18.0 * SystemScaling;
        private const double DateBasicFontSize = 12.0 * SystemScaling;
        private const double VoteBasicFontSize = 15.0 * SystemScaling;
        private const double CommentsBarBasicFontSize = 13.0 * SystemScaling;
        private const double EmbedSymbolBasicFontSize = 16.0 * SystemScaling;
        private const double AttachmentBasicFontSize = 13.0 * SystemScaling;
        private const double PMBasicFontSize = 15.0 * SystemScaling;
        private const double PMDateBasicFontSize = 12.5 * SystemScaling;
        private const double SpoilerBasicFontSize = 13.0 * SystemScaling;

        public double EntryFontSize
        {
            get { return EntryBasicFontSize * Settings.FontScaleFactor; }
        }

        public double AuthorFontSize
        {
            get { return AuthorBasicFontSize * Settings.FontScaleFactor; }
        }

        public double AuthorSexFontSize
        {
            get { return AuthorSexBasicFontSize * Settings.FontScaleFactor; }
        }

        public double DateFontSize
        {
            get { return DateBasicFontSize * Settings.FontScaleFactor; }
        }

        public double VoteFontSize
        {
            get { return VoteBasicFontSize * Settings.FontScaleFactor; }
        }

        public double CommentsBarFontSize
        {
            get { return CommentsBarBasicFontSize * Settings.FontScaleFactor; }
        }

        public double EmbedSymbolFontSize
        {
            get { return EmbedSymbolBasicFontSize * Settings.FontScaleFactor; }
        }

        public double AttachmentFontSize
        {
            get { return AttachmentBasicFontSize * Settings.FontScaleFactor; }
        }

        public double PMFontSize
        {
            get { return PMBasicFontSize * Settings.FontScaleFactor; }
        }

        public double PMDateFontSize
        {
            get { return PMDateBasicFontSize * Settings.FontScaleFactor; }
        }

        public double SpoilerFontSize
        {
            get { return SpoilerBasicFontSize * Settings.FontScaleFactor; }
        }
    }
}
