using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System;
using TMPro;
using System.Text;

public class HttpServer : MonoBehaviour
{
    private HttpListener httpListener;
    private LeaderboardManager lbManager;
    // private LeaderboardAPI lbManager;
    public TMP_Text ipAddress;
    private string localIPAddress;

    private void Awake() {
        // Find the LeaderboardManager component in the scene
        lbManager = FindObjectOfType<LeaderboardManager>();
        // lbManager = FindObjectOfType<LeaderboardAPI>();
    }

    private void Start()
    {
        localIPAddress = GetLocalIPAddress();
        //localIPAddress = "0.0.0.0";
        Debug.Log("Local IP Address: " + localIPAddress);
        ipAddress.text = localIPAddress;
        StartServer();
    }

    private string GetLocalIPAddress()
    {
        string localIP = "";
        var host = Dns.GetHostEntry(Dns.GetHostName());
        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            {
                localIP = ip.ToString();
                break; // Exit loop after finding the first valid IP
            }
        }
        return localIP;
    }

    private async void StartServer()
    {
        httpListener = new HttpListener();
        httpListener.Prefixes.Add($"http://{localIPAddress}:12345/"); // Listen on dynamic local IP
        httpListener.Start();
        Debug.Log("HTTP Server started.");

        while (true)
        {
            try
            {
                // Wait for an incoming request
                HttpListenerContext context = await httpListener.GetContextAsync();
                ProcessRequest(context);
            }
            catch (HttpListenerException ex)
            {
                Debug.LogError("HttpListener error: " + ex.Message);
            }
        }
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        try
        {
            // Read the request data
            string jsonData = string.Empty;
            if (context.Request.ContentLength64 > 0)
            {
                using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
                {
                    jsonData = reader.ReadToEnd();
                }
            }

            // Print received data to the console
            Debug.Log("Received JSON Data: " + jsonData);
            lbManager.UpdateLeaderboard(jsonData);

            // Store received JSON data in the singleton
            Singleton.Instance.jsonData = jsonData;

            // Prepare the response entity
            string responseEntity = "{\"status\":\"success\"}";
            byte[] responseBytes = Encoding.UTF8.GetBytes(responseEntity);

            // Send a response back to the client
            context.Response.StatusCode = 200; // OK
            context.Response.ContentType = "application/json"; // Set ContentType

            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(responseEntity); // Write response
                writer.Flush(); // Ensure data is sent
            }

            // Close the response with the desired blocking behavior
            context.Response.Close(responseBytes, false); // Non-blocking close
        }
        catch (Exception ex)
        {
            Debug.LogError("Error processing request: " + ex.Message);
            context.Response.StatusCode = 500; // Internal Server Error
            string errorResponse = "{\"status\":\"error\",\"message\":\"" + ex.Message + "\"}";
            byte[] errorResponseBytes = Encoding.UTF8.GetBytes(errorResponse);

            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(errorResponse); // Send error response
                writer.Flush(); // Ensure data is sent
            }

            // Close the response with blocking if necessary
            context.Response.Close(errorResponseBytes, false); // Non-blocking close
        }
        finally
        {
            // Ensure the response is closed, but you may not need this if already closing in catch
            context.Response.Close();
        }
    }


    private void OnDestroy()
    {
        httpListener.Stop(); // Stop the listener when the object is destroyed
        Debug.Log("HTTP Server stopped.");
    }
}
