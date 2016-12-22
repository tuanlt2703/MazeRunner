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

        public Chaser(int i, int j, double width, double height)
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

        #region A*
        static double h(ref Position k, Position m)
        {
            return k.h=Math.Max(Math.Abs(k.X - m.X), Math.Abs(k.Y - m.Y));
        }
        static double h(ref Position runner, Position goal,Position chaser)
        {
            double d = Math.Max(Math.Abs(runner.X - chaser.X), Math.Abs(runner.Y - chaser.Y));
            if (d < 10 && d > 0) d = (10 - d)*d;
            return runner.h= Math.Max(Math.Abs(runner.X - goal.X), Math.Abs(runner.Y - goal.Y))-d;
        }
        private static List<Position> runAstar(int[,] emuMap)
        {
            int i, j, next = 0, r, n, m, dem1, dem2, co = -1, cc = -1, ci;//co mean checkopen cc mean checkclose
            int xstart = 0, ystart = 0;
            Position emuRunner = new Position();
            int xgoal = 0, ygoal = 0;
            for (i = 0; i < emuMap.GetLength(0); i++)
            {
                for (j = 0; j < emuMap.GetLength(1); j++)
                {
                    if (emuMap[i, j] == 4) //2 runner 4 chaser;
                    {
                        xstart = i;
                        ystart = j;
                    }
                    if (emuMap[i, j] == 2)
                    {
                        xgoal = i;
                        ygoal = j;
                    }
                }
            }
            emuRunner.X = xgoal;
            emuRunner.Y = ygoal;
            m = emuMap.GetLength(0);
            n = emuMap.GetLength(1);
            bool found = false;
            int[] dx = new int[] { 0, -1, 1, 0 };
            int[] dy = new int[] { -1, 0, 0, 1 };
            List<Position> open, close;
            Position temp, temp2;
            open = new List<Position>();
            close = new List<Position>();
            temp = new Position(xstart, ystart, 0, 0);
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
                        if (emuMap[i, j] == 0 || emuMap[i, j] == 2)
                        {
                            for (dem1 = 0; dem1 < open.Count; dem1++) if (open[dem1].X == i && open[dem1].Y == j) co = dem1;// check pos x in list open
                            for (dem2 = 0; dem2 < close.Count; dem2++) if (close[dem2].X == i && close[dem2].Y == j) cc = dem2;// check pos y in close;
                            temp.X = i;
                            temp.Y = j;
                            h(ref temp, emuRunner);
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
            List<Position> path;
            path = new List<Position>();
            Position tg;
            tg = new Position();
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
            else
                return null;
        }
        private static List<Position> runAstar(int[,] emuMap,Position emuRunner,Position emuChaser,Position goal)
        {
            int i, j, next = 0, r, n, m, dem1, dem2, co = -1, cc = -1, ci;//co mean checkopen cc mean checkclose
           
            m = emuMap.GetLength(0);
            n = emuMap.GetLength(1);
            bool found = false;
            int[] dx = new int[] { 0, -1, 1, 0 };
            int[] dy = new int[] { -1, 0, 0, 1 };
            List<Position> open, close;
            Position temp, temp2;
            open = new List<Position>();
            close = new List<Position>();
            open.Add(emuRunner);
            while (open.Count != 0)
            {
                for (i = 0; i < open.Count; i++) if (open[i].f < open[next].f) next = i;// choicing best
                //-- next is current pos
                if (open[next].X == goal.X && open[next].Y == goal.Y)
                {
                    found = true;
                    break;
                };
                for (r = 0; r < 4; r++)
                {

                    i = open[next].X + dx[r];
                    j = open[next].Y + dy[r];

                    if (i >= 0 && j >= 0 && i < n && j < m)
                        if (emuMap[i, j] == 0 || emuMap[i, j] == 3)
                        {
                            for (dem1 = 0; dem1 < open.Count; dem1++) if (open[dem1].X == i && open[dem1].Y == j) co = dem1;// check pos x in list open
                            for (dem2 = 0; dem2 < close.Count; dem2++) if (close[dem2].X == i && close[dem2].Y == j) cc = dem2;// check pos y in close;
                            temp = new Position();
                            temp.X = i;
                            temp.Y = j;
                            h(ref temp, goal,emuChaser);
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
            List<Position> path;
            path = new List<Position>();
            Position tg;
            tg = new Position();
            if (found)
            {
                temp2 = open[next];
                while (temp2.X != emuRunner.X || temp2.Y != emuRunner.Y)
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
            else
                return null;
        }
        static void vl(Point oct2, int x, int y, ref double kq)
        {
            kq = Math.Sqrt((oct2.X - x) * (oct2.X - x) + (oct2.Y - y) * (oct2.X - y));
        }

        public void Asmove(int[,] MapMatrix)
        {
            List<Position> Path = runAstar(MapMatrix);
            if (Path != null)
            {
                int xt, yt;
                xt = Path[Path.Count - 1].X;
                yt = Path[Path.Count - 1].Y;
                //[0] = up, [1] = down, [2] = left, [3] = right

                this.x = xt;
                this.y = yt;
            }
        }

        public static List<int> Asmove2(int[,] MapMatrix)
        {
            List<int> kq = new List<int>();
            List<Position> Path = runAstar(MapMatrix);
            if (Path != null && Path.Count != 0)
            {
                int xt, yt;
                xt = Path[Path.Count - 1].X;
                yt = Path[Path.Count - 1].Y;

                kq.Add(xt);
                kq.Add(yt);
                return kq;
            }
            else
                return null;
        }
        public static List<int> Asmove3(int[,] emuMap)
        {
            Position emuRunner = new Position();
            Position emuChaser = new Position();
            Position goal = new Position();
            int i, j;
            for (i = 0; i < emuMap.GetLength(0); i++)
            {
                for (j = 0; j < emuMap.GetLength(1); j++)
                {
                    if (emuMap[i, j] == 2) //2 runner 3  goal 4 chaser;
                    {
                        emuRunner.X = i;
                        emuRunner.Y = j;
                    }
                    if (emuMap[i, j] == 3)
                    {
                        goal.X = i;
                        goal.Y = j;
                    }
                    if (emuMap[i, j] == 4)
                    {
                        emuChaser.X = i;
                        emuChaser.Y = j;
                    }

                }
            }
            List<int> kq = new List<int>();
            List<Position> Path = runAstar(emuMap, emuRunner, emuChaser, goal);
            if (Path != null && Path.Count != 0)
            {
                int xt, yt;
                xt = Path[Path.Count - 1].X;
                yt = Path[Path.Count - 1].Y;
                xt = emuRunner.X - xt;// this function only for runner to reach goal
                yt = emuRunner.Y - yt;
                kq.Add(xt);
                kq.Add(yt);
                return kq;
            }
            else
                return null;
        }
        #endregion
    }
}
