using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MazeRunner.Classes
{
    public class NEAT
    {
        public int TotalNodes;
        private List<ConnectionNode> Innovations;
        private List<Species> Population;
        private Chromosome Best;
        private int Pop_Size;
        private int Generations;
        
        public static double CrossOver_Prob;
        public static double Mutate_Prob;
        public static double PointMutate_Prob;
        public static double LinkMutate_Prob;
        public static double NodeMutate_Prob;
        public static double EnDisMutate_Prob;

        public bool isFinished;
        private int CurrentGen;

        #region Constructors
        public NEAT(int n_pop, int gens, double xProb, double mProb, double PProb, double LProb, double NodeProb, double EDProb)
        {
            this.TotalNodes = 0;
            this.Innovations = new List<ConnectionNode>();
            this.Population = new List<Species>();

            this.Pop_Size = n_pop;
            this.Generations = gens;
            NEAT.CrossOver_Prob = xProb;
            NEAT.Mutate_Prob = mProb;
            NEAT.PointMutate_Prob = PProb;
            NEAT.LinkMutate_Prob = LProb;
            NEAT.NodeMutate_Prob = NodeProb;
            NEAT.EnDisMutate_Prob = EDProb;
        }
        #endregion

        #region Methods
        /// <summary>
        /// To check whether new connection is alrdy existed or not
        /// </summary>
        /// <param name="connection"></param>
        public void CheckNewConnection(ConnectionNode connection)
        {
            var tmp = Innovations.Find((x => x.From.NodeID == connection.From.NodeID && x.To.NodeID == connection.To.NodeID));
            if (tmp == null)
            {
                Innovations.Add(connection);
                connection.Innovation = Innovations.Count;
            }
            else
            {
                connection.Innovation = tmp.Innovation;
            }            
        }

        private void CreatePopulation(int[,] Map, int OutputNodes = 2)
        {
            List<Node> input = new List<Node>();
            for (int i = 0; i < Map.GetLength(0)*Map.GetLength(1); i++)
            {
                var tmp = new Node(TotalNodes++);
                input.Add(tmp);
            }

            List<Node> output = new List<Node>();
            for (int i = 0; i < OutputNodes; i++)
            {
                output.Add(new Node(TotalNodes++, NodeType.Output, ActivationMethod.Binary));
            }

            Population = new List<Species>();
            Population.Add(new Species());
            for (int i = 0; i < Pop_Size; i++)
            {
                Chromosome tmp = new Chromosome(new List<Node>(input.Select(x => (Node)x.Clone()).ToList()),
                                                new List<Node>(output.Select(x => (Node)x.Clone()).ToList()), this);
                tmp.Evaluate(Map);

                bool foundSpecies = false;
                foreach (var species in Population)
                {
                    if (species.addNewChild(tmp))
                    {
                        foundSpecies = true;
                        break;
                    }
                }

                if (!foundSpecies)
                {
                    Species newSpecies = new Species();
                    newSpecies.Individuals.Add(tmp);
                    Population.Add(newSpecies);
                }
            }
        }

        private void Selection()
        {
            Population[0].Selection();
            Best = Population[0].Individuals[0];
            for (int i = 1; i < Population.Count; i++)
            {
                Population[i].Selection();
                if (Best.Fitness < Population[i].Individuals[0].Fitness)
                {
                    Best = Population[i].Individuals[0];
                }
            }
        }

        private void Breed(Random r, int[,] Map)
        {
            List<Chromosome> childs = new List<Chromosome>();
            for (int i = 0; i < Population.Count; i++)
            {
                foreach (var ch in Population[i].Breed(r))
                {
                    ch.Evaluate(Map);
                    childs.Add(ch);
                }
            }

            for (int i = 0; i < childs.Count; i++)
            {
                bool foundSpecies = false;
                foreach (var species in Population)
                {
                    if (species.addNewChild(childs[i]))
                    {
                        foundSpecies = true;
                        break;
                    }
                }

                if (!foundSpecies)
                {
                    Species newSpecies = new Species();
                    newSpecies.Individuals.Add(childs[i]);
                    Population.Add(newSpecies);
                }
            }
        }

        private void Mutate(Random r, int[,] Map)
        {
            foreach (var species in Population)
            {
                species.Mutate(r, Map, this);
            }
        }

        public void Excute(int[,] MapMatrix, MazeRunner.Controls.GameControl gc)
        {
            Population.Clear();

            //Init population
            CreatePopulation(MapMatrix);
            Selection();
            for (CurrentGen = 0; CurrentGen < Generations; CurrentGen++)
            {
                Random r = new Random(DateTime.Now.Millisecond + CurrentGen);

                //CrossOver - Mutate
                Breed(r, MapMatrix);
                Mutate(r, MapMatrix);

                Selection();

                MazeRunner.Controls.GameControl.Process pc = new Controls.GameControl.Process(gc.ShowProcess);
                gc.Dispatcher.Invoke(pc, new object[] { Best, CurrentGen });
            }

            //finished
            isFinished = true;
            //gc.SaveTrainedGA();
            MazeRunner.Controls.GameControl.Finished fin = new Controls.GameControl.Finished(gc.SaveTrainedGA);
            gc.Dispatcher.Invoke(fin);
        }
        #endregion
    }
}
