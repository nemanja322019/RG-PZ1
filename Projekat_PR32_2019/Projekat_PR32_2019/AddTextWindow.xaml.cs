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
using System.Windows.Threading;

namespace Projekat_PR32_2019
{
    /// <summary>
    /// Interaction logic for AddTextWindow.xaml
    /// </summary>
    public partial class AddTextWindow : Window
    {
        Point point;
        MainWindow mainWindow;
        Grid grid;
        TextBlock textBlock;
        bool update = false;
        public AddTextWindow(Point point, MainWindow mainWindow)
        {
            InitializeComponent();
            this.point = point;
            this.mainWindow = mainWindow;
            update = false;
        }
        public AddTextWindow(MainWindow mainWindow,Grid grid, TextBlock textBlock)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;

            this.grid = grid;
            this.textBlock = textBlock;
            update = true;
            tb_AddText.IsReadOnly = true;
            tb_AddText.Text = textBlock.Text;
        }

        FontDialog dig = new FontDialog();

        private void butonText_TextFont_Click(object sender, RoutedEventArgs e)
        {

            if (dig.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FontFamilyConverter ffc = new FontFamilyConverter();

                tb_AddText.FontSize = dig.Font.Size;
                tb_AddText.FontFamily = (FontFamily)ffc.ConvertFromString(dig.Font.Name);

                tb_AddText.FontFamily = new FontFamily(dig.Font.Name);
                tb_AddText.FontSize = dig.Font.Size * 98.0 / 72.0;
                tb_AddText.FontWeight = dig.Font.Bold ? FontWeights.Bold : FontWeights.Regular;
                tb_AddText.FontStyle = dig.Font.Italic ? FontStyles.Italic : FontStyles.Normal;
            }
        }

        Brush textColor;
        private void buttonText_TextColor_Click(object sender, RoutedEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                textColor = new SolidColorBrush(Color.FromArgb(colorDialog.Color.A, colorDialog.Color.R, colorDialog.Color.G, colorDialog.Color.B));

            }
        }

        private void button_AddText_Click(object sender, RoutedEventArgs e)
        {
            if (!tb_AddText.Text.Equals(String.Empty) && textColor !=null) {
                if (!update)
                {
                    grid = new Grid()
                    {
                        Height = Double.NaN,
                        Width = Double.NaN,
                    };

                    textBlock = new TextBlock();
                    textBlock.Text = tb_AddText.Text;
                    textBlock.Foreground = textColor;
                    textBlock.FontSize = dig.Font.Size;
                    textBlock.LayoutTransform = mainWindow.canvas.LayoutTransform.Inverse as Transform;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    grid.Children.Add(textBlock);

                    //grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                   // grid.Arrange(new Rect(0, 0, grid.DesiredSize.Width, grid.DesiredSize.Height));


                    Canvas.SetLeft(grid, point.X);
                    Canvas.SetTop(grid, point.Y);
                    mainWindow.canvas.Children.Add(grid);
                    mainWindow.History.Add(textBlock);
                    mainWindow.UndoRedoPosition++;
                    grid.MouseLeftButtonDown += (esender, ee) => EditObjects.UpdateText(esender, ee, mainWindow, grid, textBlock);
                    mainWindow.Text_RadioButton.IsChecked = false;
                    mainWindow.lastElement = grid;
                    mainWindow.isEllipse = false;
                    this.Close();
                }
                else
                {
                    textBlock.Foreground = textColor;
                    textBlock.FontSize = dig.Font.Size;
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
