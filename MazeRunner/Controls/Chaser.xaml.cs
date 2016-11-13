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
        #region A*
        double h(ref Map.Pos k, Map.Pos m)
        {
            return Math.Max(Math.Abs(k.X - m.X), Math.Abs(k.Y - m.Y));
        }
        public List<Map.Pos> runAstar(int[,] MapMatrix, Map.Pos Mummy, Map.Pos Runner)
        {

            int i, j, next = 0, r, n, m, dem1, dem2, co = -1, cc = -1, ci;//co mean checkopen cc mean checkclose
            int xstart = Mummy.X, ystart = Mummy.Y;
            int xgoal = Runner.X, ygoal = Runner.Y;
            m = 13;
            n = 13;
            bool found = false;
            int[] dx = new int[] {  0, -1,1 ,0};
            int[] dy = new int[] { -1,  0, 0,1};
            List<Map.Pos> open, close;
            Map.Pos temp, temp2;
            open = new List<Map.Pos>();
            close = new List<Map.Pos>();
            temp = new Map.Pos(xstart, ystart, 0, 0);
            open.Add(temp);
            while (open.Count != 0)
            {
                for (i = 0; i < open.Count; i++) if (open[i].f < open[next].f) next = i;// choicing best
                //-- next is current pos
                if (open[next].X == xgoal && open[next].Y == ygoal)
                {
                    found = true;
                    break;
                };
                for (r = 0; r < 4; r++)
                {

                    i = open[next].X + dx[r];
                    j = open[next].Y + dy[r];

                    if (i >= 0 && j >= 0 && i < n && j < m)
                        if (MapMatrix[i, j] == 0 || MapMatrix[i, j] == 2)
                        {
                            for (dem1 = 0; dem1 < open.Count; dem1++) if (open[dem1].X == i && open[dem1].Y == j) co = dem1;// check pos x in list open
                            for (dem2 = 0; dem2 < close.Count; dem2++) if (close[dem2].X == i && close[dem2].Y == j) cc = dem2;// check pos y in close;
                            temp.X = i;
                            temp.Y = j;
                            h(ref temp, Runner);
                            if (co != -1)
                            {
                                if (open[co].f > temp.f + open[next].g + 1)
                                {
                                    temp.g = open[next].g + 1;
                                    temp.f = temp.h + temp.g;
                                    temp.xpre = open[next].X;
                                    temp.ypre = open[next].Y;
                                    open[co] = temp;
                                };

                            }
                            else
                                if (cc == -1) // chua có trong open và close
                            {
                                temp.g = open[next].g + 1;
                                temp.f = temp.h + temp.g;
                                temp.xpre = open[next].X;
                                temp.ypre = open[next].Y;
                                open.Add(temp);
                            }
                            else// n?u có trong close 
                                    if (close[cc].f > temp.h + open[next].g + 1)
                            {
                                temp.g = open[next].g + 1;
                                temp.f = temp.h + temp.g;
                                temp.xpre = open[next].X;
                                temp.ypre = open[next].Y;
                                open.Add(temp);
                                close.RemoveAt(cc);
                            };


                        }

                    co = -1;
                    cc = -1;

                };
                close.Add(open[next]);
                open.RemoveAt(next);
                next = 0;
            }
            List<Map.Pos> path;
            path = new List<Map.Pos>();
            Map.Pos tg;
            tg = new Map.Pos();
            if (found)
            {

                temp2 = open[next];
                while (temp2.X != xstart || temp2.Y != ystart)
                {
                    tg.X = temp2.X;
                    tg.Y = temp2.Y;
                    path.Add(tg);
                    for (i = 0; i < close.Count; i++)
                    {
                        if (temp2.xpre == close[i].X && temp2.ypre == close[i].Y)
                        {
                            temp2 = close[i];
                            break;
                        }
                    }
                }
                return path;
            }
            else return null;

        }
        static void vl(Point oct2, int x, int y, ref double kq)
        {
            kq = Math.Sqrt((oct2.X - x) * (oct2.X - x) + (oct2.Y - y) * (oct2.X - y));
        }
        public int Asmove(int[,] MapMatrix, Chaser Mummy, Runner runner)
        {

            Map.Pos temp = new Map.Pos();
            temp.X = runner.x;
            temp.Y = runner.y;
            Map.Pos temp2 = new Map.Pos();
            temp2.X = Mummy.x;
            temp2.Y = Mummy.y;
            int move = 1, i;
            bool found = false;
            List<Map.Pos> Path = runAstar(MapMatrix, temp2, temp);
            if (Path != null)
            {
                int xt, yt;
                xt = Path[Path.Count - 1].X;
                yt = Path[Path.Count - 1].Y;
                Path.RemoveAt(Path.Count - 1);
                //[0] = up, [1] = down, [2] = left, [3] = right
                this.x = xt;
                this.y = yt;
                if (xt == 0 && yt == 1) return 3;
                if (xt == -1 && yt == 0) return 1;
                if (xt == 1 && yt == 0) return 0;
                if (xt == 0 && yt == -1) return 2;
                return move;
            }
            else return Run(MapMatrix);

        }
    }



}


        #endregion
