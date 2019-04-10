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
        public static double CalcChiSquared(double[] vals, DescriptiveStatistics stat, int stepsCount)
        {
            int[] mReal = new int[stepsCount];
            double[] mTheoretical = new double[stepsCount];
            double step = stat.Interval / stepsCount;
            foreach (double val in vals)
            {
                for (int i = 0; i < mReal.Length; i++)
                    if (val <= stat.Min + step * (i + 1))
                    {
                        mReal[i] += 1;
                        break;
                    }
            }

            double sqrt2pi = Math.Sqrt(2 * Math.PI);
            for (int i = 0; i < mTheoretical.Length; i++)
            {
                //double x = stat.Min + (step * (i + 1)) / 2;
                double x = stat.Min + step * i + step / 2;
                double u = (x - stat.Average) / stat.StandardDeviation;
                double f = Math.Pow(Math.E, -u * u / 2) / sqrt2pi;
                double p = (step / stat.StandardDeviation) * f;
                mTheoretical[i] = stat.Count * p;
            }

            //MessageBox.Show(string.Join(", ", mReal) + "\n" + string.Join(", ", mTheoretical));

            double chi = 0;
            for (int i = 0; i < mReal.Length; i++)
                chi += Math.Pow((double)mReal[i] - mTheoretical[i], 2) / mTheoretical[i];
            return chi;
        }

        public static bool HasCriterion(double chi)
        {
            // степень свободы
            //int k = mReal.Length - 3;
            //double a = 0.05;
            //double critChi = 5.99146; // критическое хи-квадрат для a=0.05 и k=5-3=2
            double critChi = 14.06714; // a=0.05 k=10-3=7
            return chi < critChi;
        }
    }
}
