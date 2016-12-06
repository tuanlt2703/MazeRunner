using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeRunner.Controls;
using System.Runtime.Serialization;

namespace MazeRunner.Classes
{
    [Serializable]
    public class Chromosome : IComparable<Chromosome>
    {
        public NeuralNework NN_Runner;
        public int Size
        {
            get { return NN_Runner.WeightsVector.Count; }
        }
        public double Fitness;

        #region Constructors
        /// <summary>
        /// Create a chromosome with specific NN config
        /// </summary>
        /// <param name="InputSize"></param>
        /// <param name="HiddenLayers"></param>
        public Chromosome(int InputSize, List<int> HiddenLayers)
        {
            NN_Runner = new NeuralNework(InputSize, HiddenLayers);
            Fitness = 0;
        }

        /// <summary>
        /// new product of CrossOver two Chromosome
        /// </summary>
        public Chromosome()
        {
            Fitness = 0;
        }
        #endregion

        #region Methods
        private int[,] Clone(int[,] MapMatrix, ref MazeRunner.Controls.CharacterPos CharPos)
        {
            int[,] tmp = new int[MapMatrix.GetLength(0), MapMatrix.GetLength(1)];
            for (int i = 0; i < MapMatrix.GetLength(0); i++)
            {
                for (int j = 0; j < MapMatrix.GetLength(1); j++)
                {
                    tmp[i, j] = MapMatrix[i, j];
                    if (tmp[i, j] == (int)MazeRunner.Controls.Cell.Runner)
                    {
                        CharPos.RunnerX = i;
                        CharPos.RunnerY = j;
                    }
                    else if (tmp[i, j] == (int)MazeRunner.Controls.Cell.Chaser)
                    {
                        CharPos.ChaserX = i;
                        CharPos.ChaserY = j;
                    }
                }
            }
            return tmp;
        }

        private bool isValidMove(int Movement, int[,] Map, ref CharacterPos CharPos)
        {
            if (Movement == 0) //Down
            {
                if (CharPos.RunnerX < Map.GetLength(1) - 1)
                {
                    if (Map[CharPos.RunnerX + 1, CharPos.RunnerY] != (int)Cell.Obstacle 
                        && Map[CharPos.RunnerX + 1, CharPos.RunnerY] != (int)Cell.Chaser)
                    {
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                        CharPos.RunnerX += 1;
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
                        return true;
                    }
                }
            }
            else if (Movement == 1) //Up
            {
                if (CharPos.RunnerX > 0)
                {
                    if (Map[CharPos.RunnerX - 1, CharPos.RunnerY] != (int)Cell.Obstacle
                        && Map[CharPos.RunnerX - 1, CharPos.RunnerY] != (int)Cell.Chaser)
                    {
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                        CharPos.RunnerX -= 1;
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
                        return true;
                    }
                }
            }
            else if (Movement == 2) //Right
            {
                if (CharPos.RunnerY < Map.GetLength(0) - 1)
                {
                    if (Map[CharPos.RunnerX, CharPos.RunnerY + 1] != (int)Cell.Obstacle
                        && Map[CharPos.RunnerX, CharPos.RunnerY + 1] != (int)Cell.Chaser)
                    {
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                        CharPos.RunnerY += 1;
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
                        return true;
                    }
                }
            }
            else if (Movement == 3) //Left
            {
                if (CharPos.RunnerY > 0)
                {
                    if (Map[CharPos.RunnerX, CharPos.RunnerY - 1] != (int)Cell.Obstacle
                        && Map[CharPos.RunnerX, CharPos.RunnerY - 1] != (int)Cell.Chaser)
                    {
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Moveable;
                        CharPos.RunnerY -= 1;
                        Map[CharPos.RunnerX, CharPos.RunnerY] = (int)Cell.Runner;
                        return true;
                    }
                }
            }
            return true;
        }

        public void Evaluate(int[,] MapMatrix)
        {
            MazeRunner.Controls.CharacterPos CharPos = new MazeRunner.Controls.CharacterPos();
            int[,] Map = Clone(MapMatrix, ref CharPos);
            int Steps = 0;
            bool Win = false;

            DateTime Start = DateTime.Now;
            var Elapsed = DateTime.Now - Start;
            while (true)
            {
                //runner's turn
                int Movement = NN_Runner.Run(Map); //always return true at this point.
                while (!isValidMove(Movement, Map, ref CharPos))
                {
                    NN_Runner.AdjustWeights();
                    Movement = NN_Runner.Run(Map);
                }
                Steps++;

                //chaser's turn
                var chase = Chaser.Asmove2(Map);
                Map[CharPos.ChaserX, CharPos.ChaserY] = (int)Cell.Moveable;
                CharPos.ChaserX = chase[0];
                CharPos.ChaserY = chase[1];
                Map[CharPos.ChaserX, CharPos.ChaserY] = (int)Cell.Chaser;

                //calculate score
                Elapsed = DateTime.Now - Start;
                if (Elapsed.TotalMinutes > 1)
                {
                    break;
                }
                if (CharPos.GotCaught)
                {
                    break;
                }
                if (Map[CharPos.RunnerX, CharPos.RunnerY] == (int)Cell.Goal)
                {
                    Win = true;
                    break;
                }
            }

            double kq = 60 - Elapsed.TotalSeconds;
            double hs = Win == true ? 2 : 0.5;
            if (kq > 0)
            {
                kq = (kq + 10 / Steps) * hs;
            }
            Fitness = kq;
        }

        public int CompareTo(Chromosome other)
        {
            return Fitness.CompareTo(other.Fitness);
        }

        public void Mutate(int index1, int index2)
        {
            var tmpWeights = NN_Runner.WeightsVector;
            double tmp = tmpWeights[index1];
            tmpWeights[index1] = tmpWeights[index2];
            tmpWeights[index2] = tmp;
            NN_Runner.DevelopPhenotype(tmpWeights);
        }
        #endregion

        #region Serializer
        public Chromosome(SerializationInfo info, StreamingContext ctxt)
        {
            NN_Runner = (NeuralNework)info.GetValue("Networks", typeof(NeuralNework));
            Fitness = (double)info.GetValue("Fit", typeof(double));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Networks", NN_Runner);
            info.AddValue("Fit", Fitness);
        }
        #endregion
    }
}