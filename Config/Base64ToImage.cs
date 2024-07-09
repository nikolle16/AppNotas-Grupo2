using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace App_Notas___Grupo_2.Config
{
    public class Base64ToImage : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            ImageSource imageSource = null;
            if (value != null)
            {
                string base64 = (string)value;
                byte[] fotobyte = System.Convert.FromBase64String(base64);
                var stream = new MemoryStream(fotobyte);

                imageSource = ImageSource.FromStream(() => stream);
            }

            return imageSource;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
