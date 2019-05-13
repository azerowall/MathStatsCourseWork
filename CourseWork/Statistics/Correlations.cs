using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    public class Correlations
    {
        public double[,] CorrMatrix;
        public double[,] PartialCorrMatrix;
        public double[,] SignificanceMatrix;
        public double[] MultipleCorreletaionCoeffs;
        public double[] MultipleDeterminationCoeffs;
        public double[] MultipleCorrSignificance;

        public int ParametersCount => CorrMatrix.GetLength(0);

        public double Critical;
        public bool IsSignificance(double r)
        {
            return r > Critical;
        }

        public double MultiCritical;
        public bool IsMultipleCoeffSignificance(double F) => F > MultiCritical;

        public Correlations(Table tbl, DescriptiveStatistics[] dstats)
        {
            CalcCorrMatrix(tbl, dstats);
            CalcPartialCorrMatrix(tbl);
            CalcMultipleCorrelation(tbl);
            MultipleDeterminationCoeffs = MultipleCorreletaionCoeffs.Select(a => a * a).ToArray();
            CalcMultipleSignificance(tbl);
            CalcSignificanceMatrix(tbl);
            Critical = StudentTable.GetValue(tbl.RowsCount - 2);
        }

        private void CalcCorrMatrix(Table tbl, DescriptiveStatistics[] dstats)
        {
            CorrMatrix = new double[tbl.ColumnsCount, tbl.ColumnsCount];
            for (int i = 0; i < CorrMatrix.GetLength(0); i++)
                CorrMatrix[i, i] = 1.0;
            for (int i = 1; i < CorrMatrix.GetLength(0); i++)
                for (int j = 0; j < i; j++)
                {
                    double sum1 = Enumerable.Range(0, tbl.RowsCount)
                                            .Sum(idx => tbl[idx, i] * tbl[idx, j]);
                    double num = tbl.RowsCount * sum1 - dstats[i].Sum * dstats[j].Sum;
                    double den = Math.Abs(dstats[i].Count * tbl.ColumnsValues[i].Sum(d => d * d) - 
                                          dstats[i].Sum * dstats[i].Sum) * 
                                 Math.Abs(dstats[j].Count * tbl.ColumnsValues[j].Sum(d => d * d) -
                                          dstats[j].Sum * dstats[j].Sum);
                    den = Math.Sqrt(den);
                    CorrMatrix[j, i] = CorrMatrix[i, j] = num / den;
                }
        }

        private void CalcPartialCorrMatrix(Table tbl)
        {
            PartialCorrMatrix = new double[tbl.ColumnsCount, tbl.ColumnsCount];
            for (int i = 0; i < tbl.ColumnsCount; i++)
                PartialCorrMatrix[i, i] = 1;
            for (int i = 1; i < tbl.ColumnsCount; i++)
                for (int j = 0; j < i; j++)
                {
                    double num = Matrix.AlgebraicComplement(CorrMatrix, i, j);
                    double den1 = Matrix.AlgebraicComplement(CorrMatrix, i, i);
                    double den2 = Matrix.AlgebraicComplement(CorrMatrix, j, j);

                    PartialCorrMatrix[j, i] = PartialCorrMatrix[i, j] = num / Math.Sqrt(den1 * den2);
                }
        }

        private void CalcMultipleCorrelation(Table tbl)
        {
            List<double> res = new List<double>();
            for (int i = 0; i < tbl.ColumnsCount; i++)
                res.Add(Math.Sqrt(1 - Matrix.Determinant(CorrMatrix) / Matrix.AlgebraicComplement(CorrMatrix, i, i)));
            MultipleCorreletaionCoeffs = res.ToArray();
        }

        private void CalcSignificanceMatrix(Table tbl)
        {
            SignificanceMatrix = new double[tbl.ColumnsCount, tbl.ColumnsCount];
            for (int i = 1; i < tbl.ColumnsCount; i++)
                for (int j = 0; j < i; j++)
                {
                    double r = CorrMatrix[i, j];
                    double t = Math.Abs(r) * Math.Sqrt((tbl.RowsCount - 2) / (1 - r * r));

                    SignificanceMatrix[i, j] = SignificanceMatrix[j, i] = t;
                }
        }

        private void CalcMultipleSignificance(Table tbl)
        {
            int n = tbl.RowsCount, k = tbl.ColumnsCount;
            MultipleCorrSignificance = new double[k];
            for (int i = 0; i < MultipleCorreletaionCoeffs.Length; i++)
            {
                double R = MultipleCorreletaionCoeffs[i];
                double F = (R * R * (n - k - 1)) / ((n - R * R) * k);
                MultipleCorrSignificance[i] = F;
            }
        }
    }
}
