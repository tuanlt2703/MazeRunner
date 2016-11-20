using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeRunner.Controls;
using static MazeRunner.Controls.Map;

namespace MazeRunner.GA
{
    public class Chromosome
    {
        public NeuralNework Genes;
        public List<double> weightVector;
        public double fitness;
        public Chromosome()
        {
            Genes = new NeuralNework(13 * 13, 10, 5);
            fitness = -1;
        }
        public void set(int[,] Maze)
        {
            Genes.Setinput(Maze);
            Genes.exportWeightVector();
            weightVector = Genes.WeightVector;
        }
        #region fitnescal
        public void CFC(int[,]emuMap)
        {
            Map.Pos emuRunner=new Pos(), emuChaser=new Pos();
            bool lose = false, win = false;
            Pos Goal=new Pos();
            for (int i = 0; i < 13; i++)
                for (int j = 0; j < 13; j++)
                {
                    switch (emuMap[i, j])
                    {
                        case 4:
                            emuChaser.X = i;
                            emuChaser.Y = i;
                            break;
                        case 2:
                            emuRunner.X = i;
                            emuRunner.Y = j;
                            break;
                        case 3:
                            Goal.X = i;
                            Goal.Y = j;
                            break;
                        default:
                            break;
                    }
                       
                }// Location chaser,goal and runner

            double[,] chromosomeResult = new double[1, 2];
            int chaserResult;
            DateTime start = DateTime.Now;
            var time = DateTime.Now - start;
            int count = 0;
            while (true)
            {
                count++;
                this.Genes.Setinput(emuMap);
                chromosomeResult = this.Genes.Process();
                if (chromosomeResult[0, 1] == 0 && chromosomeResult[0, 0] == 0 && emuRunner.X < 12 && emuMap[emuRunner.X + 1, emuRunner.Y] != 1) //emulatorrunner move up
                {
                    emuMap[emuRunner.X, emuRunner.Y] = 0;
                    emuRunner.X += 1; // move down
                    emuMap[emuRunner.X, emuRunner.Y] = 2;
                }
                if (chromosomeResult[0, 1] == 0 && chromosomeResult[0, 0] == 1 && emuRunner.X > 0 && emuMap[emuRunner.X - 1, emuRunner.Y] != 1) //emulatorrunner move up
                {
                    emuMap[emuRunner.X, emuRunner.Y] = 0;
                    emuRunner.X -= 1; // move up
                    emuMap[emuRunner.X, emuRunner.Y] = 2;
                }
                if (chromosomeResult[0, 1] == 1 && chromosomeResult[0, 0] == 0 && emuRunner.Y < 12 && emuMap[emuRunner.X, emuRunner.Y + 1] != 1) //emulatorrunner move up
                {
                    emuMap[emuRunner.X, emuRunner.Y] = 0;
                    emuRunner.Y += 1; // move right
                    emuMap[emuRunner.X, emuRunner.Y] = 2;
                }
                if (chromosomeResult[0, 1] == 1 && chromosomeResult[0, 0] == 1 && emuRunner.X < 12 && emuMap[emuRunner.X, emuRunner.Y - 1] != 1) //emulatorrunner move up
                {
                    emuMap[emuRunner.X, emuRunner.Y] = 0;
                    emuRunner.Y -= 1; // left
                    emuMap[emuRunner.X, emuRunner.Y] = 2;
                }
                // Runner turn finnish
                // Chaser turn
                List<int> temp = new List<int>();
                //temp =  Chaser.Asmove2(emuChaser, emuRunner, emuMap);
                temp = Chaser.Asmove2(emuChaser, emuRunner, emuMap);
                if (temp != null)
                {
                    emuMap[emuChaser.X, emuChaser.Y] = 0;
                    emuChaser.X = temp[0];
                    emuChaser.Y = temp[1];
                    emuMap[temp[0], temp[1]] = 4;
                }
                time = DateTime.Now - start;
                if (time.TotalMinutes > 3) { break; };
                if (emuRunner.X == emuChaser.X && emuRunner.Y == emuChaser.Y) { lose = true; break; };
                if (emuRunner.X == Goal.X && emuRunner.Y == Goal.Y) { win = true; break; };
               
            }
            double kq = (180-time.TotalSeconds);
            double hs= new double();
            if (win) hs = 2;
            if (lose) hs = 0.5;
            if (kq != 0) kq=(kq + count * 3) * hs;
            this.fitness = kq;
        }
#endregion
    }
}

    

