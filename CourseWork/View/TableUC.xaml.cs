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

namespace CourseWork.View
{
    /// <summary>
    /// Логика взаимодействия для TableUC.xaml
    /// </summary>
    public partial class TableUC : UserControl
    {
        //public string[] RowHeaders;
        //public string[] ColumnHeaders;
        //public Func<int, int, T, Brush> Highlighter;

        public TableUC()
        {
            InitializeComponent();
        }

        public void SetTable<T>(T[,] matrix, string[] rowHeaders, string[] colHeaders, Func<int, int, T, Brush> highlighter = null)
        {
            CreateGridAndHeaders(rowHeaders, colHeaders);
            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    SetCell(matrix[i, j], i + 1, j + 1, highlighter != null ? highlighter(i, j, matrix[i, j]) : Brushes.White);
        }

        private void CreateGridAndHeaders(string[] rowHeaders, string[] colHeaders)
        {
            for (int i = 0; i < rowHeaders.Length; i++)
            {
                grTable.RowDefinitions.Add(new RowDefinition());
                SetCell(rowHeaders[i], i + 1, 0);
            }
            for (int i = 0; i < colHeaders.Length; i++)
            {
                grTable.ColumnDefinitions.Add(new ColumnDefinition());
                SetCell(colHeaders[i], 0, i + 1);
            }
            SetCell(string.Empty, 0, 0);
        }

        private void SetCell(object content, int i, int j, Brush bg = null)
        {
            bg = bg != null ? bg : Brushes.White;
            TextBox tb = new TextBox()
            {
                IsReadOnly = true,
                Background = i == 0 || j == 0 ? Brushes.LightGray : bg,
                Text = content.ToString(),
            };
            grTable.Children.Add(tb);
            Grid.SetRow(tb, i);
            Grid.SetColumn(tb, j);
        }
    }
}
