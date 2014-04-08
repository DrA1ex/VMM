using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;

namespace VMM.Control
{
    public partial class SeekBar
    {

        public static readonly DependencyProperty SeekValueProperty = DependencyProperty.Register(
            "SeekValue", typeof(double), typeof(SeekBar), new PropertyMetadata(1.0));

        public SeekBar()
        {
            InitializeComponent();
        }

        public double SeekValue
        {
            get { return (double)GetValue(SeekValueProperty); }
            set { SetValue(SeekValueProperty, value); }
        }

        private void OnBarMouseUp(object sender, MouseButtonEventArgs e)
        {
            Point pos = e.GetPosition(this);
            SeekValue = pos.X / ActualWidth;
        }
    }
}