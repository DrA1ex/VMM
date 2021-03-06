﻿using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JetBrains.Annotations;
using VMM.Content.ViewModel;
using VMM.Helper;
using VMM.Model;

namespace VMM.Content.View
{
    public partial class MusicListView : INotifyPropertyChanged
    {
        public MusicListView()
        {
            InitializeComponent();

            DataContext = Model;
            MusciView.SelectionChanged += MusciViewOnSelectionChanged;
            Model.EntryPlayed += OnEntryPlayed;


            var itemContainerStyle = MusciView.ItemContainerStyle ?? new Style(typeof(ListViewItem));
            itemContainerStyle.Setters.Add(new Setter(AllowDropProperty, true));
            itemContainerStyle.Setters.Add(new EventSetter(PreviewMouseMoveEvent, new MouseEventHandler(MusciViewOnPreviewMouseMove)));
            itemContainerStyle.Setters.Add(new EventSetter(DropEvent, new DragEventHandler(MusciViewOnDrop)));
            itemContainerStyle.Setters.Add(new EventSetter(DragOverEvent, new DragEventHandler(MusicViewDragOver)));
            MusciView.ItemContainerStyle = itemContainerStyle;
        }


        public MusicListViewModel Model { get; } = new MusicListViewModel();

        public MusicEntry[] SelectedItems
        {
            get
            {
                var o = MusciView.SelectedItems.Cast<MusicEntry>();
                var x = o.ToArray();
                Model.SelectedItems = x;

                return x;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnEntryPlayed(object sender, MusicEntry musicEntry)
        {
            if(MusciView.SelectedItems == null || MusciView.SelectedItems.Count <= 1)
            {
                MusciView.SelectedItem = musicEntry;
            }
        }

        private void MusicViewDragOver(object sender, DragEventArgs e)
        {
            const int scrollThreshold = 25;
            const int scrollOffset = 3;

            var pos = e.GetPosition(MusciView);
            var scrollView = UiTreeHelper.FindOfType(MusciView, typeof(ScrollViewer)) as ScrollViewer;

            if(scrollView != null)
            {
                if(pos.Y < scrollThreshold)
                    scrollView.ScrollToVerticalOffset(scrollView.VerticalOffset - scrollOffset);
                else if(pos.Y > MusciView.ActualHeight - scrollThreshold)
                    scrollView.ScrollToVerticalOffset(scrollView.VerticalOffset + scrollOffset);
            }
        }

        private void MusciViewOnPreviewMouseMove(object sender, MouseEventArgs eventArgs)
        {
            var draggedItem = sender as ListViewItem;

            if(draggedItem != null && eventArgs.LeftButton == MouseButtonState.Pressed
               && !(eventArgs.OriginalSource is Button))
            {
                DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
                draggedItem.IsSelected = true;
            }
        }

        private void MusciViewOnDrop(object sender, DragEventArgs dragEventArgs)
        {
            var droppedData = dragEventArgs.Data.GetData(typeof(MusicEntry)) as MusicEntry;
            var target = ((ListViewItem)sender).DataContext as MusicEntry;

            var srcIndex = Model.Music.IndexOf(droppedData);
            var targetIndex = Model.Music.IndexOf(target);

            Model.MoveSong(srcIndex, targetIndex);
        }

        private void MusciViewOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            OnPropertyChanged(nameof(SelectedItems));
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            if(Model.Music.Count == 0 && Vk.Instance.LoggedIn)
            {
                Model.Refresh();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}