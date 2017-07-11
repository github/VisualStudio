using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace GitHub.UI
{
    public class PromptRichTextBox : RichTextBox, IShortcutContainer
    {
        public const int BadCommitMessageLength = 51;

        public static readonly DependencyProperty PromptTextProperty =
            DependencyProperty.Register("PromptText", typeof(string), typeof(PromptRichTextBox), new UIPropertyMetadata(""));

        public static readonly DependencyProperty PromptForegroundProperty =
            DependencyProperty.Register("PromptForeground", typeof(Brush), typeof(PromptRichTextBox), new UIPropertyMetadata(Brushes.Gray));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(PromptRichTextBox), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnTextPropertyChanged));

        public static readonly DependencyProperty IsOverCharacterLimitProperty =
            DependencyProperty.Register("IsOverCharacterLimit", typeof(bool), typeof(PromptRichTextBox), new UIPropertyMetadata(false));

        public static readonly DependencyProperty CharacterLimitToolTipWarningProperty =
            DependencyProperty.Register("CharacterLimitToolTipWarning", typeof(string), typeof(PromptRichTextBox), new UIPropertyMetadata(""));

        static readonly Thickness desiredPagePadding = new Thickness(2, 0, 0, 0);

        public PromptRichTextBox()
        {
            Text = "";
            TextChanged += OnTextChanged;
            DataObject.AddPastingHandler(this, OnPaste);
        }

        public Brush PromptForeground
        {
            get { return (Brush)GetValue(PromptForegroundProperty); }
            set { SetValue(PromptForegroundProperty, value); }
        }

        [Localizability(LocalizationCategory.Text)]
        [DefaultValue("")]
        public string PromptText
        {
            get { return (string)GetValue(PromptTextProperty); }
            set { SetValue(PromptTextProperty, value); }
        }

        [Localizability(LocalizationCategory.Text)]
        [DefaultValue("")]
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value ?? ""); }
        }

        [Localizability(LocalizationCategory.Text)]
        [DefaultValue("")]
        public string CharacterLimitToolTipWarning
        {
            get { return (string)GetValue(CharacterLimitToolTipWarningProperty); }
            set { SetValue(CharacterLimitToolTipWarningProperty, value); }
        }

        [DefaultValue(false)]
        public bool IsOverCharacterLimit
        {
            get { return (bool)GetValue(IsOverCharacterLimitProperty); }
            set { SetValue(IsOverCharacterLimitProperty, value); }
        }

        [DefaultValue(true)]
        public bool SupportsKeyboardShortcuts { get; set; }

        void SetDocumentPadding()
        {
            if (Document == null) return;

            // This is a bug in FlowDocument - you can't set the padding. Well you can, but it doesn't stick.
            Document.PagePadding = desiredPagePadding;
        }

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            base.OnGotFocus(e);
            SetDocumentPadding();
        }

        protected override void OnGotKeyboardFocus(System.Windows.Input.KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            SetDocumentPadding();
        }

        // Implementation from StackOverflow: http://stackoverflow.com/a/5049150/598
        static void OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (!e.SourceDataObject.GetDataPresent(DataFormats.Rtf, true)) return;
            var richTextBox = sender as RichTextBox;
            if (richTextBox == null) return;

            var rtf = e.SourceDataObject.GetData(DataFormats.Rtf) as string;

            var document = new FlowDocument();

            var content = SetDocumentContent(document, rtf, DataFormats.Rtf);

            if (content.Text == null)
            {
                throw new GitHubLogicException("WPF ensures this is not null. WPF failed on the job");
            }

            var d = new DataObject();
            var textContent = content.Text;
            if (!richTextBox.AcceptsReturn)
            {
                // Fit the content into one line.
                textContent = textContent.Replace(Environment.NewLine, " ");
            }

            // Take that RTF data and set it as text stripping all the crap. Also, put it on one line.
            d.SetData(DataFormats.Text, textContent);
            e.DataObject = d;
        }

        // When the user changes the text in the RichTextBox (via typing in it), data binding will update the
        // corresponding property of the view model. Two-way binding will then trigger the OnTextPropertyChanged
        // method which will try to update the RichTextBox. That secondary update is unnecessary.
        // This little field will help ensure we don't do that unnecessarily.
        bool updateDocument = true;

        void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (Document == null) return;
            updateDocument = false;
            TextChanged -= OnTextChanged;
            try
            {
                SetDocumentPadding();

                // RichTextBox appends a \r\n at the end.
                var documentEndWithoutNewLine = Document.ContentEnd.GetPositionAtOffset(-1 * Environment.NewLine.Length)
                    ?? Document.ContentEnd;
                var documentRange = new TextRange(Document.ContentStart, documentEndWithoutNewLine);

                if (documentRange.Text.Length > 140)
                {
                    documentEndWithoutNewLine.DeleteTextInRun(140 - documentRange.Text.Length);
                    documentRange = new TextRange(Document.ContentStart, documentEndWithoutNewLine);
                }

                documentRange.ClearAllProperties();
                IsOverCharacterLimit = false;
                Text = documentRange.Text;
                
                var be = GetBindingExpression(TextProperty);
                if (be != null)
                    be.UpdateSource();

                if (documentRange.Text.Length <= BadCommitMessageLength) return;

                var start = Document.ContentStart.GetPositionAtOffset(BadCommitMessageLength, LogicalDirection.Backward);

                var range = new TextRange(start, documentEndWithoutNewLine);
                // The next line changes the CaretPosition by 2.
                range.ApplyPropertyValue(TextElement.ForegroundProperty, GitHubBrushes.CommitMessageTooLongBrush);
                IsOverCharacterLimit = true;
            }
            finally
            {
                updateDocument = true;
                TextChanged += OnTextChanged;
            }
        }

        static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var richTextBox = (PromptRichTextBox)d;
            if (!richTextBox.updateDocument) return;
            var value = e.NewValue as string;
            var document = richTextBox.Document;
            if (document == null)
            {
                document = new FlowDocument();
                richTextBox.Document = document;
            }

            try
            {
                SetDocumentContent(document, value, DataFormats.Text);
            }
            finally
            {
                richTextBox.CaretPosition = document.ContentEnd.GetPositionAtOffset(0);
            }
        }

        static TextRange SetDocumentContent(FlowDocument document, string text, string dataFormat)
        {
            var content = new TextRange(document.ContentStart, document.ContentEnd);

            if (content.CanLoad(dataFormat))
            {
                if (string.IsNullOrEmpty(text))
                {
                    content.Text = text;
                }
                else
                {
                    using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(text)))
                    {
                        content.Load(stream, dataFormat);
                    }
                }
            }

            return content;
        }
    }
}
