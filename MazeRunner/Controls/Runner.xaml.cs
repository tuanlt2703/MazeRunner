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
    /// Interaction logic for Runner.xaml
    /// </summary>
    public partial class Runner : UserControl
    {
        private Image Mummy;
        public int x, y;

        public Runner(int i, int j, int width = 45, int height = 45)
        {
            InitializeComponent();

            Mummy = new Image();
            Mummy.Source = new BitmapImage(new Uri(@"pack://application:,,,/MazeRunner;component/Images/Explorer.png"));
            Mummy.HorizontalAlignment = HorizontalAlignment.Left;
            Mummy.VerticalAlignment = VerticalAlignment.Top;
            Mummy.Height = height;
            Mummy.Width = width;

            this.grd.Children.Add(Mummy);

            x = i;
            y = j;
        }
    }
}
