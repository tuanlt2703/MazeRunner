using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeRunner.Classes
{
    //a collection of chromosome
    public class Species : IComparable<Species>
    {
        public List<Chromosome> Natives;
        public double TotalFitness;

        private static readonly double DeltaDisjoint = 2;
        private static readonly double DeltaWeights = 1;
        private static readonly double DeltaThreshold = 1;

        #region Constructors
        public Species()
        {
            this.Natives = new List<Chromosome>();
            this.TotalFitness = 0;
        }
        #endregion


        #region Methods
        public int CompareTo(Species other)
        {
            return other.TotalFitness.CompareTo(TotalFitness);
        }

        private double cacl_Disjoint(Chromosome child)
        {
            List<int> list1 = new List<int>();
            foreach (var connection in child.ConnectionGenes)
            {
                list1.Add(connection.Innovation);
            }

            List<int> list2 = new List<int>();
            foreach (var connection in Natives[0].ConnectionGenes)
            {
                list2.Add(connection.Innovation);
            }

            double disjointGenes = 0;
            for (int i = 0; i < list1.Count; i++)
            {
                if (!list2.Contains(list1[i]))
                {
                    disjointGenes++;
                }
            }
            for (int i = 0; i < list2.Count; i++)
            {
                if (!list1.Contains(list2[i]))
                {
                    disjointGenes++;
                }
            }

            return Species.DeltaDisjoint * (disjointGenes / Math.Max(child.ConnectionGenes.Count, Natives[0].ConnectionGenes.Count));
        }

        private double cacl_Weights(Chromosome child)
        {
            double sum = 0;
            int matchingGenes = 0;

            var connections = Natives[0].ConnectionGenes;
            for (int i = 0; i < child.ConnectionGenes.Count; i++)
            {
                var gene = connections.Find((x => x.Innovation == child.ConnectionGenes[i].Innovation));
                if (gene != null) //match gen found
                {
                    matchingGenes++;
                    sum += Math.Abs(gene.Weight - child.ConnectionGenes[i].Weight);
                }
            }

            return Species.DeltaWeights * (sum / matchingGenes);
        }

        public bool addNewChild(Chromosome child)
        {
            if (Natives.Count == 0)
            {
                Natives.Add(child);
                return true;
            }

            if ((cacl_Disjoint(child) + cacl_Weights(child)) < Species.DeltaThreshold)
            {
                Natives.Add(child);
                return true;
            }

            return false;
        }

        public void Selection()
        {
            Natives.Sort();

            //because there will be a lot of species existing, we need to keep only the best (or somes) individual in each species 
            if (Natives.Count > 6)
            {
                Natives.RemoveRange(6, Natives.Count - 6);
            }

            TotalFitness = 0;
            foreach (var chrome in Natives)
            {
                TotalFitness += chrome.Fitness;
            }
        }

        private Chromosome CrossOver(Chromosome p1, Chromosome p2)
        {
            if (p2.Fitness > p1.Fitness)
            {
                var tmp = p1;
                p2 = p1;
                p1 = tmp;
            }

            //copy all genes(nodes and connections) from p1 to child
            Chromosome child = p1.Clone();

            //copy all mismatch genes(nodes and connections) from p2 to child
            foreach (var connection in p2.ConnectionGenes)
            {
                Node cnn_from = connection.From;
                Node cnn_to = connection.To;
                ConnectionNode cnn = new ConnectionNode(cnn_from, cnn_to);

                cnn = child.ConnectionGenes.Find((x => x.From.ID == cnn_from.ID && x.To.ID == cnn_to.ID));
                if (cnn == null) //mismatch connection-gene found
                {
                    //check if from/to node exist in child
                    Node from_tmp = child.NodeGenes.Find((x => x.ID == cnn_from.ID));
                    if (from_tmp == null)
                    {
                        from_tmp = cnn_from.Clone();
                        child.NodeGenes.Add(from_tmp);
                    }

                    Node to_tmp = child.NodeGenes.Find((x => x.ID == cnn_to.ID));
                    if (to_tmp == null)
                    {
                        to_tmp = cnn_to.Clone();
                        child.NodeGenes.Add(to_tmp);
                    }

                    ConnectionNode tmp = new ConnectionNode(from_tmp, to_tmp, connection.Weight, connection.Innovation, connection.isEnable);
                    //neat.CheckNewConnection(tmp); //copy from p2
                    child.ConnectionGenes.Add(tmp);
                }
            }

            return child;
        }

        private List<Chromosome> CrossOver(List<Chromosome> parents)
        {
            List<Chromosome> Childs = new List<Chromosome>();
            for (int i = 0; i < parents.Count; i += 2)
            {
                Childs.Add(CrossOver(parents[i], parents[i + 1]));
            }

            return Childs;
        }

        public List<Chromosome> Breed()
        {
            List<Chromosome> childs;

            //Select parents to breed
            Random rand = new Random(System.DateTime.Now.Millisecond);
            List<Chromosome> parents = new List<Chromosome>();
            if (Natives.Count >= 2)
            {
                parents.Add(Natives[0]);
                parents.Add(Natives[1]);

                for (int i = 2; i < Natives.Count; i++)
                {
                    if (rand.NextDouble() < NEAT.CrossOver_Prob)
                    {
                        parents.Add(Natives[i]);
                    }
                }
                if (parents.Count % 2 == 1)
                {
                    parents.Add(Natives[rand.Next(0, Natives.Count)]);
                }
                childs = CrossOver(parents);
            }
            else
            {
                childs = new List<Chromosome>();
                childs.Add(Natives[rand.Next(0, Natives.Count)].Clone());
            }
            return childs;
        }

        public void Mutate(int[,] Map, NEAT neat)
        {
            foreach (var gene in Natives)
            {
                gene.Mutate(neat);
                gene.Evaluate(Map);
            }
        }
        #endregion
    }
}
