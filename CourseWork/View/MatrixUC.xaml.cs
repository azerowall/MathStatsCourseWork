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
    public partial class MatrixUC : UserControl
    {
        /*
        public string[] RowsHeaders;
        public string[] ColumnsHeaders;
        public Func<object, int, int, UIElement> CellCreator;
        public object Data;
        public int RowsCount;
        public int ColumnsCount;
        */
        public static readonly DependencyProperty StringFormatProperty;
        public static readonly DependencyProperty DataProperty;
        public static readonly DependencyProperty RowsHeadersProperty;
        public static readonly DependencyProperty ColumnsHeadersProperty;
        //public static readonly DependencyProperty RowsCountProperty;
        //public static readonly DependencyProperty ColumnsCountProperty;


        public string StringFormat
        {
            get { return (string)GetValue(StringFormatProperty); }
            set { SetValue(StringFormatProperty, value); }
        }
        public System.Collections.IEnumerable Data
        {
            get { return (System.Collections.IEnumerable)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }
        public IEnumerable<string> RowsHeaders
        {
            get { return (IEnumerable<string>)GetValue(RowsHeadersProperty); }
            set { SetValue(RowsHeadersProperty, value); }
        }
        public IEnumerable<string> ColumnsHeaders
        {
            get { return (IEnumerable<string>)GetValue(ColumnsHeadersProperty); }
            set { SetValue(ColumnsHeadersProperty, value); }
        }

        static MatrixUC()
        {
            StringFormatProperty = DependencyProperty.Register(
                "StringFormat",
                typeof(string),
                typeof(MatrixUC),
                new FrameworkPropertyMetadata("{0}"));
            DataProperty = DependencyProperty.Register(
                "Data",
                typeof(System.Collections.IEnumerable),
                typeof(MatrixUC),
                new FrameworkPropertyMetadata(OnDataChanged));
            RowsHeadersProperty = DependencyProperty.Register("RowsHeaders", typeof(IEnumerable<string>), typeof(MatrixUC),
                new FrameworkPropertyMetadata(OnDataChanged));
            ColumnsHeadersProperty = DependencyProperty.Register("ColumnsHeaders", typeof(IEnumerable<string>), typeof(MatrixUC),
                new FrameworkPropertyMetadata(OnDataChanged));
        }

        static void OnDataChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MatrixUC matrix = (MatrixUC)o;
            if (matrix.RowsHeaders != null && matrix.ColumnsHeaders != null && matrix.Data != null)
                (o as MatrixUC).FillMatrix();
        }

        public MatrixUC()
        {
            InitializeComponent();
        }

        public void SetTable<T>(T[,] matrix, string[] rowHeaders, string[] colHeaders, Func<int, int, T, Brush> highlighter = null)
        {
            string format = StringFormat;
            CreateGridAndHeaders(rowHeaders, colHeaders);
            for (int i = 0; i < matrix.GetLength(0); i++)
                for (int j = 0; j < matrix.GetLength(1); j++)
                    SetCell(string.Format(format, matrix[i, j]),
                            i + 1, j + 1,
                            highlighter != null ? highlighter(i, j, matrix[i, j]) : Brushes.White);
        }
        public void SetTable<T>(IEnumerable<IEnumerable<T>> matrix, string[] rowHeaders, string[] colHeaders, Func<int, int, T, Brush> highlighter = null)
        {
            int i = 0, j = 0;
            CreateGridAndHeaders(rowHeaders, colHeaders);
            foreach (var row in matrix)
            {
                j = 0;
                foreach (var item in row)
                {
                    SetCell(item.ToString(), i + 1, j + 1,
                            highlighter != null ? highlighter(i, j, item) : Brushes.White);
                    j += 1;
                }
                i += 1;
            }
        }

        private void FillMatrix()
        {
            if (grTable.RowDefinitions.Count == 0 || grTable.ColumnDefinitions.Count == 0)
                CreateGridAndHeaders(RowsHeaders, ColumnsHeaders);

            var data = Data;
            string format = StringFormat;
            if (data is Array && (data as Array).Rank == 2)
            {
                Array arr = (Array)data;
                for (int i = 0; i < arr.GetLength(0); i++)
                    for (int j = 0; j < arr.GetLength(1); j++)
                        SetCell(string.Format(format, arr.GetValue(i, j)), i + 1, j + 1);
            }
            else
            {
                int i = 0;
                foreach (var row in data)
                {
                    int j = 0;
                    foreach (var item in (row as System.Collections.IEnumerable))
                    {
                        SetCell(string.Format(format, item), i, j);
                        j += 1;
                    }
                    i += 1;
                }
            }
        }

        private void CreateGridAndHeaders(IEnumerable<string> rowHeaders, IEnumerable<string> colHeaders)
        {
            grTable.RowDefinitions.Add(new RowDefinition());
            grTable.ColumnDefinitions.Add(new ColumnDefinition());
            int i = 0;
            foreach (var h in rowHeaders)
            {
                grTable.RowDefinitions.Add(new RowDefinition());
                SetCell(h, i + 1, 0);
                i += 1;
            }
            i = 0;
            foreach (var h in colHeaders)
            {
                grTable.ColumnDefinitions.Add(new ColumnDefinition());
                SetCell(h, 0, i + 1);
                i += 1;
            }
            SetCell(string.Empty, 0, 0);
        }

        private void SetCell(string content, int i, int j, Brush bg = null)
        {
            bg = bg != null ? bg : Brushes.White;
            TextBox tb = new TextBox()
            {
                IsReadOnly = true,
                Background = i == 0 || j == 0 ? Brushes.LightGray : bg,
                Text = content,
            };
            grTable.Children.Add(tb);
            Grid.SetRow(tb, i);
            Grid.SetColumn(tb, j);
        }
    }
}
