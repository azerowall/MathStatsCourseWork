using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    static class CriticalChiSquaredTable
    {
        static readonly double[] Values = new double[]
        {
            3.84146, 5.99146, 7.81473, 9.48773, 11.07050, 12.59159, 14.06714, 15.50731, 16.91898,
            18.30704, 19.67514, 21.02607, 22.36203, 23.68479, 24.99579, 26.29623, 27.58711,
            28.86930, 30.14353, 31.41043, 32.67057, 33.92444, 35.17246, 36.41503, 37.65248,
            38.88514, 40.11327, 41.33714, 42.55697, 43.77297
        };

        /*
        static readonly double[] DegsOfFreedom = new double[]
        {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
            21, 22, 23, 24, 25, 26, 27, 28, 29, 30
        };*/

        public static double GetValue(int k)
        {
            if (k >= 30) return Values[Values.Length - 1];

            return Values[k];
        }
    }
}
