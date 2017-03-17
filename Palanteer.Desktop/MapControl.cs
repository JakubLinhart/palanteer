using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Palanteer.Desktop
{
    public class MapControl : Border
    {
        private readonly Dictionary<object, Button> markerControls = new Dictionary<object, Button>();

        private Canvas canvas;
        private UIElement child;
        private Point origin;
        private Point start;
        private MapViewModel mapViewModel;

        public void Initialize(MapViewModel mapViewModel, MapSource mapSource)
        {
            this.mapViewModel = mapViewModel;
            canvas = new Canvas();

            var imageControl = new Image();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var image = mapSource.GetMapImage();
                imageControl.Source = image;
            }
            canvas.Children.Add(imageControl);

            mapViewModel.PropertyChanged += OnMapViewModelPropertyChanged;
            mapViewModel.Places.CollectionChanged += OnMarkersCollectionChanged;
            mapViewModel.Players.CollectionChanged += OnMarkersCollectionChanged;

            Child = canvas;
            Initialize(canvas);
        }

        private void OnMapViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "TrackedPlayer":
                    if (this.mapViewModel.TrackedPlayer != null)
                    {
                        CenterTo(this.mapViewModel.TrackedPlayer);
                    }
                    break;
            }
        }

        private void OnMarkersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (INotifyPropertyChanged marker in e.OldItems)
                    {
                        var button = markerControls[marker];
                        button.Click -= OnMarkerButtonClicked;

                        marker.PropertyChanged -= OnMarkerPropertyChanged;

                        markerControls.Remove(marker);
                        canvas.Children.Remove(button);
                    }
                    break;

                case NotifyCollectionChangedAction.Add:
                    foreach (IMarker marker in e.NewItems)
                    {
                        var button = new Button
                        {
                            RenderTransform = new ScaleTransform(1.5, 1.5),
                            Background = Brushes.White,
                            BorderBrush = Brushes.Black,
                        };
                        canvas.Children.Add(button);

                        Canvas.SetLeft(button, marker.X);
                        Canvas.SetTop(button, marker.Y);
                        button.Content = marker.Name;


                        button.Click += OnMarkerButtonClicked;
                        button.Tag = marker;

                        markerControls[marker] = button;
                        marker.PropertyChanged += OnMarkerPropertyChanged;
                    }
                    break;
            }
        }

        private void OnMarkerButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is PlaceMarker marker)
                marker.Select();
        }

        private void OnMarkerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is IMarker marker)
            {
                var button = markerControls[marker];
                button.Content = marker.Name;
                Canvas.SetLeft(button, marker.X);
                Canvas.SetTop(button, marker.Y);
            }

            if (sender is PlayerMarker player && mapViewModel.TrackedPlayer?.Id == player.Id)
            {
                CenterTo(player);
            }
        }

        private void CenterTo(IMarker marker)
        {
            var translateTransform = GetTranslateTransform(child);
            var scaleTransform = GetScaleTransform(child);
            translateTransform.X = (-marker.X + (this.ActualWidth / scaleTransform.ScaleX / 2)) * scaleTransform.ScaleX;
            translateTransform.Y = (-marker.Y + (this.ActualHeight / scaleTransform.ScaleY / 2)) * scaleTransform.ScaleY;
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform) ((TransformGroup) element.RenderTransform)
                .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform) ((TransformGroup) element.RenderTransform)
                .Children.First(tr => tr is ScaleTransform);
        }

        private void Initialize(UIElement element)
        {
            child = element;
            if (child != null)
            {
                var group = new TransformGroup();
                var st = new ScaleTransform();
                group.Children.Add(st);
                var tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                MouseWheel += child_MouseWheel;
                MouseLeftButtonDown += child_MouseLeftButtonDown;
                MouseLeftButtonUp += child_MouseLeftButtonUp;
                MouseMove += child_MouseMove;
                PreviewMouseRightButtonDown += child_PreviewMouseRightButtonDown;
            }
        }

        public void Reset()
        {
            if (child != null)
            {
                // reset zoom
                var st = GetScaleTransform(child);
                st.ScaleX = 1.0;
                st.ScaleY = 1.0;

                // reset pan
                var tt = GetTranslateTransform(child);
                tt.X = 0.0;
                tt.Y = 0.0;
            }
        }

        private void child_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (child != null)
            {
                var st = GetScaleTransform(child);
                var tt = GetTranslateTransform(child);

                var zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                    return;

                var relative = e.GetPosition(child);
                double abosuluteX;
                double abosuluteY;

                abosuluteX = relative.X * st.ScaleX + tt.X;
                abosuluteY = relative.Y * st.ScaleY + tt.Y;

                st.ScaleX += zoom;
                st.ScaleY += zoom;

                tt.X = abosuluteX - relative.X * st.ScaleX;
                tt.Y = abosuluteY - relative.Y * st.ScaleY;
            }
        }

        private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                var tt = GetTranslateTransform(child);
                start = e.GetPosition(this);
                origin = new Point(tt.X, tt.Y);
                Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                Cursor = Cursors.Arrow;
            }
        }

        private void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    var v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                }
            }
        }
    }
}