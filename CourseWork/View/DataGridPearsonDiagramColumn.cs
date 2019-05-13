using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace CourseWork.View
{
    class DataGridPearsonDiagramColumn : DataGridBoundColumn
    {
        protected override FrameworkElement GenerateElement(DataGridCell cell, object dataItem)
        {
            var pcs = (PearsonChiSquared)dataItem;
            var canvas = cell.Content != null ? (cell.Content as Canvas) : new Canvas()
            {
                MinWidth = 200,
                MinHeight = 70,
                //RenderSize = new Size(200, 70)
                //Width = 200,
                //Height = 70,
                //DesiredSize = new Size(200, 70)
            };
            canvas.SizeChanged += Canvas_SizeChanged;
            canvas.Tag = pcs;
            Diagrams.PearsonDiagram.Draw(canvas, pcs);
            return canvas;
        }

        private void Canvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Canvas canvas = (Canvas)sender;
            Diagrams.PearsonDiagram.Draw(canvas, (PearsonChiSquared)canvas.Tag);
        }

        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            throw new NotImplementedException();
        }
    }
}
