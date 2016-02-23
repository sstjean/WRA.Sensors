using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WRA.Sensors.UI
{

    class MainPageViewModel : INotifyPropertyChanged
    {
        enum SensorData
        {
            DHT22Response = 0,
            Temp,
            Humidity,
            Distance,
            Light,
            MotionDetected
        };

        public void setRawData(string value)
        {
            var values = value.Split(',');
            TemperatureCelcius = float.Parse(values[(int)SensorData.Temp]);
            TemperatureFarenheit = TemperatureCelcius * 9 / 5 + 32;
            RelativeHumidity = float.Parse(values[(int)SensorData.Humidity]);
            DistanceCM = long.Parse(values[(int)SensorData.Distance]);
            DHT22Response = values[(int)SensorData.DHT22Response];
            Light = int.Parse(values[(int)SensorData.Light]);
            MotionDetected = values[(int)SensorData.MotionDetected];
        }

        private bool _isConnected;
        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                RaisePropertyChanged();
            }
        }

        private bool _isCancelled;

        public bool IsCancelled
        {
            get { return _isCancelled; }
            set { _isCancelled = value; }
        }


        private string _sensorName;

        public string SensorName
        {
            get { return _sensorName; }
            set
            {
                _sensorName = value;
                RaisePropertyChanged();
            }
        }

        private DateTime _sensorReadingDateTime;

        public DateTime SensorReadingDateTime
        {
            get { return _sensorReadingDateTime; }
            set
            {
                _sensorReadingDateTime = value;
                RaisePropertyChanged();
            }
        }
        private float _tempF;

        public float TemperatureFarenheit
        {
            get { return _tempF; }
            private set
            {
                _tempF = value;
                RaisePropertyChanged();
            }
        }

        private float _tempC;

        public float TemperatureCelcius
        {
            get { return _tempC; }
            private set
            {
                _tempC = value;
                RaisePropertyChanged();
            }
        }

        private float _relativeHumidity;

        public float RelativeHumidity
        {
            get { return _relativeHumidity; }
            private set
            {
                _relativeHumidity = value;
                RaisePropertyChanged();
            }
        }

        private long _distanceCM;


        public long DistanceCM
        {
            get { return _distanceCM; }
            private set
            {
                _distanceCM = value;
                RaisePropertyChanged();
            }

        }

        private string _dht22Response;

        public string DHT22Response
        {
            get { return _dht22Response; }
            private set
            {
                _dht22Response = value;
                RaisePropertyChanged();
            }
        }

        private int _light;

        public int Light
        {
            get { return _light; }
            set
            {
                _light = value;
                RaisePropertyChanged();
            }
        }

        private string _motionDetected;

        public string MotionDetected
        {
            get { return _motionDetected; }
            set
            {
                _motionDetected = value;
                RaisePropertyChanged();
            }
        }

        private int _readingInterval;

        public int ReadingInterval
        {
            get { return _readingInterval; }
            set
            {
                if (value != _readingInterval)
                {
                    _readingInterval = value;
                    RaisePropertyChanged();
                }
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged([CallerMemberName] string caller = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(caller));
            }
        }



    }
}
