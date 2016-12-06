using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeRunner.Classes
{
    public class GA
    {
        #region Properties
        private List<Chromosome> Population;
        private int Pop_Size;
        private int Generations;
        private double Mutate_Prob;
        private double CrossOver_Prob;
        private double TopologyMuate_Prob;

        private double LastBestCost;
        private int Counter;
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

        public void Excute(int [,] MapMatrix, List<int> HiddenLayers, MazeRunner.Controls.GameControl gc)
        {
            Population.Clear();
            
            //Init population
            CreatePopulation(MapMatrix, HiddenLayers);
            Selection();

            for (int i = 0; i < Generations; i++)
            {
                Random r = new Random(DateTime.Now.Millisecond + i);

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
                gc.Dispatcher.Invoke(pc, new object[] { Population[0], i });
            }
        }
        #endregion
    }
}
