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
        
        public static readonly DependencyProperty StringFormatProperty = DependencyProperty.Register(
                "StringFormat",
                typeof(string),
                typeof(MatrixUC),
                new FrameworkPropertyMetadata("{0}"));
        public static readonly DependencyProperty DataProperty = DependencyProperty.Register(
                "Data",
                typeof(System.Collections.IEnumerable),
                typeof(MatrixUC),
                new FrameworkPropertyMetadata(OnDataChanged));
        public static readonly DependencyProperty RowsHeadersProperty = DependencyProperty.Register(
                "RowsHeaders",
                typeof(IEnumerable<string>),
                typeof(MatrixUC));
        public static readonly DependencyProperty ColumnsHeadersProperty = DependencyProperty.Register(
                "ColumnsHeaders",
                typeof(IEnumerable<string>),
                typeof(MatrixUC));
        public static readonly DependencyProperty HighlighterProperty = DependencyProperty.Register(
                "Highlighter",
                typeof(Func<int, int, object, Brush>),
                typeof(MatrixUC));


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
        public Func<int, int, object, Brush> Highlighter
        {
            get { return (Func<int, int, object, Brush>)GetValue(HighlighterProperty); }
            set { SetValue(HighlighterProperty, value); }
        }

        public MatrixUC()
        {
            InitializeComponent();
            Loaded += MatrixUC_Loaded;
        }

        static void OnDataChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            MatrixUC matrix = (MatrixUC)o;
            //if (matrix.IsLoaded)
                (o as MatrixUC).CreateMatrix();
        }

        static void MatrixUC_Loaded(object sender, RoutedEventArgs e)
        {
            MatrixUC matrix = (MatrixUC)sender;
            // вызывается даже при переключении вкладок
            // а нам нужно только при первой загрузки всех биндингов, поэтому
            matrix.Loaded -= MatrixUC_Loaded;
            matrix.CreateMatrix();

        }

        private void CreateMatrix()
        {
            ClearTable();
            CreateHeaders();

            var data = Data;
            string format = StringFormat;
            Func<int, int, object, Brush> highlighter = Highlighter;

            if (data == null)
                return;

            if (data is Array && (data as Array).Rank == 2)
            {
                Array arr = (Array)data;
                for (int i = 0; i < arr.GetLength(0); i++)
                    for (int j = 0; j < arr.GetLength(1); j++)
                        CreateCell(string.Format(format, arr.GetValue(i, j)),
                                   i + 1,
                                   j + 1,
                                   highlighter?.Invoke(i, j, arr.GetValue(i, j) ?? Brushes.White));
            }
            else
            {
                int i = 0;
                foreach (var row in data)
                {
                    int j = 0;
                    foreach (var item in (row as System.Collections.IEnumerable))
                    {
                        CreateCell(string.Format(format, item),
                                   i + 1,
                                   j + 1,
                                   highlighter?.Invoke(i, j, item) ?? Brushes.White);
                        j += 1;
                    }
                    i += 1;
                }
            }
        }


        private void CreateHeaders()
        {
            var columnsHeaders = ColumnsHeaders;
            var rowsHeaders = RowsHeaders;
            
            CreateCell(string.Empty, 0, 0, Brushes.LightGray);

            if (columnsHeaders != null)
            {
                int i = 0;
                foreach (var h in columnsHeaders)
                    CreateCell(h, 0, ++i, Brushes.LightGray);
            }
            else
                grTable.RowDefinitions[0].Height = new GridLength(0);

            if (rowsHeaders != null)
            {
                int i = 0;
                foreach (var h in rowsHeaders)
                    CreateCell(h, ++i, 0, Brushes.LightGray);
            }
            else
                grTable.ColumnDefinitions[0].Width = new GridLength(0);
        }

        private void CreateCell(string content, int i, int j, Brush bg)
        {
            // добавление новых строк/столбцов решил сделать здесь
            // т.к. заголовков может и не быть,
            // а отдельно задавать кол-во столбцов/строк - это только лишняя морока
            // Такой проверки достаточно, т.к. ячейки добавляются последовательно
            if (grTable.RowDefinitions.Count == i)
                grTable.RowDefinitions.Add(new RowDefinition());
            if (grTable.ColumnDefinitions.Count == j)
                grTable.ColumnDefinitions.Add(new ColumnDefinition());

            TextBox tb = new TextBox()
            {
                IsReadOnly = true,
                Background = bg,
                Text = content,
            };
            grTable.Children.Add(tb);
            Grid.SetRow(tb, i);
            Grid.SetColumn(tb, j);
        }

        private void ClearTable()
        {
            grTable.RowDefinitions.Clear();
            grTable.ColumnDefinitions.Clear();
            grTable.Children.Clear();
        }
    }
}
