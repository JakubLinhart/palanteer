using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Palanteer.Desktop
{
    public class MapControl : Border
    {
        private readonly Dictionary<object, Button> markerControls = new Dictionary<object, Button>();

        private Canvas canvas;
        private UIElement child;
        private Image imageControl;

        private BitmapImage mapImage;
        private MapViewModel mapViewModel;
        private Point origin;
        private Point start;
        private bool xRay;
        private BitmapImage xrayMapImage;

        public bool XRay
        {
            get { return xRay; }
            set
            {
                if (xRay != value)
                    imageControl.Source = value ? xrayMapImage : mapImage;
                xRay = value;
            }
        }

        public void Initialize(MapViewModel mapViewModel, MapSource mapSource)
        {
            this.mapViewModel = mapViewModel;
            canvas = new Canvas();

            imageControl = new Image();
            if (!DesignerProperties.GetIsInDesignMode(this))
                Task.Run(() => LoadImages(mapSource));
            canvas.Children.Add(imageControl);

            mapViewModel.PropertyChanged += OnMapViewModelPropertyChanged;
            mapViewModel.Places.CollectionChanged += OnMarkersCollectionChanged;
            mapViewModel.Players.CollectionChanged += OnMarkersCollectionChanged;

            Child = canvas;
            Initialize(canvas);
        }

        private async void LoadImages(MapSource mapSource)
        {
            mapImage = await mapSource.GetMapImage();
            xrayMapImage = await mapSource.GetXRayMapImage();

            Dispatcher.Invoke(() => imageControl.Source = mapImage);
        }

        private void OnMapViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "TrackedPlayer":
                    if (mapViewModel.TrackedPlayer != null)
                        CenterTo(mapViewModel.TrackedPlayer);
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
                        var transformGroup = new TransformGroup();
                        transformGroup.Children.Add(new RotateTransform(-45));

                        var button = new Button
                        {
                            RenderTransform = transformGroup,
                            Background = Brushes.White,
                            BorderBrush = Brushes.Black
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
                CenterTo(player);
        }

        private void CenterTo(IMarker marker)
        {
            var translateTransform = GetTranslateTransform(child);
            var scaleTransform = GetScaleTransform(child);
            var rotateTransform = GetRotateTransform(child);

            var dimensions = new Point(ActualWidth, ActualHeight);
            var origDim = rotateTransform.Inverse.Transform(scaleTransform.Inverse.Transform(dimensions));

            var point = new Point();
            point.X = -marker.X + origDim.X / 2;
            point.Y = -marker.Y + origDim.Y / 2;

            var rotatedPoint = scaleTransform.Transform(rotateTransform.Transform(point));

            translateTransform.X = rotatedPoint.X;
            translateTransform.Y = rotatedPoint.Y;
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

        private RotateTransform GetRotateTransform(UIElement element)
        {
            return ((TransformGroup) element.RenderTransform)
                .Children.OfType<RotateTransform>().First();
        }

        private void Initialize(UIElement element)
        {
            child = element;
            if (child != null)
            {
                var group = new TransformGroup();
                var rotateTransform = new RotateTransform(45);
                group.Children.Add(rotateTransform);
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
                var scaleTransform = GetScaleTransform(child);
                var translateTransform = GetTranslateTransform(child);
                var rotateTransform = GetRotateTransform(child);

                var zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && (scaleTransform.ScaleX < .4 || scaleTransform.ScaleY < .4))
                    return;

                var relative = e.GetPosition(child);
                var transformedRelative = rotateTransform.Transform(scaleTransform.Transform(relative));

                var abosuluteX = transformedRelative.X + translateTransform.X;
                var abosuluteY = transformedRelative.Y + translateTransform.Y;

                scaleTransform.ScaleX += zoom;
                scaleTransform.ScaleY += zoom;

                transformedRelative = rotateTransform.Transform(scaleTransform.Transform(relative));
                translateTransform.X = abosuluteX - transformedRelative.X;
                translateTransform.Y = abosuluteY - transformedRelative.Y;

                var group = new TransformGroup();
                if (scaleTransform.ScaleX < 1)
                    group.Children.Add((Transform) scaleTransform.Inverse);
                group.Children.Add((Transform) rotateTransform.Inverse);
                foreach (var markerControl in markerControls.Values)
                {
                    if (markerControl.Tag is PlayerMarker)
                        markerControl.RenderTransform = group;
                }
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