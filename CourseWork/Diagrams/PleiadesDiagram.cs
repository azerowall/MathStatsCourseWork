using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;

namespace CourseWork.Diagrams
{
    static class PleiadesDiagram
    {
        public static void Draw(Canvas canvas, Correlations correlations)
        {
            canvas.Children.Clear();

            double size = Math.Min(canvas.ActualWidth, canvas.ActualHeight);

            const double margin = 30.0;
            double centX, centY;
            centX = centY = size / 2;
            double radius = size / 2 - margin;

            // круг
            GeometryGroup ggCircle = new GeometryGroup();
            ggCircle.Children.Add(new EllipseGeometry(new Point(centX, centY), radius, radius));

            // точки
            List<Point> points = new List<Point>();
            double angle = 2 * Math.PI / correlations.ParametersCount;
            for (int i = 0; i < correlations.ParametersCount; i++)
            {
                var point = new Point(radius * Math.Cos(angle * i) + centX,
                                      radius * Math.Sin(angle * i) + centY);
                points.Add(point);
                ggCircle.Children.Add(new EllipseGeometry(point, 2, 2));
            }

            // линии
            GeometryGroup ggLinesStrong = new GeometryGroup();
            GeometryGroup ggLinesCollinear = new GeometryGroup();
            for (int i = 1; i < correlations.ParametersCount; i++)
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
            for (int i = 0; i < correlations.ParametersCount; i++)
            {
                double x = (radius + margin / 2) * Math.Cos(angle * i) + centX - margin * 0.3;
                double y = (radius + margin / 2) * Math.Sin(angle * i) + centY - margin * 0.3;
                var tbMark = new TextBlock();
                tbMark.Text = "X" + i;
                Canvas.SetLeft(tbMark, x);
                Canvas.SetTop(tbMark, y);
                canvas.Children.Add(tbMark);
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
            canvas.Children.Add(pathCircle);
            canvas.Children.Add(pathLinesStrong);
            canvas.Children.Add(pathLinesCollinear);
        }
    }
}
