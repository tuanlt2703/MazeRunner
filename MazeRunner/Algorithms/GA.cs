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
        List<Chromosome> Population;
        int N;//
        int Gen;//count;
        int[,] Map;
        public Evoluion(int [,]Map,int N)
        {
            int i;
            Chromosome x ;
            for( i=0;i<N;i++)
            {
                x = new Chromosome();
                x.set(Map);
                x.CFC(Map); // Create Population
            }


        }
        private List<Chromosome> Crossover(Chromosome p1, Chromosome p2)
        {
            int pos;
            List<Chromosome> Child= new List<Chromosome>();
            Chromosome c1= new Chromosome(),c2 = new Chromosome();
            c1.set(Map);
            c2.set(Map);
            Random rnd = new Random();
            pos = rnd.Next(0, p1.weightVector.Count);
            for(int i=0;i<pos;i++)
                c1.weightVector
            return Child;
        }    
        






    }
}
