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
    /// Логика взаимодействия для VectorInput.xaml
    /// </summary>
    public partial class VectorInput : UserControl
    {
        public static readonly DependencyProperty VectorLengthProperty = DependencyProperty.Register(
            "VectorLength",
            typeof(int),
            typeof(VectorInput),
            new FrameworkPropertyMetadata(ItemsCountChanged));
        public static readonly DependencyProperty VectorProperty = DependencyProperty.Register(
            "Vector",
            typeof(double[]),
            typeof(VectorInput));

        public int VectorLength
        {
            get { return (int)GetValue(VectorLengthProperty); }
            set { SetValue(VectorLengthProperty, value); }
        }
        public double[] Vector
        {
            get { return (double[])GetValue(VectorProperty); }
            set { SetValue(VectorProperty, value); }
        }
        
        Brush DefaultBrush = Brushes.LightBlue;
        Brush DefaultBgBrush = Brushes.White;
        Brush ErrorBrush = Brushes.Red;
        Brush ErrorBgBrush = Brushes.LightPink;

        public VectorInput()
        {
            InitializeComponent();
        }

        static void ItemsCountChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            VectorInput vinput = (VectorInput)o;
            if (vinput.IsLoaded)
                vinput.CreateInput();
        }

        private void Tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool allFilled = true;
            foreach (TextBox tb in panel.Children)
                if (string.IsNullOrEmpty(tb.Text))
                    allFilled = false;
            if (allFilled)
                ReadInput();

        }



        void CreateInput()
        {
            if (panel.Children.Count > 0)
                foreach (TextBox tb in panel.Children)
                    tb.TextChanged -= Tb_TextChanged;
            panel.Children.Clear();

            int count = VectorLength;
            for (int i = 0; i < count; i++)
            {
                TextBox tb = new TextBox()
                {
                    Width = 70,
                    Margin = new Thickness(0, 0, 5, 0),
                    Padding = new Thickness(5),
                };
                tb.TextChanged += Tb_TextChanged;
                panel.Children.Add(tb);
            }
        }

        void ReadInput()
        {
            bool hasErrors = false;
            List<double> items = new List<double>();
            foreach (TextBox tb in panel.Children)
            {
                double value;
                if (double.TryParse(tb.Text, out value))
                {
                    items.Add(double.Parse(tb.Text));
                    tb.BorderBrush = DefaultBrush;
                    tb.Background = DefaultBgBrush;
                }
                else
                {
                    hasErrors = true;
                    tb.BorderBrush = ErrorBrush;
                    tb.Background = ErrorBgBrush;
                }
            }
            if (!hasErrors)
                Vector = items.ToArray();
        }
    }
}
