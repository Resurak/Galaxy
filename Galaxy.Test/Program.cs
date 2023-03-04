using Galaxy.Commons;
using Galaxy.Sync;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

var jsonFile = Path.Combine(desktop, "jsonTest.json");
var root = @"C:\Program Files (x86)\Steam\steamapps";

var dt = DateTime.Now;

var share = new SyncShare(root, "Placeholder");
share.Updated += Share_Updated;

dt = DateTime.Now;
share.Update();

void Share_Updated()
{
    Log.Information("share updated in {ms}", (DateTime.Now - dt).TotalMilliseconds);
    var json = JsonConvert.SerializeObject(share, Formatting.Indented);

    if (File.Exists(jsonFile))
        File.Delete(jsonFile);

    File.WriteAllText(jsonFile, json);
}

//var storage = new SyncStorage(root);

//void Storage_UpdateCompleted()
//{
//    var json = JsonConvert.SerializeObject(storage, Formatting.Indented);

//    File.Delete(jsonFile);
//    File.WriteAllText(jsonFile, json);

//    Log.Information("Saved");
//}

//storage.Update();

Console.ReadLine();

class Test
{
    public int Age { get; set; }
    public string Name { get; set; }
}