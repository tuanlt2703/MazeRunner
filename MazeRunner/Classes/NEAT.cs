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
        private Genotype Best;
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

        #region Constructor
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
        public void CheckNewConnection(ConnectionNode newConnection)
        {
            var tmp = Innovations.Find((x => x.From.ID == newConnection.From.ID && x.To.ID == newConnection.To.ID));
            if (tmp == null)
            {
                Innovations.Add(newConnection);
                newConnection.Innovation = Innovations.Count;
            }
            else
            {
                newConnection.Innovation = tmp.Innovation;
            }
        }

        private void CreatePopulation(int[,] MapMatrix, int output = 2)
        {
            Population = new List<Species>();
            Population.Add(new Species());
            for (int i = 0; i < Pop_Size; i++)
            {
                Genotype tmp = new Genotype(MapMatrix.GetLength(0)* MapMatrix.GetLength(1), output, this);
                tmp.Evaluate(MapMatrix, this);

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
                    newSpecies.Natives.Add(tmp);
                    Population.Add(newSpecies);
                }
            }

            TotalNodes = MapMatrix.GetLength(0) * MapMatrix.GetLength(1) + output;
        }

        private void Selection()
        {
            Population[0].Selection();
            Best = Population[0].Natives[0];
            for (int i = 1; i < Population.Count; i++)
            {
                Population[i].Selection();
                if (Best.Fitness < Population[i].Natives[0].Fitness)
                {
                    Best = Population[i].Natives[0];
                }
            }
        }

        private void Breed(int[,] Map, NEAT neat)
        {
            List<Genotype> childs = new List<Genotype>();
            foreach (var species in Population)
            {
                foreach (var ch in species.Breed(this))
                {
                    ch.Evaluate(Map, neat);
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
                    newSpecies.Natives.Add(childs[i]);
                    Population.Add(newSpecies);
                }
            }
        }

        private void Mutate(int[,] Map)
        {
            foreach (var species in Population)
            {
                species.Mutate(Map, this);
            }
        }

        public void Execute(int[,] MapMatrix, MazeRunner.Controls.GameControl gc)
        {
            Population.Clear();

            //Init population
            CreatePopulation(MapMatrix);
            Selection();

            for (CurrentGen = 0; CurrentGen < Generations; CurrentGen++)
            {
                //CrossOver - Mutate
                Breed(MapMatrix, this);
                Mutate(MapMatrix);

                Selection();

                MazeRunner.Controls.GameControl.Process pc = new Controls.GameControl.Process(gc.ShowProcess);
                gc.Dispatcher.Invoke(pc, new object[] { Best, CurrentGen });
            }

            ////finished
            //isFinished = true;
            //MazeRunner.Controls.GameControl.Finished fin = new Controls.GameControl.Finished(gc.SaveTrainedGA);
            //gc.Dispatcher.Invoke(fin);
        }
        #endregion
    }
}
