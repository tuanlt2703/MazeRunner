using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeRunner.Controls;
using MathNet.Numerics.LinearAlgebra.Double;
namespace MazeRunner
{
    class NeuralNework
    {
        private double[,] input; //  13*13 
        private double[,] weightMatrixH1; // 10*169
        private double[,] weightMatrixH2;// 5*10
        private double[,] weightMatrixOutput;//  2*5;
        public List<double> WeightVector;
        private double[,] output; //1*2;
        /*
            Hidden Layer Have two sub layer 
            1st layer have 10 Neurals
            2nd layer  have 5 neural
         */
        public NeuralNework(int inputSize,int fstLayersize , int sndLayersize,int outPutsize)
        {
            /*
             *  Create Random Weight Matrix
             */
            Random rnd = new Random();
            weightMatrixH1 = new double[ fstLayersize , inputSize];// input size = 13*13
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 169; j++)
                {
                    weightMatrixH1[i, j] = rnd.NextDouble();
                }
            weightMatrixH2 = new double[sndLayersize,fstLayersize];
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 10; j++)
                {
                    weightMatrixH2[i, j] = rnd.NextDouble();
                }
            this.weightMatrixOutput = new double[outPutsize, sndLayersize];
            for (int i = 0; i < 2; i++)
                for (int j = 0; j < 5; j++)
                {
                    weightMatrixH1[i, j] = rnd.NextDouble();
                }
            /*
             *input matrix have 13*13  
             */
            this.input = new double[inputSize,1];
            this.output = new double[outPutsize, 1];
        }
        public void Setinput(int[,] MapMatrix)
        {
            int i, j, k = 0;
            for (i = 0; i < 13; i++)
                for (j = 0; j < 13; j++)
                {
                    this.input[k,0] = MapMatrix[i, j] / 3;
                    k++;
                }
        }
        public static float Sigmoid(double value)
        {
            return 1.0f / (1.0f + (float)Math.Exp(-value));
        }
        double[,] Mul(double[,] matrix1, double[,] matrix2)
        {
            double[,] Kq;
            var M1 = DenseMatrix.OfArray(matrix1);
            var M2 = DenseMatrix.OfArray(matrix2);
            var M3 = M1 * M2;
            Kq = Convert(M3);
            return Kq;
        }
        double[,] Convert(DenseMatrix A)
        {
            double[,] kq;
            int i = A.RowCount;
            int j = A.ColumnCount;
            kq = new double[i, j];
            return kq;
        }
        void sigmoidArray(double[,] M,int x,int y)
        {
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    M[i, j] = Sigmoid(M[i, j]);
        }    
        void acctArray(double [,]  M,int x,int y)
        {
            for(int i=0;i<x;i++)
                for(int j=0;j<y;j++)
                    if (M[i, j] < 0) M[i, j] = 0;
                    else M[i, j] = 1;
        }
        public double[,] Process()
        {
            double[,] resultH1;
            double[,] resultH2;
            resultH1 = Mul(input, weightMatrixH1);// Matrix [13*13,1] * matrix [13*13,10] = Matrix [10,1]
            sigmoidArray(resultH1, 1, 10);
            resultH2 = Mul(resultH1, weightMatrixH2);// 10,1 * 5 10
            sigmoidArray(resultH2,1,5);
            output = Mul(resultH2, weightMatrixOutput); // 1,5* 2,5 = 1,2
            acctArray(output, 1, 2);
            return output;
        }
        void exportWeightVector()
        {
            int i, j;
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

