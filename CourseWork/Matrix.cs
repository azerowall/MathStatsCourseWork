using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    static class Matrix
    {

        #region Решение СЛАУ, обратная матрица, детерминант

        enum GaussMethod
        {
            SwapIfZero,
            LeadingInRow,
            LeadingInColumn,
            LeadingInMatrix
        }

        /// <summary>
        /// Решение СЛАУ методом Гаусса-Жордана
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double[] SolveSLE(double[,] mat, double[] b)
        {
            mat = (double[,])mat.Clone();
            b = (double[])b.Clone();
            int n = mat.GetLength(0);
            
            for (int i = 0; i < n; i++)
            {
                SwapRowsIfZero(mat, i);

                double t = mat[i, i];
                for (int icol = 0; icol < n; icol++)
                    mat[i, icol] /= t;
                b[i] /= t;

                for (int irow = 0; irow < n; irow++)
                {
                    if (irow == i) continue;
                    t = -mat[irow, i];
                    for (int icol = i; icol < n; icol++)
                        mat[irow, icol] += t * mat[i, icol];
                    b[irow] += t * b[i];
                }
            }
            return b;
        }

        public static double Determinant(double[,] mat)
        {
            mat = (double[,])mat.Clone();
            for (int i = 0; i < mat.GetLength(0) - 1; i++)
            {
                SwapRowsIfZero(mat, i);

                double inv = 1 / mat[i, i];
                for (int row = i + 1; row < mat.GetLength(0); row++)
                {
                    double factor = -inv * mat[row, i];
                    for (int col = i; col < mat.GetLength(1); col++)
                        mat[row, col] += factor * mat[i, col];
                }
            }
            double det = 1;
            for (int i = 0; i < mat.GetLength(0); i++)
                det *= mat[i, i];
            return det;
        }

        static void SwapRowsIfZero(double[,] mat, int i)
        {
            if (mat[i, i] == 0)
            {
                int idx = i + 1;
                while (idx < mat.GetLength(0) && mat[idx, i] == 0)
                    idx++;
                if (idx != mat.GetLength(0) - 1)
                    SwapRows(mat, i, idx);
            }
        }

        static void SwapRows(double[,] mat, int i, int j)
        {
            for (int col = 0; col < mat.GetLength(1); col++)
            {
                double t = mat[i, col];
                mat[i, col] = mat[j, col];
                mat[j, col] = t;
            }
        }

        public static double[,] GetInverse(double[,] mat)
        {
            int n = mat.GetLength(0);
            double[] b = new double[n];
            double[,] result = new double[n, n];

            for (int i = 0; i < n; i++)
            {
                b[i] = 1;
                double[] tvect = SolveSLE(mat, b);
                for (int j = 0; j < n; j++)
                    result[j, i] = tvect[j];
                b[i] = 0;
            }
            return result;
        }

        #endregion


        #region Минор и алгебраическое дополнение

        public static double Minor(double[,] mat, int row, int col)
        {
            double[,] m = new double[mat.GetLength(0) - 1, mat.GetLength(0) - 1];
            for (int i = 0; i < mat.GetLength(0) - 1; i++)
                for (int j = 0; j < mat.GetLength(1) - 1; j++)
                {
                    int roff = i >= row ? 1 : 0;
                    int coff = j >= col ? 1 : 0;
                    m[i, j] = mat[i + roff, j + coff];
                }
            return Determinant(m);
        }
        public static double AlgebraicComplement(double[,] mat, int row, int col)
        {
            double m = Minor(mat, row, col);
            return (row + col) % 2 == 0 ? m : -m;
        }

        #endregion

        #region Стандартные операции

        public static double[,] GetTranspose(double[,] mat)
        {
            double[,] tmat = new double[mat.GetLength(1), mat.GetLength(0)];
            for (int i = 0; i < mat.GetLength(0); i++)
                for (int j = 0; j < mat.GetLength(1); j++)
                    tmat[j, i] = mat[i, j];
            return tmat;
        }

        public static double[,] Mul(double[,] mat1, double[,] mat2)
        {
            if (mat1.GetLength(1) != mat2.GetLength(0))
                throw new ArgumentException();
            double[,] res = new double[mat1.GetLength(0), mat2.GetLength(1)];
            for (int i = 0; i < mat1.GetLength(0); i++)
                for (int j = 0; j < mat2.GetLength(1); j++)
                {
                    res[i, j] = 0;
                    for (int k = 0; k < mat1.GetLength(1); k++)
                        res[i, j] += mat1[i, k] * mat2[k, j];
                }
            return res;
        }
        public static double[] MulVect(double[,] mat, double[] vect)
        {
            if (mat.GetLength(1) != vect.Length)
                throw new ArgumentException();
            double[] result = new double[mat.GetLength(0)];
            for (int i = 0; i < mat.GetLength(0); i++)
            {
                result[i] = 0;
                for (int j = 0; j < mat.GetLength(1); j++)
                    result[i] += mat[i, j] * vect[j];
            }
            return result;
        }

        public static double ScalarMul(IEnumerable<double> v1, IEnumerable<double> v2)
        {
            return v1.Zip(v2, (a, b) => a * b).Sum();
        }

        #endregion
    }
}
