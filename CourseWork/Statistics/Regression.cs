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
        //public int DependentIndex { get; private set; }

        public Regression(Table tbl, int iy)
        {
            Coeffs = CalcRegressionCoeffs(tbl, iy);
            Error = CalcSLMError(tbl, iy);
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
            double[] coeffs = Matrix.MulVect(xTx_xT, y);

            // свободный член ставим в начало
            // а остальные смещаем дальше
            double t = coeffs[iy];
            Array.Copy(coeffs, 0, coeffs, 1, iy);
            coeffs[0] = t;
            return coeffs;
        }

        /// <summary>
        /// Вычислить ошибку - сумму квадратов отклонений
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="iy"></param>
        /// <param name="coeffs"></param>
        /// <returns></returns>
        private double CalcSLMError(Table tbl, int iDepended)
        {
            double error = 0;
            for (int i = 0; i < tbl.RowsCount; i++)
            {
                //double calculatedY = coeffs[0];
                //for (int j = 0; j < tbl.ColumnsCount; j++)
                //{
                //    if (j == iy) continue;
                //    calculatedY += tbl[i, j] * coeffs[j];
                //}
                double calculatedY = CalcY(Enumerable.Range(0, tbl.ColumnsCount)
                                              .Where(j => j != iDepended)
                                              .Select(j => tbl[i, j]));

                double absError = tbl[i, iDepended] - calculatedY;
                error += absError * absError;
            }
            return error;
        }
    }
}
