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

        public static readonly DependencyProperty DocumentStyleProperty =
            Markdown.DocumentStyleProperty.AddOwner(typeof(MarkdownViewer),
                new FrameworkPropertyMetadata(UpdateLinkedProperties));

        public Style Heading1Style
        {
            get { return (Style)GetValue(Heading1StyleProperty); }
            set { SetValue(Heading1StyleProperty, value); }
        }

        public static readonly DependencyProperty Heading1StyleProperty =
            Markdown.Heading1StyleProperty.AddOwner(typeof(MarkdownViewer),
                new FrameworkPropertyMetadata(UpdateLinkedProperties));

        public Style Heading2Style
        {
            get { return (Style)GetValue(Heading2StyleProperty); }
            set { SetValue(Heading2StyleProperty, value); }
        }

        public static readonly DependencyProperty Heading2StyleProperty =
            Markdown.Heading2StyleProperty.AddOwner(typeof(MarkdownViewer),
                new FrameworkPropertyMetadata(UpdateLinkedProperties));

        public Style Heading3Style
        {
            get { return (Style)GetValue(Heading3StyleProperty); }
            set { SetValue(Heading3StyleProperty, value); }
        }

        public static readonly DependencyProperty Heading3StyleProperty =
            Markdown.Heading3StyleProperty.AddOwner(typeof(MarkdownViewer),
                new FrameworkPropertyMetadata(UpdateLinkedProperties));

        public Style Heading4Style
        {
            get { return (Style)GetValue(Heading4StyleProperty); }
            set { SetValue(Heading4StyleProperty, value); }
        }

        public static readonly DependencyProperty Heading4StyleProperty =
            Markdown.Heading4StyleProperty.AddOwner(typeof(MarkdownViewer),
                new FrameworkPropertyMetadata(UpdateLinkedProperties));

        public Style CodeStyle
        {
            get { return (Style)GetValue(CodeStyleProperty); }
            set { SetValue(CodeStyleProperty, value); }
        }

        public static readonly DependencyProperty CodeStyleProperty =
            Markdown.CodeStyleProperty.AddOwner(typeof(MarkdownViewer),
                new FrameworkPropertyMetadata(UpdateLinkedProperties));

        public Style LinkStyle
        {
            get { return (Style)GetValue(LinkStyleProperty); }
            set { SetValue(LinkStyleProperty, value); }
        }

        public static readonly DependencyProperty LinkStyleProperty =
            Markdown.LinkStyleProperty.AddOwner(typeof(MarkdownViewer),
                new FrameworkPropertyMetadata(UpdateLinkedProperties));

        public Style ImageStyle
        {
            get { return (Style)GetValue(ImageStyleProperty); }
            set { SetValue(ImageStyleProperty, value); }
        }

        public static readonly DependencyProperty ImageStyleProperty =
            Markdown.ImageStyleProperty.AddOwner(typeof(MarkdownViewer),
                new FrameworkPropertyMetadata(UpdateLinkedProperties));

        public Style SeparatorStyle
        {
            get { return (Style)GetValue(SeparatorStyleProperty); }
            set { SetValue(SeparatorStyleProperty, value); }
        }

        public static readonly DependencyProperty SeparatorStyleProperty =
            Markdown.SeparatorStyleProperty.AddOwner(typeof(MarkdownViewer),
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
