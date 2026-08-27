using System;
using System.IO.Ports;

namespace Next.Services;

public class Reader
{
    private SerialPort SerialPort { get; set; }
    private int BaudRate { get; set; }

    public Reader()
    {
        StartConnection();
    }
    public void StartConnection()
    {
        GetPorts();
        SerialPort = new SerialPort("COM3", BaudRate);
        SerialPort.DataReceived += ReceiveData;

        try
        {
            SerialPort.Open();
            System.Console.WriteLine("USB Connectado");
        } catch (Exception e ) { System.Console.WriteLine($"Erro ao abrir a porta {e.Message}"); }
    }

    private void ReceiveData(object sender, SerialDataReceivedEventArgs e)
    {
        string data = SerialPort.ReadLine();
        System.Console.WriteLine(data);
    }

    private void GetPorts()
    {
        var portList = SerialPort.GetPortNames();
        foreach (var item in portList)
        {
            System.Console.WriteLine(item);
            
        }
    }
}