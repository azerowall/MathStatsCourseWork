using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CourseWork
{
    public class PearsonChiSquared
    {
        public int[] MReal;
        public double[] MTheoretical;
        public string ParameterName { get; private set; }
        public double ChiSquared { get; private set; }
        public int IntervalsCount => MReal.Length;

        //static double criticalChi = 5.99146;  // k=5-3=2
        //static double criticalChi = 14.06714; // k=10-3=7
        //static double criticalChi = 9.48773; // k = 7-3 = 4
        double CriticalChi = 7.81473; // k = 6-3 = 3
        public bool HasCriterion => ChiSquared < CriticalChi;

        public PearsonChiSquared(string name, double[] vals, DescriptiveStatistics ds, int intervalsCount)
        {
            ParameterName = name;
            CalcChiSquared(vals, ds, intervalsCount);
            CriticalChi = CriticalChiSquaredTable.GetValue(intervalsCount - 3);
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
    }
}
