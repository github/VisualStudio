using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.TeamFoundation.Controls;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using System.Windows.Media;
using GitHub.VisualStudio.Helpers;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell.Interop;
using System.Runtime.InteropServices;

namespace GitHub.VisualStudio
{
    [TeamExplorerNavigationItem(PulseNavigationItemId, 10, TargetPageId = TeamExplorerPageIds.Home)]
    class PulseNavigationItem : TeamExplorerNavigationItemBase
    {
        public const string PulseNavigationItemId = "5245767A-B657-4F8E-BFEE-F04159F1DDA2";

        [ImportingConstructor]
        public PulseNavigationItem([Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Text = "Pulse";
            IsVisible = true;
            IsEnabled = true;

            Image = UI.Controls.NavigationIcon.GetImage(ServiceProvider, GitHub.UI.Octicon.pulse);
        }

        /*
        void SetImage()
        {
            var page = new Page();
            var c = new Canvas();
            var fp = new GitHub.UI.FixedAspectRatioPanel();
            var v = new Viewbox();
            var i = new GitHub.UI.OcticonImage();

            v.Child = i;
            fp.Children.Add(v);
            c.Children.Add(fp);
            page.Content = c;

            page.Resources.Source = new Uri("pack://application:,,,/GitHub.UI;component/Assets/Controls.xaml");

            fp.HorizontalAlignment = HorizontalAlignment.Center;
            fp.VerticalAlignment = VerticalAlignment.Center;
            
            i.Icon = GitHub.UI.Octicon.pulse;
            i.Foreground = (Brush)Application.Current.Resources[VsBrushes.ToolWindowTextKey];
            i.Width = 32;
            i.Height = 32;
            i.SnapsToDevicePixels = true;

            RenderTargetBitmap source = page.Render(Brushes.Transparent, 32, 32, new Rect(0, 0, 0, 0));
            //Clipboard.SetImage(source);

            BitmapEncoder encoder = new PngBitmapEncoder();
            System.Drawing.Bitmap pg;
            using (System.IO.MemoryStream myStream = new System.IO.MemoryStream())
            {

                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(myStream);
                pg = new System.Drawing.Bitmap(32, 32);
                System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(pg);
                //
                // Background
                //
                gr.FillRectangle(System.Drawing.Brushes.Transparent, 0, 0, 32, 32);
                //
                gr.DrawImage(System.Drawing.Bitmap.FromStream(myStream), 0, 0);
            }

            //var icon = Microsoft.Internal.VisualStudio.PlatformUI.WpfPropertyValue.CreateBitmapObject(source);
            var img = new VsUIObject(pg);
            IVsImageService imgSvc = (IVsImageService)ServiceProvider.GetService(typeof(SVsImageService));
            if (imgSvc == null)
                return;
            imgSvc.Add("github_pulse", img);

            
            object imageAsObject;
            img.get_Data(out imageAsObject);
            Image = imageAsObject as System.Drawing.Image;
            
        }
        */
    }


}
