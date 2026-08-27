using System;
using System.IO.Ports;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Next.Services;

public class Reader
{
    private SerialPort SerialPort { get; set; }
    private int BaudRate { get; set; } = 115200;
    
    public Reader()
    {
        StartConnection();
    }
    public void StartConnection()
    {
        GetConnectionWithDevice();
        SerialPort = new SerialPort("COM3", BaudRate);
        SerialPort.DataReceived += ReceiveData;

        try
        {
            SerialPort.Open();
            App.Logger.WriteLine("USB Connectado");
        } catch (Exception e ) { App.Logger.WriteLine($"Erro ao abrir a porta: {e.Message}"); }
    }

    private void ReceiveData(object sender, SerialDataReceivedEventArgs e)
    {
        string data = SerialPort.ReadLine();
        App.Logger.WriteLine(data);
    }

    private bool GetConnectionWithDevice()
    {
        var portList = SerialPort.GetPortNames();
        
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

                testPort.Open();

                testPort.WriteLine("next_module");
                Thread.Sleep(100);

                string response = testPort.ReadLine().Trim();

                if(response == "next_module")
                {
                    SerialPort = testPort;
                    App.Logger.WriteLine($"Connection established in port: {port}");
                    return true;
                }

            }catch (TimeoutException)
            {
                App.Logger.WriteLine($"Timeout exception ({port})");
            }catch (Exception e)
            {
                App.Logger.WriteLine($"Error on conect to {port}: {e.Message}");
                
            }
        }

        App.Logger.WriteLine($"The device is not founded.");
        return false;
    }
}