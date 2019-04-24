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
        Table table;
        DescriptiveStatistics[] stats;
        PearsonChiSquared[] chiSquared;
        Correlations correlations;

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
                chiSquared[i] = new PearsonChiSquared(table.ColumnsValues[i], stats[i], 7);


            var sb = new StringBuilder();
            for (int i = 0; i < table.ColumnsCount; i++)
            {
                sb.AppendLine(table.Headers[i]);
                sb.AppendLine(stats[i].ToString());
            }
            tbDescriptiveStatistics.Text = sb.ToString();

            correlations = new Correlations(table, stats);
            

            // хи-квадраты
            MakeMatrix(grPearsonChiSquared,
                table.Headers,
                new[] { "Хи-квадрат", "Нормальность" },
                (i, j) => {
                    if (i == 0) return Math.Round(chiSquared[j].ChiSquared, 4).ToString();
                    else
                        return PearsonChiSquared.HasCriterion(chiSquared[j].ChiSquared) ? "+" : "-";
                });
            string[] headers = Enumerable.Range(0, table.ColumnsCount).Select(i => "X" + i).ToArray();
            

            tblCorrMatrix.SetTable<double>(correlations.CorrMatrix, table.ShortedHeaders, table.ShortedHeaders, CorrelationsHighlighter);
            tblPartialCorrMatrix.SetTable<double>(correlations.PartialCorrMatrix, table.ShortedHeaders, table.ShortedHeaders, CorrelationsHighlighter);


            tblSignificanceMatrix.SetTable<double>(correlations.SignificanceMatrix, table.ShortedHeaders, table.ShortedHeaders, SignificanceHighlighter);
            /*
            MakeMatrix(grCorrCompareMatrix, headers, headers,
                       (i, j) => Math.Abs(corr.CorrMatrix[i, j]) > Math.Abs(corr.PartialCorrMatrix[i, j]) ? "усиление" : "ослабление");
            MakeMatrix(grCorrCompareMatrix, headers, headers,
                       (i, j) => {
                           if (i == j) return "-";
                           else
                               return Math.Abs(correlations.CorrMatrix[i, j]) > Math.Abs(correlations.PartialCorrMatrix[i, j]) ? "усиление" : "ослабление";
                       });

            MakeMatrix(grMultipleCorrelationCoeffs, headers, new []{ "R", "R^2" },
                       (i, j) => {
                           if (i == 0)
                               return Math.Round(correlations.MultipleCorrletaionCoeffs[j], 4).ToString();
                           else
                               return Math.Round(correlations.MultipleCorrletaionCoeffs[j] * correlations.MultipleCorrletaionCoeffs[j], 4).ToString();
                       });
            */

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
            double critical = 1.9761224936033632;
            if (d > critical)
                return Brushes.LightGreen;
            return Brushes.White;
        }


        public delegate string CellGetter(int i, int j);
        public void MakeMatrix(Grid gr, string[] colHeaders, string[] rowHeaders, CellGetter getter)
        {
            gr.Children.Clear();
            MatrixFillHeaders(gr, colHeaders, rowHeaders);
            for (int i = 0; i < rowHeaders.Length; i++)
            {
                for (int j = 0; j < colHeaders.Length; j++)
                    SetMatrixCell(gr, i+1, j+1, getter(i, j));
            }
        }

        public void MatrixFillHeaders(Grid gr, string[] colHeaders, string[] rowHeaders)
        {
            gr.ColumnDefinitions.Add(new ColumnDefinition());
            gr.RowDefinitions.Add(new RowDefinition());

            for (int i = 0; i < colHeaders.Length; i++)
            {
                gr.ColumnDefinitions.Add(new ColumnDefinition());
                SetMatrixCell(gr, 0, i + 1, colHeaders[i]);
            }
            for (int i = 0; i < rowHeaders.Length; i++)
            {
                gr.RowDefinitions.Add(new RowDefinition());
                SetMatrixCell(gr, i + 1, 0, rowHeaders[i]);
            }
        }

        public void SetMatrixCell(Grid gr, int i, int j, string value)
        {
            TextBlock tb = new TextBlock();
            tb.Text = value;
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.TextAlignment = TextAlignment.Center;
            tb.TextWrapping = TextWrapping.Wrap;
            tb.Margin = new Thickness(3);

            Border border = new Border();
            border.BorderBrush = Brushes.Gray;
            border.BorderThickness = new Thickness(1);
            border.Child = tb;

            gr.Children.Add(border);
            Grid.SetRow(border, i);
            Grid.SetColumn(border, j);
        }

        private void cPleiadesDiagram_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Diagrams.PleiadesDiagram.Draw(cPleiadesDiagram, correlations);
        }

        private void cPearsonDiag_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Diagrams.PearsonDiagram.Draw(cPearsonDiag, chiSquared[0]);
        }
    }
}
