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
        // SerialPort.DataReceived += ReceiveData;

        // try
        // {
        //     SerialPort.Open();
        //     App.Logger.WriteLine("USB Connectado");
        // } catch (Exception e ) { App.Logger.WriteLine($"Erro ao abrir a porta: {e.Message}"); }
    }

    private void ReceiveData(object sender, SerialDataReceivedEventArgs e)
    {
        string data = SerialPort.ReadLine();
        App.Logger.WriteLine(data);
    }

    private bool GetConnectionWithDevice()
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