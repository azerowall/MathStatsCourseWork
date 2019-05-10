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
                OnPropertyChanged("Parameters");
                OnPropertyChanged("ShortedParameters");
                OnPropertyChanged("PerametersLegend");
            }
        }

        public IEnumerable<string> Parameters => Table?.Headers;
        public IEnumerable<string> ShortedParameters => Table?.ShortedHeaders;
        public string PerametersLegend => Table != null ?
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


        #region Регрессия

        int _dependentParameter;
        public int DependentParameter
        {
            get { return _dependentParameter; }
            set
            {
                _dependentParameter = value;
                OnPropertyChanged("DependentParameter");
                if (Table != null)
                    Regression = new Regression(Table, _dependentParameter);
            }
        }

        Regression _regr;
        public Regression Regression
        {
            get { return _regr; }
            set
            {
                _regr = value;
                OnPropertyChanged("Regression");
                RegressionCoeffs = Enumerable.Range(0, _regr.Coeffs.Length)
                                             .Select(i => new RegressionCoefficient(i, _regr, Table, DependentParameter))
                                             .ToArray();
                classificator = new RegressionClassificator(_regr, Table.ColumnsValues[DependentParameter]);
                RegressionYs = Enumerable.Range(0, _regr.RealY.Length)
                                         .Select(i => new RegressionYInfo(i, _regr, classificator))
                                         .ToArray();
                RegressionEquationInfo = GetRegressionEquationInfo(_regr);
            }
        }

        RegressionClassificator classificator;

        public class RegressionCoefficient
        {
            int idx;
            Regression regr;
            Table table;
            int idxy;
            public RegressionCoefficient(int i, Regression r, Table t, int iy)
            {
                idx = i; regr = r; table = t; idxy = iy;
            }
            public string ParameterName
            {
                get
                {
                    if (idx == 0) return "-";
                    if (idx - 1 < idxy) return table.Headers[idx - 1];
                    return table.Headers[idx];
                }
            }
            public double Value => regr.Coeffs[idx];
            public double T => regr.CoeffsT[idx];
            public bool IsSignificance => regr.IsSignificanceCoeff(T);
            public double IntervalEstimate => regr.CoeffsIntervalEstimates[idx];
        }

        RegressionCoefficient[] _regrCoeffs;
        public RegressionCoefficient[] RegressionCoeffs
        {
            get { return _regrCoeffs; }
            set { _regrCoeffs = value; OnPropertyChanged("RegressionCoeffs"); }
        }

        public class RegressionYInfo
        {
            int idx;
            Regression regr;
            RegressionClassificator clstor;
            public RegressionYInfo(int i, Regression r, RegressionClassificator c)
            {
                idx = i; regr = r; clstor = c;
            }
            public double Real => regr.RealY[idx];
            public double Calculated => regr.CalculatedY[idx];
            public double AbsError => Real - Calculated;
            public double IntervalEstimate => regr.IntervalEstimates[idx];
            public bool IsInInterval
            {
                get
                {
                    double c = Calculated, interv = IntervalEstimate;
                    return c - interv < Real && Real < c + interv;
                }
            }
            public bool IsClassifcationValid => clstor.Classificate(Calculated) == Real;
        }

        RegressionYInfo[] _regrYs;
        public RegressionYInfo[] RegressionYs
        {
            get { return _regrYs; }
            set { _regrYs = value; OnPropertyChanged("RegressionYs"); }
        }

        static string GetRegressionEquationInfo(Regression regr)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"t-крит: {regr.TCritical:f4} (Коэфф. значим если t > t-крит)");
            sb.AppendLine($"F: {regr.F:f4}");
            sb.AppendLine($"F-крит: {regr.FCritical:f4}");
            if (regr.IsSignificance)
                sb.AppendLine("F > F-крит => Уравнение значимо");
            else
                sb.AppendLine("F ≤ F-крит => Уравнение незначимо");
            sb.AppendLine($"Ошибка аппроксимации: {regr.ApproximationError:f4}");
            sb.AppendLine($"Уравнение: {regr.EquationToString()}");
            return sb.ToString();
        }

        string _regrEquationInfo;
        public string RegressionEquationInfo
        {
            get { return _regrEquationInfo; }
            set { _regrEquationInfo = value; OnPropertyChanged("RegressionEquationInfo"); }
        }

        #endregion
    }
}
