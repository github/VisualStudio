using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Media;
using GitHub.Extensions;

namespace GitHub.UI
{
    public static class GitHubBrushes
    {
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush AccentBrush = CreateBrush("#4ea6ea");
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush NewChangeBrush = CreateBrush(Color.FromArgb(255, 88, 202, 48));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush DeletedChangeBrush = CreateBrush(Color.FromArgb(255, 234, 40, 41));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush RenamedChangeBrush = CreateBrush(Color.FromArgb(255, 240, 180, 41));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush ConflictedChangeBrush = CreateBrush(Color.FromRgb(0x6e, 0x54, 0x94));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush DefaultChangeBrush = CreateBrush(Color.FromArgb(255, 0x33, 0x33, 0x33));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush GitRepoForegroundBrush = CreateBrush(Color.FromRgb(0x66, 0x66, 0x66));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush NonGitRepoForegroundBrush = CreateBrush(Color.FromRgb(0x99, 0x99, 0x99));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush NewCommitBackgroundBrush = CreateBrush(Color.FromRgb(0xFF, 0xFB, 0x85));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush CommitBackgroundBrush = CreateBrush(Color.FromRgb(0xed, 0xed, 0xed));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush CommitMessageTooLongBrush = CreateBrush(Color.FromRgb(0x99, 0x99, 0x99));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush NoChangeBrush = CreateBrush(Color.FromRgb(0xEE, 0xEE, 0xEE));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush DiffStatNewChangeBrush = CreateBrush(Color.FromRgb(0x6C, 0xC6, 0x44));
        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes",
            Justification = "We freeze these brushes so they're is effectively immutable.")]
        public static readonly SolidColorBrush DiffStatDeletedChangeBrush = CreateBrush(Color.FromRgb(0xD7, 0x43, 0x1B));

        public static SolidColorBrush CreateBrush(Color color)
        {
            var brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }
        
        public static SolidColorBrush CreateBrush(string color)
        {
            Guard.ArgumentNotNull(color, nameof(color));

            var colorObject = ColorConverter.ConvertFromString(color);
            return CreateBrush((Color)colorObject);
        }
    }
}
