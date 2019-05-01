using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CourseWork
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Table table;
        public DescriptiveStatistics[] stats;
        public PearsonChiSquared[] chiSquared;
        public Correlations correlations;
        public Regression regression;

        public PearsonChiSquared[] ChiSquared => chiSquared;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            table = new Table(@"iris.csv");
            table.Normalize();

            stats = new DescriptiveStatistics[table.ColumnsCount];
            for (int i = 0; i < table.ColumnsCount; i++)
                stats[i] = new DescriptiveStatistics(table.ColumnsValues[i]);

            chiSquared = new PearsonChiSquared[table.ColumnsCount];
            for (int i = 0; i < table.ColumnsCount; i++)
                chiSquared[i] = new PearsonChiSquared(table.Headers[i], table.ColumnsValues[i], stats[i], 7);


            var sb = new StringBuilder();
            for (int i = 0; i < table.ColumnsCount; i++)
            {
                sb.AppendLine(table.Headers[i]);
                sb.AppendLine(stats[i].ToString());
            }
            tbDescriptiveStatistics.Text = sb.ToString();

            correlations = new Correlations(table, stats);
            regression = new Regression(table, 4);


            tblCorrMatrix.SetTable<double>(correlations.CorrMatrix, table.ShortedHeaders, table.ShortedHeaders, CorrelationsHighlighter);
            tblPartialCorrMatrix.SetTable<double>(correlations.PartialCorrMatrix, table.ShortedHeaders, table.ShortedHeaders, CorrelationsHighlighter);


            tblSignificanceMatrix.SetTable<double>(correlations.SignificanceMatrix, table.ShortedHeaders, table.ShortedHeaders, SignificanceHighlighter);


            var compCorr = Enumerable.Range(0, correlations.CorrMatrix.GetLength(0)).Select(i =>
                                Enumerable.Range(0, correlations.CorrMatrix.GetLength(1)).Select(j =>
                                {
                                    double r = Math.Abs(correlations.CorrMatrix[i, j]);
                                    double pr = Math.Abs(correlations.PartialCorrMatrix[i, j]);
                                    if (r > pr)
                                        return "усиление";
                                    else if (r < pr)
                                        return "ослабление";
                                    else
                                        return "-";
                                }));
            tblCorrCompareMatrix.SetTable(compCorr, table.ShortedHeaders, table.ShortedHeaders);

            var multCorr = Enumerable.Range(0, correlations.CorrMatrix.GetLength(0)).Select(i =>
                                Enumerable.Range(0, correlations.CorrMatrix.GetLength(1)).Select(j =>
                                {
                                    double mcc = correlations.MultipleCorrletaionCoeffs[j];
                                    if (i == 0)
                                        return Math.Round(mcc, 4);
                                    else
                                        return Math.Round(mcc * mcc, 4);
                                }));
            tblMultipleCorrelationCoeffs.SetTable<double>(multCorr, new[] { "R", "D" }, table.ShortedHeaders);

            

            tbLegend.Text = string.Join("\n", table.ShortedHeaders.Zip(table.Headers, (sh, h) => $"{sh} - {h}"));
        }

        Brush WeakCorrelationColor = new SolidColorBrush(Color.FromRgb(230, 230, 254));
        Brush MediumCorrelationColor = new SolidColorBrush(Color.FromRgb(190, 190, 254));
        Brush StrongCorrelationColor = new SolidColorBrush(Color.FromRgb(140, 140, 254));
        private Brush CorrelationsHighlighter(int i, int j, double d)
        {
            d = Math.Abs(d);
            if (d >= 0.4 && d < 0.6)
                return WeakCorrelationColor;
            else if (d >= 0.6 && d < 0.9)
                return MediumCorrelationColor;
            else if (d >= 0.9)
                return StrongCorrelationColor;
            else
                return Brushes.White;
        }

        private Brush SignificanceHighlighter(int i, int j, double d)
        {
            if (Correlations.IsSignificance(d))
                return Brushes.LightGreen;
            return Brushes.White;
        }

        private IEnumerable<IEnumerable<double>> EnumerateMat(double[,] mat)
        {
            return Enumerable.Range(0, mat.GetLength(0)).Select(i =>
                        Enumerable.Range(0, mat.GetLength(1)).Select(j =>
                            Math.Round(mat[i, j], 5)));
        }
        /*
        private IEnumerable<string> EnumerateDS(DescriptiveStatistics ds)
        {
            yield return ds.Average;
            yield return ds.Dispersion;
        }*/

        private void cPleiadesDiagram_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Diagrams.PleiadesDiagram.Draw(cPleiadesDiagram, correlations);
        }
    }
}
