using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    public class DescriptiveStatistics
    {
        public string ParameterName { get; private set; }

        public double Average { get; private set; }
        public double Dispersion { get; private set; }
        public double StandardDeviation { get; private set; }
        public double StandardError { get; private set; }
        public double Max { get; private set; }
        public double Min { get; private set; }
        public double Median { get; private set; }
        public double? Mode { get; private set; }
        public double Interval { get; private set; }
        public double Sum { get; private set; }
        public double Excess { get; private set; }
        public double Asymmetry { get; private set; }
        public int Count { get; private set; }
        public double MarginalError { get; private set; }
        public int RequiredSize { get; private set; }
        public double MarginalErrorWithRequiredSize { get; private set; }

        public DescriptiveStatistics(string name, double[] vals)
        {
            ParameterName = name;
            
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

            double t = StudentTable.GetValue(Count - 1);
            MarginalError = t * StandardError;
            double neededError = 0.01;
            RequiredSize = (int)(t * t * Dispersion / (neededError * neededError));
            MarginalErrorWithRequiredSize = t * StandardDeviation / Math.Sqrt(RequiredSize);
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
