using System;
using System.Windows.Media;
using FirstFloor.ModernUI.Presentation;
using VMM.Helper;
using VMM.Model;
using VMM.Player;

namespace VMM
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            ContentSource = Vk.Instance.LoggedIn ? new Uri("/Pages/MainPage.xaml", UriKind.Relative) : new Uri("/Pages/Authorization.xaml", UriKind.Relative);

            ApplyAppearance();
        }

        private void OnClosed(object sender, EventArgs e)
        {
            MusicPlayer.Instance.Dispose();
        }

        private static void ApplyAppearance()
        {
            var settings = SettingsVault.Read();
            AppearanceManager.Current.FontSize = settings.FontSize;

            if(settings.Theme != null && !String.IsNullOrEmpty(settings.Theme.ToString()))
            {
                AppearanceManager.Current.ThemeSource = settings.Theme;
            }

            var accent = ColorSerializationHelper.FromString(settings.AccentColor);
            if(accent != Colors.White)
            {
                AppearanceManager.Current.AccentColor = accent;
            }
        }
    }
}