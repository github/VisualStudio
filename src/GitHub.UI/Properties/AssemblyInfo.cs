using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Markup;

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page, 
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page, 
                                              // app, or any theme specific resource dictionaries)
    )]

[assembly: XmlnsDefinition("https://github.com/github/VisualStudio", "GitHub.Helpers")]
[assembly: XmlnsDefinition("https://github.com/github/VisualStudio", "GitHub.UI")]
[assembly: XmlnsDefinition("https://github.com/github/VisualStudio", "GitHub.UI.Controls")]
[assembly: XmlnsDefinition("https://github.com/github/VisualStudio", "GitHub.UI.Converters")]
[assembly: XmlnsDefinition("https://github.com/github/VisualStudio", "GitHub.UI.Helpers")]
[assembly: XmlnsDefinition("https://github.com/github/VisualStudio", "GitHub.UI.TestAutomation")]
