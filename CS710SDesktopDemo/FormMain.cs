﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using CSLibrary;

namespace CS108DesktopDemo
{
    public partial class FormMain : Form
    {
        HighLevelInterface _reader = new HighLevelInterface();
        private System.Collections.Generic.SortedDictionary<string, int> TagInfoListSpeedup = new SortedDictionary<string, int>();


        public FormMain()
        {
            InitializeComponent();

            CSLibrary.DeviceFinder.OnSearchCompleted += DeviceWatcher_Added;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count < 1)
                System.Console.WriteLine("Please select reader first!");
            else
            {
                textBox3.Text = "Please wait, connecting..." + Environment.NewLine;
                _reader.rfid.OnStateChanged += new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);
                _reader.ConnectAsync(CSLibrary.DeviceFinder.GetDeviceInformation(listView1.SelectedIndices[0]), RFIDDEVICE.MODEL.CS710S);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listView1.Clear();
            CSLibrary.DeviceFinder.SearchDevice();
        }

        private async void DeviceWatcher_Added(object sender, object deviceInfo)
        {
            // We must update the collection on the UI thread because the collection is databound to a UI element.
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                lock (this)
                {
                    CSLibrary.DeviceFinder.DeviceFinderArgs dfa = (CSLibrary.DeviceFinder.DeviceFinderArgs)deviceInfo;
                    CSLibrary.DeviceFinder.DeviceInfomation di = (CSLibrary.DeviceFinder.DeviceInfomation)dfa.Found;
                    //DeviceInformation ndi = (DeviceInformation)di.nativeDeviceInformation;

                    string a = String.Format("Added  {0} {1}", di.ID, di.deviceName);
                    Debug.WriteLine(a);
                    listView1.Items.Add(di.deviceName);

                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                _reader.rfid.SetPowerLevel(int.Parse(textBox_Power.Text));
                _reader.rfid.SetDuplicateEliminationRollingWindow(uint.Parse(textBox_DuplicateWindow.Text));
            }
            catch (Exception ex)
            {
                MessageBox.Show ("Power value error: set power to 300 now");
                _reader.rfid.SetPowerLevel(300);
            }

            _reader.rfid.OnAsyncCallback += new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
            _reader.rfid.Options.TagRanging.flags = 0;
            _reader.rfid.StartOperation(CSLibrary.Constants.Operation.TAG_RANGING);
        }

        void StateChangedEvent(object sender, CSLibrary.Events.OnStateChangedEventArgs e)
        {
            if (e.state == CSLibrary.Constants.RFState.INITIALIZATION_COMPLETE)
            {
                textBox3.Text += "Connected" + Environment.NewLine;
            }
        }


        void TagInventoryEvent(object sender, CSLibrary.Events.OnAsyncCallbackEventArgs e)
        {
            if (e.type != CSLibrary.Constants.CallbackType.TAG_RANGING)
                return;

            try
            {
                var EPC = e.info.epc.ToString();

                TagInfoListSpeedup.Add(EPC, TagInfoListSpeedup.Count);

                BeginInvoke(new Action(() =>
                {
                    try
                    {
                        textBox2.Text += EPC + Environment.NewLine;
                    }
                    catch (Exception ex)
                    {
                    }
                }));
            }
            catch (Exception ex)
            {
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            _reader.rfid.StopOperation();
            _reader.rfid.OnAsyncCallback -= new EventHandler<CSLibrary.Events.OnAsyncCallbackEventArgs>(TagInventoryEvent);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            _reader.rfid.OnStateChanged -= new EventHandler<CSLibrary.Events.OnStateChangedEventArgs>(StateChangedEvent);
            _reader.DisconnectAsync();
            textBox3.Text += "Please wait to disconnect CS108, BT led will change to flash" + Environment.NewLine;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox2.Text = "";
            TagInfoListSpeedup.Clear();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            //button5_Click(null, null);
            //Thread.Sleep(5000);
            CSLibrary.DeviceFinder.Disconnect();
            Application.DoEvents();
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
        }

        private void FormMain_Load(object sender, EventArgs e)
        {

        }
    }
}
