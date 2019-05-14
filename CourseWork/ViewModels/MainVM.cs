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
        public MainVM()
        {
            LoadFileCommand = new Commands.DelegateCommand(LoadFileUsingDialog);
            RegressionCalculateYCommand = new Commands.DelegateCommand(CalcY);
            ExcludeParametersCommand = new Commands.DelegateCommand(ExcludeParameters);
        }

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

        
        string _loadedFile;
        public string LoadedFile
        {
            get { return _loadedFile; }
            set { _loadedFile = value; OnPropertyChanged("LoadedFile"); }
        }
        public Commands.DelegateCommand LoadFileCommand { get; private set; }
        void LoadFileUsingDialog(object o)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "csv files (*.csv)|*.csv";
            dialog.Multiselect = false;
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
            DescriptiveStatistics[] ds = new DescriptiveStatistics[Table.ColumnsCount];
            for (int i = 0; i < Table.ColumnsCount; i++)
                ds[i] = new DescriptiveStatistics(Table.Headers[i], Table.ColumnsValues[i]);
            DS = ds;
            ChiSquaredIntervalsCount = 5;
            Correlations = new Correlations(Table, DS);
        }

        #endregion

        DescriptiveStatistics[] _ds;
        public DescriptiveStatistics[] DS
        {
            get { return _ds; }
            set { _ds = value; OnPropertyChanged("DS"); }
        }

        #region Критерий Пирсона

        PearsonChiSquared[] _chiSquared;
        public PearsonChiSquared[] ChiSquared
        {
            get { return _chiSquared; }
            set { _chiSquared = value; OnPropertyChanged("ChiSquared"); }
        }

        public int _intervalsCount = 5;
        public int ChiSquaredIntervalsCount
        {
            get { return _intervalsCount; }
            set
            {
                _intervalsCount = value;
                OnPropertyChanged("ChiSquaredIntervalsCount");
                PearsonChiSquared[] pcs = new PearsonChiSquared[Table.ColumnsCount];
                for (int i = 0; i < pcs.Length; i++)
                    pcs[i] = new PearsonChiSquared(Table.Headers[i], Table.ColumnsValues[i],
                                                   DS[i], _intervalsCount);
                ChiSquared = pcs;
            }
        }

        #endregion


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
                    Correlations.MultipleCorreletaionCoeffs,
                    Correlations.MultipleDeterminationCoeffs
                };
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

        Brush _MultipleCorrelationsHighlighter(int i, int j, object o)
        {
            double d = (double)o;
            double F = Correlations.MultipleCorrSignificance[j];
            if (i == 0 && Correlations.IsMultipleCoeffSignificance(F))
            {
                return Brushes.LightGreen;
            }
            else if (i == 1 && d >= 0.8) return Brushes.LightGreen;
            return Brushes.White;
        }
        public Func<int, int, object, Brush> MultipleCorrelationsHighlighter => _MultipleCorrelationsHighlighter;

        #endregion


        #region Регрессия


        Regression _regr;
        public Regression Regression
        {
            get { return _regr; }
            set
            {
                _regr = value;
                OnPropertyChanged("Regression");

                RegressionCoefficient[] coeffs = new RegressionCoefficient[_regr.Coeffs.Length];
                for (int i = 0; i < coeffs.Length; i++)
                {
                    string name = i == 0 ? "-" : Table.Headers[_notExcludedParams[i - 1]];
                    coeffs[i] = new RegressionCoefficient(i, _regr, name);
                }
                RegressionCoeffs = coeffs;
                classificator = new RegressionClassificator(_regr, Table.ColumnsValues[DependentParameter]);
                RegressionYs = Enumerable.Range(0, _regr.RealY.Length)
                                         .Select(i => new RegressionYInfo(i, _regr, classificator))
                                         .ToArray();
                RegressionEquationInfo = GetRegressionEquationInfo(_regr);
                RegressionParametersCount = _regr.Coeffs.Length - 1;
            }
        }

        int _dependentParameter;
        public int DependentParameter
        {
            get { return _dependentParameter; }
            set
            {
                _dependentParameter = value;
                OnPropertyChanged("DependentParameter");
                if (Table != null)
                {
                    var parametes = Table.ColumnsValues
                                         .Where((_, i) => i != _dependentParameter)
                                         .ToArray();
                    ParametersForExclusion = Enumerable.Range(0, Table.ColumnsCount)
                                                       .Where(i => i != _dependentParameter)
                                                       .Select(i => new ParameterExcl(i, Table.Headers[i]))
                                                       .ToArray();
                    Regression = new Regression(parametes, Table.ColumnsValues[_dependentParameter]);
                }
            }
        }
        public class ParameterExcl
        {
            public int Index;
            public string Name { get; private set; }
            public bool IsExcluded { get; set; }
            public ParameterExcl(int i, string name)
            {
                Index = i; Name = name; IsExcluded = false;
            }
        }
        ParameterExcl[] _paramsForExcl;
        public ParameterExcl[] ParametersForExclusion
        {
            get { return _paramsForExcl; }
            set
            {
                _paramsForExcl = value;
                OnPropertyChanged("ParametersForExclusion");
                _notExcludedParams = Enumerable.Range(0, Table.ColumnsCount)
                                               .Where(i => i != DependentParameter)
                                               .ToArray();
            }
        }

        public int[] _notExcludedParams;
        public Commands.DelegateCommand ExcludeParametersCommand { get; set; }
        void ExcludeParameters(object o)
        {
            if (ParametersForExclusion == null) return;

            // среди ParametersForExclusion уже нет зависимого,
            // поэтому и проверку делать не нужно
            _notExcludedParams = ParametersForExclusion.Where(p => !p.IsExcluded)
                                                       .Select(p => p.Index)
                                                       .ToArray();
            if (_notExcludedParams.Length == 0)
            {
                MessageBox.Show("Нельзя исключить все параметры");
                return;
            }
            double[][] parameters = _notExcludedParams.Select(i => Table.ColumnsValues[i])
                                                      .ToArray();
            
            Regression = new Regression(parameters, Table.ColumnsValues[DependentParameter]);
        }


        RegressionClassificator classificator;

        public class RegressionCoefficient
        {
            int idx;
            Regression regr;
            public RegressionCoefficient(int i, Regression r, string name)
            {
                idx = i; regr = r; ParameterName = name;
            }
            public string ParameterName { get; set; }
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

        int _regrParametersCount;
        public int RegressionParametersCount
        {
            get { return _regrParametersCount; }
            set { _regrParametersCount = value; OnPropertyChanged("RegressionParametersCount"); }
        }

        public double[] InputtedX { get; set; }
        public Commands.DelegateCommand RegressionCalculateYCommand { get; set; }
        void CalcY(object o)
        {
            if (InputtedX == null)
                return;
            RegressionCalculatedY = Regression.CalcY(InputtedX);
            RegressionPredictionInterval = Regression.CalcPredictionInterval(InputtedX);
            ClassificationClass = classificator.Classificate(RegressionCalculatedY);

            OnPropertyChanged("RegressionCalculatedY");
            OnPropertyChanged("RegressionPredictionInterval");
            OnPropertyChanged("ClassificationClass");
        }
        public double RegressionCalculatedY { get; set; }
        public double RegressionPredictionInterval { get; set; }
        public double ClassificationClass { get; set; }

        #endregion
    }
}
