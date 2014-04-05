using System;
using VMM.Model;

namespace VMM
{
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            ContentSource = Vk.Instance.LoggedIn ? new Uri("/Pages/MainPage.xaml", UriKind.Relative) : new Uri("/Pages/Authorization.xaml", UriKind.Relative);
        }
    }
}