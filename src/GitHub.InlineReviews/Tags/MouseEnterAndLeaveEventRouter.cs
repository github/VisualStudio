using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GitHub.InlineReviews.Tags
{
    class MouseEnterAndLeaveEventRouter<T> where T : FrameworkElement
    {
        T previousMouseOverElement;

        public void Add(UIElement sourceElement, UIElement targetElement)
        {
            sourceElement.MouseMove += (t, e) => MouseMove(targetElement, e);
            sourceElement.MouseLeave += (t, e) => MouseLeave(targetElement, e);
        }

        void MouseMove(object target, MouseEventArgs e)
        {
            T mouseOverElement = null;
            Action<T> visitAction = element =>
            {
                mouseOverElement = element;
            };

            var visitor = new Visitor(e, visitAction);
            visitor.Visit(target);

            if (mouseOverElement != previousMouseOverElement)
            {
                MouseLeave(previousMouseOverElement, e);
                MouseEnter(mouseOverElement, e);
            }
        }

        void MouseLeave(object target, MouseEventArgs e)
        {
            MouseLeave(previousMouseOverElement, e);
        }

        void MouseEnter(T element, MouseEventArgs e)
        {
            element?.RaiseEvent(new MouseEventArgs(e.MouseDevice, e.Timestamp)
            {
                RoutedEvent = Mouse.MouseEnterEvent,
            });

            previousMouseOverElement = element;
        }

        void MouseLeave(T element, MouseEventArgs e)
        {
            element?.RaiseEvent(new MouseEventArgs(e.MouseDevice, e.Timestamp)
            {
                RoutedEvent = Mouse.MouseLeaveEvent,
            });

            previousMouseOverElement = null;
        }

        class Visitor
        {
            MouseEventArgs mouseEventArgs;
            Action<T> action;

            internal Visitor(MouseEventArgs mouseEventArgs, Action<T> action)
            {
                this.mouseEventArgs = mouseEventArgs;
                this.action = action;
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
                    action(element);
                }
            }
        }
    }
}
