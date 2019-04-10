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
        double[] chiSquared;
        Correlations corr;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            table = new Table(@"iris.csv");
            table.Normalize();

            stats = new DescriptiveStatistics[table.Columns.Length];
            for (int i = 0; i < table.Columns.Length; i++)
                stats[i] = new DescriptiveStatistics(table.Columns[i].Values);

            chiSquared = new double[table.Columns.Length];
            for (int i = 0; i < table.Columns.Length; i++)
                chiSquared[i] = PearsonChiSquared.CalcChiSquared(table.Columns[i].Values, stats[i], 10);


            var sb = new StringBuilder();
            for (int i = 0; i < table.Columns.Length; i++)
            {
                sb.AppendLine(table.Columns[i].Header);
                sb.AppendLine(stats[i].ToString());
            }
            tbDescriptiveStatistics.Text = sb.ToString();

            corr = new Correlations(table, stats);
            

            // хи-квадраты
            MakeMatrix(grPearsonChiSquared,
                table.Columns.Select(c => c.Header).ToArray(),
                new[] { "Хи-квадрат", "Нормальность" },
                (i, j) => {
                    if (i == 0) return chiSquared[j].ToString();
                    else
                        return PearsonChiSquared.HasCriterion(chiSquared[j]) ? "+" : "-";
                });
            string[] headers = Enumerable.Range(0, table.Columns.Length).Select(i => "X" + i).ToArray();

            MakeMatrix(grCorrMatrix, headers, headers,
                       (i, j) => Math.Round(corr.CorrMatrix[i, j],4).ToString());
            MakeMatrix(grPartialCorrMatrix, headers, headers,
                       (i, j) => Math.Round(corr.PartialCorrMatrix[i, j], 4).ToString());
            MakeMatrix(grCorrCompareMatrix, headers, headers,
                       (i, j) => Math.Abs(corr.CorrMatrix[i, j]) > Math.Abs(corr.PartialCorrMatrix[i, j]) ? "+" : "-");


            tbLegend.Text = string.Join("\n", table.Columns.Select((col, i) => $"X{i} - {col.Header}"));
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

            //Border border = new Border();

            gr.Children.Add(tb);
            Grid.SetRow(tb, i);
            Grid.SetColumn(tb, j);
        }

        public void DrawDiag()
        {
            double size = Math.Min(cPleiadesDiagram.ActualWidth, cPleiadesDiagram.ActualHeight);

            const double margin = 30.0;
            double centX, centY;
            centX = centY = size / 2;
            double radius = size / 2 - margin;

            // круг
            GeometryGroup ggCircle = new GeometryGroup();
            ggCircle.Children.Add(new EllipseGeometry(new Point(centX, centY), radius, radius));

            // точки
            List<Point> points = new List<Point>();
            double angle = 2 * Math.PI / table.ColumnsCount;
            for (int i = 0; i < table.ColumnsCount; i++)
            {
                var point = new Point(radius * Math.Cos(angle * i) + centX,
                                      radius * Math.Sin(angle * i) + centY);
                points.Add(point);
                ggCircle.Children.Add(new EllipseGeometry(point, 2, 2));
            }

            // линии
            GeometryGroup ggLinesStrong = new GeometryGroup();
            GeometryGroup ggLinesCollinear = new GeometryGroup();
            for (int i = 1; i < table.ColumnsCount; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    if (Math.Abs(corr.CorrMatrix[i, j]) >= 0.7)
                        ggLinesCollinear.Children.Add(new LineGeometry(points[i], points[j]));
                    else if (Math.Abs(corr.CorrMatrix[i, j]) >= 0.6)
                        ggLinesStrong.Children.Add(new LineGeometry(points[i], points[j]));
                }
            }

            // метки
            for (int i = 0; i < table.ColumnsCount; i++)
            {
                double x = (radius + margin / 2) * Math.Cos(angle * i) + centX - margin * 0.3;
                double y = (radius + margin / 2) * Math.Sin(angle * i) + centY - margin * 0.3;
                var tbMark = new TextBlock();
                tbMark.Text = "X" + i;
                Canvas.SetLeft(tbMark, x);
                Canvas.SetTop(tbMark, y);
                cPleiadesDiagram.Children.Add(tbMark);
            }

            var pathCircle = new Path()
            {
                Stroke = Brushes.Black,
                Data = ggCircle
            };
            var pathLinesStrong = new Path()
            {
                Stroke = Brushes.LightGray,
                StrokeDashArray = new DoubleCollection(new[] { 4.0, 4.0 }),
                Data = ggLinesStrong
            };
            var pathLinesCollinear = new Path()
            {
                Stroke = Brushes.LightGray,
                Data = ggLinesCollinear
            };
            cPleiadesDiagram.Children.Add(pathCircle);
            cPleiadesDiagram.Children.Add(pathLinesStrong);
            cPleiadesDiagram.Children.Add(pathLinesCollinear);
        }

        private void cPleiadesDiagram_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            cPleiadesDiagram.Children.Clear();
            DrawDiag();
        }
    }
}
