using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Maker.Serial;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Firmata;
using Windows.UI.Core;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Text;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace WRA.Sensors.UI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        MainPageViewModel sensorData;
        static DeviceClient deviceClient;        const string iotHubUri = "HostName=SensorDataDemo.azure-devices.net;DeviceId=SENSOR-01;SharedAccessKeyName=iothubowner;SharedAccessKey={enter shared access key here}";



        public MainPage()
        {
            sensorData = new MainPageViewModel();
            sensorData.ReadingInterval = 15000; //15 secs
            this.sensorData.IsConnected = false;
            this.DataContext = sensorData;
            this.InitializeComponent();
            deviceClient = DeviceClient.CreateFromConnectionString(iotHubUri, TransportType.Http1);
            ReadingIntervalTextBox.Text = (sensorData.ReadingInterval / 1000).ToString();
        }


        private BluetoothSerial bluetooth = null;
        private RemoteDevice arduino = null;
        private UwpFirmata firmata = null;
        const string BT_DEVICE_NAME = "SENSOR-01";
        const byte SENSOR_PAD_RESPONSE = 0x43;
        const byte SENSOR_PAD_QUERY = 0x42;

        public async Task SetupFirmataBluetooth()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
             {
                 bluetooth = new BluetoothSerial(BT_DEVICE_NAME);
                 firmata = new UwpFirmata();
                 firmata.StringMessageReceived += Firmata_StringMessageReceived;
                 arduino = new RemoteDevice(firmata);
                 firmata.begin(bluetooth);
                 bluetooth.begin(57600, SerialConfig.SERIAL_8N1);

                 arduino.DeviceReady += OnDeviceReady;
                 arduino.DeviceConnectionFailed += OnConnectionFailed;
                 arduino.DeviceConnectionLost += OnConnectionLost;

             });
        }


        private async Task RequestSensorData()
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
           {
               firmata.sendSysex(SENSOR_PAD_QUERY, new byte[] { 99 }.AsBuffer());
               firmata.flush();
           });
        }

        private async void Firmata_StringMessageReceived(UwpFirmata caller, StringCallbackEventArgs argv)
        {
            var content = argv.getString();
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                sensorData.SensorName = BT_DEVICE_NAME;
                sensorData.SensorReadingDateTime = DateTime.Now;
                sensorData.setRawData(content);
                var telemetryDataPoint = new
                {
                    sensorReadingDateTime = sensorData.SensorReadingDateTime,
                    deviceId = BT_DEVICE_NAME,
                    tempCelcius = sensorData.TemperatureCelcius,
                    tempFarenheit = sensorData.TemperatureFarenheit,
                    relativeHumidity = sensorData.RelativeHumidity,
                    distanceCM = sensorData.DistanceCM,
                    motionDetected = sensorData.MotionDetected,
                    light = sensorData.Light
                };
                var messageString = JsonConvert.SerializeObject(telemetryDataPoint);
                var message = new Message(Encoding.ASCII.GetBytes(messageString));

                await deviceClient.SendEventAsync(message);
                this.MessageTextBlock.Text = String.Format("{0} >> Sending message:\n {1}", DateTime.Now, messageString);
            });
        }

        private async void Firmata_FirmataConnectionFailed(string message)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                this.MessageTextBlock.Text = "Firmata Connection lost: " + message;
            }));
        }

        private async void GetSensorDataButton_Click(object sender, RoutedEventArgs e)
        {
            while (true && !sensorData.IsCancelled)
            {
                await Task.Run(this.RequestSensorData);
                await System.Threading.Tasks.Task.Delay(sensorData.ReadingInterval); //wait for 15 seconds between readings (= 2000ms)
            }

        }

        private void SetNewIntervalButton_Click(object sender, RoutedEventArgs e)
        {
            double val;
            if (double.TryParse(ReadingIntervalTextBox.Text, out val))
            {
                if (val <= 0)
                {
                    this.NewIntervalTextBlock.Text = "Error:  Cannot set interval to zero or fewer seconds. Please re-enter.";
                    this.ReadingIntervalTextBox.Text = "1";
                    return;
                }

                sensorData.ReadingInterval = (int)Math.Truncate(val * 1000);
                double newInterval = (float)sensorData.ReadingInterval / 1000;
                this.NewIntervalTextBlock.Text = String.Format("The new interval [ {0:0.00} sec ] will take effect on the next sensor reading", newInterval);
                this.ReadingIntervalTextBox.Text = newInterval.ToString("#0.0#");
            }
        }

        #region Arduino/RemoteDevice Event Handlers
        private void OnConnectionLost(string message)
        {
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
            {
                this.MessageTextBlock.Text = "Connection lost: " + message;
                sensorData.IsConnected = false;
            }));
        }

        private void OnConnectionFailed(string message)
        {
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
         {
             this.MessageTextBlock.Text = "Connection attempt failed: " + message;
             sensorData.IsConnected = false;
         }));
        }

        private void OnDeviceReady()
        {
            var action = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, new Windows.UI.Core.DispatchedHandler(() =>
           {
               this.MessageTextBlock.Text = "Successfully connected!";
               sensorData.IsConnected = true;
               GetSensorDataButton_Click(this, null);
           }));
        }
        #endregion

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Task.Run(this.SetupFirmataBluetooth);
        }


        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            sensorData.IsCancelled = true;
            this.MessageTextBlock.Text = "Sensor readings have been cancelled.";
        }

        #region Interval text box up/down code

        private void cmdUp_Click(object sender, RoutedEventArgs e)
        {
            double value;
            if (double.TryParse(ReadingIntervalTextBox.Text, out value))
            {

                value++;
                ReadingIntervalTextBox.Text = value.ToString();
            }
        }

        private void cmdDown_Click(object sender, RoutedEventArgs e)
        {
            double value;
            if (double.TryParse(ReadingIntervalTextBox.Text, out value))
            {
                if (value - 1 <= 0) return;

                value--;
                ReadingIntervalTextBox.Text = value.ToString();
            }
        }
        #endregion
    }
}
