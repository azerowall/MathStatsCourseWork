using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CourseWork
{
    class PearsonChiSquared
    {
        public int[] MReal;
        public double[] MTheoretical;
        public double ChiSquared;
        public int IntervalsCount => MReal.Length;

        public PearsonChiSquared(double[] vals, DescriptiveStatistics ds, int intervalsCount)
        {
            CalcChiSquared(vals, ds, intervalsCount);
        }

        private void CalcChiSquared(double[] vals, DescriptiveStatistics ds, int stepsCount)
        {
            MReal = new int[stepsCount];
            MTheoretical = new double[stepsCount];
            double step = ds.Interval / stepsCount;
            foreach (double val in vals)
            {
                for (int i = 0; i < MReal.Length; i++)
                    if (val <= ds.Min + step * (i + 1))
                    {
                        MReal[i] += 1;
                        break;
                    }
            }

            double sqrt2pi = Math.Sqrt(2 * Math.PI);
            for (int i = 0; i < MTheoretical.Length; i++)
            {
                //double x = stat.Min + (step * (i + 1)) / 2;
                double x = ds.Min + step * i + step / 2;
                double u = (x - ds.Average) / ds.StandardDeviation;
                double f = Math.Pow(Math.E, -u * u / 2) / sqrt2pi;
                double p = (step / ds.StandardDeviation) * f;
                MTheoretical[i] = ds.Count * p;
            }

            ChiSquared = 0;
            for (int i = 0; i < MReal.Length; i++)
                ChiSquared += Math.Pow((double)MReal[i] - MTheoretical[i], 2) / MTheoretical[i];
        }

        public static bool HasCriterion(double chi)
        {
            // степень свободы
            //int k = mReal.Length - 3;
            //double a = 0.05;
            //double critChi = 5.99146; // критическое хи-квадрат для a=0.05 и k=5-3=2
            //double critChi = 14.06714; // a=0.05 k=10-3=7
            double critChi = 9.48773; // k = 4
            return chi < critChi;
        }
    }
}
