using System;
using System.Collections;
using System.Net.Sockets;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using UnityEngine;

public class PythonConnector : MonoBehaviour
{
    private TcpClient client;
    private Stream stream;
    private byte[] data;
    
    SerialPort serialPort = new SerialPort("COM8", 9600);
    void Start()
    {
        // Ensure the main thread dispatcher is initialized on the main thread
        UnityMainThreadDispatcher.Instance().Enqueue(() => {});
        
        ThreadStart ts = new ThreadStart(ConnectToServer);
        Thread t = new Thread(ts);
        t.Start();

        if (!serialPort.IsOpen) {
            serialPort.Open();
        }
    }
    
    private void ConnectToServer()
    {
        try
        {
            Debug.Log("Attempting to connect to server...");
            client = new TcpClient("127.0.0.1", 3850);
            stream = client.GetStream();
            data = new byte[1024]; // Adjust buffer size as needed

            Debug.Log("Connected to server. Starting data loop...");

            // Continue reading data from the stream in a loop
            while (client.Connected)
            {
                int bytes = stream.Read(data, 0, data.Length);
                if (bytes == 0)
                    break; // If the server closes the connection, exit the loop

                string response = Encoding.UTF8.GetString(data, 0, bytes);
                response = response.TrimEnd('\n'); // Remove any trailing newline characters
                    
                // Dispatch to the main thread
                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    Debug.Log("Received: " + response);
                    ProcessReceivedData(response);
                });
            }
        }
        catch (Exception e)
        {
            // Now we check for null because we're on a background thread
            if (UnityMainThreadDispatcher.Instance() != null)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(() => 
                {
                    Debug.LogError("Socket error: " + e.Message);
                });
            }
        }
        finally
        {
            // Clean up the resources
            Debug.Log("Closing connection to server...");
            stream?.Close();
            client?.Close();
        }
    }

    void Update() {
        if (serialPort.IsOpen && serialPort.BytesToRead > 0) {
            string data = serialPort.ReadLine();
            Debug.Log("Received data: " + data);
            ProcessReceivedData(data);
        }
    }

    void OnDestroy() {
        if (serialPort != null && serialPort.IsOpen) {
            serialPort.Close();
        }
    }

    void ProcessReceivedData(string data)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            GameObject astronaut = GameObject.Find("Stylized Astronaut");
            if (astronaut != null)
            {
                AstronautController astronautController = astronaut.GetComponent<AstronautController>();
                if (astronautController != null)
                {
                    astronautController.RotateAstronaut(data.Trim());
                }
                else
                {
                    Debug.LogError("No AstronautController attached to the Player GameObject.");
                }
            }
            else
            {
                Debug.LogError("No GameObject named 'Stylized Astronaut' found in the scene.");
            }
        });
    }
}