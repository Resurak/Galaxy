using Galaxy.Commons;
using Galaxy.Sync;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

var jsonFile = @"C:\Users\danie\Desktop\test.json";
var root = @"C:\Program Files (x86)\Steam\steamapps";

var storage = new SyncStorage(root);

void Storage_UpdateCompleted()
{
    var json = JsonConvert.SerializeObject(storage, Formatting.Indented);

    File.Delete(jsonFile);
    File.WriteAllText(jsonFile, json);

    Log.Information("Saved");
}

storage.Update();

Console.ReadLine();

class Test
{
    public int Age { get; set; }
    public string Name { get; set; }
}