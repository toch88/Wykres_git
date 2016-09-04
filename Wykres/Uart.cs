using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SerialCommunication;
using Windows.Storage.Streams;

namespace Wykres
{
    class Uart
    {
        private ObservableCollection<DeviceInformation> listOfDevices = new ObservableCollection<DeviceInformation>();
        private SerialDevice serialPort = null;
        DataReader dataReaderObject = null;
        public CancellationTokenSource ReadCancellationTokenSource;
        public string buffer;
        public delegate Task onBufferUpdateDelegate();
        public event onBufferUpdateDelegate onBufferUpdateEvent;

        public Uart()
        {

        }

        public async Task<ObservableCollection<DeviceInformation>> ListAvailablePorts()
        {
            try
            {
                string aqs = SerialDevice.GetDeviceSelector();
                var dis = await DeviceInformation.FindAllAsync(aqs);
                for (int i = 0; i < dis.Count; i++)
                {
                    listOfDevices.Add(dis[i]);                    
                }
                return listOfDevices;
            }
            catch (Exception ex)
            {
                listOfDevices.Clear();
                return listOfDevices;
            }

            
        }

        private void initSerialPort(SerialDevice serialPort, UInt32 baudRate)
        {
            serialPort.WriteTimeout = TimeSpan.FromMilliseconds(-1);
            serialPort.ReadTimeout = TimeSpan.FromMilliseconds(-1);
            serialPort.BaudRate = (uint)baudRate;
            serialPort.Parity = SerialParity.None;
            serialPort.StopBits = SerialStopBitCount.One;
            serialPort.DataBits = 8;
            serialPort.Handshake = SerialHandshake.None;
        }

        public async Task connectAsync(DeviceInformation entry)
        {
            try
            {
                serialPort = await SerialDevice.FromIdAsync(entry.Id);                
                // Configure serial settings
                initSerialPort(serialPort, 256000);
                // Create cancellation token object to close I/O operations when closing the device
                ReadCancellationTokenSource = new CancellationTokenSource();
                // Enable 'WRITE' button to allow sending data  
                listenAsync();                         
            }
            catch (Exception ex)
            {
                 
            }
        }

        private Object ReadCancelLock = new Object();



        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            Task<UInt32> loadAsyncTask;
            uint ReadBufferLength=50;
           
            lock (ReadCancelLock)
            {
                // If task cancellation was requested, comply
                cancellationToken.ThrowIfCancellationRequested();
                // Set InputStreamOptions to complete the asynchronous read operation when one or more bytes is available
                dataReaderObject.InputStreamOptions = InputStreamOptions.Partial;
                // Create a task object to wait for data on the serialPort.InputStream
                loadAsyncTask = dataReaderObject.LoadAsync(ReadBufferLength).AsTask(cancellationToken);
            }

            // Launch the task and wait
            UInt32 bytesRead = await loadAsyncTask;
           
            if (bytesRead > 0)
            {
                buffer = dataReaderObject.ReadString(bytesRead);
                onBufferUpdateEvent();
            }

        }

        //funkcja wywołująca cały czas równoległą pętlę odczytującą dane
        private async Task listenAsync()
        {
            try
            {
                if (serialPort != null)
                {
                    dataReaderObject = new DataReader(serialPort.InputStream);
                    
                    // keep reading the serial input
                    while (true)
                    {
                        await ReadAsync(ReadCancellationTokenSource.Token);
                    }                    
                }
                
            }
            catch (Exception ex)
            {
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    CloseDevice();
                    
                }
                else
                {
                   
                }
                
            }
            finally
            {
                // Cleanup once complete
                if (dataReaderObject != null)
                {
                    dataReaderObject.DetachStream();
                    dataReaderObject = null;
                }
            }
        }

        public void CloseDevice()
        {
            if (serialPort != null)
            {
                serialPort.Dispose();
            }
            serialPort = null;            
            listOfDevices.Clear();
        }

        public void CancelReadTask()
        {
            if (ReadCancellationTokenSource != null)
            {
                if (!ReadCancellationTokenSource.IsCancellationRequested)
                {
                    ReadCancellationTokenSource.Cancel();
                }
            }
        }
    }
}
