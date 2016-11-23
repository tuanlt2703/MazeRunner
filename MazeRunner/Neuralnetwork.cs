using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MazeRunner.Controls;
using MathNet.Numerics.LinearAlgebra.Double;
namespace MazeRunner
{
    public class NeuralNework
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
        public NeuralNework(int inputSize, int fstLayersize, int sndLayersize)
        {
            /*
             *  Create Random Weight Matrix
             */
            Random rnd = new Random(System.DateTime.Now.Millisecond);
            weightMatrixH1 = new double[inputSize, fstLayersize];// input size = 13*13
            for (int i = 0; i < 169; i++)
                for (int j = 0; j < 10 ; j++)
                {
                    weightMatrixH1[i, j] = rnd.NextDouble();
                }
            weightMatrixH2 = new double[fstLayersize, sndLayersize];
            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 5; j++)
                {
                    weightMatrixH2[i, j] = rnd.NextDouble();
                }
            this.weightMatrixOutput = new double[sndLayersize, 2];
            for (int i = 0; i < 5; i++)
                for (int j = 0; j < 2; j++)
                {
                    weightMatrixH1[i, j] = rnd.NextDouble();
                }
            /*
             *input matrix have 13*13  
             */
            this.input = new double[1, inputSize];
            this.output = new double[2, 1];
        }
        public NeuralNework(List<double>WvI,int[,] MapMatrix) // When you already have a Weight vector and want tocreate a new neural network , import it hear
        {
            int i=0, j, k = 0,m1,n1,m2,n2,mo,no;
            this.input = new double[1, 13 * 13];
            this.output = new double[2, 1];
            this.weightMatrixH1 = new double[169, 10];
            this.weightMatrixH2 = new double[10, 5];
            this.weightMatrixOutput = new double[5, 2];

            for (i = 0; i < 13; i++)
                for (j = 0; j < 13; j++)
                {
                    this.input[0, k] = (double)(MapMatrix[i, j]) / 4;
                    k++;
                } //import new Map
            // import new weigth vector 
            bool v1=false, v2=false, v3=false;
            
           for (m1 = 0; m1 < 169; m1++)
                    for (n1 = 0; n1 < 10; n1++)
                    {
                        weightMatrixH1[m1, n1] = WvI[i];
                        i++;
                    };
            for (m2 = 0; m2 < 10; m2++)
                for (n2 = 0; n2 < 5; n2++)
                {
                    weightMatrixH1[m2, n2] = WvI[i];
                    i++;
                };
            for (mo = 0; mo < 5; mo++)
                for (no = 0; no < 2; no++)
                {
                    weightMatrixOutput[mo, no] = WvI[i];
                    i++;
                };
        }
        public void Setinput(int[,] MapMatrix)
        {
            int i, j, k = 0;
            for (i = 0; i < 13; i++)
                for (j = 0; j < 13; j++)
                {
                    this.input[0, k] = (double)(MapMatrix[i, j] )/ 4;
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
        void sigmoidArray(double[,] M, int x, int y)
        {
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                    M[i, j] = Sigmoid(M[i, j]);
        }
        void acctArray(double[,] M)
        {
            for (int i = 0; i < 2; i++)
                if (M[0, i] < 0.2) M[0, i] = 0;
                else M[0, i] = 1;
            // 11 Up -- 10 right  01 left 00 down 
        }
        public double[,] Process()
        {
            double[,] resultH1;
            double[,] resultH2;
            resultH1 = Mul(input, weightMatrixH1);// Matrix [13*13,1] * matrix [13*13,10] = Matrix [10,1]
            sigmoidArray(resultH1, 1, 10);
            resultH2 = Mul(resultH1, weightMatrixH2);// 10,1 * 5 10
            sigmoidArray(resultH2, 1, 5);
            output = Mul(resultH2, weightMatrixOutput); // 1,5* 2,5 = 1,2
            acctArray(output);
            return output;
        }
        public void exportWeightVector()
        {
            int i, j;
            WeightVector = new List<double>();
            for (i = 0; i < 169; i++)
                for (j = 0; j < 10; j++)
                    WeightVector.Add(weightMatrixH1[i, j]);
            for (i = 0; i < 10; i++)
                for (j = 0; j < 5; j++)
                    WeightVector.Add(weightMatrixH2[i, j]);
            for (i = 0; i < 5; i++)
                for (j = 0; j < 2; j++)
                    WeightVector.Add(weightMatrixOutput[i, j]);
        }
    }
}

