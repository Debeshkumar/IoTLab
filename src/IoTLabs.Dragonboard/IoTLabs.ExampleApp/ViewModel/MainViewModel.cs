﻿using System;
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using IoTLabs.Dragonboard;

namespace IoTLabs.ExampleApp.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public bool Initialized { get; set; } = false;

        ISensor<GroveRedLedSensorState> LedSensor;
        ISensor<GroveBaramoterSensorState> BarometerSensor;
        ISensor<GroveDigitalAccelerometerState> AccelSensor;
        ISensor<GroveMiniPIRMotionSensorState> MotionSensor;
        ISensor<GroveButtonSensorState> ButtonSensor;

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
        private string _accelerometerX = "-";
        private string _accelerometerY = "-";
        private string _accelerometerZ = "-";
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

        public bool IsMotionDetected => (_IsFallingEdgeMotion||_IsRisingEdgeMotion);
        

        public bool LedIsHigh
        {
            get => _ledIsHigh;
            set { _ledIsHigh = value; RaisePropertyChanged("LedIsHigh"); }
        }

        public string Temperature
        {
            get => _temperature;
            set { _temperature = value; RaisePropertyChanged("Temperature");}
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
                                   LedSensor.WriteState(new GroveRedLedSensorState() {CurrentValue=GpioPinValue.High});
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
                        //Create Instances
                        LedSensor = GroveSensorFactory.CreateRedLedSensorService();
                        BarometerSensor = GroveSensorFactory.CreateBarometerSensorService();
                        MotionSensor = GroveSensorFactory.CreateMiniPIRMotionSensorService();
                        AccelSensor = GroveSensorFactory.CreateAccelerometerSensorService();
                        ButtonSensor = GroveSensorFactory.CreateGroveButtonSensorService();


                        //Initialize
                        await LedSensor.Initialize();
                        await BarometerSensor.Initialize();
                        await MotionSensor.Initialize();
                        await AccelSensor.Initialize();
                        await ButtonSensor.Initialize();

                        //MotionSensor
                        ((IObservableSensor<GroveMiniPIRMotionSensorState>)MotionSensor).Register(new Action<GroveMiniPIRMotionSensorState>(
                            (GroveMiniPIRMotionSensorState item) =>
                            {
                                DispatcherHelper.CheckBeginInvokeOnUI(async () =>
                                    {
                                        MotionEdge = item.CurrentEdge.ToString();
                                        MotionPin = item.PinNumber.ToString();

                                        if (item.CurrentEdge == GpioPinEdge.FallingEdge)
                                        {
                                            IsFallingEdgeMotion = true;
                                            await Task.Delay(TimeSpan.FromMilliseconds(2000));
                                            IsFallingEdgeMotion = false;
                                        }
                                        else
                                        {
                                            IsRisingEdgeMotion = true;
                                            await Task.Delay(TimeSpan.FromMilliseconds(2000));
                                            IsRisingEdgeMotion = false;
                                        }

                                    });
                            }));


                        //Button Sensor
                        ((IObservableSensor<GroveButtonSensorState>)ButtonSensor).Register(new Action<GroveButtonSensorState>(
                            (GroveButtonSensorState item) =>
                            {
                                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                                {
                                    ButtonIsPressed = item.IsPressed;
                                });
                            }));



                        //Barometer Sensor
                        ((IPollingSensor<GroveBaramoterSensorState>)BarometerSensor).Register(new Action<GroveBaramoterSensorState>(
                           (GroveBaramoterSensorState item) =>
                           {
                               DispatcherHelper.CheckBeginInvokeOnUI(() =>
                               {
                                   Temperature = item.TemperatureCelcius.ToString("##0.0") + " C";
                                   Humidity = item.HumidityPercent.ToString("##0.0") + "%";
                                   Pressure = item.PressureKilopascals.ToString("##0.0") + " kPA";
                                   LastBarometerUpdate = item.TimeStamp.ToShortDateString() + " " + item.TimeStamp.ToLongTimeString();
                               });
                           }), 5000);


                        //Accelerometer Sensor
                        ((IPollingSensor<GroveDigitalAccelerometerState>)AccelSensor).Register(new Action<GroveDigitalAccelerometerState>(
                           (GroveDigitalAccelerometerState item) =>
                           {
                               DispatcherHelper.CheckBeginInvokeOnUI(() =>
                               {
                                   AccelerometerX = item.X.ToString("##0.00");
                                   AccelerometerY = item.Y.ToString("##0.00");
                                   AccelerometerZ = item.Z.ToString("##0.00");
                                   LastAccelerometerUpdate = item.TimeStamp.ToShortDateString() + " " + item.TimeStamp.ToLongTimeString();
                               });
                           }), 5000);



                        //Get Defaults
                        LedIsHigh = LedSensor.GetState().CurrentValue == GpioPinValue.High;
                        ButtonIsPressed = ButtonSensor.GetState().IsPressed;

                    }
                    catch
                    {
                    }
                });

                act.Invoke();
            }
            catch (Exception e)
            {
                
            }
            Initialized = true;
        }

        public void UnloadSensors()
        {
            try
            {

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
                    ((IPollingSensor<GroveBaramoterSensorState>)BarometerSensor).Unregister();
                    BarometerSensor.Close();
                }
                

            }
            catch (Exception e)
            {
                
            }
        }

        #endregion
    }
}
