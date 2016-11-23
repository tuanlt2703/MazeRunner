using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeRunner;
using MazeRunner.GA;
namespace MazeRunner
{
    class Evoluion
    {
        List<Chromosome> Population= new List<Chromosome>();
        int N;//
        int[,] Map;
        public Chromosome Best= new Chromosome();
        public Evoluion()
        {
        
        }
        public void Mainflow(int n,int[,]Maze,int Gen)
        {
            Map = Maze;
            int gencount = 0 ;
            this.N = n;
            int i;
            Chromosome x;
            double Curent_POPfitness= new double(),ran,AVGfitness;
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            for (i = 0; i < N; i++)
            {
                x = new Chromosome();
                x.set(Map);
                x.CFC(Map); // Create Population
                Population.Add(x);
            };
            Best = Population[0];
            while(gencount<Gen)
            { 
                ran = rnd.NextDouble();
                for(i=0;i<Population.Count;i++)
                {
                    Curent_POPfitness += Population[i].fitness;
                }
                AVGfitness = Curent_POPfitness/Population.Count;
                for (i=0;i<N;i++)
                {
                    if (Population[i].fitness > AVGfitness) break;
                }
                this.sort();
                Population.RemoveRange(i, Population.Count - i );
                for(i=0;i<Population.Count();i++)
                {
                    if(rnd.NextDouble()<0.7)// Mutate Probability 
                    {
                        x = Muta(Population[i]);
                        if (x.fitness>Population[Population.Count-1].fitness) Population.Add(x);
                    }
                }
                for(i=0;i<Population.Count/2;i++)
                {
                    if(rnd.NextDouble()<0.7)// Crossover Range;
                    {
                        foreach( var child in Crossover(Population[i],Population[i+Population.Count/2] ))
                            {
                            if (child.fitness > Population[Population.Count].fitness)
                                Population.Add(child);
                            }
                    }
                }
                if (Best.fitness < Population[0].fitness)
                {
                    Best = new Chromosome();
                    Best = Population[0];
                };
                gencount++;
            }
            


        }
         void sort()
        {
            int i,j;
            Chromosome temp;
            for (i = 0; i < Population.Count; i++)
               for(j=i+1;j<Population.Count;j++)
                    if(Population[i].fitness<Population[j].fitness)
                    {
                        temp = new Chromosome();
                        temp = Population[i];
                        Population[i] = new Chromosome();
                        Population[i] = Population[j];
                        Population[j] = new Chromosome();
                        Population[j] = temp;
                    }
                
        }
        private List<Chromosome> Crossover(Chromosome p1, Chromosome p2)
        {
            int pos,i,j;
            List<Chromosome> Child= new List<Chromosome>();
            List<double> w1 = new List<double>(), w2= new List<double>();
            Chromosome c1= new Chromosome(),c2 = new Chromosome();
            c1.set(Map);
            c2.set(Map);
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            pos = rnd.Next(0, p1.weightVector.Count);
            for (i = 0; i < pos; i++)
            {
                w1.Add(p1.weightVector[i]);
                w2.Add(p2.weightVector[i]);
            };
            for (j = i; j < p1.weightVector.Count; j++)
            {
                w1.Add(p2.weightVector[j]);
                w2.Add(p1.weightVector[j]);
            };
            c1 = new Chromosome();
            c1.set(this.Map, w1);
            c1.CFC(this.Map);
            c2 = new Chromosome();
            c2.set(this.Map, w2);
            c2.CFC(this.Map);
            Child.Add(c1);
            Child.Add(c2);
            return Child;
        }    
        private Chromosome Muta( Chromosome p)
        {
            Chromosome child;
            int pos = 20,k1,k2;
            double temp;
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            while (pos--==0)
            {
                k1 = rnd.Next(0, p.weightVector.Count);
                k2 = rnd.Next(0, p.weightVector.Count);
                temp = p.Genes.WeightVector[k1];
                p.Genes.WeightVector[k1]=p.Genes.WeightVector[k2];
                p.Genes.WeightVector[k2]=temp;
            }
            child = p;
            child.CFC(this.Map);
            return child;
            
        }





    }
}
