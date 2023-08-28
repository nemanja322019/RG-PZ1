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
    /// Interaction logic for DrawEllipseWindow.xaml
    /// </summary>
    public partial class DrawEllipseWindow : Window
    {
        public Point point { get; set; }
        public MainWindow mainWindow { get; set; }
        public bool update;
        public Grid grid;
        public Ellipse ellipse;
        public DrawEllipseWindow(Point point, MainWindow mainWindow)
        {
            InitializeComponent();
            this.point = point;
            this.mainWindow = mainWindow;
            update = false;

        }

        public DrawEllipseWindow(MainWindow mainWindow,Grid grid,Ellipse ellipse)
        {
            
            InitializeComponent();
            this.mainWindow = mainWindow;

            this.grid = grid;
            this.ellipse = ellipse;
            update = true;
            tb_RadiusX.Text = (grid.Width/4).ToString();
            tb_RadiusY.Text = (grid.Height/4).ToString();
            tb_RadiusX.IsReadOnly = true;
            tb_RadiusY.IsReadOnly = true;
            tb_Thickness.Text = ellipse.StrokeThickness.ToString();
        }

        Brush ellipseColor;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ellipseColor = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));
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

        private void button_DrawEllipse_Click(object sender, RoutedEventArgs e)
        {
            double stroke = 0;
            if (!tb_RadiusY.Text.Equals(String.Empty) && Double.TryParse(tb_RadiusY.Text, out stroke) && stroke > 0 && !tb_RadiusX.Text.Equals(String.Empty) && Double.TryParse(tb_RadiusX.Text, out stroke) && stroke > 0 && !tb_Thickness.Text.Equals(String.Empty) && Double.TryParse(tb_Thickness.Text, out stroke) && stroke > 0 && ellipseColor != null)
            {
                if (!update)
                {
                    grid = new Grid()
                    {
                        Width = 2 * double.Parse(tb_RadiusX.Text) * 2,
                        Height = 2 * double.Parse(tb_RadiusY.Text) * 2 
                    };
                    ellipse = new Ellipse();

                    ellipse.Fill = ellipseColor;
                    ellipse.Stroke = Brushes.Black;
                    ellipse.StrokeThickness = double.Parse(tb_Thickness.Text);
                    if (button_Transparent.IsChecked == true)
                        ellipse.Opacity = 0.25;
                    grid.Children.Add(ellipse);

                    TextBlock textBlock = new TextBlock();
                    textBlock.Text = tb_AddText.Text;
                    textBlock.Foreground = textColor;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    textBlock.LayoutTransform = mainWindow.canvas.LayoutTransform.Inverse as Transform;
                    grid.Children.Add(textBlock);

                    double adjustedX = point.X - grid.Width;

                    Canvas.SetLeft(grid, adjustedX);
                    Canvas.SetTop(grid, point.Y);

                    mainWindow.History.Add(ellipse);
                    mainWindow.UndoRedoPosition++;

                    mainWindow.canvas.Children.Add(grid);
                    grid.MouseLeftButtonDown += (esender, ee) => EditObjects.UpdateEllipse(esender, ee, mainWindow, grid, ellipse);
                    mainWindow.Ellipse_RadioButton.IsChecked = false;
                    mainWindow.lastElement = grid;
                    mainWindow.isEllipse = true;
                    this.Close();

                }
                else
                {
                    ellipse.Fill = ellipseColor;
                    ellipse.StrokeThickness = double.Parse(tb_Thickness.Text);
                    if (button_Transparent.IsChecked == true)
                        ellipse.Opacity = 0.25;
                    else
                        ellipse.Opacity = 1;
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
