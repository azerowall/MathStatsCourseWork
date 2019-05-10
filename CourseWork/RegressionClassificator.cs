using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    class RegressionClassificator
    {
        public double[] Classes;

        public RegressionClassificator(Regression r, double[] y)
        {
            List<double> classes = new List<double>();
            foreach (double yi in y)
                if (!classes.Contains(yi))
                    classes.Add(yi);
            Classes = classes.ToArray();
        }

        public double Classificate(double calculated)
        {
            int iclass = 0;
            double min = Math.Abs(Classes[iclass] - calculated);
            for (int i = 1; i < Classes.Length; i++)
            {
                double t = Math.Abs(Classes[i] - calculated);
                if (t < min)
                {
                    iclass = i;
                    min = t;
                }
            }
            return Classes[iclass];
        }
    }
}
