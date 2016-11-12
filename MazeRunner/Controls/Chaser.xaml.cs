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
    /// Interaction logic for Chaser.xaml
    /// </summary>
    public partial class Chaser : UserControl
    {
        private Image Mummy;
        public int x, y;

        public Chaser(int i, int j, int width = 45, int height = 45)
        {
            InitializeComponent();

            Mummy = new Image();
            Mummy.Source = new BitmapImage(new Uri(@"pack://application:,,,/MazeRunner;component/Images/Mummy.png"));
            Mummy.HorizontalAlignment = HorizontalAlignment.Left;
            Mummy.VerticalAlignment = VerticalAlignment.Top;
            Mummy.Height = height;
            Mummy.Width = width;

            this.grd.Children.Add(Mummy);

            x = i;
            y = j;
        }

        public int Run(int[,] MapMatrix)
        {
            //random
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            int move, i, j;
            bool validmove;
            do
            {
                i = x;
                j = y;
                //[0] = up, [1] = down, [2] = left, [3] = right
                move = rnd.Next(4);
                switch (move)
                {
                    case 0:
                        {
                            i -= 1;
                            break;
                        }
                    case 1:
                        {
                            i += 1;
                            break;
                        }
                    case 2:
                        {
                            j -= 1;
                            break;
                        }
                    case 3:
                        {
                            j += 1;
                            break;
                        }
                    default:
                        break;
                }
                validmove = true;
                if ((i < 0 || i >= MapMatrix.GetLength(0)) || (j < 0 || j >= MapMatrix.GetLength(1)))
                {
                    validmove = false;
                }
                else if (MapMatrix[i, j] == 1)
                {
                    validmove = false;
                }
            } while (!validmove);


            x = i;
            y = j;
            return move;
        }
    }
}
