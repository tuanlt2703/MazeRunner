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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MazeRunner.Controls
{
    /// <summary>
    /// Interaction logic for GameControl.xaml
    /// </summary>
    public partial class GameControl : UserControl
    {
        public MainWindow Main;

        public GameControl()
        {
            InitializeComponent();
        }

        #region Events
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            Main.StartGame();
        }

        private void btnRandom_Click(object sender, RoutedEventArgs e)
        {
            Main.Map.LoadRandomMap();
        }

        private void cmbStages_DropDownClosed(object sender, EventArgs e)
        {
            if (cmbStages.SelectedIndex == 0)
            {
                Main.Map.gdMap.Children.Clear();
                Main.Map.Load(cmbStages.SelectedIndex);
            }
            else
            {
                Main.Map.Load(cmbStages.SelectedIndex);
            }
        }
        #endregion
    }
}
