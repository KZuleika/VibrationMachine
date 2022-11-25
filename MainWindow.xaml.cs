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
        private bool flag = false;
        public MainWindow()
        {
            InitializeComponent();
            GraphInit(HGraph); myport.BaudRate = 9600;
            foreach(string s in SerialPort.GetPortNames())
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = s;
                PortsComboBox.Items.Add(comboBoxItem);
            }
            //myport.PortName = "COM6";
            //myport.DataReceived += serialPort1_DataReceived;
            //myport.Open();
        }
 
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            StopButton.IsEnabled = true;
            StartButton.IsEnabled = false;
            StartTime = DateTime.Now;
            var rand = new Random();
            //UpdateChart(rand.NextDouble(), 6, HGraph);
            //StartTime = DateTime.Now;
            if (myport.IsOpen) MessageBox.Show("PORT OPEN");
            flag = true;
        }


        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            StopButton.IsEnabled = false;
            StartButton.IsEnabled = true;
            flag = false;
            //StopTime = DateTime.Now;
            //double time = StopTime.Subtract(StartTime).TotalSeconds;
            //double secrest = (time / 60.0 - Math.Floor(time / 60.0)) * 60;
            //TLabel.Content = Math.Floor(time / 60.0).ToString("00") + ":" + secrest.ToString("00.00");
        }

        private void HGraph_Loaded(object sender, RoutedEventArgs e)
        {

        }
        private void LineReceived(string line)
        {
            StopTime = DateTime.Now;
            double time = StopTime.Subtract(StartTime).TotalSeconds;
            double secrest = (time / 60.0 - Math.Floor(time / 60.0)) * 60;
            double[] packet = dataparse(line);

            SaveData(
                Math.Floor(time / 60.0).ToString("00") + ":" + secrest.ToString("00.00") 
                + ","+ 
                line);

            
            this.Dispatcher.Invoke(() =>
            {
                TLabel.Content = Math.Floor(time / 60.0).ToString("00") + ":" + secrest.ToString("00.00");
                UpdateChart(packet[0], time, HGraph, 0);
                MaxHLabel.Content = packet[1];
                LastX_Label.Content = packet[0];
            });

        }

        private void serialPort1_DataReceived(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {
            if (flag)
            {
                string line = myport.ReadLine();
                LineReceived(line);
            }

        }

        private void UpdateChart(double p, double t, CartesianChart chart)
        {
            chart.Series[0].Values.Add(p);
            chart.Series[0].Values.RemoveAt(0);
            chart.AxisX[0].Labels.Add(String.Format("{0:0.##}", t));
            chart.AxisX[0].Labels.RemoveAt(0);
            var maxaux = Math.Truncate(p * 1000) / 1000;
            MaxHLabel.Content = maxaux.ToString();
        }

        private void UpdateChart(double y, double x, CartesianChart chart, int i)
        {
            //chart.Series[0].Values.Add(x);
            //chart.Series[0].Values.RemoveAt(0);
            //chart.AxisX[0].Labels.Add(String.Format("{0:0.##}", y));
            //chart.AxisX[0].Labels.RemoveAt(0);

            chart.Series[i].Values.Add(new ObservablePoint { X = x, Y = y });
            chart.Series[i].Values.RemoveAt(0);
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
                string portName = PortsComboBox.SelectedItem.ToString().Replace
                    ("System.Windows.Controls.ComboBoxItem:"," ").Trim();
                myport.PortName = portName;
                myport.BaudRate = 9600;
                myport.DataReceived += serialPort1_DataReceived;
                myport.Open();
                MessageBox.Show("Conexión exitosa");
                StatusLabel.Content = "Estado: Conectado";
                ConnectButton.IsEnabled = false;
                DisconnectButton.IsEnabled = true;
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
                StatusLabel.Content = "Estado: Desconectado";
            }
            catch(Exception)
            {
                MessageBox.Show("Primero conecte su dispositivo, luego haga clic en desconectar.");
            }
            ConnectButton.IsEnabled = true;
            DisconnectButton.IsEnabled = false;
        }

        private void OpenButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Guardado automático con éxito. " +
                "Disponible en \n" +
                "VibrationMachine/bin/Debug/dataSaved.csv ");
        }
       
        private void SaveData(List<string> dat)
        {
            StreamWriter writer = new StreamWriter("data.csv");
            dat.ForEach(s => writer.WriteLine(s));
            writer.Close();
        }
        private void SaveData(string dat)
        {
            StreamWriter writer = new StreamWriter("dataSaved.csv", true);
            writer.WriteLine(dat);
            writer.Close();
        }

        private void GraphInit(CartesianChart chart)
        {
            List<double> yinit = zerolistd(50);
            List<double> xinit = zerolistd(50);

            ChartValues<ObservablePoint> xy = new ChartValues<ObservablePoint>();
            for (int i = 0; i < yinit.Count; i++)
            {
                xy.Add(new ObservablePoint
                {
                    X = xinit[i],
                    Y = yinit[i]
                });
            }


            chart.AxisX.Clear();
            chart.AxisY.Clear();
            chart.Series.Clear();

            chart.AxisX.Add(new Axis()
            {
                Title = "Tiempo (t/s)",
                LabelFormatter = value => value.ToString("0.##"),
                Foreground = new SolidColorBrush(Colors.LightBlue)
            });
            chart.AxisY.Add(new Axis()
            {
                Title = "Position (h/m)",
                LabelFormatter = value => value.ToString("0.##"),
                Foreground = new SolidColorBrush(Colors.LightBlue)
            });
            chart.Series.Add(new LineSeries()
            {
                Title = "Position",
                LineSmoothness = 0,
                Values = xy
            });
            chart.ToolTip = null;
            chart.AxisX[0].Separator.Stroke = new SolidColorBrush(Colors.LightBlue);
            chart.AxisY[0].Separator.Stroke = new SolidColorBrush(Colors.LightBlue);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            if (myport.IsOpen)
            {
                if (Convert.ToBoolean(PWM_RadioButton10.IsChecked) == true) myport.Write("a");
                if (Convert.ToBoolean(PWM_RadioButton15.IsChecked) == true) myport.Write("b");
                if (Convert.ToBoolean(PWM_RadioButton20.IsChecked) == true) myport.Write("c");
                if (Convert.ToBoolean(PWM_RadioButton25.IsChecked) == true) myport.Write("d");
                if (Convert.ToBoolean(PWM_RadioButton30.IsChecked) == true) myport.Write("e");
                if (Convert.ToBoolean(PWM_RadioButton40.IsChecked) == true) myport.Write("f");
                if (Convert.ToBoolean(PWM_RadioButton50.IsChecked) == true) myport.Write("g");
                if (Convert.ToBoolean(PWM_RadioButton60.IsChecked) == true) myport.Write("h");
                if (Convert.ToBoolean(PWM_RadioButton70.IsChecked) == true) myport.Write("i");
                if (Convert.ToBoolean(PWM_RadioButton80.IsChecked) == true) myport.Write("j");
                if (Convert.ToBoolean(PWM_RadioButton90.IsChecked) == true) myport.Write("k");
                if (Convert.ToBoolean(PWM_RadioButton95.IsChecked) == true) myport.Write("l");
            }
        }

        double[] dataparse(string line)
        {
            double[] aux = new double[2];
            string[] cut = line.Split(',');
            int i = 0;
            double a = 0;
            foreach (string c in cut)
            {
                if (double.TryParse(c, out a))
                {
                    aux[i] = a;
                }
                else aux[i] = 0;
                i++;
            }
            return aux;
        }


    }
}
