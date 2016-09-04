using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;




// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Wykres
{
   public sealed partial class MainPage : Page
    {

        private Uart uart = new Uart();
        private Parse block;
        private int temp=0;
        private int quantitiyOfData=0;      
        public DispatcherTimer refreshTimer;
        private CanvasDrawingSession cds;
        private CanvasControl controlOfCanvas;
        public MainPage()
        {
            this.InitializeComponent();
            initAsync();
        }
        public Stopwatch watch= new Stopwatch();
      
        private async void initAsync()
        {
            disconnectButton.IsEnabled = false;
            baudRate_text.IsEnabled = true;
            RefreshButtons();
            var listOfDevices=  await uart.ListAvailablePorts();
            status.Text = "załadowano liste";
            uart.onBufferUpdateEvent += Uart_onBufferUpdateEvent;              
            DeviceListSource.Source = listOfDevices;
            if (comboBox.Items.Count > 0)
            {
                comboBox.SelectedIndex = 1;                             
            }
         
            refreshTimer = new DispatcherTimer();
            refreshTimer.Interval = TimeSpan.FromMilliseconds((1/34)*1000);
            refreshTimer.Tick += RefreshTimer_Tick;
            refreshTimer.Start();
            
           
            block = new Parse();
            watch.Start();
            block.onBufferFullEvent += Block_onBufferFullEvent;            
        }

        private void Block_onBufferFullEvent(object sender)
        {
            if (block.list.Count > 2500)
            {
                //Task.Run(() => block.list.RemoveRange(0, 10));    
            }
          
           // block.list.RemoveRange(0, 25);

        }

        private void RefreshTimer_Tick(object sender, object e)
        {
            System.Diagnostics.Debug.WriteLine("Ilość w kolejce=" + block.list.Count + " CZAS OD POCZĄTKU= " + watch.ElapsedMilliseconds);
            float temp = (float)block.list.Count / (float)5000;            
            temp *= 100;
            System.Diagnostics.Debug.WriteLine("Ilość w kolejce=" + temp);
            progress.Value = temp;
            status.Text = block.list.Count.ToString(); 
            if (controlOfCanvas != null && temp>1) controlOfCanvas.Invalidate();
        }
            
         

        private async Task Uart_onBufferUpdateEvent()
        {
                var buffer = uart.buffer;
                quantitiyOfData++;
                try
                {
                    var test = buffer.LastIndexOf('<');
                    buffer = buffer.Substring(buffer.IndexOf('<'), buffer.LastIndexOf('<') - buffer.IndexOf('<'));
                    await block.doParse(buffer, new char[] { '\n' });                   
                }
                catch (Exception ex)
                {
                    var exe = ex.Message;
                }           
        }

        private void RefreshButtons()
        {
            Int32 baudRate;
            if (Int32.TryParse(baudRate_text.Text, out baudRate))
                if (baudRate % 2 == 0)
                {
                    connectButtton.IsEnabled = true;
                }
            else { connectButtton.IsEnabled = false; }
        }

        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshButtons();
        }

        private async void connectButtton_Click(object sender, RoutedEventArgs e)
        {
            var selection = comboBox.SelectedItem;
            DeviceInformation entry = (DeviceInformation)selection;
            Task frist=uart.connectAsync(entry);
            connectButtton.IsEnabled = false;
            disconnectButton.IsEnabled = true;
            await frist;
            watch.Start();

        }

        private void disconnect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                uart.CancelReadTask();
                uart.CloseDevice();
                initAsync();
            }
            catch (Exception ex)
            {
              
            }
        }

        private void mySurface_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            controlOfCanvas = (CanvasControl)sender;
        }

        private void mySurface_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            cds = args.DrawingSession;
            int j = 1;
                if(block.list.Count > 5000)
                {
                    for (int i = block.list.Count - 1000; i < block.list.Count - 1; i++)
                    {
                         cds.DrawLine(j - 1, block.list[i].deg / 2, j, block.list[i + 1].deg / 2, Windows.UI.Colors.White);
                         cds.DrawLine(j - 1, block.list[i].sin * 50 + 75,  j, block.list[i + 1].sin * 50 + 75, Windows.UI.Colors.Red);
                         cds.DrawLine(j - 1, block.list[i].cos * 50 + 75,  j, block.list[i + 1].cos * 50 + 75, Windows.UI.Colors.Blue);
                         j++;
                    }
            }
                
                //temp +=1;
            
            
            
        }
    }
}
