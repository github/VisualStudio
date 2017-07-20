using System;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using GitHub.UI.Helpers;

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

            DynamicResources = new ResourceDictionary();
            MergeResources(DynamicResources, "pack://application:,,,/GitHub.VisualStudio.UI;component/SharedDictionary.xaml");
            MergeResources(DynamicResources, "pack://application:,,,/GitHub.UI;component/SharedDictionary.xaml");
            MergeResources(DynamicResources, "pack://application:,,,/GitHub.UI.Reactive;component/SharedDictionary.xaml");
            UpdateResources(DynamicResources);

            editorFormatMap.FormatMappingChanged += OnFormatMappingChanged;
        }

        void OnFormatMappingChanged(object sender, FormatItemsEventArgs e)
        {
            UpdateResources(DynamicResources);
        }

        void UpdateResources(ResourceDictionary resources)
        {
            resources["DiffChangeBackground.Add"] = GetBackground(addProperties);
            resources["DiffChangeBackground.Delete"] = GetBackground(deleteProperties);
            resources["DiffChangeBackground.None"] = GetBackground(noneProperties);
        }

        static void MergeResources(ResourceDictionary resources, string url)
        {
            var sharedResources = new SharedDictionaryManager();
            sharedResources.Source = new Uri(url);
            resources.MergedDictionaries.Add(sharedResources);
        }

        static Brush GetBackground(ResourceDictionary dictionary)
        {
            return dictionary["Background"] as Brush;
        }

        internal ResourceDictionary DynamicResources { get; }
    }
}
