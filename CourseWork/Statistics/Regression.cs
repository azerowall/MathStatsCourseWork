using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    public class Regression
    {
        public double[] Coeffs { get; private set; }
        public double QR { get; private set; }
        public double QResidual { get; private set; }
        public double F { get; private set; }

        public Regression(Table tbl, int iy)
        {
            double[,] x = TableToMatrix(tbl, iy);
            double[] y = (double[])tbl.ColumnsValues[iy].Clone();
            Coeffs = CalcRegressionCoeffs(x, y);
            QResidual = CalcSLMError(x, y);

            double[] xbT = Matrix.MulVect(x, Coeffs);
            QR = xbT.Zip(xbT, (a, b) => a * b).Sum();

            F = (QR / (x.GetLength(1) + 1)) / (QResidual / (x.GetLength(0) - x.GetLength(1) - 1));
            //F = (QR / 2) / (QResidual / (x.GetLength(1) - 2));
        }

        
        private static double[,] TableToMatrix(Table tbl, int iy)
        {
            double[,] mat = new double[tbl.RowsCount, tbl.ColumnsCount];
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                mat[i, 0] = 1;
                for (int j = 1; j < mat.GetLength(1); j++)
                {
                    if (j <= iy)
                        mat[i, j] = tbl[i, j - 1];
                    else
                        mat[i, j] = tbl[i, j];
                }
            }
            return mat;
        }

        /// <summary>
        /// Вычислить 'y' по вектору 'x'
        /// </summary>
        /// <param name="x">Вектор 'x'</param>
        /// <returns></returns>
        public double CalcY(IEnumerable<double> x)
        {
            return Coeffs[0] + Coeffs.Skip(1).Zip(x, (c, xi) => c * xi).Sum();
        }

        /// <summary>
        /// Вычисление коэффициентов регрессии
        /// </summary>
        /// <param name="tbl">Таблица с выборкой</param>
        /// <param name="iy">Индекс зависимого параметра в таблице</param>
        public static double[] CalcRegressionCoeffs(double[,] x, double[] y)
        {
            double[,] xT = Matrix.GetTranspose(x);
            double[,] xTx = Matrix.Mul(xT, x);
            double[,] xTx_ = Matrix.GetInverse(xTx);
            double[,] xTx_xT = Matrix.Mul(xTx_, xT);
            double[] coeffs = Matrix.MulVect(xTx_xT, y);
            return coeffs;
        }

        /// <summary>
        /// Вычислить ошибку - сумму квадратов отклонений
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="iy"></param>
        /// <param name="coeffs"></param>
        /// <returns></returns>
        private double CalcSLMError(double[,] x, double[] y)
        {
            double error = 0;
            for (int i = 0; i < x.GetLength(0); i++)
            {
                double calculatedY = CalcY(Enumerable.Range(1, x.GetLength(1)).Select(j => x[i, j]));
                double absError = y[i] - calculatedY;
                error += absError * absError;
            }
            return error;
        }
    }
}
