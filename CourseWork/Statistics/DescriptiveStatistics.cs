﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    public class DescriptiveStatistics
    {
        //public double[] Values;

        public double Average;
        public double Dispersion;
        public double StandardDeviation;
        public double StandardError;
        public double Max;
        public double Min;
        public double Median;
        public double? Mode;
        public double Interval;
        public double Sum;
        public double Excess;
        public double Asymmetry;
        public double Count;

        public DescriptiveStatistics(double[] vals)
        {
            //Values = vals;
            Sum = vals.Sum();
            Count = vals.Length;
            Average = Sum / Count;
            Dispersion = vals.Sum(e => Math.Pow(e - Average, 2)) / (Count - 1);
            StandardDeviation = Math.Sqrt(Dispersion);
            StandardError = StandardDeviation / Math.Sqrt(Count);
            Min = vals.Min();
            Max = vals.Max();
            Interval = Max - Min;

            Median = GetMedian(vals);
            Mode = GetMode(vals);
            Excess = GetExcess(vals, Average, StandardDeviation);
            Asymmetry = GetAsymmetry(vals, Average, StandardDeviation);
        }
        

        /// <summary>
        /// Медиана
        /// </summary>
        public static double GetMedian(double[] vals)
        {
            double[] selection = (double[])vals.Clone();
            Array.Sort(selection);
            if (selection.Length % 2 == 1)
                return selection[(selection.Length + 1) / 2 - 1];
            else
            {
                double m1 = selection[selection.Length / 2];
                double m2 = selection[selection.Length / 2 - 1];
                return (m1 + m2) / 2;
            }
        }

        /// <summary>
        /// Мода
        /// </summary>
        public static double? GetMode(double[] vals)
        {
            Dictionary<double, int> dictionary = new Dictionary<double, int>();
            foreach (double val in vals)
            {
                if (dictionary.ContainsKey(val))
                    ++dictionary[val];
                else
                    dictionary[val] = 1;
            }
            int maxCount = dictionary.Max(e => e.Value);
            if (maxCount == 1)
                return null;
            else
                return dictionary.First(e => e.Value == maxCount).Key;
        }

        /// <summary>
        /// Эксцесс
        /// </summary>
        public static double GetExcess(double[] vals, double average, double stdDeviation)
        {
            double n = vals.Length;
            double l = (n * (n + 1)) / ((n - 1) * (n - 2) * (n - 3));
            double sum = vals.Select(val => Math.Pow((val - average) / stdDeviation, 4)).Sum();
            double r = (3 * Math.Pow(n - 1, 2)) / ((n - 2) * (n - 3));
            return l * sum - r;
        }

        /// <summary>
        /// Ассиметрия
        /// </summary>
        public static double GetAsymmetry(double[] vals, double average, double stdDeviations)
        {
            double n = vals.Length;
            double l = n / ((n - 1) * (n - 2));
            double sum = vals.Select(val => Math.Pow((val - average) / stdDeviations, 3)).Sum();
            return l * sum;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Среднее {Average}");
            sb.AppendLine($"Стандартная ошибка {StandardError}");
            sb.AppendLine($"Медиана {Median}");
            sb.AppendLine($"Мода {Mode}");
            sb.AppendLine($"Стандартное отклонение {StandardDeviation}");
            sb.AppendLine($"Дисперсия выборки {Dispersion}");
            sb.AppendLine($"Эксцесс {Excess}");
            sb.AppendLine($"Ассиметричность {Asymmetry}");
            sb.AppendLine($"Интервал {Interval}");
            sb.AppendLine($"Минимум {Min}");
            sb.AppendLine($"Максимум {Max}");
            sb.AppendLine($"Сумма {Sum}");
            sb.AppendLine($"Счет {Count}");

            return sb.ToString();
        }
    }
}
