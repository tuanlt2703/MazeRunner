using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeRunner.Classes
{
    public class Species
    {
        private static readonly double DeltaDisjoint = 2.0;
        private static readonly double DeltaWeights = 0.4;
        private static readonly double DeltaThreshold = 1.0;

        public List<Chromosome> Individuals;

        #region Constructors
        public Species()
        {
            this.Individuals = new List<Chromosome>();
        }
        #endregion

        #region Methods
        private double cacl_Disjoint(Chromosome child)
        {
            List<int> list1 = new List<int>();
            foreach (var connection in child.ConnectionGens)
            {
                list1.Add(connection.Innovation);
            }

            List<int> list2 = new List<int>();
            foreach (var connection in Individuals[0].ConnectionGens)
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

            return Species.DeltaDisjoint * (disjointGenes / Math.Max(child.ConnectionGens.Count, Individuals[0].ConnectionGens.Count));
        }

        private double cacl_Weights(Chromosome child)
        {
            double sum = 0;
            int matchingGenes = 0;

            var connections = Individuals[0].ConnectionGens;
            for (int i = 0; i < child.ConnectionGens.Count; i++)
            {
                var gene = connections.Find((x => x.Innovation == child.ConnectionGens[i].Innovation));
                if (gene != null)
                {
                    matchingGenes++;
                    sum += Math.Abs(gene.Weight - child.ConnectionGens[i].Weight);
                }
            }

            return Species.DeltaWeights * (sum / matchingGenes);
        }

        public bool addNewChild(Chromosome child)
        {
            if (Individuals.Count == 0)
            {
                Individuals.Add(child);
                return true;
            }

            if ((cacl_Disjoint(child) + cacl_Weights(child)) < Species.DeltaThreshold)
            {
                Individuals.Add(child);
                return true;
            }

            return false;
        }

        public void Selection()
        {
            Individuals.Sort();
            Individuals.Reverse();

            if (Individuals.Count > 10)
            {
                Individuals.RemoveRange(Individuals.Count / 2, Individuals.Count / 2 - 1);
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

            //cx 2 chromosome
            Chromosome child = new Chromosome(new List<Node>(p1.OutputNodes.Select(x => (Node)x.Clone()).ToList()));
            //copy all input node from p1 to child
            child.InputNodes = p1.InputNodes;
            for (int i = 0; i < p1.InputNodes; i++)
            {
                child.Nodes.Add((Node)p1.Nodes[i].Clone());
            }

            //cross over connections
            ////copy all connections from p1 (both match and mismatch(disjoint) genes)
            for (int i = 0; i < p1.ConnectionGens.Count; i++)
            {
                child.AddConnection((ConnectionNode)p1.ConnectionGens[i].Clone());
                
            }
            ////copy all disjoint genes from p2
            for (int i = 0; i < p2.ConnectionGens.Count; i++)
            {
                var gene = p1.ConnectionGens.Find((x => x.Innovation == p2.ConnectionGens[i].Innovation));
                if (gene == null) //disjoint gene found
                {
                    child.AddConnection((ConnectionNode)p2.ConnectionGens[i].Clone());
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

        public List<Chromosome> Breed(Random r)
        {
            List<Chromosome> childs;
            List<Chromosome> parents = new List<Chromosome>();
            if (Individuals.Count >= 2)
            {
                parents.Add(Individuals[0]);
                parents.Add(Individuals[1]);
                for (int i = 2; i < Individuals.Count; i++)
                {
                    if (r.NextDouble() < NEAT.CrossOver_Prob)
                    {
                        parents.Add(Individuals[i]);
                    }
                }
                if (parents.Count % 2 == 1) 
                {
                    parents.Add(Individuals[r.Next(0, Individuals.Count)]);
                }
                childs = CrossOver(parents);
            }
            else
            {
                childs = new List<Chromosome>();
                childs.Add((Chromosome)Individuals[r.Next(0, Individuals.Count)].Clone());
            }
            return childs;
        }

        public void Mutate(Random r, int[,] Map, NEAT neat)
        {
            for (int i = 1; i < Individuals.Count; i++)
            {
                Individuals[i].Mutate(r, neat);
                Individuals[i].Evaluate(Map);
            }
        }
        #endregion
    }
}
