using System;
using System.ComponentModel;
using System.Globalization;

namespace GitHub.Primitives
{
    public class UriStringConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string);
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) => new UriString((string)value);
    }
}
