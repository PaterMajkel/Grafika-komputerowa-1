using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Diagnostics;

namespace grafa1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DispatcherTimer timer = new DispatcherTimer();

        public MainWindow()
        {
            InitializeComponent();
            timer.Tick += MainTimerEvent;
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Start();
        }

        Brush color = new SolidColorBrush(Color.FromRgb((byte)255, (byte)255, (byte)255));
        Point dragStart, offset, lastPosition;
        TextBlock textBlock;

        bool movable = false;
        Shape shape;
        UIElement dragObject = null;
        List<Shape> shapes = new List<Shape>();
        string mode = String.Empty;
        private void Rectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            return;
        }

        private void Canva_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Canva.CaptureMouse();

            lastPosition = dragStart = e.MouseDevice.GetPosition(Canva);
            this.color = PickBrush();
            shape = this.mode switch
            {
                "ellipse" => new Ellipse
                {
                    Stroke = color,
                    StrokeThickness = 0,
                    Fill = color,
                },
                "line" => new Line
                {
                    Stroke = color,
                    StrokeThickness = 4,
                    Fill = color,
                },
                "polygon" => new Polygon
                {
                    Stroke = color,
                    StrokeThickness = 0,
                    Fill = color,
                },
                "pencil" => new Line
                {
                    Stroke = color,
                    StrokeThickness = 4,
                    Fill = color,
                },
                _ => new Rectangle
                {
                    Stroke = color,
                    StrokeThickness = 0,
                    Fill = color,
                }
            };
            if (mode == "text")
            {
                this.textBlock = new()
                {
                    FontSize = 30,
                    Foreground = color,
                };
                Canvas.SetTop(textBlock, dragStart.Y);
                Canvas.SetLeft(textBlock, dragStart.X);
                Canva.Children.Add(textBlock);
                shape = null;
                return;

            }
            if (this.mode == "pencil")
                return;
            Canva.Children.Add(shape);
            shapes.Add(shape);
            this.dragObject = shapes[shapes.Count - 1];
        }
        private Brush PickBrush()
        {
            Brush result = Brushes.Transparent;

            Random rnd = new Random();

            Type brushesType = typeof(Brushes);

            PropertyInfo[] properties = brushesType.GetProperties();

            int random = rnd.Next(properties.Length);
            result = (Brush)properties[random].GetValue(null, null);

            return result;
        }
        private void Canva_MouseMove(object sender, MouseEventArgs e)
        {
            if (!Canva.IsMouseCaptured || shape == null)
            {
                return;
            }

            Point location = e.MouseDevice.GetPosition(Canva);

            double minX = Math.Min(location.X, dragStart.X);
            double minY = Math.Min(location.Y, dragStart.Y);
            double maxX = Math.Max(location.X, dragStart.X);
            double maxY = Math.Max(location.Y, dragStart.Y);

            if (this.mode == "pencil")
            {
                var shape = new Line
                {
                    Stroke = color,
                    StrokeThickness = 4,
                    Fill = color,
                };

                ((Line)shape).X1 = lastPosition.X;
                ((Line)shape).Y1 = lastPosition.Y;
                ((Line)shape).X2 = location.X;
                ((Line)shape).Y2 = location.Y;

                lastPosition = e.GetPosition(Canva);

                Canva.Children.Add(shape);
                shapes.Add(shape);
                this.dragObject = shapes[shapes.Count - 1];
                return;
            }

            if (this.mode == "polygon")
            {
                ((Polygon)shape).Points = new() { new Point(dragStart.X, dragStart.Y), new Point(location.X, location.Y), new Point(dragStart.X, location.Y) };
                return;
            }

            if (this.mode == "line")
            {
                ((Line)shape).X1 = dragStart.X;
                ((Line)shape).Y1 = dragStart.Y;
                ((Line)shape).X2 = location.X;
                ((Line)shape).Y2 = location.Y;
                return;
            }
            Canvas.SetTop(shape, minY);
            Canvas.SetLeft(shape, minX);

            double height = maxY - minY;
            double width = maxX - minX;


            shape.Height = Math.Abs(height);
            shape.Width = Math.Abs(width);

            /* else if (this.dragObject != null)
             {
                 var position = e.GetPosition(sender as IInputElement);
                 Canvas.SetTop(this.dragObject, position.Y - this.offset.Y);
                 Canvas.SetLeft(this.dragObject, position.X - this.offset.X);
             }
             else return;*/

        }

        private void MainTimerEvent(object sender, EventArgs e)
        {
            //dlaczego tak, a nie inczaej? Bo windows głupio zbiera klikane przyciski...
            if (Keyboard.IsKeyDown(Key.LeftShift))
                movable = true;
            if (dragObject == null)
                return;
            this.offset.Y -= Canvas.GetTop(this.dragObject);
            this.offset.X -= Canvas.GetLeft(this.dragObject);
            double X = Canvas.GetLeft(this.dragObject);
            double Y = Canvas.GetTop(this.dragObject);
            int x = 2;
            int y = 2;
            //var position = e.GetPosition(sender as IInputElement);
            if (mode == "polygon")
            {

                if (Keyboard.IsKeyDown(Key.Up))
                {
                    if (!movable)
                        ((Polygon)this.dragObject).Points = new() { new Point(((Polygon)this.dragObject).Points[0].X, ((Polygon)this.dragObject).Points[0].Y - y),
                                new Point(((Polygon)this.dragObject).Points[1].X, ((Polygon)this.dragObject).Points[1].Y - y),
                                new Point(((Polygon)this.dragObject).Points[2].X, ((Polygon)this.dragObject).Points[2].Y - y) };
                    else
                        ((Polygon)this.dragObject).Points = new() { new Point(((Polygon)this.dragObject).Points[0].X, ((Polygon)this.dragObject).Points[0].Y - y),
                                new Point(((Polygon)this.dragObject).Points[1].X, ((Polygon)this.dragObject).Points[1].Y),
                                new Point(((Polygon)this.dragObject).Points[2].X, ((Polygon)this.dragObject).Points[2].Y) };
                }
                if (Keyboard.IsKeyDown(Key.Down))
                {
                    if (!movable)
                        ((Polygon)this.dragObject).Points = new() { new Point(((Polygon)this.dragObject).Points[0].X, ((Polygon)this.dragObject).Points[0].Y + y),
                                new Point(((Polygon)this.dragObject).Points[1].X, ((Polygon)this.dragObject).Points[1].Y + y),
                                new Point(((Polygon)this.dragObject).Points[2].X, ((Polygon)this.dragObject).Points[2].Y + y) };
                    else
                        ((Polygon)this.dragObject).Points = new() { new Point(((Polygon)this.dragObject).Points[0].X, ((Polygon)this.dragObject).Points[0].Y),
                                new Point(((Polygon)this.dragObject).Points[1].X, ((Polygon)this.dragObject).Points[1].Y + y),
                                new Point(((Polygon)this.dragObject).Points[2].X, ((Polygon)this.dragObject).Points[2].Y + y) };
                }
                if (Keyboard.IsKeyDown(Key.Left))
                {
                    if (!movable)
                        ((Polygon)this.dragObject).Points = new() { new Point(((Polygon)this.dragObject).Points[0].X - x, ((Polygon)this.dragObject).Points[0].Y),
                                new Point(((Polygon)this.dragObject).Points[1].X - x, ((Polygon)this.dragObject).Points[1].Y),
                                new Point(((Polygon)this.dragObject).Points[2].X - x, ((Polygon)this.dragObject).Points[2].Y) };
                    else
                        ((Polygon)this.dragObject).Points = new() { new Point(((Polygon)this.dragObject).Points[0].X, ((Polygon)this.dragObject).Points[0].Y),
                                new Point(((Polygon)this.dragObject).Points[1].X, ((Polygon)this.dragObject).Points[1].Y),
                                new Point(((Polygon)this.dragObject).Points[2].X - x, ((Polygon)this.dragObject).Points[2].Y) };
                }
                if (Keyboard.IsKeyDown(Key.Right))
                {
                    if (!movable)
                        ((Polygon)this.dragObject).Points = new() { new Point(((Polygon)this.dragObject).Points[0].X + x, ((Polygon)this.dragObject).Points[0].Y),
                                new Point(((Polygon)this.dragObject).Points[1].X + x, ((Polygon)this.dragObject).Points[1].Y),
                                new Point(((Polygon)this.dragObject).Points[2].X + x, ((Polygon)this.dragObject).Points[2].Y) };
                    else
                        ((Polygon)this.dragObject).Points = new() { new Point(((Polygon)this.dragObject).Points[0].X, ((Polygon)this.dragObject).Points[0].Y),
                                new Point(((Polygon)this.dragObject).Points[1].X + x , ((Polygon)this.dragObject).Points[1].Y),
                                new Point(((Polygon)this.dragObject).Points[2].X, ((Polygon)this.dragObject).Points[2].Y) };
                }
            }
            else
            {
                if (Keyboard.IsKeyDown(Key.Up))
                {
                    Canvas.SetTop(this.dragObject, Y - y >= 0 ? Y - y : Y);
                    if (movable)
                        ((Shape)this.dragObject).Height += y;
                }
                if (Keyboard.IsKeyDown(Key.Down))
                {
                    if (movable)
                        ((Shape)this.dragObject).Height += y;
                    else
                        Canvas.SetTop(this.dragObject, Y + y);
                }
                if (Keyboard.IsKeyDown(Key.Left))
                {
                    Canvas.SetLeft(this.dragObject, X - x >= 0 ? X - x : X);
                    if (movable)
                        ((Shape)this.dragObject).Width += x;
                }
                if (Keyboard.IsKeyDown(Key.Right))
                {
                    if (movable)
                        ((Shape)this.dragObject).Width += x;
                    else
                        Canvas.SetLeft(this.dragObject, X + x);
                }
            }

            movable = false;

        }
        private void WriteSmth(object sender, KeyEventArgs e)
        {
            if (mode == "text")
            {
                this.textBlock.Text += e.Key.ToString().ToLower();
            }
        }

        private void Save(object sender, RoutedEventArgs e)
        {
            RenderTargetBitmap rtb = new RenderTargetBitmap((int)Canva.ActualWidth,
            (int)Canva.ActualHeight, 96d, 96d, System.Windows.Media.PixelFormats.Default);
            rtb.Render(Canva);


            BitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(rtb));

            using (var fs = System.IO.File.OpenWrite("canva.png"))
            {
                pngEncoder.Save(fs);
            }
        }

        private void Pick_Mode(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            this.mode = ((Button)sender).Tag.ToString();
        }

        private void Canva_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //this.dragObject = null;
            Canva.ReleaseMouseCapture();
            shape = null;
        }
    }
}
