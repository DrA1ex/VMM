using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace VMM.Control
{
    public class IconButton : Button
    {
        public static readonly DependencyProperty IconPaddingProperty = DependencyProperty.Register(
            "IconPadding", typeof(Thickness), typeof(IconButton), new PropertyMetadata(new Thickness(3)));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(Geometry), typeof(IconButton), new PropertyMetadata(default(Geometry)));

        public IconButton()
        {
            DefaultStyleKey = typeof(IconButton);
        }

        public Geometry Icon
        {
            get { return (Geometry)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public Thickness IconPadding
        {
            get { return (Thickness)GetValue(IconPaddingProperty); }
            set { SetValue(IconPaddingProperty, value); }
        }
    }
}