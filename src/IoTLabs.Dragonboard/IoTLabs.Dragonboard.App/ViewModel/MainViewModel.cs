﻿using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using IoTLabs.Dragonboard.Common;
using System.Diagnostics;
using System.Collections.Generic;
using Windows.UI.Xaml;
using IoTLabs.Dragonboard.App.Services;

namespace IoTLabs.Dragonboard.App.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public bool Initialized { get; set; } = false;


        string iotHubConnectionString = "HostName=msiotlabs-user01-winiot.azure-devices.net;DeviceId=dragonboard;SharedAccessKey=RVGMdK7Dci4ofnMfZsj4yp856Dr1hxI5ZhuQdLMRqUA=";
        IoTHubService _ioTHubService = null;
        DispatcherTimer _tmrIoTHub = null;

        ISensor<GroveRedLedSensorState> LedSensor;
        ISensor<GroveBarometerSensorState> BarometerSensor;
        ISensor<GroveDigitalAccelerometerState> AccelSensor;
        ISensor<GroveMiniPIRMotionSensorState> MotionSensor;
        ISensor<GroveButtonSensorState> ButtonSensor;
        ISensor<GpsLocationSensorState> LocationSensor;

        public MainViewModel()
        {

        }

        #region "Properties"

        private bool _IsRisingEdgeMotion = false;
        private bool _IsFallingEdgeMotion = false;

        private bool _ledIsHigh = false;
        private bool _buttonIsPressed = false;

        private string _temperature = "-";
        private string _humidity = "-";
        private string _pressure = "-";
        private string _lastBarometerUpdate = "-";
        private string _lastAccelerometerUpdate = "-";
        private string _lastGpsLocationUpdate = "-";
        private string _accelerometerX = "-";
        private string _accelerometerY = "-";
        private string _accelerometerZ = "-";
        private string _gpsLocationAltitude = "-";
        private string _gpsLocationLongitude = "-";
        private string _gpsLocationLattitude = "-";


        private string _motionEdge = "-";
        private string _motionPin = "-";


        public bool ButtonIsPressed
        {
            get => _buttonIsPressed;
            set { _buttonIsPressed = value; RaisePropertyChanged("ButtonIsPressed"); }
        }

        public bool IsFallingEdgeMotion
        {
            get => _IsFallingEdgeMotion;
            set { _IsFallingEdgeMotion = value; RaisePropertyChanged("IsFallingEdgeMotion"); RaisePropertyChanged("IsMotionDetected"); }
        }

        public bool IsRisingEdgeMotion
        {
            get => _IsRisingEdgeMotion;
            set { _IsRisingEdgeMotion = value; RaisePropertyChanged("IsRisingEdgeMotion"); RaisePropertyChanged("IsMotionDetected"); }
        }

        public bool IsMotionDetected => (_IsFallingEdgeMotion || _IsRisingEdgeMotion);


        public bool LedIsHigh
        {
            get => _ledIsHigh;
            set { _ledIsHigh = value; RaisePropertyChanged("LedIsHigh"); }
        }

        public string Temperature
        {
            get => _temperature;
            set { _temperature = value; RaisePropertyChanged("Temperature"); }
        }

        public string Humidity
        {
            get => _humidity;
            set { _humidity = value; RaisePropertyChanged("Humidity"); }
        }

        public string Pressure
        {
            get => _pressure;
            set { _pressure = value; RaisePropertyChanged("Pressure"); }
        }

        public string LastBarometerUpdate
        {
            get => _lastBarometerUpdate;
            set { _lastBarometerUpdate = value; RaisePropertyChanged("LastBarometerUpdate"); }
        }

        public string LastAccelerometerUpdate
        {
            get => _lastAccelerometerUpdate;
            set { _lastAccelerometerUpdate = value; RaisePropertyChanged("LastAccelerometerUpdate"); }
        }

        public string LastGpsLocationUpdate
        {
            get => _lastGpsLocationUpdate;
            set { _lastGpsLocationUpdate = value; RaisePropertyChanged("LastGpsLocationUpdate"); }
        }



        public string AccelerometerX
        {
            get => _accelerometerX;
            set { _accelerometerX = value; RaisePropertyChanged("AccelerometerX"); }
        }

        public string AccelerometerY
        {
            get => _accelerometerY;
            set { _accelerometerY = value; RaisePropertyChanged("AccelerometerY"); }
        }

        public string AccelerometerZ
        {
            get => _accelerometerZ;
            set { _accelerometerZ = value; RaisePropertyChanged("AccelerometerZ"); }
        }

        public string GpsLocationAltitude
        {
            get => _gpsLocationAltitude;
            set { _gpsLocationAltitude = value; RaisePropertyChanged("GpsLocationAltitude"); }
        }

        public string GpsLocationLongitude
        {
            get => _gpsLocationLongitude;
            set { _gpsLocationLongitude = value; RaisePropertyChanged("GpsLocationLongitude"); }
        }

        public string GpsLocationLattitude
        {
            get => _gpsLocationLattitude;
            set { _gpsLocationLattitude = value; RaisePropertyChanged("GpsLocationLattitude"); }
        }


        public string MotionPin
        {
            get => _motionPin;
            set { _motionPin = value; RaisePropertyChanged("MotionPin"); }
        }

        public string MotionEdge
        {
            get => _motionEdge;
            set { _motionEdge = value; RaisePropertyChanged("MotionEdge"); }
        }

        #endregion

        #region "Commands"

        private RelayCommand _setFullLedCommand;
        private RelayCommand _setHalfLedCommand;

        public RelayCommand SetFullLedCommand
        {
            get
            {
                return _setFullLedCommand
                       ?? (_setFullLedCommand = new RelayCommand(
                           () =>
                           {
                               if (LedSensor != null)
                               {
                                   LedSensor.WriteState(new GroveRedLedSensorState() { CurrentValue = GpioPinValue.High });
                                   LedIsHigh = LedSensor.GetState().CurrentValue == GpioPinValue.High;
                               }

                           }));
            }
        }

        public RelayCommand SetHalfLedCommand
        {
            get
            {
                return _setHalfLedCommand
                       ?? (_setHalfLedCommand = new RelayCommand(
                           () =>
                           {
                               LedSensor.WriteState(new GroveRedLedSensorState() { CurrentValue = GpioPinValue.Low });
                               LedIsHigh = LedSensor.GetState().CurrentValue == GpioPinValue.High;
                           }));
            }
        }

        #endregion

        #region "Sensors Load/Unload"

        public void LoadSensors()
        {
            if (Initialized) return;

            try
            {
                Action act = new Action(async () =>
                {
                    try
                    {


                        //
                        //=== ENTER SENSOR CONFIG IS HERE ===
                        //




                        //
                        //=== SENSOR CONFIG ENDS HERE ===
                        //

                        if (!string.IsNullOrWhiteSpace(iotHubConnectionString))
                        {
                            _ioTHubService = new IoTHubService(iotHubConnectionString);
                            _tmrIoTHub = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(3000) };
                            _tmrIoTHub.Tick += async (sender, args) =>
                            {

                                //collect values
                                if (_ioTHubService != null)
                                {
                                    //Create a payload to send
                                    IoTHubSensorPack pck = new IoTHubSensorPack()
                                    {
                                        SensorValues = new Dictionary<string, ISensorState>()
                                    };

                                    if (LedSensor != null) pck.SensorValues.Add("LED", LedSensor.GetState());
                                    if (MotionSensor != null) pck.SensorValues.Add("Motion", MotionSensor.GetState());
                                    if (BarometerSensor != null) pck.SensorValues.Add("Barometer", BarometerSensor.GetState());
                                    if (AccelSensor != null) pck.SensorValues.Add("Accelerometer", AccelSensor.GetState());
                                    if (ButtonSensor != null) pck.SensorValues.Add("Button", ButtonSensor.GetState());
                                    if (LocationSensor != null) pck.SensorValues.Add("GPS", LocationSensor.GetState());

                                    //Send the payload if values exist
                                    if (pck.SensorValues.Count > 0)
                                        await _ioTHubService.SubmitSensorsPayload(pck);

                                }
                            };

                            _tmrIoTHub.Start();

                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                });

                act.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            Initialized = true;
        }

        public async Task UnloadSensors()
        {
            try
            {

                if (_tmrIoTHub != null)
                {
                    if (_tmrIoTHub.IsEnabled)
                        _tmrIoTHub.Stop();
                    _tmrIoTHub = null;
                }

                if (_ioTHubService!=null)
                    {
                        await _ioTHubService.Close();
                    }

                if (LedSensor != null)
                    LedSensor.Close();

                if (AccelSensor != null)
                {
                    ((IPollingSensor<GroveMiniPIRMotionSensorState>)AccelSensor).Unregister();
                    AccelSensor.Close();
                }

                if (ButtonSensor != null)
                {
                    ((IObservableSensor<GroveButtonSensorState>)ButtonSensor).Unregister();
                    ButtonSensor.Close();
                }

                if (MotionSensor != null)
                {
                    ((IObservableSensor<GroveMiniPIRMotionSensorState>)MotionSensor).Unregister();
                    MotionSensor.Close();
                }

                if (BarometerSensor != null)
                {
                    ((IPollingSensor<GroveBarometerSensorState>)BarometerSensor).Unregister();
                    BarometerSensor.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        #endregion
    }
}
