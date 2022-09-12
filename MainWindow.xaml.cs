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
using MahApps.Metro.Controls;
using LiveCharts.Wpf;
using LiveCharts;
using System.IO.Ports;
using LiveCharts.Defaults;
using System.IO;



namespace VibrationMachine
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SerialPort myport = new SerialPort();
        private DateTime StartTime, StopTime;
        public MainWindow()
        {
            InitializeComponent();
            GraphInit(HGraph);
        }
 
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StopButton.IsEnabled = true;
            StartButton.IsEnabled = false;
            var rand = new Random();
            UpdateChart(rand.NextDouble(), 6, HGraph);
            StartTime = DateTime.Now;
        }


        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopButton.IsEnabled = false;
            StartButton.IsEnabled = true;
            StopTime = DateTime.Now;
            double time = StopTime.Subtract(StartTime).TotalSeconds;
            double secrest = (time / 60.0 - Math.Floor(time / 60.0)) * 60;
            TLabel.Content = Math.Floor(time / 60.0).ToString("00") + ":" + secrest.ToString("00.00");
        }

        private void HGraph_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateChart(double p, double t, CartesianChart chart)
        {
            chart.Series[0].Values.Add(p);
            chart.Series[0].Values.RemoveAt(0);
            chart.AxisX[0].Labels.Add(String.Format("{0:0.##}", t));
            chart.AxisX[0].Labels.RemoveAt(0);

        }

        private List<double> zerolistd(int n)
        {
            List<double> zeros = new List<double>();
            for (int i = 0; i < n; i++)
            {
                zeros.Add(0d);
            }
            return zeros;
        }
        private List<string> zerolists(int n)
        {
            List<string> zeros = new List<string>();
            for (int i = 0; i < n; i++)
            {
                zeros.Add("0");
            }
            return zeros;
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string portName = PortsComboBox.SelectedItem as string;
                myport.PortName = portName;
                myport.BaudRate = 9600;
                myport.Open();
                MessageBox.Show("Conexión exitosa");
                StatusLabel.Content = "Conectado";
            }
            catch(Exception)
            {
                MessageBox.Show("Ingrese un número de puerto válido o revise la conexión del puerto");
            }
        }

        private void PortsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedcomboitem = sender as ComboBox;
            string name = selectedcomboitem.SelectedItem as string;
        }

        private void DisconnectButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                myport.Close();
                StatusLabel.Content = "Desconectado";
            }
            catch(Exception)
            {
                MessageBox.Show("Primero conecte su dispositivo, luego haga clic en desconectar.");
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveData(zerolists(50));
            MessageBox.Show("Guardado con éxito.");
        }
        private void SaveData(List<string> dat)
        {
            StreamWriter writer = new StreamWriter("data.txt");
            dat.ForEach(s => writer.WriteLine(s));
            writer.Close();
        }

        private void GraphInit(CartesianChart chart)
        {
            List<double> yinit = zerolistd(50);
            List<string> xinit = zerolists(50);

            chart.AxisX.Clear();
            chart.AxisY.Clear();
            chart.Series.Clear();

            chart.AxisX.Add(new Axis()
            {
                Title = "Time",
                Labels = xinit,
                Foreground = new SolidColorBrush(Colors.LightBlue)
            });
            chart.AxisY.Add(new Axis()
            {
                Title = "Position",
                LabelFormatter = value => value.ToString("0.##"),
                Foreground = new SolidColorBrush(Colors.LightBlue)
            });
            chart.Series.Add(new LineSeries()
            {
                Title = "Position",
                LineSmoothness = 0,
                Values = new ChartValues<double>(yinit)
            });
            chart.ToolTip = null;
            chart.AxisX[0].Separator.Stroke = new SolidColorBrush(Colors.LightBlue);
            chart.AxisY[0].Separator.Stroke = new SolidColorBrush(Colors.LightBlue);
        }


    }
}
