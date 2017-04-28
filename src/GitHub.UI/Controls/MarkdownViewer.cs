using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace GitHub.UI
{
    public class MarkdownViewer : FlowDocumentScrollViewer
    {
        public static readonly DependencyProperty MarkdownProperty =
            DependencyProperty.Register("Markdown", typeof(Markdown), typeof(MarkdownViewer), new PropertyMetadata(null));

        public static readonly DependencyProperty RawContentProperty =
            DependencyProperty.Register("RawContent", typeof(string), typeof(MarkdownViewer), new PropertyMetadata(null, UpdateDocument));

        public Markdown Markdown
        {
            get { return (Markdown)GetValue(MarkdownProperty); }
            set { SetValue(MarkdownProperty, value); }
        }

        public string RawContent
        {
            get { return (string)GetValue(RawContentProperty); }
            set { SetValue(RawContentProperty, value); }
        }

        public ICommand HyperlinkCommand
        {
            get { return (ICommand)GetValue(HyperlinkCommandProperty); }
            set { SetValue(HyperlinkCommandProperty, value); }
        }

        public static readonly DependencyProperty HyperlinkCommandProperty =
            Markdown.HyperlinkCommandProperty.AddOwner(typeof(MarkdownViewer),
                new FrameworkPropertyMetadata(UpdateLinkedProperties));

        public Style DocumentStyle
        {
            get { return (Style)GetValue(DocumentStyleProperty); }
            set { SetValue(DocumentStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DocumentStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentStyleProperty =
            Markdown.DocumentStyleProperty.AddOwner(typeof(MarkdownViewer),
                new FrameworkPropertyMetadata(UpdateLinkedProperties));

        public Style LinkStyle
        {
            get { return (Style)GetValue(LinkStyleProperty); }
            set { SetValue(LinkStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LinkStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkStyleProperty =
            Markdown.LinkStyleProperty.AddOwner(typeof(MarkdownViewer),
                new FrameworkPropertyMetadata(UpdateLinkedProperties));

        public MarkdownViewer()
        {
            Markdown = new Markdown();
        }
        static void UpdateLinkedProperties(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var c = (MarkdownViewer)d;
            c.Markdown?.SetCurrentValue(e.Property, e.NewValue);
        }

        static void UpdateDocument(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (MarkdownViewer)d;
            var raw = e.NewValue as string;
            control.SetCurrentValue(DocumentProperty, raw != null ? control.Markdown?.Transform(raw) : null);
        }
    }
}
