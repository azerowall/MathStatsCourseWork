using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Media;

namespace CourseWork.ViewModels
{
    class MainVM : BaseVM
    {

        #region Таблица

        Table _table;
        public Table Table
        {
            get { return _table; }
            set
            {
                _table = value;
                OnPropertyChanged("Table");
                OnPropertyChanged("Headers");
                OnPropertyChanged("ShortedHeaders");
                OnPropertyChanged("HeadersLegend");
            }
        }

        public IEnumerable<string> Headers => Table?.Headers;
        public IEnumerable<string> ShortedHeaders => Table?.ShortedHeaders;
        public string HeadersLegend => Table != null ?
                                    string.Join("\n", Table.ShortedHeaders.Zip(Table.Headers, (s, h) => $"{s} - {h}")) :
                                    string.Empty;

        #endregion

        DescriptiveStatistics[] _ds;
        public DescriptiveStatistics[] DS
        {
            get { return _ds; }
            set { _ds = value; OnPropertyChanged("DS"); }
        }
        PearsonChiSquared[] _chiSquared;
        public PearsonChiSquared[] ChiSquared
        {
            get { return _chiSquared; }
            set { _chiSquared = value; OnPropertyChanged("ChiSquared"); }
        }
        Regression _regr;
        public Regression Regression
        {
            get { return _regr; }
            set { _regr = value; OnPropertyChanged("Regression"); }
        }

        public MainVM()
        {
            LoadFileCommand = new Commands.DelegateCommand(_LoadFile);
        }

        string _loadedFile;
        public string LoadedFile
        {
            get { return _loadedFile; }
            set { _loadedFile = value; OnPropertyChanged("LoadedFile"); }
        }
        public Commands.DelegateCommand LoadFileCommand { get; private set; }
        void _LoadFile(object o)
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
            {
                LoadFile(dialog.FileName);
                LoadedFile = dialog.FileName;
            }
        }
        void LoadFile(string path)
        {
            Table = new Table(path);
            Table.Normalize();
            DS = new DescriptiveStatistics[Table.ColumnsCount];
            ChiSquared = new PearsonChiSquared[Table.ColumnsCount];
            for (int i = 0; i < Table.ColumnsCount; i++)
            {
                DS[i] = new DescriptiveStatistics(Table.Headers[i], Table.ColumnsValues[i]);
                ChiSquared[i] = new PearsonChiSquared(Table.Headers[i], Table.ColumnsValues[i], DS[i], 7);
            }
            Correlations = new Correlations(Table, DS);
            Regression = new Regression(Table, 4);
        }

        #region Корреляция

        Correlations _corr;
        public Correlations Correlations
        {
            get { return _corr; }
            set
            {
                _corr = value;
                OnPropertyChanged("Correlations");
                OnPropertyChanged("CorrelationsMatrix");
                OnPropertyChanged("PartialCorrelationsMatrix");
                OnPropertyChanged("ComparisonCorrelationsMatrix");
                OnPropertyChanged("SignificanceCorrelationsMatrix");
                OnPropertyChanged("MultipleCorrelationCoeffs");
            }
        }
        public double[,] CorrelationsMatrix => Correlations?.CorrMatrix;
        public double[,] PartialCorrelationsMatrix => Correlations?.PartialCorrMatrix;
        public IEnumerable<IEnumerable<string>> ComparisonCorrelationsMatrix
        {
            get
            {
                if (Correlations != null)
                {
                    return Enumerable.Range(0, Correlations.CorrMatrix.GetLength(0)).Select(i =>
                            Enumerable.Range(0, Correlations.CorrMatrix.GetLength(1)).Select(j =>
                            {
                                double r = Math.Abs(Correlations.CorrMatrix[i, j]);
                                double pr = Math.Abs(Correlations.PartialCorrMatrix[i, j]);
                                if (r > pr) return "усил.";
                                else if (r < pr) return "ослаб.";
                                else return "-";
                            }));
                }
                else return null;
            }
        }
        public double[,] SignificanceCorrelationsMatrix => Correlations?.SignificanceMatrix;
        public IEnumerable<IEnumerable<double>> MultipleCorrelationCoeffs
        {
            get
            {
                if (Correlations == null) return null;
                return new[] {
                    Correlations.MultipleCorrletaionCoeffs,
                    Correlations.MultipleCorrletaionCoeffs.Select(d => d*d)};
            }
        }
        public IEnumerable<string> MultipleCorrelationsHeaders
        {
            get
            {
                yield return "R";
                yield return "D";
            }
        }

        #endregion


        #region Подсветка

        static Brush WeakCorrelationColor = new SolidColorBrush(Color.FromRgb(230, 230, 254));
        static Brush MediumCorrelationColor = new SolidColorBrush(Color.FromRgb(190, 190, 254));
        static Brush StrongCorrelationColor = new SolidColorBrush(Color.FromRgb(140, 140, 254));
        static Brush _CorrelationsHighlighter(int i, int j, object o)
        {
            double d = Math.Abs((double)o);
            if (d >= 0.4 && d < 0.6)
                return WeakCorrelationColor;
            else if (d >= 0.6 && d < 0.9)
                return MediumCorrelationColor;
            else if (d >= 0.9)
                return StrongCorrelationColor;
            else
                return Brushes.White;
        }
        public Func<int, int, object, Brush> CorrelationsHighlighter => _CorrelationsHighlighter;

        Brush _SignificanceHighlighter(int i, int j, object o)
        {
            if (Correlations.IsSignificance((double)o))
                return Brushes.LightGreen;
            return Brushes.White;
        }
        public Func<int, int, object, Brush> SignificanceHighlighter => _SignificanceHighlighter;

        #endregion
    }
}
