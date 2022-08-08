using System.Diagnostics;
using System.Reflection;

namespace TMP.WORK.AramisChetchiki
{
    /// <summary>
    /// Interaction logic for AppSplashScreen.xaml
    /// </summary>
    [SplashScreen.SplashScreen(MinimumVisibilityDuration = 10, FadeoutDuration = 1)]
    public partial class AppSplashScreen
    {
        public AppSplashScreen()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the file version info.
        /// </summary>
        public FileVersionInfo FileVersionInfo { get; } = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
    }
}
