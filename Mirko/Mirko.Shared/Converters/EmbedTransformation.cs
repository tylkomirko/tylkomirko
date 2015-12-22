using FFImageLoading.Work;
using Mirko.Utils;
using Mirko.ViewModel;
using System;
using System.Collections.Generic;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class EmbedTransformation : IValueConverter
    {
        private static BlitTransformation GIFTransformation = new BlitTransformation(ImageOverlay.GIF);
        private static BlitTransformation VideoTransformation = new BlitTransformation(ImageOverlay.Video);

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var VM = value as EmbedViewModel;
            if (VM == null || VM.EmbedData == null) return null;

            var sourceURL = VM.EmbedData.Source;
            if (sourceURL.EndsWith(".gif") || sourceURL.Contains("gfycat"))
                return new List<ITransformation>() { GIFTransformation };
            else if (sourceURL.Contains("youtube") || sourceURL.Contains("youtu.be"))
                return new List<ITransformation>() { VideoTransformation };

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
