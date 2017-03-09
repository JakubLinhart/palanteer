using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Palanteer.Desktop
{
    public class MapControl : Border
    {
        private UIElement child = null;
        private Point origin;
        private Point start;

        public ObservableCollection<Place> PlacesCollection { get; set; } = new ObservableCollection<Place>();

        private readonly Canvas canvas;

        public MapControl()
        {
            canvas = new Canvas();

            var imageControl = new Image();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var image = new MapSource().GetMapImage();
                imageControl.Source = image;
            }
            canvas.Children.Add(imageControl);

            PlacesCollection.CollectionChanged += PlacesCollection_CollectionChanged;

            base.Child = canvas;
            Initialize(canvas);
        }

        private Dictionary<Place, Button> placeControls = new Dictionary<Place, Button>();

        private void PlacesCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Remove:
                    foreach (Place place in e.OldItems)
                    {
                        var button = placeControls[place];
                        button.Click -= OnPlaceButtonClicked;
                        place.PropertyChanged -= OnPlacePropertyChanged;

                        placeControls.Remove(place);
                        canvas.Children.Remove(button);
                    }
                    break;

                case NotifyCollectionChangedAction.Add:
                    foreach (Place place in e.NewItems)
                    {
                        var button = new Button
                        {
                            RenderTransform = new ScaleTransform(1.5, 1.5),
                            Background = Brushes.White,
                            BorderBrush = Brushes.Black,
                            Content = place.Name,
                        };
                        canvas.Children.Add(button);

                        Canvas.SetLeft(button, place.X);
                        Canvas.SetTop(button, place.Y);

                        button.Click += OnPlaceButtonClicked;
                        button.Tag = place;

                        placeControls[place] = button;
                        place.PropertyChanged += OnPlacePropertyChanged;
                    }
                    break;
            }
        }

        private void OnPlaceButtonClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Place place)
            {
                place.Select();
            }
        }

        private void OnPlacePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Place place)
            {
                var button = placeControls[place];
                button.Content = place.Name;
                Canvas.SetLeft(button, place.X);
                Canvas.SetTop(button, place.Y);
            }
        }

        private TranslateTransform GetTranslateTransform(UIElement element)
        {
            return (TranslateTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is TranslateTransform);
        }

        private ScaleTransform GetScaleTransform(UIElement element)
        {
            return (ScaleTransform)((TransformGroup)element.RenderTransform)
              .Children.First(tr => tr is ScaleTransform);
        }

        private void Initialize(UIElement element)
        {
            this.child = element;
            if (child != null)
            {
                TransformGroup group = new TransformGroup();
                ScaleTransform st = new ScaleTransform();
                group.Children.Add(st);
                TranslateTransform tt = new TranslateTransform();
                group.Children.Add(tt);
                child.RenderTransform = group;
                child.RenderTransformOrigin = new Point(0.0, 0.0);
                this.MouseWheel += child_MouseWheel;
                this.MouseLeftButtonDown += child_MouseLeftButtonDown;
                this.MouseLeftButtonUp += child_MouseLeftButtonUp;
                this.MouseMove += child_MouseMove;
                this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
                  child_PreviewMouseRightButtonDown);
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

                double zoom = e.Delta > 0 ? .2 : -.2;
                if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
                    return;

                Point relative = e.GetPosition(child);
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
                this.Cursor = Cursors.Hand;
                child.CaptureMouse();
            }
        }

        private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (child != null)
            {
                child.ReleaseMouseCapture();
                this.Cursor = Cursors.Arrow;
            }
        }

        void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void child_MouseMove(object sender, MouseEventArgs e)
        {
            if (child != null)
            {
                if (child.IsMouseCaptured)
                {
                    var tt = GetTranslateTransform(child);
                    Vector v = start - e.GetPosition(this);
                    tt.X = origin.X - v.X;
                    tt.Y = origin.Y - v.Y;
                }
            }
        }
    }
}
