﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    class Regression
    {
        public double[] Coeffs;
        public double F;
        public double FCritical;
        public bool IsSignificance => F > FCritical;
        public double[] CoeffsT;
        public double TCritical;

        public double[] IntervalEstimates;
        public double[] CoeffsIntervalEstimates;

        public bool IsSignificanceCoeff(double t) => t > TCritical;

        public double ApproximationError;

        double[,] XMat;
        public double[] RealY;
        public double[] CalculatedY;
        double[,] XTXInvMat;
        double S2;

        public Regression(double[][] columns, double[] y)
        {
            XMat = TableToMatrix(columns);
            XTXInvMat = Matrix.GetInverse(Matrix.Mul(Matrix.GetTranspose(XMat), XMat));

            RealY = (double[])y.Clone();
            Coeffs = CalcRegressionCoeffs(XMat, RealY);
            CalculatedY = Matrix.MulVect(XMat, Coeffs);

            CalcEstimates();
        }

        /// <summary>
        /// Конвертирует таблицу с выборкой в матрицу с единичным первым столбцом и без столбца iy
        /// </summary>
        /// <param name="tbl"></param>
        /// <param name="iy"></param>
        /// <returns></returns>
        private static double[,] TableToMatrix(double[][] columns)
        {
            int rowsCount = columns[0].Length;
            int colsCount = columns.Length;

            double[,] mat = new double[rowsCount, colsCount + 1];
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                mat[i, 0] = 1;
                for (int j = 1; j < mat.GetLength(1); j++)
                    mat[i, j] = columns[j - 1][i];
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
        /// <param name="x">Таблица с выборкой</param>
        /// <param name="y">Индекс зависимого параметра в таблице</param>
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
        /// Вычислить интервал предсказания для заданного
        /// набора параметров x (без единицы для свободного члена)
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public double CalcPredictionInterval(double[] x)
        {
            double[] fullX = new double[Coeffs.Length];
            fullX[0] = 1;
            Array.Copy(x, 0, fullX, 1, fullX.Length - 1);
            return TCritical *
                    Math.Sqrt(S2 * Matrix.ScalarMul(fullX, Matrix.MulVect(XTXInvMat, fullX)) + 1);
        }

        void CalcEstimates()
        {
            int n = XMat.GetLength(0), k = XMat.GetLength(1) - 1;
            
            // коэффициенты, необходимые далее
            double Qost = CalculatedY.Zip(RealY, (a, b) => (a - b) * (a - b)).Sum();
            double QR = Matrix.ScalarMul(CalculatedY, CalculatedY);
            S2 = Qost / (n - k - 1);

            // точечная оценка регресии
            F = (QR / (k + 1)) / (Qost / (n - k - 1));
            FCritical = FisherTable.GetValue(k + 1, n - k - 1);

            // точечная оценка параметров регрессии
            double[] Sb = Enumerable.Range(0, k + 1)
                                    .Select(j => Math.Sqrt(S2 * XTXInvMat[j, j]))
                                    .ToArray();
            CoeffsT = Coeffs.Zip(Sb, (a, b) => Math.Abs(a / b)).ToArray();
            TCritical = StudentTable.GetValue(n - k - 1);

            // интервальная оценка регрессии
            IntervalEstimates = new double[n];
            for (int i = 0; i < n; i++)
            {
                var x0 = Enumerable.Range(0, XMat.GetLength(1)).Select(j => XMat[i, j]).ToArray();
                IntervalEstimates[i] = TCritical *
                    Math.Sqrt(S2 * Matrix.ScalarMul(x0, Matrix.MulVect(XTXInvMat, x0)));
            }
            // интервальная оценка параметров регрессии
            CoeffsIntervalEstimates = Sb.Select(s => Math.Abs(s * TCritical)).ToArray();

            ApproximationError = RealY.Zip(CalculatedY, (a, b) => Math.Abs((a - b) / a)).Sum() / RealY.Length;
        }

        public string EquationToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0:f4}", Coeffs[0]);
            for (int i = 1; i < Coeffs.Length; i++)
            {
                sb.Append(" + ");
                if (Coeffs[i] < 0)
                    sb.AppendFormat("({0:f4})", Coeffs[i]);
                else
                    sb.AppendFormat("{0:f4}", Coeffs[i]);
                sb.AppendFormat(" * X{0}", i - 1);
            }
            return sb.ToString();
        }
    }
}
