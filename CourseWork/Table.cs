using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace CourseWork
{
    struct TableColumn
    {
        public string Header;
        public double[] Values;
    }

    class Table
    {
        public TableColumn[] Columns;

        public Table(string csvfile)
        {
            using (TextFieldParser csv = new TextFieldParser(csvfile))
            {
                csv.SetDelimiters(";");
                var headers = csv.ReadFields();
                Columns = new TableColumn[headers.Length];
                for (int i = 0; i < Columns.Length; i++)
                    Columns[i].Header = headers[i];

                var values = TableReader(csv).ToArray();
                for (int i = 0; i < Columns.Length; i++)
                    Columns[i].Values = values.Select(r => r[i]).ToArray();
            }
        }

        public int ColumnsCount => Columns.Length;
        public int RowsCount => Columns[0].Values.Length;
        public double this[int r, int c]
        {
            get { return Columns[c].Values[r]; }
            set { Columns[c].Values[r] = value; }
        }

        public IEnumerable<double[]> TableReader(TextFieldParser csv)
        {
            while (!csv.EndOfData)
                yield return csv.ReadFields().Select(s => double.Parse(s)).ToArray();
        }

        public void Normalize()
        {
            foreach (var col in Columns)
                Normalize(col.Values);
        }

        void Normalize(double[] vals)
        {
            double interval = vals.Max() - vals.Min();
            for (int i = 0; i < vals.Length; i++)
                vals[i] /= interval;
        }
    }
}
