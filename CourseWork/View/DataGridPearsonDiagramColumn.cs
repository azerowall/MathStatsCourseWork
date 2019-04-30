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
                RenderSize = new Size(200, 70)
            };
            Diagrams.PearsonDiagram.Draw(canvas, pcs);
            var b = new Border()
            {
                BorderThickness = new Thickness(3),
                BorderBrush = Brushes.Black
            };
            return canvas;
        }
        protected override FrameworkElement GenerateEditingElement(DataGridCell cell, object dataItem)
        {
            throw new NotImplementedException();
        }
    }
}
