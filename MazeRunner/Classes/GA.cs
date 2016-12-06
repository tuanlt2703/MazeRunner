using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MazeRunner.Classes
{
    [Serializable]
    public class GA
    {
        #region Properties
        private List<Chromosome> Population;
        public int Pop_Size;
        public int Generations;
        public double Mutate_Prob;
        public double CrossOver_Prob;
        public double TopologyMuate_Prob;
        public List<int> HLayers
        {
            get
            {
                return Population[0].NN_Runner.Layers;
            }
        }

        private double LastBestCost;
        private int Counter;

        //
        public bool isFinished;
        private int CurrentGen;
        #endregion

        public GA(int n_pop, int gens, double m_prob, double x_prob, double top_prob)
        {
            this.Population = new List<Chromosome>();
            this.Pop_Size = n_pop;
            this.Generations = gens;
            this.Mutate_Prob = m_prob;
            this.CrossOver_Prob = x_prob;
            this.TopologyMuate_Prob = top_prob;

            this.LastBestCost = 0;
            this.Counter = 0;

            this.isFinished = false;
            this.CurrentGen = 0;
        }

        #region Methods
        private void CreatePopulation(int[,] Map, List<int> HiddenLayers)
        {
            for (int i = 0; i < Pop_Size; i++)
            {
                Chromosome tmp = new Chromosome(Map.GetLength(0) * Map.GetLength(1), HiddenLayers);
                tmp.Evaluate(Map);
                Population.Add(tmp);
            }
        }

        private void Selection()
        {
            Population.Sort();
            Population.Reverse();

            if (Pop_Size < Population.Count)
                Population.RemoveRange(Pop_Size, Population.Count - Pop_Size);

            using (var writer = new System.IO.StreamWriter(@"C:\test.txt", true))
            {
                writer.WriteLine(Population[0].Fitness.ToString());
            }
        }

        private List<Chromosome> Select_Parent(Random rand)
        {
            List<Chromosome> p = new List<Chromosome>();
            p.Add(Population[0]);
            p.Add(Population[1]);
            for (int j = 2; j < Population.Count; j++)
            {
                if (rand.NextDouble() < CrossOver_Prob)
                    p.Add(Population[j]);
            }
            if (p.Count % 2 == 1)
                p.Add(Population[rand.Next(0, Population.Count)]);
            return p;
        }

        private Chromosome CrossOver(Chromosome p1, Chromosome p2, int CXPoint)
        {
            Chromosome child = new Chromosome();
            List<double> Weights = new List<double>();

            int i = 0;
            for (; i < CXPoint; i++)
            {
                Weights.Add(p1.NN_Runner.WeightsVector[i]);
            }
            for (; i < p2.NN_Runner.WeightsVector.Count; i++)
            {
                Weights.Add(p2.NN_Runner.WeightsVector[i]);
            }

            child.NN_Runner = new NeuralNework(p1.NN_Runner.Layers);
            child.NN_Runner.DevelopPhenotype(Weights);

            return child;
        }

        private List<Chromosome> CrossOver(List<Chromosome> parents)
        {
            List<Chromosome> childs = new List<Chromosome>();
            for (int i = 0; i < parents.Count; i += 2)
            {
                Random rand = new Random(DateTime.Now.Millisecond);
                int CXPoint = rand.Next(parents[i].Size);
                childs.Add(CrossOver(parents[i], parents[i + 1], CXPoint));
                childs.Add(CrossOver(parents[i + 1], parents[i], CXPoint));
            }

            return childs;
        }

        private void Mutate(Chromosome chr)
        {
            Random rand = new Random(DateTime.Now.Millisecond);

            for (int i = 0; i < chr.Size; i++)
            {
                int index1 = rand.Next(chr.Size);
                int index2;
                do
                {
                    index2 = rand.Next(chr.Size);
                }
                while (index2 == index1);
                chr.Mutate(index1, index2);
            }

            if (rand.NextDouble() > TopologyMuate_Prob)
            {
                
            }
        }

        /// <summary>
        /// Start training completely new GA
        /// </summary>
        /// <param name="MapMatrix"></param>
        /// <param name="HiddenLayers"></param>
        /// <param name="gc"></param>
        public void Excute(int [,] MapMatrix, List<int> HiddenLayers, MazeRunner.Controls.GameControl gc)
        {
            Population.Clear();
            
            //Init population
            CreatePopulation(MapMatrix, HiddenLayers);
            Selection();

            for (CurrentGen = 0; CurrentGen < Generations; CurrentGen++)
            {
                Random r = new Random(DateTime.Now.Millisecond + CurrentGen);

                //CrossOver
                List<Chromosome> parents = Select_Parent(r);
                foreach (var c in CrossOver(parents))
                {
                    c.Evaluate(MapMatrix);
                    Population.Add(c);
                }

                //Mutate
                for (int j = 1; j < Population.Count; j++)
                {
                    if (r.NextDouble() < Mutate_Prob)
                    {
                        Mutate(Population[j]);
                        Population[j].Evaluate(MapMatrix);
                    }
                }

                //
                Selection();

                MazeRunner.Controls.GameControl.Process pc = new Controls.GameControl.Process(gc.ShowProcess);
                gc.Dispatcher.Invoke(pc, new object[] { Population[0], CurrentGen });
            }

            //finished
            isFinished = true;
            //gc.SaveTrainedGA();
            MazeRunner.Controls.GameControl.Finished fin = new Controls.GameControl.Finished(gc.SaveTrainedGA);
            gc.Dispatcher.Invoke(fin);
        }

        /// <summary>
        /// Continue previous training
        /// </summary>
        /// <param name="MapMatrix"></param>
        /// <param name="gc"></param>
        public void Continue(int[,] MapMatrix, MazeRunner.Controls.GameControl gc)
        {
            for (; CurrentGen < Generations; CurrentGen++)
            {
                Random r = new Random(DateTime.Now.Millisecond + CurrentGen);

                //CrossOver
                List<Chromosome> parents = Select_Parent(r);
                foreach (var c in CrossOver(parents))
                {
                    c.Evaluate(MapMatrix);
                    Population.Add(c);
                }

                //Mutate
                for (int j = 1; j < Population.Count; j++)
                {
                    if (r.NextDouble() < Mutate_Prob)
                    {
                        Mutate(Population[j]);
                        Population[j].Evaluate(MapMatrix);
                    }
                }

                //
                Selection();

                MazeRunner.Controls.GameControl.Process pc = new Controls.GameControl.Process(gc.ShowProcess);
                gc.Dispatcher.Invoke(pc, new object[] { Population[0], CurrentGen });
            }

            //finished
            //gc.SaveTrainedGA();
            MazeRunner.Controls.GameControl.Finished fin = new Controls.GameControl.Finished(gc.SaveTrainedGA);
            gc.Dispatcher.Invoke(fin);
        }
        #endregion

        #region Serializer
        //Deserialization constructor
        public GA(SerializationInfo info, StreamingContext ctxt)
        {
            Population = (List<Chromosome>)info.GetValue("Pop", typeof(List<Chromosome>));
            Pop_Size = (int)info.GetValue("PopSize", typeof(int));
            Generations = (int)info.GetValue("Gens", typeof(int));
            Mutate_Prob = (double)info.GetValue("Mutate", typeof(double));
            CrossOver_Prob = (double)info.GetValue("CX", typeof(double));
            TopologyMuate_Prob = (double)info.GetValue("Topology", typeof(double));
            isFinished = (bool)info.GetValue("Last", typeof(bool));
            CurrentGen = (int)info.GetValue("LastGen", typeof(int));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Pop", Population);
            info.AddValue("PopSize", Pop_Size);
            info.AddValue("Gens", Generations);
            info.AddValue("Mutate", Mutate_Prob);
            info.AddValue("CX", CrossOver_Prob);
            info.AddValue("Topology", TopologyMuate_Prob);
            info.AddValue("Last", isFinished);
            info.AddValue("LastGen", CurrentGen);
        }
        #endregion
    }
}
