using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeRunner.Classes
{
    public class Species
    {
        public List<Genotype> Natives;

        private static readonly double DeltaDisjoint = 2.0;
        private static readonly double DeltaWeights = 0.4;
        private static readonly double DeltaThreshold = 1.0;

        #region Constructors
        public Species()
        {
            this.Natives = new List<Genotype>();
        }
        #endregion

        #region Methods
        private double cacl_Disjoint(Genotype child)
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

        private double cacl_Weights(Genotype child)
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

        public bool addNewChild(Genotype child)
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
            Natives.Reverse();

            Natives.RemoveRange(1, Natives.Count - 1);
            //if (Natives.Count > 10)
            //{
            //    Natives.RemoveRange(Natives.Count / 2, Natives.Count / 2 - 1);
            //}
        }

        #region CX
        private Genotype CrossOver(Genotype p1, Genotype p2, NEAT neat)
        {
            if (p2.Fitness > p1.Fitness)
            {
                var tmp = p1;
                p2 = p1;
                p1 = tmp;
            }

            //copy all genes(nodes and connections) from p1 to child
            Genotype child = p1.Clone();

            //copy all mismatch genes(nodes and connections) from p2 to child
            foreach (var connection in p2.ConnectionGenes)
            {
                Node from = connection.From;
                Node to = connection.To;
                ConnectionNode cnn = new ConnectionNode(from, to);

                cnn = child.ConnectionGenes.Find((x => x.From.ID == from.ID && x.To.ID == to.ID));
                if (cnn == null) //mismatch connection-gene found
                {
                    //check if from/to node exist in child
                    Node from_tmp = child.NodeGenes.Find((x => x.ID == from.ID));
                    if (from_tmp == null) 
                    {
                        from_tmp = from.Clone();
                        child.NodeGenes.Add(from_tmp);
                    }

                    Node to_tmp = child.NodeGenes.Find((x => x.ID == to.ID));
                    if (to_tmp == null)
                    {
                        to_tmp = to.Clone();
                        child.NodeGenes.Add(to_tmp);
                    }

                    ConnectionNode tmp = new ConnectionNode(from_tmp, to_tmp, connection.Weight, connection.Innovation, connection.isEnable);
                    //neat.CheckNewConnection(tmp); //copy from p2
                    child.ConnectionGenes.Add(tmp);
                }
            }

            return child;
        }

        private List<Genotype> CrossOver(List<Genotype> parents, NEAT neat)
        {
            List<Genotype> Childs = new List<Genotype>();
            for (int i = 0; i < parents.Count; i += 2)
            {
                Childs.Add(CrossOver(parents[i], parents[i + 1], neat));
            }

            return Childs;
        }
        #endregion

        public List<Genotype> Breed(NEAT neat)
        {
            List<Genotype> childs;

            //Select parents to breed
            Random rand = new Random(System.DateTime.Now.Millisecond);
            List<Genotype> parents = new List<Genotype>();
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
                childs = CrossOver(parents, neat);
            }
            else
            {
                childs = new List<Genotype>();
                childs.Add(Natives[rand.Next(0, Natives.Count)].Clone());
            }
            return childs;
        }
        
        public void Mutate(int[,] Map, NEAT neat)
        {
            foreach (var gene in Natives)
            {
                gene.Mutate(neat);
                gene.Evaluate(Map, neat);
            }
        }
        #endregion
    }
}
