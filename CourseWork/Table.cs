using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace CourseWork
{

    class Table
    {
        public string[] Headers;
        public double[][] ColumnsValues;
        public string[] ShortedHeaders;

        public Table(string csvfile)
        {
            using (TextFieldParser csvParser = new TextFieldParser(csvfile))
            {
                csvParser.TextFieldType = FieldType.Delimited;
                csvParser.SetDelimiters(";");

                Headers = csvParser.ReadFields();
                ColumnsValues = new double[Headers.Length][];

                List<double[]> rows = new List<double[]>();
                while (!csvParser.EndOfData)
                    rows.Add(csvParser.ReadFields().Select(double.Parse).ToArray());
                
                for (int i = 0; i < ColumnsCount; i++)
                    ColumnsValues[i] = rows.Select(r => r[i]).ToArray();
            }
            ShortedHeaders = Enumerable.Range(0, ColumnsCount).Select(i => $"X{i}").ToArray();
        }

        public int ColumnsCount => ColumnsValues.Length;
        public int RowsCount => ColumnsValues[0].Length;
        public double this[int i, int j]
        {
            get { return ColumnsValues[j][i]; }
            set { ColumnsValues[j][i] = value; }
        }

        public IEnumerable<double[]> TableReader(TextFieldParser csv)
        {
            while (!csv.EndOfData)
                yield return csv.ReadFields().Select(s => double.Parse(s)).ToArray();
        }

        public void Normalize()
        {
            foreach (var col in ColumnsValues)
                Normalize(col);
        }

        void Normalize(double[] vals)
        {
            double interval = vals.Max() - vals.Min();
            for (int i = 0; i < vals.Length; i++)
                vals[i] /= interval;
        }
    }
}
