using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    static class Matrix
    {
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
                {
                    SwapRows(mat, i, idx);
                }
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
    }
}
