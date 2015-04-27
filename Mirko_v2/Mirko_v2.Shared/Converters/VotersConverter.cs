using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using WykopAPI.Models;

namespace Mirko.Converters
{
    public class VotersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string str)
        {
            var voters = value as ObservableCollection<Voter>;
            string result = string.Empty;

            if (voters.Count == 1)
            {
                result = voters[0].AuthorName;
            }
            else if(voters.Count == 2)
            {
                result = voters[0].AuthorName + ", " + voters[1].AuthorName;
            }
            else if (voters.Count > 2)
            {
                var votersLeft = voters.Count - 2;
                result = voters[0].AuthorName + ", " + voters[1].AuthorName + " i " + votersLeft + " innych";
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string str)
        {
            throw new NotImplementedException();
        }
    }
}
