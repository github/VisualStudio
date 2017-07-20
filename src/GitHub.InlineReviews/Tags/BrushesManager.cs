using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;

namespace GitHub.InlineReviews.Tags
{
    class BrushesManager
    {
        const string AddPropertiesKey = "deltadiff.add.word";
        const string DeletePropertiesKey = "deltadiff.remove.word";
        const string NonePropertiesKey = "Indicator Margin";

        readonly ResourceDictionary addProperties;
        readonly ResourceDictionary deleteProperties;
        readonly ResourceDictionary noneProperties;

        internal BrushesManager(IEditorFormatMap editorFormatMap)
        {
            addProperties = editorFormatMap.GetProperties(AddPropertiesKey);
            deleteProperties = editorFormatMap.GetProperties(DeletePropertiesKey);
            noneProperties = editorFormatMap.GetProperties(NonePropertiesKey);

            Resources = new ResourceDictionary();
            UpdateResources();

            editorFormatMap.FormatMappingChanged += OnFormatMappingChanged;
        }

        void OnFormatMappingChanged(object sender, FormatItemsEventArgs e)
        {
            UpdateResources();
        }

        void UpdateResources()
        {
            Resources["DiffChangeBackground.Add"] = GetBackground(addProperties);
            Resources["DiffChangeBackground.Delete"] = GetBackground(deleteProperties);
            Resources["DiffChangeBackground.None"] = GetBackground(noneProperties);
        }

        static Brush GetBackground(ResourceDictionary dictionary)
        {
            return dictionary["Background"] as Brush;
        }

        internal ResourceDictionary Resources { get; }
    }
}
