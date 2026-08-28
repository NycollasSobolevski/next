using System;
using System.IO.Ports;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Next.Services;

public class Reader
{
    private SerialPort SerialPort { get; set; } = null!;
    private int BaudRate { get; set; } = 115200;


    public void StartListening()
    {
        if(SerialPort == null || !SerialPort.IsOpen) return;
        SerialPort.DataReceived += ReceiveData;
    }

    public void ReceiveData(object sender, SerialDataReceivedEventArgs e)
    {
        string data = SerialPort.ReadLine();
        App.Logger.WriteLine(data);
    }

    public void SendData(string value)
    {
        if(SerialPort == null || !SerialPort.IsOpen) {
            App.Logger.WriteLine($"Serial port is not open. {SerialPort}/{SerialPort?.IsOpen}");
            
            return;
        }
        SerialPort.WriteLine(value);
    }

    public  bool GetConnectionWithDevice()
    { 
        App.Logger.WriteLine("Trying to connect device");
        var portList = SerialPort.GetPortNames();
        foreach(string port in portList)
        {
            App.Logger.WriteLine(port);
        }
        foreach (var port in portList)
        {
            App.Logger.WriteLine($"Trying connect to port: {port}");
            try
            {
                SerialPort testPort = new(port, BaudRate)
                {
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };
                App.Logger.WriteLine("  Trying to open port");

                testPort.Open();


                App.Logger.WriteLine("  Trying to send command");
                testPort.WriteLine("next_module");

                App.Logger.WriteLine("  Waiting for response");
                Thread.Sleep(100);

                string response = testPort.ReadLine().Trim();
                App.Logger.WriteLine("  Response: " + response);

                if(response == "next_module")
                {
                    SerialPort = testPort;
                    App.Logger.WriteLine($"Connection established in port: {port}");
                    return true;
                }

            }catch (TimeoutException)
            {
                App.Logger.WriteLine($"ERROR: Timeout exception ({port})");
            }catch (Exception e)
            {
                App.Logger.WriteLine($"ERROR: on conect to {port}: {e.Message}");
                
            }
        }

        App.Logger.WriteLine($"The device is not founded.");
        return false;
    }
}