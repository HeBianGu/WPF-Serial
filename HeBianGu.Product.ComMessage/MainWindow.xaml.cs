using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using port = System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Threading;
using System.IO.Ports;

namespace HeBianGu.Product.ComMessage
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        MainNotifyClass _vm = new MainNotifyClass();
        public MainWindow()
        {
            InitializeComponent();

            this.DataContext = _vm;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }
    }



    partial class MainNotifyClass
    {


        private string _com;
        /// <summary> 说明  </summary>
        public string Com
        {
            get { return _com; }
            set
            {
                _com = value;
                RaisePropertyChanged("Com");
            }
        }


        private string _rate = "115200";
        /// <summary> 说明  </summary>
        public string Rate
        {
            get { return _rate; }
            set
            {
                _rate = value;
                RaisePropertyChanged("Rate");
            }
        }


        private port.Parity _parity = port.Parity.None;
        /// <summary> 说明  </summary>
        public port.Parity Parity
        {
            get { return _parity; }
            set
            {
                _parity = value;
                RaisePropertyChanged("Parity");
            }
        }


        private string _dataBits = "8";
        /// <summary> 说明  </summary>
        public string DataBits
        {
            get { return _dataBits; }
            set
            {
                _dataBits = value;
                RaisePropertyChanged("DataBits");
            }
        }


        private port.StopBits _stopBits = port.StopBits.One;
        /// <summary> 说明  </summary>
        public port.StopBits StopBits
        {
            get { return _stopBits; }
            set
            {
                _stopBits = value;
                RaisePropertyChanged("StopBits");
            }
        }


        private string _text;
        /// <summary> 说明  </summary>
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                RaisePropertyChanged("Text");
            }
        }


        private string _message;
        /// <summary> 说明  </summary>
        public string Message
        {
            get { return _message; }
            set
            {
                _message = value;
                RaisePropertyChanged("Message");

                if (_message == null) return;

                Task.Run(() =>
                {
                    Thread.Sleep(3000);

                    Message = null;

                });
            }
        }

        port.SerialPort ComDevice;

        public void RelayMethod(object obj)
        {
            string command = obj.ToString();

            //  Do：应用
            if (command == "open")
            {

                if (ComDevice != null && ComDevice.IsOpen)
                {
                    this.Message = "串口已經打開";
                    return;
                }

                if (string.IsNullOrEmpty(this.Com))
                {
                    this.Message = "请选择串口号";
                    return;
                }

                Debug.WriteLine(this.Com);
                Debug.WriteLine(this.Rate);
                Debug.WriteLine(this.Parity);
                Debug.WriteLine(this.DataBits);
                Debug.WriteLine(this.StopBits);

                ComDevice = new port.SerialPort(this.Com, int.Parse(this.Rate), this.Parity, 8, this.StopBits);

                ///设置无协议
                ComDevice.Handshake = Handshake.None;
                ComDevice.DtrEnable = true;


                ComDevice.DataReceived += (l, k) =>
                {
                    try
                    {
                        Debug.WriteLine(DateTime.Now + "接收到数据");

                        // 开辟接收缓冲区
                        byte[] ReDatas = new byte[ComDevice.BytesToRead];

                        //从串口读取数据
                        ComDevice.Read(ReDatas, 0, ReDatas.Length);
                        //实现数据的解码与显示

                        //AddData(ReDatas);

                        this.Text += DateTime.Now.ToString("hh:mm:ss") + ":   " + ReDatas.Select(m => Convert.ToString(m, 16)).Aggregate((m, n) => m.ToString() + " " + n.ToString()) + Environment.NewLine;
                    }
                    catch (Exception ex)
                    {
                        Message = ex.Message;
                    }

                };

                try
                {
                    //开启串口
                    ComDevice.Open();
                }
                catch (Exception ex)
                {
                    Message = ex.Message;
                }

                //Task.Run(()=>
                //{
                //    while(true)
                //    {
                //        Thread.Sleep(1000);

                //        this.Text += "35 34 36 30 35 36 38 31 39 36 34 36"+Environment.NewLine;
                //    }
                //});
            }
            //  Do：取消
            else if (command == "Cancel")
            {


            }
        }

        public bool CanRelayMethod(object obj)
        {
            string command = obj.ToString();


            //  Do：应用
            if (command == "open")
            {
                if (ComDevice == null) return true;

                if (ComDevice.IsOpen) return false;

                return true;

            }
            //  Do：取消
            else if (command == "Cancel")
            {


            }

            return true;
        }



        private Visibility _visiblity;
        /// <summary> 说明  </summary>
        public Visibility Visibility
        {
            get { return _visiblity; }
            set
            {
                _visiblity = value;
                RaisePropertyChanged("Visibility");
            }
        }



    }

    partial class MainNotifyClass : INotifyPropertyChanged
    {
        public RelayCommand RelayCommand { get; set; }

        public MainNotifyClass()
        {
            RelayCommand = new RelayCommand(RelayMethod, CanRelayMethod);

            RelayCommand.CanExecuteChanged += RelayCommand_CanExecuteChanged;

        }

        private void RelayCommand_CanExecuteChanged(object sender, EventArgs e)
        {
            bool v = this.CanRelayMethod(sender);

            this.Visibility = v ? Visibility.Visible : Visibility.Collapsed;
        }
        #region - MVVM -

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary> 带参数的命令 </summary>
    public class RelayCommand : ICommand
    {
        private Action<object> _action;

        private Predicate<object> _canAction;

        public RelayCommand(Action<object> action, Predicate<object> canAction)
        {
            _action = action;

            _canAction = canAction;
        }

        Boolean _canExecuteCache;

        #region ICommand Members
        public bool CanExecute(object parameter)
        {
            if (_canAction == null) return true;

             return _canAction(parameter);

            //////CanExecuteChanged?.Invoke(v, EventArgs.Empty);

            ////return v;

            //if (this._canAction == null)
            //    return true;

            //Boolean bResult = this._canAction(parameter);

            //if (bResult != _canExecuteCache)
            //{
            //    _canExecuteCache = bResult;

            //    EventHandler handler = CanExecuteChanged;

            //    if (handler != null)
            //        handler(parameter, EventArgs.Empty);
            //}

            //return bResult;

        }
        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (parameter != null)
            {
                _action(parameter);
            }
            else
            {
                _action("Hello");
            }
        }
        #endregion



        ///// <summary> 隐式转换 </summary>
        //static public implicit operator RelayCommand(Action<object> action,Predicate<object> canAction)
        //{
        //    RelayCommand s = new RelayCommand(action, canAction);
        //    return s;
        //}
    }

}
