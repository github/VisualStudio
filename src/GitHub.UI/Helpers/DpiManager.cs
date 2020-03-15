namespace GitHub.UI.Helpers
{
    public interface IDpiManager
    {
        Dpi CurrentDpi { get; set; }
    }

    public class DpiManager : IDpiManager
    {
        static readonly DpiManager dpiManagerInstance = new DpiManager();
        
        public static DpiManager Instance { get { return dpiManagerInstance; } }

        private DpiManager()
        {
            CurrentDpi = Dpi.Default;
        }

        public Dpi CurrentDpi { get; set; }
    }
}
