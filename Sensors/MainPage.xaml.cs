﻿using System;
using Tizen.Wearable.CircularUI.Forms;
using Xamarin.Forms.Xaml;
using Samsung.Sap;
using System.Diagnostics;
using System.IO;
using System.Text;
using Tizen.Sensor;
using Sensors.Model;
using Tizen.Security;
using System.Threading;
using System.Collections.Generic;

namespace Sensors
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Main : CirclePage
    {
        public Main()
        {
            InitializeComponent();

            initDataSourcesWithPrivileges();

            try
            {
                initAgentConnection();
            }
            catch (Exception e)
            {
                log(e.Message);
            }
        }

        protected override void OnAppearing()
        {
            terminateFilesCounterThread();
            startFilesCounterThread();

            base.OnAppearing();
        }

        protected override void OnDisappearing()
        {
            terminateFilesCounterThread();

            base.OnDisappearing();
        }

        private void initDataSourcesWithPrivileges()
        {
            PrivacyPrivilegeManager.ResponseContext context = null;
            if (PrivacyPrivilegeManager.GetResponseContext(Tools.HEALTHINFO_PRIVILEGE).TryGetTarget(out context))
                context.ResponseFetched += (s, e) =>
                {
                    if (e.result != RequestResult.AllowForever)
                    {
                        Toast.DisplayText("Please provide the necessary privileges for the application to run!");
                        Environment.Exit(1);
                    }
                    else
                        initDataSources();
                };
            else
            {
                Toast.DisplayText("Please provide the necessary privileges for the application to run!");
                Environment.Exit(1);
            }

            switch (PrivacyPrivilegeManager.CheckPermission(Tools.HEALTHINFO_PRIVILEGE))
            {
                case CheckResult.Allow:
                    initDataSources();
                    break;
                case CheckResult.Deny:
                    Toast.DisplayText("Please provide the necessary privileges for the application to run!");
                    Environment.Exit(1);
                    break;
                case CheckResult.Ask:
                    PrivacyPrivilegeManager.RequestPermission(Tools.HEALTHINFO_PRIVILEGE);
                    break;
                default:
                    break;
            }
        }

        private void initDataSources()
        {
            #region Assign sensor model references
            accelerometerModel = new AccelerometerModel
            {
                IsSupported = Accelerometer.IsSupported,
                SensorCount = Accelerometer.Count
            };
            gravityModel = new GravityModel
            {
                IsSupported = GravitySensor.IsSupported,
                SensorCount = GravitySensor.Count
            };
            gyroscopeModel = new GyroscopeModel
            {
                IsSupported = Gyroscope.IsSupported,
                SensorCount = Gyroscope.Count
            };
            hRMModel = new HRMModel
            {
                IsSupported = HeartRateMonitor.IsSupported,
                SensorCount = HeartRateMonitor.Count
            };
            humidityModel = new HumidityModel
            {
                IsSupported = HumiditySensor.IsSupported,
                SensorCount = HumiditySensor.Count
            };
            lightModel = new LightModel
            {
                IsSupported = LightSensor.IsSupported,
                SensorCount = LightSensor.Count
            };
            linearAccelerationModel = new LinearAccelerationModel
            {
                IsSupported = LinearAccelerationSensor.IsSupported,
                SensorCount = LinearAccelerationSensor.Count
            };
            magnetometerModel = new MagnetometerModel
            {
                IsSupported = Magnetometer.IsSupported,
                SensorCount = Magnetometer.Count
            };
            orientationModel = new OrientationModel
            {
                IsSupported = OrientationSensor.IsSupported,
                SensorCount = OrientationSensor.Count
            };
            pressureModel = new PressureModel
            {
                IsSupported = PressureSensor.IsSupported,
                SensorCount = PressureSensor.Count
            };
            proximityModel = new ProximityModel
            {
                IsSupported = ProximitySensor.IsSupported,
                SensorCount = ProximitySensor.Count
            };
            temperatureModel = new TemperatureModel
            {
                IsSupported = TemperatureSensor.IsSupported,
                SensorCount = TemperatureSensor.Count
            };
            ultravioletModel = new UltravioletModel
            {
                IsSupported = UltravioletSensor.IsSupported,
                SensorCount = UltravioletSensor.Count
            };
            #endregion

            #region Assign sensor references and sensor measurement event handlers
            if (accelerometerModel.IsSupported)
            {
                accelerometer = new Accelerometer();
                accelerometer.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                accelerometer.DataUpdated += storeAccelerometerDataCallback;
            }
            if (gravityModel.IsSupported)
            {
                gravity = new GravitySensor();
                gravity.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                gravity.DataUpdated += storeGravitySensorDataCallback;
            }
            if (gyroscopeModel.IsSupported)
            {
                gyroscope = new Gyroscope();
                gyroscope.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                gyroscope.DataUpdated += storeGyroscopeDataCallback;
            }
            if (hRMModel.IsSupported)
            {
                hRM = new HeartRateMonitor();
                hRM.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                hRM.DataUpdated += storeHeartRateMonitorDataCallback;
            }
            if (humidityModel.IsSupported)
            {
                humidity = new HumiditySensor();
                humidity.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                humidity.DataUpdated += storeHumiditySensorDataCallback;
            }
            if (lightModel.IsSupported)
            {
                light = new LightSensor();
                light.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                light.DataUpdated += storeLightSensorDataCallback;
            }
            if (linearAccelerationModel.IsSupported)
            {
                linearAcceleration = new LinearAccelerationSensor();
                linearAcceleration.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                linearAcceleration.DataUpdated += storeLinearAccelerationSensorDataCallback;
            }
            if (magnetometerModel.IsSupported)
            {
                magnetometer = new Magnetometer();
                magnetometer.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                magnetometer.DataUpdated += storeMagnetometerDataCallback;
            }
            if (orientationModel.IsSupported)
            {
                orientation = new OrientationSensor();
                orientation.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                orientation.DataUpdated += storeOrientationSensorDataCallback;
            }
            if (pressureModel.IsSupported)
            {
                pressure = new PressureSensor();
                pressure.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                pressure.DataUpdated += storePressureSensorDataCallback;
            }
            if (proximityModel.IsSupported)
            {
                proximity = new ProximitySensor();
                proximity.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                proximity.DataUpdated += storeProximitySensorDataCallback;
            }
            if (temperatureModel.IsSupported)
            {
                temperature = new TemperatureSensor();
                temperature.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                temperature.DataUpdated += storeTemperatureSensorDataCallback;
            }
            if (ultravioletModel.IsSupported)
            {
                ultraviolet = new UltravioletSensor();
                ultraviolet.Interval = Tools.SENSOR_SAMPLING_INTERVAL;
                ultraviolet.DataUpdated += storeUltravioletSensorDataCallback;
            }
            #endregion
        }

        private async void initAgentConnection()
        {
            agent = await Agent.GetAgent("/sample/hello", onConnect: (con) =>
            {
                if (con.Peer.ProfileVersion == agent.ProfileVersion)
                {
                    con.DataReceived += (sender, evt) =>
                    {
                        if (evt.Channel.ID == Tools.CHANNEL_ID)
                        {
                            byte request = evt.Data[0];

                            if (request == Tools.REQUEST_DATA)
                                reportDataCollection(reportDataColButton, new EventArgs());
                        }
                    };
                    conn = con;
                    peer = conn.Peer;
                    return true;
                }
                else
                    return false;
            });
        }

        #region Variables
        // Log properties
        private Thread filesCounterThread;
        private Thread submitDataThread;
        private string openLogStreamStamp;
        private StreamWriter logStreamWriter;
        private int logLinesCount = 1;
        private int filesCount;
        private bool stopFilesCounterThread;
        private bool stopSubmitDataThread;

        // Connection properties
        private Agent agent;
        private Connection conn;
        private Peer peer;

        // Sensors and their SensorModels
        internal Accelerometer accelerometer { get; private set; }
        internal AccelerometerModel accelerometerModel { get; private set; }
        internal GravityModel gravityModel { get; private set; }
        internal GravitySensor gravity { get; private set; }
        internal GyroscopeModel gyroscopeModel { get; private set; }
        internal Gyroscope gyroscope { get; private set; }
        internal HRMModel hRMModel { get; private set; }
        internal HeartRateMonitor hRM { get; private set; }
        internal HumidityModel humidityModel { get; private set; }
        internal HumiditySensor humidity { get; private set; }
        internal LightModel lightModel { get; private set; }
        internal LightSensor light { get; private set; }
        internal LinearAccelerationModel linearAccelerationModel { get; private set; }
        internal LinearAccelerationSensor linearAcceleration { get; private set; }
        internal MagnetometerModel magnetometerModel { get; private set; }
        internal Magnetometer magnetometer { get; private set; }
        internal OrientationModel orientationModel { get; private set; }
        internal OrientationSensor orientation { get; private set; }
        internal PressureModel pressureModel { get; private set; }
        internal PressureSensor pressure { get; private set; }
        internal ProximityModel proximityModel { get; private set; }
        internal ProximitySensor proximity { get; private set; }
        internal TemperatureModel temperatureModel { get; private set; }
        internal TemperatureSensor temperature { get; private set; }
        internal UltravioletModel ultravioletModel { get; private set; }
        internal UltravioletSensor ultraviolet { get; private set; }
        #endregion

        #region UI Event callbacks
        private void reportDataCollection(object sender, EventArgs e)
        {
            terminateSubmitDataThread();
            startSubmitDataThread();
        }

        private void startDataCollectionClick(object sender, EventArgs e)
        {
            log("Sensor data collection started");
            accelerometer?.Start();
            gravity?.Start();
            gyroscope?.Start();
            hRM?.Start();
            humidity?.Start();
            light?.Start();
            linearAcceleration?.Start();
            magnetometer?.Start();
            orientation?.Start();
            pressure?.Start();
            proximity?.Start();
            temperature?.Start();
            ultraviolet?.Start();

            startDataColButton.IsEnabled = false;
            stopDataColButton.IsEnabled = true;
        }

        private void stopDataCollectionClick(object sender, EventArgs e)
        {
            log("Sensor data collection stopped");
            accelerometer?.Stop();
            gravity?.Stop();
            gyroscope?.Stop();
            hRM?.Stop();
            humidity?.Stop();
            light?.Stop();
            linearAcceleration?.Stop();
            magnetometer?.Stop();
            orientation?.Stop();
            pressure?.Stop();
            proximity?.Stop();
            temperature?.Stop();
            ultraviolet?.Stop();

            startDataColButton.IsEnabled = true;
            stopDataColButton.IsEnabled = false;
        }
        #endregion

        #region Sensor DataUpdated Callbacks
        private void storeAccelerometerDataCallback(object sender, AccelerometerDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter?.Flush();
            logStreamWriter?.WriteLine($"{Tools.ACCELEROMETER},{e.X},{e.Y},{e.Z}");
        }
        private void storeGravitySensorDataCallback(object sender, GravitySensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.GRAVITY},{e.X},{e.Y},{e.Z}");
            }
        }
        private void storeGyroscopeDataCallback(object sender, GyroscopeDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.GYROSCOPE},{e.X},{e.Y},{e.Z}");
            }
        }
        private void storeHeartRateMonitorDataCallback(object sender, HeartRateMonitorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.HRM},{e.HeartRate}");
            }
        }
        private void storeHumiditySensorDataCallback(object sender, HumiditySensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.HUMIDITY},{e.Humidity}");
            }
        }
        private void storeLightSensorDataCallback(object sender, LightSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.LIGHT},{e.Level}");
            }
        }
        private void storeLinearAccelerationSensorDataCallback(object sender, LinearAccelerationSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.LINEARACCELERATION},{e.X},{e.Y},{e.Z}");
            }
        }
        private void storeMagnetometerDataCallback(object sender, MagnetometerDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.MAGNETOMETER},{e.X},{e.Y},{e.Z}");
            }
        }
        private void storeOrientationSensorDataCallback(object sender, OrientationSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.ORIENTATION},{e.Azimuth}, {e.Pitch}, {e.Roll}");
            }
        }
        private void storePressureSensorDataCallback(object sender, PressureSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.PRESSURE},{e.Pressure}");
            }
        }
        private void storeProximitySensorDataCallback(object sender, ProximitySensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.PROXIMITY},{e.Proximity}");
            }
        }
        private void storeTemperatureSensorDataCallback(object sender, TemperatureSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.TEMPERATURE},{e.Temperature}");
            }
        }
        private void storeUltravioletSensorDataCallback(object sender, UltravioletSensorDataUpdatedEventArgs e)
        {
            checkUpdateCurrentLogStream();
            logStreamWriter.Flush();
            lock (logStreamWriter)
            {
                logStreamWriter.WriteLine($"{Tools.ULTRAVIOLET},{e.UltravioletIndex}");
            }
        }
        #endregion

        private void reportToETAgent(
            string message = default(string),
            string path = default(string),
            EventHandler<FileTransferFinishedEventArgs> fileTransferFinishedHandler = default(EventHandler<FileTransferFinishedEventArgs>))
        {
            if (message != default(string))
            {
                conn.Send(agent.Channels[Tools.CHANNEL_ID], Encoding.UTF8.GetBytes(message));
                Debug.WriteLine(Tools.TAG, $"Message has been submitted on BLE. length={message.Length}");
            }
            else if (path != default(string))
            {
                OutgoingFileTransfer oft = new OutgoingFileTransfer(peer, path);
                oft.Send();
                oft.Finished += (s, e) => { Debug.WriteLine(Tools.TAG, $"File has been submitted on BLE. Result => {e.Result}"); };
            }
            log("Data uploaded");
        }

        private void log(string message)
        {
            if (logLinesCount == logLabel.MaxLines)
                logLabel.Text = $"{logLabel.Text.Substring(logLabel.Text.IndexOf('\n') + 1)}\n{message}";
            else
            {
                logLabel.Text = $"{logLabel.Text}\n{message}";
                logLinesCount++;
            }
        }

        private void eraseSensorData()
        {
            foreach (string file in Directory.GetFiles(Tools.APP_DIR, "*.csv"))
                File.Delete(file);
        }

        private int countSensorDataFiles()
        {
            return Directory.GetFiles(Tools.APP_DIR, "*.csv").Length;
        }

        private void checkUpdateCurrentLogStream()
        {
            DateTime nowTimestamp = DateTime.Now;
            nowTimestamp = new DateTime(year: nowTimestamp.Year, month: nowTimestamp.Month, day: nowTimestamp.Day, hour: nowTimestamp.Hour, minute: nowTimestamp.Minute, second: 0);
            string nowStamp = $"{nowTimestamp.Ticks / 100000000}";

            if (logStreamWriter == default(StreamWriter))
            {
                openLogStreamStamp = nowStamp;
                string filePath = Path.Combine(Tools.APP_DIR, $"{nowStamp}.csv");
                logStreamWriter = new StreamWriter(path: filePath, append: true);

                log("Data-log file created/attached");
            }
            else if (nowStamp.CompareTo(openLogStreamStamp) != 0)
            {
                logStreamWriter.Flush();
                logStreamWriter.Close();

                openLogStreamStamp = nowStamp;
                string filePath = Path.Combine(Tools.APP_DIR, $"{nowStamp}.csv");
                logStreamWriter = new StreamWriter(path: filePath, append: false);

                log("New data-log file created");
            }
        }

        private void terminateFilesCounterThread()
        {
            stopFilesCounterThread = true;
            filesCounterThread?.Join();
            stopFilesCounterThread = false;
        }

        private void startFilesCounterThread()
        {
            filesCounterThread = new Thread(() =>
            {
                using (FileSystemWatcher watcher = new FileSystemWatcher(Tools.APP_DIR))
                {
                    filesCount = countSensorDataFiles();

                    filesCountLabel.Text = $"FILES: {filesCount}";

                    watcher.Filter = "*.csv";
                    watcher.Deleted += (s, e) => { filesCountLabel.Text = $"FILES: {--filesCount}"; };
                    watcher.Created += (s, e) => { filesCountLabel.Text = $"FILES: {++filesCount}"; };

                    watcher.EnableRaisingEvents = true;
                    while (!stopFilesCounterThread) ;

                    watcher.EnableRaisingEvents = false;
                }
            });
            filesCounterThread.IsBackground = true;
            filesCounterThread.Start();
        }

        private void terminateSubmitDataThread()
        {
            stopSubmitDataThread = true;
            submitDataThread?.Join();
            stopSubmitDataThread = false;
        }

        private void startSubmitDataThread()
        {
            submitDataThread = new Thread(() =>
            {
                string[] filePaths = Directory.GetFiles(Tools.APP_DIR, "*.csv");
                List<long> fileNamesInLong = new List<long>();
                for (int n = 0; !stopSubmitDataThread && n < filePaths.Length; n++)
                {
                    string tmp = filePaths[n].Substring(filePaths[n].LastIndexOf('/') + 1);
                    fileNamesInLong.Add(long.Parse(tmp.Substring(0, tmp.LastIndexOf('.'))));
                }
                fileNamesInLong.Sort();
                for (int n = 0; !stopSubmitDataThread && n < fileNamesInLong.Count - 1; n++)
                {
                    string filepath = Path.Combine(Tools.APP_DIR, $"{fileNamesInLong[n]}.csv");
                    reportToETAgent(path: filepath);
                    File.Delete(filepath);
                }
            });
            submitDataThread.IsBackground = true;
            submitDataThread.Start();
        }
    }
}
