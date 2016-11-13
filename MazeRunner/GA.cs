using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeRunner;
namespace MazeRunner
{
    class GA
    {
        private List<NeuralNework> Population;// A population
        int N;//
        int[,] Map;
        //mỗi chorosom gồm 1 vector weight và ouput
        public GA()// Create the population
        {
            int i;
            for(i=0;i<N;N++)
            {
                NeuralNework temp = new NeuralNework(13 * 13, 10, 5, 2);
                temp.Setinput(Map);
                //temp.
            }
        }
        
    }
}
