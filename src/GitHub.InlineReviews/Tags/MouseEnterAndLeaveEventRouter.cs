using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitHub.InlineReviews.Tags
{
    class MouseEnterAndLeaveEventRouter
    {
        public void MouseMove<T>(object target, MouseEventArgs e) where T : FrameworkElement
        {
            var visitor = new Visitor<T>(this, e);
            visitor.Visit(target);
        }

        FrameworkElement MouseOverElement { get; set; }

        class Visitor<T> where T : FrameworkElement
        {
            MouseEnterAndLeaveEventRouter router;
            MouseEventArgs mouseEventArgs;

            internal Visitor(MouseEnterAndLeaveEventRouter router, MouseEventArgs mouseEventArgs)
            {
                this.router = router;
                this.mouseEventArgs = mouseEventArgs;
            }

            internal void Visit(object obj)
            {
                if (obj is Panel)
                {
                    Visit((Panel)obj);
                    return;
                }

                if (obj is T)
                {
                    Visit((T)obj);
                    return;
                }
            }

            internal void Visit(Panel panel)
            {
                foreach (var child in panel.Children)
                {
                    Visit(child);
                }
            }

            internal void Visit(T element)
            {
                var point = mouseEventArgs.GetPosition(element);
                if (point.Y >= 0 && point.Y < element.ActualHeight)
                {
                    if (element != router.MouseOverElement)
                    {
                        if (router.MouseOverElement != null)
                        {
                            router.MouseOverElement.RaiseEvent(new MouseEventArgs(mouseEventArgs.MouseDevice, mouseEventArgs.Timestamp)
                            {
                                RoutedEvent = Mouse.MouseLeaveEvent,
                            });
                        }

                        router.MouseOverElement = element;
                        router.MouseOverElement.RaiseEvent(new MouseEventArgs(mouseEventArgs.MouseDevice, mouseEventArgs.Timestamp)
                        {
                            RoutedEvent = Mouse.MouseEnterEvent,
                        });
                    }
                }
            }
        }
    }
}
