using System;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

namespace Next.Services;

public partial class Reader
{
    private SerialPort? SerialPort { get; set; } = null;
    private int BaudRate { get; set; } = 115200;
    private CancellationTokenSource? _cancellationTokenSource;

    public Reader()
    {
        System.Console.WriteLine("Creating Reader service");
        SerialPort = new SerialPort("/dev/ttyUSB0", BaudRate);
        SerialPort.ReadTimeout = 500;
        SerialPort.WriteTimeout = 500;
        SerialPort.Open();

        StartListening();
        System.Console.WriteLine("Created Reader service");
    }

    public SerialPort? GetSerialPort()
        => SerialPort;

    public void StartListening()
    {
        try
        {
            Console.WriteLine($"Starting to listen on port: {SerialPort?.PortName}");
            // if (SerialPort is null || !SerialPort.IsOpen)
            // {
            //     Console.WriteLine($"Serial port is not open. {SerialPort}/{SerialPort?.IsOpen}");
            //     return;
            // }

            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Run(() => ReadLoop(_cancellationTokenSource.Token));

            Console.WriteLine($"Listening on port: {SerialPort.PortName} in background task.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error starting to listen: {e.Message}");
        }
    }

    private void ReadLoop(CancellationToken token)
    {
        Console.WriteLine("Initializing loop");
        while (!token.IsCancellationRequested && SerialPort != null && SerialPort.IsOpen)
        {
            try
            {
                if (SerialPort.BytesToRead > 0)
                {
                    string data = SerialPort.ReadExisting();
                    if (!string.IsNullOrEmpty(data))
                    {
                        Console.WriteLine(data);
                    }
                }

                // Pausa muito curta para evitar que o loop consuma 100% da CPU (Busy Wait)
                Thread.Sleep(10);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Erro no loop de leitura da Serial]: {ex.Message}");
                break; // Sai do loop em caso de erro crítico na porta
            }
        }
        Console.WriteLine($"{token.IsCancellationRequested} && {SerialPort != null} && {SerialPort.IsOpen}");
    }

    public void SendData(string value)
    {
        if (SerialPort == null || !SerialPort.IsOpen)
        {
            Console.WriteLine($"Serial port is not open. {SerialPort}/{SerialPort?.IsOpen}");

            return;
        }
        SerialPort.WriteLine(value);
    }

    public bool GetConnectionWithDevice()
    {
        Console.WriteLine("Trying to connect device");
        var portList = SerialPort.GetPortNames();
        foreach (string port in portList)
        {
            Console.WriteLine(port);
        }
        foreach (var port in portList)
        {
            Console.WriteLine($"Trying connect to port: {port}");
            try
            {
                System.Console.WriteLine($"Trying connect to port: {port}");
                SerialPort testPort = new(port, BaudRate)
                {
                    ReadTimeout = 500,
                    WriteTimeout = 500
                };
                Console.WriteLine("  Trying to open port");

                testPort.Open();


                Console.WriteLine("  Trying to send command");
                testPort.WriteLine("next_module");

                Console.WriteLine("  Waiting for response");
                Thread.Sleep(100);

                string response = testPort.ReadLine().Trim();
                Console.WriteLine("  Response: " + response);

                if (response == "next_module")
                {
                    SerialPort = testPort;
                    Console.WriteLine($"Connection established in port: {port}");
                    return true;
                }

            }
            catch (TimeoutException)
            {
                Console.WriteLine($"ERROR: Timeout exception ({port})");
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR: on conect to {port}: {e.Message}");

            }
        }

        Console.WriteLine($"The device is not founded.");
        return false;
    }
}