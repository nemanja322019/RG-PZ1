using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Projekat_PR32_2019
{
    /// <summary>
    /// Interaction logic for DrawPolygonWindow.xaml
    /// </summary>
    public partial class DrawPolygonWindow : Window
    {
        public MainWindow mainWindow;
        List<Point> pointList;
        public Grid grid;
        public Polygon polygon;
        public bool update = false;
        public DrawPolygonWindow(MainWindow main, List<Point> points)
        {
            InitializeComponent();
            this.mainWindow = main;
            this.pointList = points;
            update = false;
        }

        public DrawPolygonWindow(MainWindow main, Grid grid, Polygon polygon)
        {
            InitializeComponent();
            this.mainWindow = main;
            
            this.grid = grid;
            this.polygon = polygon;
            update=true;
            tb_AddText.IsReadOnly = true;
            button_TextColor.IsEnabled = false;
            tb_cThickness.Text = polygon.StrokeThickness.ToString();
        }

        Brush polygonColor;

        private void button_PolygonColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                polygonColor = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));

            }
        }

        Brush textColor;
        private void button_TextColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textColor = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));

            }
        }

        private void button_DrawPolygon_Click(object sender, RoutedEventArgs e)
        {
            double stroke = 0;
            if (!tb_cThickness.Text.Equals(String.Empty) && Double.TryParse(tb_cThickness.Text, out stroke) && stroke > 0 && polygonColor != null) {
                if (!update)
                {
                    double minX = Double.MaxValue;
                    double minY = Double.MaxValue;
                    double maxX = Double.MinValue;
                    double maxY = Double.MinValue;
                    foreach (var item in pointList)
                    {
                        if (item.X > maxX)
                        {
                            maxX = item.X;
                        }
                        if (item.Y > maxY)
                        {
                            maxY = item.Y;
                        }
                        if (item.X < minX)
                        {
                            minX = item.X;
                        }
                        if (item.Y < minY)
                        {
                            minY = item.Y;
                        }
                    }

                    grid = new Grid()
                    {
                        Height = maxY - minY,
                        Width = maxX - minX
                    };

                    polygon = new Polygon()
                    {
                        Fill = polygonColor,
                        StrokeThickness = double.Parse(tb_cThickness.Text),
                        Stroke = Brushes.Black,

                    };
                    if (button_Transparent.IsChecked == true)
                        polygon.Opacity = 0.25;

                    foreach (var item in pointList)
                    {
                        Point point = new Point(item.X, item.Y);
                        point.X -= minX;
                        point.Y -= minY;
                        polygon.Points.Add(point);

                    }


                    grid.Children.Add(polygon);

                    //mainWindow.canvas.Children.Add(polygon);

                    mainWindow.PointsList.Clear();


                    TextBlock textBlock = new TextBlock();
                    textBlock.LayoutTransform = mainWindow.canvas.LayoutTransform.Inverse as Transform;
                    textBlock.Text = tb_AddText.Text;
                    textBlock.Foreground = textColor;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    grid.Children.Add(textBlock);

                    Canvas.SetLeft(grid, minX);
                    Canvas.SetTop(grid, minY);
                    mainWindow.canvas.Children.Add(grid);
                    polygon.MouseLeftButtonDown += (esender, ee) => EditObjects.UpdatePolygon(esender, ee, mainWindow, grid, polygon);
                    mainWindow.Polygon_RadioButton.IsChecked = false;
                    mainWindow.History.Add(polygon);
                    mainWindow.UndoRedoPosition++;
                    mainWindow.lastElement = grid;
                    mainWindow.isEllipse = false;
                    this.Close();
                }
                else
                {
                    polygon.StrokeThickness = double.Parse(tb_cThickness.Text);
                    polygon.Fill = polygonColor;
                    if (button_Transparent.IsChecked == true)
                        polygon.Opacity = 0.25;
                    else
                        polygon.Opacity = 1;
                    mainWindow.Edit_RadioButton.IsChecked = false;
                    this.Close();
                } 
            }
            else
            {
                System.Windows.MessageBox.Show("Invalid input!", "Error!", MessageBoxButton.OK, MessageBoxImage.Error);

            }
        }
    }
}
