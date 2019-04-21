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
    static class PearsonDiagram
    {
        public static void Draw(Canvas canvas, PearsonChiSquared pcs)
        {
            canvas.Children.Clear();

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
    }
}
