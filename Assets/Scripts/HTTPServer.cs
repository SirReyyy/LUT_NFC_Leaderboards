using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using System.IO;
using System;

public class HttpServer : MonoBehaviour
{
    private HttpListener httpListener;
    private LeaderboardManager lbManager;
    private string localIPAddress;

    private void Awake() {
        // Find the LeaderboardManager component in the scene
        lbManager = FindObjectOfType<LeaderboardManager>();
    }

    private void Start()
    {
        localIPAddress = GetLocalIPAddress();
        Debug.Log("Local IP Address: " + localIPAddress);
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
            string jsonData;
            using (var reader = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding))
            {
                jsonData = reader.ReadToEnd();
            }

            // Print received data to the console
            Debug.Log("Received JSON Data: " + jsonData);
            lbManager.UpdateLeaderboard(jsonData);

            // Store received JSON data in the singleton
            Singleton.Instance.jsonData = jsonData;

            // Send a response back to the client
            context.Response.StatusCode = 200; // OK
            context.Response.ContentType = "application/json"; // Set ContentType

            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write("{\"status\":\"success\"}"); // Example response
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Error processing request: " + ex.Message);
            context.Response.StatusCode = 500; // Internal Server Error
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write("{\"status\":\"error\",\"message\":\"" + ex.Message + "\"}"); // Send error response
            }
        }
        finally
        {
            context.Response.Close();
        }
    }

    private void OnDestroy()
    {
        httpListener.Stop(); // Stop the listener when the object is destroyed
        Debug.Log("HTTP Server stopped.");
    }
}
