using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GitHub.VisualStudio.Helpers;
using System.Windows.Media;
using Microsoft.VisualStudio.Shell;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitHub.VisualStudio.UI.Controls
{
    public class NavigationIcon
    {
        public static int Width { get; set; }
        public static int Height { get; set; }

        static NavigationIcon()
        {
            Width = Height = 16;
        }

        public static System.Drawing.Image GetImage(IServiceProvider serviceProvider, GitHub.UI.Octicon icon)
        {
            IVsImageService imgSvc = (IVsImageService)serviceProvider.GetService(typeof(SVsImageService));
            if (imgSvc == null)
                return null;

            var img = imgSvc.Get(icon.ToString());
            if (img == null)
            {
                Render(serviceProvider, icon);
                img = imgSvc.Get(icon.ToString());
            }

            object imageAsObject;
            img.get_Data(out imageAsObject);
            return imageAsObject as System.Drawing.Image;
        }

        static void Render(IServiceProvider serviceProvider, GitHub.UI.Octicon icon)
        {
            var page = new Page();
            var c = new Canvas();
            var i = new GitHub.UI.OcticonImage();
            c.Children.Add(i);
            page.Content = c;

            i.Icon = icon;
            i.Foreground = (Brush)Application.Current.Resources[VsBrushes.ToolWindowBackgroundKey];
            i.Width = Width;
            i.Height = Height;
            page.Resources.Source = new Uri("pack://application:,,,/GitHub.UI;component/Assets/Controls.xaml");

            RenderTargetBitmap source = page.Render(Brushes.Transparent, Width, Height, new Rect(0, 0, 0, 0));

            BitmapEncoder encoder = new PngBitmapEncoder();
            System.Drawing.Bitmap pg;
            using (System.IO.MemoryStream myStream = new System.IO.MemoryStream())
            {
                encoder.Frames.Add(BitmapFrame.Create(source));
                encoder.Save(myStream);
                pg = new System.Drawing.Bitmap(Width, Height);
                System.Drawing.Graphics gr = System.Drawing.Graphics.FromImage(pg);
                gr.FillRectangle(System.Drawing.Brushes.Transparent, 0, 0, Width, Height);
                gr.DrawImage(System.Drawing.Image.FromStream(myStream), 0, 0);
            }

            var img = new VsUIObject(pg);
            IVsImageService imgSvc = (IVsImageService)serviceProvider.GetService(typeof(SVsImageService));
            if (imgSvc == null)
                return;
            imgSvc.Add(icon.ToString(), img);
        }

        class VsUIObject : IVsUIObject
        {
            object obj;
            public VsUIObject(object o)
            {
                obj = o;
            }

            public int Equals(IVsUIObject other, out bool areEqual)
            {
                object o;
                other.get_Data(out o);
                areEqual = obj == o;
                return 0;
            }

            public int get_Data(out object pVar)
            {
                pVar = obj;
                return 0;
            }

            public int get_Format(out uint pdwDataFormat)
            {
                throw new NotImplementedException();
            }

            public int get_Type(out string pTypeName)
            {
                throw new NotImplementedException();
            }
        }
    }


}
