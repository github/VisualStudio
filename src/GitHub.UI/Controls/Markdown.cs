/*
 * Based on https://github.com/theunrepentantgeek/Markdown.XAML (e4435c0291)
 */

using GitHub.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GitHub.UI
{
    public class Markdown : DependencyObject
    {
        /// <summary>
        /// maximum nested depth of [] and () supported by the transform; implementation detail
        /// </summary>
        const int nestDepth = 6;

        /// <summary>
        /// Tabs are automatically converted to spaces as part of the transform  
        /// this constant determines how "wide" those tabs become in spaces  
        /// </summary>
        const int tabWidth = 4;

        const string markerUL = @"[*+-]";
        const string markerOL = @"\d+[.]";

        int listLevel;

        /// <summary>
        /// when true, bold and italic require non-word characters on either side  
        /// WARNING: this is a significant deviation from the markdown spec
        /// </summary>
        /// 
        public bool StrictBoldItalic { get; set; }

        public ICommand HyperlinkCommand
        {
            get { return (ICommand)GetValue(HyperlinkCommandProperty); }
            set { SetValue(HyperlinkCommandProperty, value); }
        }

        public static readonly DependencyProperty HyperlinkCommandProperty =
            DependencyProperty.Register("HyperlinkCommand", typeof(ICommand), typeof(Markdown), new PropertyMetadata(null));

        public Style DocumentStyle
        {
            get { return (Style)GetValue(DocumentStyleProperty); }
            set { SetValue(DocumentStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DocumentStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DocumentStyleProperty =
            DependencyProperty.Register("DocumentStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading1Style
        {
            get { return (Style)GetValue(Heading1StyleProperty); }
            set { SetValue(Heading1StyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Heading1Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading1StyleProperty =
            DependencyProperty.Register("Heading1Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading2Style
        {
            get { return (Style)GetValue(Heading2StyleProperty); }
            set { SetValue(Heading2StyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Heading2Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading2StyleProperty =
            DependencyProperty.Register("Heading2Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading3Style
        {
            get { return (Style)GetValue(Heading3StyleProperty); }
            set { SetValue(Heading3StyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Heading3Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading3StyleProperty =
            DependencyProperty.Register("Heading3Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style Heading4Style
        {
            get { return (Style)GetValue(Heading4StyleProperty); }
            set { SetValue(Heading4StyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Heading4Style.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Heading4StyleProperty =
            DependencyProperty.Register("Heading4Style", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style CodeStyle
        {
            get { return (Style)GetValue(CodeStyleProperty); }
            set { SetValue(CodeStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CodeStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeStyleProperty =
            DependencyProperty.Register("CodeStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style CodeBlockStyle
        {
            get { return (Style)GetValue(CodeBlockStyleProperty); }
            set { SetValue(CodeBlockStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CodeBlockStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CodeBlockStyleProperty =
            DependencyProperty.Register("CodeBlockStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style LinkStyle
        {
            get { return (Style)GetValue(LinkStyleProperty); }
            set { SetValue(LinkStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LinkStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LinkStyleProperty =
            DependencyProperty.Register("LinkStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style ImageStyle
        {
            get { return (Style)GetValue(ImageStyleProperty); }
            set { SetValue(ImageStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageStyleProperty =
            DependencyProperty.Register("ImageStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public Style SeparatorStyle
        {
            get { return (Style)GetValue(SeparatorStyleProperty); }
            set { SetValue(SeparatorStyleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SeparatorStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SeparatorStyleProperty =
            DependencyProperty.Register("SeparatorStyle", typeof(Style), typeof(Markdown), new PropertyMetadata(null));

        public string AssetPathRoot
        {
            get { return (string)GetValue(AssetPathRootProperty); }
            set { SetValue(AssetPathRootProperty, value); }
        }

        public static readonly DependencyProperty AssetPathRootProperty =
            DependencyProperty.Register("AssetPathRootRoot", typeof(string), typeof(Markdown), new PropertyMetadata(null));

        public Markdown()
        {
            HyperlinkCommand = NavigationCommands.GoToPage;
        }

        public FlowDocument Transform(string text)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            text = Normalize(text);
            var document = Create<FlowDocument, Block>(RunBlockGamut(text));

            if (DocumentStyle != null)
            {
                document.Style = DocumentStyle;
            }
            else
            {
                document.PagePadding = new Thickness(0);
            }

            return document;
        }

        /// <summary>
        /// Perform transformations that form block-level tags like paragraphs, headers, and list items.
        /// </summary>
        IEnumerable<Block> RunBlockGamut(string text)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            return DoHeaders(text,
                s1 => DoHorizontalRules(s1,
                s2 => DoLists(s2,
                s3 => DoCodeBlocks(s3, 
                sn => FormParagraphs(sn)))));

            //text = DoCodeBlocks(text);
            //text = DoBlockQuotes(text);

            //// We already ran HashHTMLBlocks() before, in Markdown(), but that
            //// was to escape raw HTML in the original Markdown source. This time,
            //// we're escaping the markup we've just created, so that we don't wrap
            //// <p> tags around block-level tags.
            //text = HashHTMLBlocks(text);

            //text = FormParagraphs(text);

            //return text;
        }

        /// <summary>
        /// Perform transformations that occur *within* block-level tags like paragraphs, headers, and list items.
        /// </summary>
        IEnumerable<Inline> RunSpanGamut(string text)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            return DoCodeSpans(text,
                s0 => DoImages(s0,
                s1 => DoAnchors(s1,
                s2 => DoItalicsAndBold(s2,
                s3 => DoText(s3)))));

            //text = EscapeSpecialCharsWithinTagAttributes(text);
            //text = EscapeBackslashes(text);

            //// Images must come first, because ![foo][f] looks like an anchor.
            //text = DoImages(text);
            //text = DoAnchors(text);

            //// Must come after DoAnchors(), because you can use < and >
            //// delimiters in inline links like [this](<url>).
            //text = DoAutoLinks(text);

            //text = EncodeAmpsAndAngles(text);
            //text = DoItalicsAndBold(text);
            //text = DoHardBreaks(text);

            //return text;
        }

        static Regex newlinesLeadingTrailing = new Regex(@"^\n+|\n+\z", RegexOptions.Compiled);
        static Regex newlinesMultiple = new Regex(@"\n{2,}", RegexOptions.Compiled);
        static Regex leadingWhitespace = new Regex(@"^[ ]*", RegexOptions.Compiled);

        /// <summary>
        /// splits on two or more newlines, to form "paragraphs";    
        /// </summary>
        IEnumerable<Block> FormParagraphs(string text)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            // split on two or more newlines
            string[] grafs = newlinesMultiple.Split(newlinesLeadingTrailing.Replace(text, ""));

            foreach (var g in grafs)
            {
                yield return Create<Paragraph, Inline>(RunSpanGamut(g));
            }
        }

        static string nestedBracketsPattern;

        /// <summary>
        /// Reusable pattern to match balanced [brackets]. See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        static string GetNestedBracketsPattern()
        {
            // in other words [this] and [this[also]] and [this[also[too]]]
            // up to nestDepth
            if (nestedBracketsPattern == null)
                nestedBracketsPattern =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^\[\]]+      # Anything other than brackets
                     |
                       \[
                           ", nestDepth) + RepeatString(
                    @" \]
                    )*"
                    , nestDepth);
            return nestedBracketsPattern;
        }

        static string nestedParensPattern;

        /// <summary>
        /// Reusable pattern to match balanced (parens). See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        static string GetNestedParensPattern()
        {
            // in other words (this) and (this(also)) and (this(also(too)))
            // up to nestDepth
            if (nestedParensPattern == null)
                nestedParensPattern =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^()\s]+      # Anything other than parens or whitespace
                     |
                       \(
                           ", nestDepth) + RepeatString(
                    @" \)
                    )*"
                    , nestDepth);
            return nestedParensPattern;
        }

        static string nestedParensPatternWithWhiteSpace;

        /// <summary>
        /// Reusable pattern to match balanced (parens), including whitespace. See Friedl's 
        /// "Mastering Regular Expressions", 2nd Ed., pp. 328-331.
        /// </summary>
        static string GetNestedParensPatternWithWhiteSpace()
        {
            // in other words (this) and (this(also)) and (this(also(too)))
            // up to nestDepth
            if (nestedParensPatternWithWhiteSpace == null)
                nestedParensPatternWithWhiteSpace =
                    RepeatString(@"
                    (?>              # Atomic matching
                       [^()]+      # Anything other than parens
                     |
                       \(
                           ", nestDepth) + RepeatString(
                    @" \)
                    )*"
                    , nestDepth);
            return nestedParensPatternWithWhiteSpace;
        }

        static Regex imageInline = new Regex(string.Format(@"
                (                           # wrap whole match in $1
                    !\[
                        ({0})               # link text = $2
                    \]
                    \(                      # literal paren
                        [ ]*
                        ({1})               # href = $3
                        [ ]*
                        (                   # $4
                        (['""])           # quote char = $5
                        (.*?)               # title = $6
                        \5                  # matching quote
                        #[ ]*                # ignore any spaces between closing quote and )
                        )?                  # title is optional
                    \)
                )", GetNestedBracketsPattern(), GetNestedParensPatternWithWhiteSpace()),
                  RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        static Regex anchorInline = new Regex(string.Format(@"
                (                           # wrap whole match in $1
                    \[
                        ({0})               # link text = $2
                    \]
                    \(                      # literal paren
                        [ ]*
                        ({1})               # href = $3
                        [ ]*
                        (                   # $4
                        (['""])           # quote char = $5
                        (.*?)               # title = $6
                        \5                  # matching quote
                        [ ]*                # ignore any spaces between closing quote and )
                        )?                  # title is optional
                    \)
                )", GetNestedBracketsPattern(), GetNestedParensPattern()),
                  RegexOptions.Singleline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        static Regex anchorAuto = new Regex(@"([A-Za-z][A-Za-z0-9.+-]{1,31}:[^<>\x00-\x20]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown images into images
        /// </summary>
        /// <remarks>
        /// ![image alt](url) 
        /// </remarks>
        IEnumerable <Inline> DoImages(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            return Evaluate(text, imageInline, ImageInlineEvaluator, defaultHandler);
        }

        Inline ImageInlineEvaluator(Match match)
        {
            Guard.ArgumentNotNull(match, nameof(match));

            string linkText = match.Groups[2].Value;
            string url = match.Groups[3].Value;
            BitmapImage imgSource = null;
            try
            {
                if (!Uri.IsWellFormedUriString(url, UriKind.Absolute) && !System.IO.Path.IsPathRooted(url))
                {
                    url = System.IO.Path.Combine(AssetPathRoot ?? string.Empty, url);
                }

                imgSource = new BitmapImage(new Uri(url, UriKind.RelativeOrAbsolute));
            }
            catch (Exception)
            {
                return new Run("!" + url) { Foreground = Brushes.Red };
            }

            Image image = new Image { Source = imgSource, Tag = linkText };
            if (ImageStyle == null)
            {
                image.Margin = new Thickness(0);
            }
            else
            {
                image.Style = ImageStyle;
            }

            // Bind size so document is updated when image is downloaded
            if (imgSource.IsDownloading)
            {
                Binding binding = new Binding(nameof(BitmapImage.Width));
                binding.Source = imgSource;
                binding.Mode = BindingMode.OneWay;

                BindingExpressionBase bindingExpression = BindingOperations.SetBinding(image, Image.WidthProperty, binding);
                EventHandler downloadCompletedHandler = null;
                downloadCompletedHandler = (sender, e) =>
                {
                    imgSource.DownloadCompleted -= downloadCompletedHandler;
                    bindingExpression.UpdateTarget();
                };
                imgSource.DownloadCompleted += downloadCompletedHandler;
            }
            else
            {
                image.Width = imgSource.Width;
            }

            return new InlineUIContainer(image);
        }

        /// <summary>
        /// Turn Markdown link shortcuts into hyperlinks
        /// </summary>
        /// <remarks>
        /// [link text](url "title") 
        /// </remarks>
        IEnumerable<Inline> DoAnchors(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            return Evaluate(text, anchorInline, AnchorInlineEvaluator, 
                x => Evaluate(x, anchorAuto, AnchorAutoEvaluator, defaultHandler));
        }

        Inline AnchorInlineEvaluator(Match match)
        {
            Guard.ArgumentNotNull(match, nameof(match));

            string linkText = match.Groups[2].Value;
            string url = match.Groups[3].Value;
            string title = match.Groups[6].Value;

            var result = Create<Hyperlink, Inline>(RunSpanGamut(linkText));
            result.Command = HyperlinkCommand;
            result.CommandParameter = url;
            if (LinkStyle != null)
            {
                result.Style = LinkStyle;
            }

            return result;
        }

        Inline AnchorAutoEvaluator(Match match)
        {
            Guard.ArgumentNotNull(match, nameof(match));

            string url = match.Groups[1].Value;

            var run = new Run();
            run.Text = url;

            var result = new Hyperlink(run);
            result.Command = HyperlinkCommand;
            result.CommandParameter = url;
            if (LinkStyle != null)
            {
                result.Style = LinkStyle;
            }

            return result;
        }

        static Regex headerSetext = new Regex(@"
                ^(.+?)
                [ ]*
                \n
                (=+|-+)     # $1 = string of ='s or -'s
                [ ]*
                \n+",
    RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        static Regex headerAtx = new Regex(@"
                ^(\#{1,6})  # $1 = string of #'s
                [ ]*
                (.+?)       # $2 = Header text
                [ ]*
                \#*         # optional closing #'s (not counted)
                \n+",
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown headers into HTML header tags
        /// </summary>
        /// <remarks>
        /// Header 1  
        /// ========  
        /// 
        /// Header 2  
        /// --------  
        /// 
        /// # Header 1  
        /// ## Header 2  
        /// ## Header 2 with closing hashes ##  
        /// ...  
        /// ###### Header 6  
        /// </remarks>
        IEnumerable<Block> DoHeaders(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            return Evaluate<Block>(text, headerSetext, m => SetextHeaderEvaluator(m),
                s => Evaluate<Block>(s, headerAtx, m => AtxHeaderEvaluator(m), defaultHandler));
        }

        Block SetextHeaderEvaluator(Match match)
        {
            Guard.ArgumentNotNull(match, nameof(match));

            string header = match.Groups[1].Value;
            int level = match.Groups[2].Value.StartsWith("=") ? 1 : 2;

            //TODO: Style the paragraph based on the header level
            return CreateHeader(level, RunSpanGamut(header.Trim()));
        }

        Block AtxHeaderEvaluator(Match match)
        {
            Guard.ArgumentNotNull(match, nameof(match));

            string header = match.Groups[2].Value;
            int level = match.Groups[1].Value.Length;
            return CreateHeader(level, RunSpanGamut(header));
        }

        public Block CreateHeader(int level, IEnumerable<Inline> content)
        {
            Guard.ArgumentNotNull(content, nameof(content));

            var block = Create<Paragraph, Inline>(content);

            switch (level)
            {
                case 1:
                    if (Heading1Style != null)
                    {
                        block.Style = Heading1Style;
                    }
                    break;

                case 2:
                    if (Heading2Style != null)
                    {
                        block.Style = Heading2Style;
                    }
                    break;

                case 3:
                    if (Heading3Style != null)
                    {
                        block.Style = Heading3Style;
                    }
                    break;

                case 4:
                    if (Heading4Style != null)
                    {
                        block.Style = Heading4Style;
                    }
                    break;
            }

            return block;
        }

        static Regex horizontalRules = new Regex(@"
            ^[ ]{0,3}         # Leading space
                ([-*_])       # $1: First marker
                (?>           # Repeated marker group
                    [ ]{0,2}  # Zero, one, or two spaces.
                    \1        # Marker character
                ){2,}         # Group repeated at least twice
                [ ]*          # Trailing spaces
                $             # End of line.
            ", RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown horizontal rules into HTML hr tags
        /// </summary>
        /// <remarks>
        /// ***  
        /// * * *  
        /// ---
        /// - - -
        /// </remarks>
        IEnumerable<Block> DoHorizontalRules(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            return Evaluate(text, horizontalRules, RuleEvaluator, defaultHandler);
        }

        Block RuleEvaluator(Match match)
        {
            Guard.ArgumentNotNull(match, nameof(match));

            Line line = new Line();
            if (SeparatorStyle == null)
            {
                line.X2 = 1;
                line.StrokeThickness = 1.0;
            }
            else
            {
                line.Style = SeparatorStyle;
            }

            var container = new BlockUIContainer(line);
            return container;
        }

        static string wholeList = string.Format(@"
            (                               # $1 = whole list
              (                             # $2
                [ ]{{0,{1}}}
                ({0})                       # $3 = first list item marker
                [ ]+
              )
              (?s:.+?)
              (                             # $4
                  \z
                |
                  \n{{2,}}
                  (?=\S)
                  (?!                       # Negative lookahead for another list item marker
                    [ ]*
                    {0}[ ]+
                  )
              )
            )", string.Format("(?:{0}|{1})", markerUL, markerOL), tabWidth - 1);

        static Regex listNested = new Regex(@"^" + wholeList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        static Regex listTopLevel = new Regex(@"(?:(?<=\n\n)|\A\n?)" + wholeList,
            RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown lists into HTML ul and ol and li tags
        /// </summary>
        IEnumerable<Block> DoLists(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            // We use a different prefix before nested lists than top-level lists.
            // See extended comment in ProcessListItems().
            if (listLevel > 0)
                return Evaluate(text, listNested, ListEvaluator, defaultHandler);
            else
                return Evaluate(text, listTopLevel, ListEvaluator, defaultHandler);
        }

        Block ListEvaluator(Match match)
        {
            Guard.ArgumentNotNull(match, nameof(match));

            string list = match.Groups[1].Value;
            string listType = Regex.IsMatch(match.Groups[3].Value, markerUL) ? "ul" : "ol";

            // Turn double returns into triple returns, so that we can make a
            // paragraph for the last item in a list, if necessary:
            list = Regex.Replace(list, @"\n{2,}", "\n\n\n");

            var resultList = Create<List, ListItem>(ProcessListItems(list, listType == "ul" ? markerUL : markerOL));

            resultList.MarkerStyle = listType == "ul" ? TextMarkerStyle.Disc : TextMarkerStyle.Decimal;

            return resultList;
        }

        /// <summary>
        /// Process the contents of a single ordered or unordered list, splitting it
        /// into individual list items.
        /// </summary>
        IEnumerable<ListItem> ProcessListItems(string list, string marker)
        {
            // The listLevel global keeps track of when we're inside a list.
            // Each time we enter a list, we increment it; when we leave a list,
            // we decrement. If it's zero, we're not in a list anymore.

            // We do this because when we're not inside a list, we want to treat
            // something like this:

            //    I recommend upgrading to version
            //    8. Oops, now this line is treated
            //    as a sub-list.

            // As a single paragraph, despite the fact that the second line starts
            // with a digit-period-space sequence.

            // Whereas when we're inside a list (or sub-list), that line will be
            // treated as the start of a sub-list. What a kludge, huh? This is
            // an aspect of Markdown's syntax that's hard to parse perfectly
            // without resorting to mind-reading. Perhaps the solution is to
            // change the syntax rules such that sub-lists must start with a
            // starting cardinal number; e.g. "1." or "a.".

            listLevel++;
            try
            {
                // Trim trailing blank lines:
                list = Regex.Replace(list, @"\n{2,}\z", "\n");

                string pattern = string.Format(
                  @"(\n)?                      # leading line = $1
                (^[ ]*)                    # leading whitespace = $2
                ({0}) [ ]+                 # list marker = $3
                ((?s:.+?)                  # list item text = $4
                (\n{{1,2}}))      
                (?= \n* (\z | \2 ({0}) [ ]+))", marker);

                var regex = new Regex(pattern, RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline);
                var matches = regex.Matches(list);
                foreach (Match m in matches)
                {
                    yield return ListItemEvaluator(m);
                }
            }
            finally
            {
                listLevel--;
            }
        }

        ListItem ListItemEvaluator(Match match)
        {
            Guard.ArgumentNotNull(match, nameof(match));

            string item = match.Groups[4].Value;
            string leadingLine = match.Groups[1].Value;

            if (!String.IsNullOrEmpty(leadingLine) || Regex.IsMatch(item, @"\n{2,}"))
                // we could correct any bad indentation here..
                return Create<ListItem, Block>(RunBlockGamut(item));
            else
            {
                // recursion for sub-lists
                return Create<ListItem, Block>(RunBlockGamut(item));
            }
        }

        static Regex codeBlock = new Regex(@"
                    ^           # Beginning of line
                    ```         # Three ` backticks
                    \s*?        # Whitespace
                    ([\S]+)?    # $1 = language
                    \n          # Newline
                    ([\s\S]+?)  # $2 = The code block
                    \n          # Newline
                    ```", RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown ``` code blocks into a PRE tags
        /// </summary>
        IEnumerable<Block> DoCodeBlocks(string text, Func<string, IEnumerable<Block>> defaultHandler)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            return Evaluate(text, codeBlock, CodeBlockEvaluator, defaultHandler);
        }

        Block CodeBlockEvaluator(Match match)
        {
            Guard.ArgumentNotNull(match, nameof(match));

            var inline = new Run();
            inline.Text = match.Groups[2].Value;

            var result = new Paragraph(inline);

            if (CodeBlockStyle != null)
            {
                result.Style = CodeBlockStyle;
            }

            return result;
        }

        static Regex codeSpan = new Regex(@"
                    (?<!\\)   # Character before opening ` can't be a backslash
                    (`+)      # $1 = Opening run of `
                    (.+?)     # $2 = The code block
                    (?<!`)
                    \1
                    (?!`)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown `code spans` into HTML code tags
        /// </summary>
        IEnumerable<Inline> DoCodeSpans(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            //    * You can use multiple backticks as the delimiters if you want to
            //        include literal backticks in the code span. So, this input:
            //
            //        Just type ``foo `bar` baz`` at the prompt.
            //
            //        Will translate to:
            //
            //          <p>Just type <code>foo `bar` baz</code> at the prompt.</p>
            //
            //        There's no arbitrary limit to the number of backticks you
            //        can use as delimters. If you need three consecutive backticks
            //        in your code, use four for delimiters, etc.
            //
            //    * You can use spaces to get literal backticks at the edges:
            //
            //          ... type `` `bar` `` ...
            //
            //        Turns to:
            //
            //          ... type <code>`bar`</code> ...         
            //

            return Evaluate(text, codeSpan, CodeSpanEvaluator, defaultHandler);
        }

        Inline CodeSpanEvaluator(Match match)
        {
            Guard.ArgumentNotNull(match, nameof(match));

            string span = match.Groups[2].Value;
            span = Regex.Replace(span, @"^[ ]*", ""); // leading whitespace
            span = Regex.Replace(span, @"[ ]*$", ""); // trailing whitespace

            var result = new Run(span);
            if (CodeStyle != null)
            {
                result.Style = CodeStyle;
            }

            return result;
        }

        static Regex bold = new Regex(@"(\*\*|__) (?=\S) (.+?[*_]*) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        static Regex strictBold = new Regex(@"([\W_]|^) (\*\*|__) (?=\S) ([^\r]*?\S[\*_]*) \2 ([\W_]|$)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        static Regex italic = new Regex(@"(\*|_) (?=\S) (.+?) (?<=\S) \1",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);
        static Regex strictItalic = new Regex(@"([\W_]|^) (\*|_) (?=\S) ([^\r\*_]*?\S) \2 ([\W_]|$)",
            RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.Compiled);

        /// <summary>
        /// Turn Markdown *italics* and **bold** into HTML strong and em tags
        /// </summary>
        IEnumerable<Inline> DoItalicsAndBold(string text, Func<string, IEnumerable<Inline>> defaultHandler)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            // <strong> must go first, then <em>
            if (StrictBoldItalic)
            {
                return Evaluate<Inline>(text, strictBold, m => BoldEvaluator(m, 3),
                    s1 => Evaluate<Inline>(s1, strictItalic, m => ItalicEvaluator(m, 3),
                    s2 => defaultHandler(s2)));
            }
            else
            {
                return Evaluate<Inline>(text, bold, m => BoldEvaluator(m, 2),
                   s1 => Evaluate<Inline>(s1, italic, m => ItalicEvaluator(m, 2),
                   s2 => defaultHandler(s2)));
            }
        }

        Inline ItalicEvaluator(Match match, int contentGroup)
        {
            Guard.ArgumentNotNull(match, nameof(match));

            var content = match.Groups[contentGroup].Value;
            return Create<Italic, Inline>(RunSpanGamut(content));
        }

        Inline BoldEvaluator(Match match, int contentGroup)
        {
            Guard.ArgumentNotNull(match, nameof(match));
            var content = match.Groups[contentGroup].Value;
            return Create<Bold, Inline>(RunSpanGamut(content));
        }

        static Regex outDent = new Regex(@"^[ ]{1," + tabWidth + @"}", RegexOptions.Multiline | RegexOptions.Compiled);

        /// <summary>
        /// Remove one level of line-leading spaces
        /// </summary>
        string Outdent(string block)
        {
            return outDent.Replace(block, "");
        }

        /// <summary>
        /// convert all tabs to tabWidth spaces; 
        /// standardizes line endings from DOS (CR LF) or Mac (CR) to UNIX (LF); 
        /// makes sure text ends with a couple of newlines; 
        /// removes any blank lines (only spaces) in the text
        /// </summary>
        string Normalize(string text)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            var output = new StringBuilder(text.Length);
            var line = new StringBuilder();
            bool valid = false;

            for (int i = 0; i < text.Length; i++)
            {
                switch (text[i])
                {
                    case '\n':
                        if (valid)
                            output.Append(line);
                        output.Append('\n');
                        line.Length = 0;
                        valid = false;
                        break;
                    case '\r':
                        if ((i < text.Length - 1) && (text[i + 1] != '\n'))
                        {
                            if (valid)
                                output.Append(line);
                            output.Append('\n');
                            line.Length = 0;
                            valid = false;
                        }
                        break;
                    case '\t':
                        int width = (tabWidth - line.Length % tabWidth);
                        for (int k = 0; k < width; k++)
                            line.Append(' ');
                        break;
                    case '\x1A':
                        break;
                    default:
                        if (!valid && text[i] != ' ')
                            valid = true;
                        line.Append(text[i]);
                        break;
                }
            }

            if (valid)
                output.Append(line);
            output.Append('\n');

            // add two newlines to the end before return
            return output.Append("\n\n").ToString();
        }

        /// <summary>
        /// this is to emulate what's evailable in PHP
        /// </summary>
        static string RepeatString(string text, int count)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            var sb = new StringBuilder(text.Length * count);
            for (int i = 0; i < count; i++)
                sb.Append(text);
            return sb.ToString();
        }

        TResult Create<TResult, TContent>(IEnumerable<TContent> content)
            where TResult : IAddChild, new()
        {
            var result = new TResult();
            foreach (var c in content)
            {
                result.AddChild(c);
            }

            return result;
        }

        IEnumerable<T> Evaluate<T>(string text, Regex expression, Func<Match, T> build, Func<string, IEnumerable<T>> rest)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            var matches = expression.Matches(text);
            var index = 0;
            foreach (Match m in matches)
            {
                if (m.Index > index)
                {
                    var prefix = text.Substring(index, m.Index - index);
                    foreach (var t in rest(prefix))
                    {
                        yield return t;
                    }
                }

                yield return build(m);

                index = m.Index + m.Length;
            }

            if (index < text.Length)
            {
                var suffix = text.Substring(index, text.Length - index);
                foreach (var t in rest(suffix))
                {
                    yield return t;
                }
            }
        }

        static Regex eoln = new Regex("\\s+");

        public IEnumerable<Inline> DoText(string text)
        {
            Guard.ArgumentNotNull(text, nameof(text));

            var t = eoln.Replace(text, " ");
            yield return new Run(t);
        }
    }
}
