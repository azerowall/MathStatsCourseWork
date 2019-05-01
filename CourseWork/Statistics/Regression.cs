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
        public double Error { get; private set; }
        public int DependentIndex { get; private set; }

        public Regression(Table tbl, int iy)
        {
            DependentIndex = iy;
            Coeffs = CalcRegressionCoeffs(tbl, iy);
            Error = CalcSLMError(tbl, iy, Coeffs);
        }
        
        /// <summary>
        /// Вычислить 'y' по вектору 'x'
        /// </summary>
        /// <param name="x">Вектор 'x'</param>
        /// <returns></returns>
        public double CalcY(double[] x)
        {
            double y = Coeffs[DependentIndex];
            for (int i = 0; i < x.Length; i++)
            {
                if (i >= DependentIndex)
                    y += x[i] * Coeffs[i + 1];
                else
                    y += x[i] * Coeffs[i];
            }
            return y;
        }

        /// <summary>
        /// Вычисление коэффициентов регрессии
        /// </summary>
        /// <param name="tbl">Таблица с выборкой</param>
        /// <param name="iy">Индекс зависимого параметра в таблице</param>
        public static double[] CalcRegressionCoeffs(Table tbl, int iy)
        {
            double[,] x = new double[tbl.RowsCount, tbl.ColumnsCount];
            
            for (int i = 0; i < x.GetLength(0); i++)
                for (int j = 0; j < x.GetLength(1); j++)
                    x[i, j] = tbl[i, j];

            double[] y = (double[])tbl.ColumnsValues[iy].Clone();
            // возможно перед этим нужно поменять столбцы местами
            for (int i = 0; i < x.GetLength(0); i++)
                x[i, iy] = 1;

            double[,] xT = Matrix.GetTranspose(x);
            double[,] xTx = Matrix.Mul(xT, x);
            double[,] xTx_ = Matrix.GetInverse(xTx);
            double[,] xTx_xT = Matrix.Mul(xTx_, xT);
            return Matrix.MulVect(xTx_xT, y);
        }

        /// <summary>
        /// Вычислить ошибку - сумму квадратов отклонений
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="iy"></param>
        /// <param name="coeffs"></param>
        /// <returns></returns>
        private static double CalcSLMError(Table tbl, int iy, double[] coeffs)
        {
            double error = 0;
            for (int i = 0; i < tbl.RowsCount; i++)
            {
                double calculatedY = coeffs[iy];
                for (int j = 0; j < tbl.ColumnsCount; j++)
                {
                    if (j == iy) continue;
                    calculatedY += tbl[i, j] * coeffs[j];
                }
                //calculatedY = CalcY(Enumerable.Range(0, tbl.ColumnsCount)
                //                              .Where(j => j != iy)
                //                              .Select(j => tbl[i, j])
                //                              .ToArray());

                double absError = tbl[i, iy] - calculatedY;
                error += absError * absError;
            }
            return error;
        }
    }
}
