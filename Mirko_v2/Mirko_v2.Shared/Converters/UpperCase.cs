﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace Mirko.Converters
{
    public class UpperCase : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            if (value == null) return null;

            var input = value as string;
            return input.ToUpper();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }
}
