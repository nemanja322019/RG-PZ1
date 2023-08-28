using Projekat_PR32_2019.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Projekat_PR32_2019
{
    /// <summary>
    /// Interaction logic for SwitchStatusWindow.xaml
    /// </summary>
    public partial class SwitchStatusWindow : Window
    {
        public MainWindow mainWindow;
        public SwitchEntity switchEntity;
        Ellipse ellipse;
        public SwitchStatusWindow(SwitchEntity switchEntity, MainWindow mainWindow, Ellipse ellipse)
        {
            InitializeComponent();
            this.mainWindow = mainWindow;
            this.switchEntity = switchEntity;
            this.ellipse = ellipse;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(cmbStatus.SelectedItem != null)
            {
                switchEntity.Status = ((ComboBoxItem)cmbStatus.SelectedItem).Content.ToString();
                ellipse.SetValue(FrameworkElement.TagProperty, switchEntity);

                string tooltip = ellipse.ToolTip.ToString();
                int statusIndex = tooltip.IndexOf("Status: ");
                string newTooltip = "";

                if (statusIndex != -1)
                {
                    int statusValueStart = statusIndex + "Status: ".Length;
                    int statusValueEnd = tooltip.IndexOf('\n', statusValueStart);
                    if (statusValueEnd == -1)
                        statusValueEnd = tooltip.Length;

                    newTooltip = tooltip.Substring(0, statusValueStart) + switchEntity.Status + tooltip.Substring(statusValueEnd);

                }
                ellipse.ToolTip = newTooltip;
                this.Close();
            }
        }
    }
}
