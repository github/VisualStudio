using System;
using System.Diagnostics;
using System.Globalization;

namespace GitHub.UI
{
    public class StringConverter : ValueConverterMarkupExtension<StringConverter>
    {
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;
            if (String.IsNullOrEmpty(text)) return null;

            var conversionType = GetConversionTypeFromParameter(parameter);

            switch (conversionType)
            {
                case StringConverterType.LowerCase:
                    return text.ToLower(culture);
                case StringConverterType.UpperCase:
                    return text.ToUpper(culture);
                default:
                    return text;
            }
        }

        private static StringConverterType GetConversionTypeFromParameter(object parameter)
        {
            if (parameter == null)
            {
                return StringConverterType.None;
            }

            if (parameter is StringConverterType)
            {
                return (StringConverterType)parameter;
            }

            var parameterAsString = parameter as string;
            if (parameterAsString != null)
            {
                StringConverterType conversionType;

                if (Enum.TryParse(parameterAsString, ignoreCase: true, result: out conversionType))
                    return conversionType;
            }

            return StringConverterType.None;
        }
    }

    public enum StringConverterType
    {
        None,
        LowerCase,
        UpperCase,
        // Note: Potential options for the future: SentenceCase, TitleCase, PascalCase, CamelCase, SplitPascalCase
    }
}
