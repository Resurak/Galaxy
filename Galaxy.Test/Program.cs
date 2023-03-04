using Galaxy.Commons;
using Galaxy.Sync;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;
using System.Text;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Logging started".AddCaller());

var client = new GalaxyClient();
var server = new GalaxyServer();

var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
var line = Console.ReadLine();

if (line == "client")
{
    client.Connected += Client_Connected;
    client.Disconnected += Client_Disconnected;
    client.DataReceived += Client_DataReceived;

    client.Connect();

    while (true)
    {
        await Task.Delay(1000);
    }
}
else
{
    server.Connected += Client_Connected;
    server.Disconnected += Client_Disconnected;
    server.DataReceived += Client_DataReceived;
    server.Started += Server_Started;
    server.Stopped += Server_Stopped;

    server.StartServer();

    while (true)
    {
        await Task.Delay(1000);
    }
}

void Server_Stopped()
{
    Log.Information("Server stopped".AddCaller());
}

async void Server_Started()
{
    Log.Information("Server started".AddCaller());

    while (server.ServerStatus == ServerStatus.Running)
    {
        line = Console.ReadLine();
        if (line == "stop")
        {
            server.Stop();
            break;
        }

        if (line == "close")
        {
            Log.Information("Closing app...");
            Console.ReadLine();

            Environment.Exit(0);
        }

        Log.Information("sending");
        await server.SendDataAsync(Encoding.UTF8.GetBytes(line ?? ""));
    }
}

void Client_DataReceived(byte[] data)
{
    var txt = Encoding.UTF8.GetString(data);
    Log.Information(txt.AddCaller());
}

void Client_Disconnected()
{
    Log.Information("Client disconnected".AddCaller());
    Log.Information("Closing app...");
    Console.ReadLine();

    Environment.Exit(0);
}

async void Client_Connected()
{
    Log.Information("Client connected".AddCaller());

    while (client.ConnectionStatus == ConnectionStatus.Connected)
    {
        line = "test " + new Random().Next(1, 5000);
        line = Console.ReadLine();
        if (line == "stop")
        {
            client.Disconnect();
            break;
        }

        if (line == "close")
        {
            Log.Information("Closing app...");
            Console.ReadLine();

            Environment.Exit(0);
        }

        //Log.Information("sending");
        await client.SendDataAsync(Encoding.UTF8.GetBytes(line ?? ""));
    }
}

//var jsonFile = Path.Combine(desktop, "jsonTest.json");
//var root = @"C:\Program Files (x86)\Steam\steamapps";

//var dt = DateTime.Now;

//var share = new SyncShare(root, "Placeholder");
//share.Updated += Share_Updated;

//dt = DateTime.Now;
//share.Update();

//void Share_Updated()
//{
//    Log.Information("share updated in {ms}", (DateTime.Now - dt).TotalMilliseconds);
//    var json = JsonConvert.SerializeObject(share, Formatting.Indented);

//    if (File.Exists(jsonFile))
//        File.Delete(jsonFile);

//    File.WriteAllText(jsonFile, json);
//}

Console.ReadLine();

class Test
{
    public int Age { get; set; }
    public string Name { get; set; }
}