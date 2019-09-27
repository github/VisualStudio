using System;
using Microsoft.VisualStudio.Imaging.Interop;

namespace GitHub.VisualStudio
{
    public class Images
    {
        internal const string ImageMonikerGuid = "{de556d01-f3bc-4521-abc3-3e04b537f9c9}";
        internal const int LogoId = 1;

        public static ImageMoniker Logo { get; } = new ImageMoniker { Guid = new Guid(ImageMonikerGuid), Id = LogoId };
    }
}
