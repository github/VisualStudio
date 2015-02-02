using System;
using System.IO;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GitHub.VisualStudio.Helpers
{
	public static class Images
	{
		public static RenderTargetBitmap Render(this UIElement element, Brush background, double width, double height, Rect padding)
		{
			var imageWidth = width - padding.Left - padding.Width;
			var imageHeight = height - padding.Top - padding.Height;
			Rectangle rect = PaintControl(element, imageWidth, imageHeight);

			// Default dpi settings
			const double dpiX = 96;
			const double dpiY = 96;

			var renderTarget = new RenderTargetBitmap(
				(int)width, (int)height, dpiX, dpiY, PixelFormats.Pbgra32);

			DrawingVisual drawingVisual = new DrawingVisual();
			using (DrawingContext drawingContext = drawingVisual.RenderOpen()) {
				var r = new Rect(new Point(), new Size(width, height));
				drawingContext.DrawRectangle(background, null, r);
				r = new Rect(new Point(padding.Left, padding.Top), new Size(imageWidth, imageHeight));
				VisualBrush visualBrush = new VisualBrush(rect);
				visualBrush.AlignmentX = AlignmentX.Center;
				visualBrush.AlignmentY = AlignmentY.Top;
				visualBrush.Stretch = Stretch.None;
				drawingContext.DrawRectangle(visualBrush, null, r);
			}
			renderTarget.Render(drawingVisual);

			return renderTarget;

		}

		/// <summary>
		/// Render a Visual to a render target of a fixed size. The visual is
		/// scaled uniformly to fit inside the specified size.
		/// </summary>
		/// <param name="visual"></param>
		/// <param name="height"></param>
		/// <param name="width"></param>
		/// <returns></returns>
		private static RenderTargetBitmap RenderVisual(Visual visual, double height, double width)
		{
			// Default dpi settings
			const double dpiX = 96;
			const double dpiY = 96;

			// We can only render UIElements...ContentPrensenter to 
			// the rescue!
			var presenter = new ContentPresenter { Content = visual };

			// Ensure the final visual is of the known size by creating a viewbox 
			// and adding the visual as its child.
			var viewbox = new Viewbox {
				MaxWidth = width,
				MaxHeight = height,
				Stretch = Stretch.Uniform,
				Child = presenter
			};

			// Force the viewbox to re-size otherwise we wont see anything.
			var sFinal = new Size(viewbox.MaxWidth, viewbox.MaxHeight);
			viewbox.Measure(sFinal);
			viewbox.Arrange(new Rect(sFinal));
			viewbox.UpdateLayout();

			// Render the final visual to a render target 
			var renderTarget = new RenderTargetBitmap(
				(int)width, (int)height, dpiX, dpiY, PixelFormats.Pbgra32);
			renderTarget.Render(viewbox);

			// Return the render taget with the visual rendered on it.
			return renderTarget;
		}

        /// <summary>
		/// Paints a control onto a rectangle. Gets around problems where
		/// the control maybe a child of another element or have a funny
		/// offset.
		/// </summary>
		/// <param name="control"></param>
		/// <returns></returns>
		private static Rectangle PaintControl(UIElement control, double width, double height)
        {

            // Fill a rectangle with the illustration.
            var rect = new Rectangle
            {
                Fill = new VisualBrush(control) { TileMode = TileMode.None, Stretch = Stretch.Uniform },
                Width = width,
                Height = height
            };


            // Force the rectangle to re-size
            var szRect = new Size(rect.Width, rect.Height);
            rect.Measure(szRect);
            rect.Arrange(new Rect(szRect));
            rect.UpdateLayout();
            return rect;
        }
    }
}
