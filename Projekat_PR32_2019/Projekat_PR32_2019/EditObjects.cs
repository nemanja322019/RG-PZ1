using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace Projekat_PR32_2019
{
    public class EditObjects
    {
        public static void UpdateEllipse(object sender, MouseButtonEventArgs e,MainWindow mainWindow,Grid grid, Ellipse ellipse)
        {
            if ((bool)mainWindow.Edit_RadioButton.IsChecked)
            {
                DrawEllipseWindow drawElipseWindow = new DrawEllipseWindow(mainWindow,grid, ellipse);
                drawElipseWindow.Show();
            }

        }
        public static void UpdatePolygon(object sender, MouseButtonEventArgs e, MainWindow mainWindow, Grid grid, Polygon polygon)
        {
            if ((bool)mainWindow.Edit_RadioButton.IsChecked)
            {
                DrawPolygonWindow drawPolygonWindow = new DrawPolygonWindow(mainWindow, grid, polygon);
                drawPolygonWindow.Show();
            }

        }
        public static void UpdateText(object sender, MouseButtonEventArgs e, MainWindow mainWindow, Grid grid, TextBlock textBlock)
        {
            if ((bool)mainWindow.Edit_RadioButton.IsChecked)
            {
                AddTextWindow addTextWindow = new AddTextWindow(mainWindow, grid, textBlock);
                addTextWindow.Show();
            }

        }
    }
}
