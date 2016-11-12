using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeRunner.Controls;
namespace MazeRunner
{
    class NeuralNework
    {
        private double[] input; //  13*13 
        private double[,] weightMatrixH1; // 10*169
        private double[,] weightMatrixH2;// 5*10
        private double[,] weightMatrixOutput;//  2*5;
        public List<double> WeightVector;
        private double[,] output; //1*2;
        /*
            Hidden Layer Have two sub layer 
            1st layer have 10 Neurals
            2nd layer have 5 neural
         */
        NeuralNework()
        {
            /*
             *  Create Random Weight Matrix
             */
            Random rnd = new Random();
            weightMatrixH1 = new double[10, 13 * 13];
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 169; j++)
                {
                    weightMatrixH1[i, j] = rnd.NextDouble();
                }
            weightMatrixH2 = new double[5, 10];
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 10; j++)
                {
                    weightMatrixH2[i, j] = rnd.NextDouble();
                }
            this.weightMatrixOutput = new double[2, 5];
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 5; j++)
                {
                    weightMatrixH1[i, j] = rnd.NextDouble();
                }
            /*
             *input matrix have 13*13  
             */
            this.input = new double[13 * 13];
            this.output = new double[2, 1];
        }
        void Setinput(int[,] MapMatrix)
        {
            int i, j, k = 0;
            for (i = 0; i < 13; i++)
                for (j = 0; j < 13; j++)
                {
                    this.input[k] = MapMatrix[i, j] / 3;
                    k++;
                }
        }
        public static float Sigmoid(double value)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-value));
        }
        double[,] Mul(double[,] matrix1, double[,] matrix2)
        {
            double[,] Kq = new double[2, 2];
            return Kq;
        }
        double[,] Mul(double[] matrix1, double[,] matrix2)
        {
            double[,] kq = new double[1, 2];
            return kq;
        }
        void Process()
        {
            double[,] resultH1;
            double[,] resultH2;
            resultH1 = Mul(input, weightMatrixH1);
            /*
             *Add sigmoid function for reultH1; 
             */
            resultH2 = Mul(resultH1, weightMatrixH2);
            /*
             * Add sigmoid Function for resultH2;
             */
            output = Mul(resultH2, weightMatrixOutput);

        }
        void exportWeightVector()
        {
            int i, j, k;
            for (i = 0; i < 10; i++)
                for (j = 0; j < 169; j++)
                    WeightVector.Add(weightMatrixH1[i, j]);
            for (i = 0; i < 5; i++)
                for (j = 0; j < 10; j++)
                    WeightVector.Add(weightMatrixH2[i, j]);
            for (i = 0; i < 2; i++)
                for (j = 0; j < 5; j++)
                    WeightVector.Add(weightMatrixOutput[i, j]);
        }
    }
}

