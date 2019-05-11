using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWork
{
    /// <summary>
    /// Таблица распределения Стьюдента для значимости a = 0.05
    /// </summary>
    static class StudentTable
    {
        static readonly double[] Values = new double[] {
            12.706,  4.303,  3.182,  2.776,  2.571,  2.447,  2.365,  2.306,  2.262,
            2.228,  2.201,  2.179,  2.16,  2.145,  2.131,  2.12,  2.11,  2.101,  2.093,
            2.086,  2.08,  2.074,  2.069,  2.064,  2.06,  2.056,  2.052,  2.048,  2.045,
            2.042,  2.021,  2.009,  2,  1.99,  1.984,  1.98,  1.972,  1.9695,  1.9679,  1.9659,  1.9640 };

        static readonly double[] DegreesOfFreedom = new double[] {
            1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21,
            22, 23, 24, 25, 26, 27, 28, 29, 30, 40, 50, 60, 80, 100, 120, 200, 250, 300, 400, 500
        };

        /// <summary>
        /// Получить значение из таблицы Стьюдента для a = 0.05
        /// k = n - 1 для предельной ошибки
        /// k = n - 2 для значимости коэффициентов матрицы корреляций
        /// </summary>
        /// <param name="k">Число степеней свободы</param>
        /// <returns></returns>
        public static double GetValue(int k)
        {
            int i = 0;
            while (i < DegreesOfFreedom.Length && DegreesOfFreedom[i] < k) i++;

            if (i == DegreesOfFreedom.Length) return Values[Values.Length - 1];
            if (k == DegreesOfFreedom[i]) return Values[i];
            else if (i >= DegreesOfFreedom.Length - 1) return Values[Values.Length - 1];

            return Values[i - 1] + (Values[i] - Values[i - 1]) *
                    (k - DegreesOfFreedom[i - 1]) / (DegreesOfFreedom[i] - DegreesOfFreedom[i - 1]);
        }
    }
}
