using System;
using System.Windows;
using System.Windows.Input;

namespace VMM.Control
{
    public partial class BufferedSeekBar
    {
        public static readonly DependencyProperty SeekValueProperty = DependencyProperty.Register(
            "SeekValue", typeof(double), typeof(BufferedSeekBar), new PropertyMetadata(0.0, SeekValueChangedCallback));

        public static readonly DependencyProperty BufferedValueProperty = DependencyProperty.Register(
            "BufferedValue", typeof(double), typeof(BufferedSeekBar), new PropertyMetadata(0.0, BufferedValueChangedCallback));

        public BufferedSeekBar()
        {
            InitializeComponent();
        }

        public double SeekValue
        {
            get { return (double)GetValue(SeekValueProperty); }
            set
            {
                var normalizedValue = NormalizeValue(value);
                SetValue(SeekValueProperty, normalizedValue);
                UpdateSeekPosition(normalizedValue);
            }
        }

        public double BufferedValue
        {
            get { return (double)GetValue(BufferedValueProperty); }
            set
            {
                var normalizedValue = NormalizeValue(value);
                SetValue(BufferedValueProperty, normalizedValue);
                UpdateBufferedPosition(normalizedValue);
            }
        }

        private static void SeekValueChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var seekBar = dependencyObject as BufferedSeekBar;
            seekBar?.UpdateSeekPosition(NormalizeValue((double)dependencyPropertyChangedEventArgs.NewValue));
        }

        private static void BufferedValueChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var seekBar = dependencyObject as BufferedSeekBar;
            seekBar?.UpdateBufferedPosition(NormalizeValue((double)dependencyPropertyChangedEventArgs.NewValue));
        }

        private void OnBarMouseUp(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            SeekValue = pos.X / ActualWidth;
        }

        private void UpdateSeekPosition(double value)
        {
            SeekRect.Width = value * ActualWidth;
        }

        private void UpdateBufferedPosition(double value)
        {
            BufferedRect.Width = value * ActualWidth;
        }

        private static double NormalizeValue(double value)
        {
            return Math.Min(1, Math.Max(0, value));
        }
    }
}