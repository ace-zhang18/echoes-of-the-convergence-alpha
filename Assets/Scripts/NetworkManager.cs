using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public Queue<byte[]> buffer;
    Boolean game_active = true;
    [SerializeField] private GameObject rocket;
    // Start is called before the first frame update
    void Start()
    {
        //NOTE: Opening a socket will cause the current process to hang
        //When starting a networking task, start it in a new thread
        Task.Factory.StartNew(StartServer, TaskCreationOptions.LongRunning);
        buffer = new Queue<byte[]>();
    }

    // Update is called once per frame
    void Update()
    {
        while(buffer.Count > 0)
        {
            byte[] rec = buffer.Dequeue();
            byte[] x_ba = new byte[sizeof(float)];
            byte[] y_ba = new byte[sizeof(float)];
            Array.Copy(rec, 0, x_ba, 0, sizeof(float));
            Array.Copy(rec, sizeof(float), y_ba, 0, sizeof(float));
            float x = BitConverter.ToSingle(x_ba);
            float y = BitConverter.ToSingle(y_ba);
            rocket.transform.position = new Vector3(x, y);
        }
    }

    private void OnApplicationQuit()
    {
        game_active = false;
    }

    public void StartServer()
    {
        
        // Get Host IP Address that is used to establish a connection
        // In this case, we get one IP address of localhost that is IP : 127.0.0.1
        // If a host has multiple addresses, you will get a list of addresses
        IPAddress ipAddress = new IPAddress(new byte[] { 127, 0, 0, 1 });
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);
        
        try
        {
            
            // Create a Socket that will use Tcp protocol
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // A Socket must be associated with an endpoint using the Bind method
            listener.Bind(localEndPoint);
            // Specify how many requests a Socket can listen before it gives Server busy response.
            // We will listen 10 requests at a time
            listener.Listen(10);
            
            Debug.Log("Waiting for a connection...");
            
            Socket handler = listener.Accept();

            // Incoming data from the client.
            string data = null;
            byte[] bytes = null;
            
            while (game_active)
            {
                bytes = new byte[1024];
                int bytesRec = handler.Receive(bytes); //listening for a response will also cause the current process to hang until it receives a response
                byte[] to_buf = new byte[bytesRec];
                Array.Copy(bytes, 0, to_buf, 0, bytesRec);
                buffer.Enqueue(to_buf);
            }
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
            
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
        
        Debug.Log("\n Press any key to continue...");
    }
}
