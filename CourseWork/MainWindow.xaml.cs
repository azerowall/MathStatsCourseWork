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

            MakeMatrix(grCorrMatrix, headers, headers,
                       (i, j) => Math.Round(correlations.CorrMatrix[i, j], 4).ToString());
            MakeMatrix(grPartialCorrMatrix, headers, headers,
                       (i, j) => Math.Round(correlations.PartialCorrMatrix[i, j], 4).ToString());
            //MakeMatrix(grCorrCompareMatrix, headers, headers,
            //           (i, j) => Math.Abs(corr.CorrMatrix[i, j]) > Math.Abs(corr.PartialCorrMatrix[i, j]) ? "усиление" : "ослабление");
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

            
            tbLegend.Text = string.Join("\n", table.ShortedHeaders.Zip(table.Headers, (sh, h) => $"{sh} - {h}"));
            //tbLegend.Text = string.Join("\n", table.Columns.Select((col, i) => $"X{i} - {col.Header}"));

            DrawChiSquaredDiagram(cPearsonDiag, chiSquared[0]);
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
                    if (Math.Abs(correlations.CorrMatrix[i, j]) >= 0.7)
                        ggLinesCollinear.Children.Add(new LineGeometry(points[i], points[j]));
                    else if (Math.Abs(correlations.CorrMatrix[i, j]) >= 0.6)
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

        private void DrawChiSquaredDiagram(Canvas canvas, PearsonChiSquared pcs)
        {
            GeometryGroup ggReal = new GeometryGroup();
            GeometryGroup ggTheoretical = new GeometryGroup();

            double stepSize = canvas.ActualWidth / pcs.IntervalsCount;
            double canvasHeightOnePercent = canvas.ActualHeight / 100;
            double dataOnePercent = Math.Max(pcs.MReal.Max(), pcs.MTheoretical.Max()) / 100;
            for (int i = 0; i < pcs.IntervalsCount; i++)
            {
                double x = i * stepSize;
                double height = (pcs.MReal[i] / dataOnePercent) * canvasHeightOnePercent;
                ggReal.Children.Add(new RectangleGeometry(new Rect(x, canvas.ActualHeight - height, stepSize, height)));

                height = (pcs.MTheoretical[i] / dataOnePercent) * canvasHeightOnePercent;
                ggTheoretical.Children.Add(new RectangleGeometry(new Rect(x, canvas.ActualHeight - height, stepSize, height)));
            }

            Path pathMReal = new Path()
            {
                Fill = Brushes.Blue,
                Data = ggReal,
                Opacity = 0.5
            };
            Path pathMTheoretical = new Path()
            {
                Fill = Brushes.Green,
                Data = ggTheoretical,
                Opacity = 0.5
            };
            canvas.Children.Add(pathMReal);
            canvas.Children.Add(pathMTheoretical);
        }

        private void cPearsonDiag_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            cPearsonDiag.Children.Clear();
            DrawChiSquaredDiagram(cPearsonDiag, chiSquared[0]);
        }
    }
}
