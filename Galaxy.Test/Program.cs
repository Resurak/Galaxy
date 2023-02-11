using Galaxy.Commons;
using Galaxy.Sync;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

Log.Information("Creating folder data");
var root = @"C:\Program Files (x86)\Steam\steamapps\workshop\content";

var sw = Stopwatch.StartNew();

var folder = new FolderData();
await folder.Create(root);

sw.Stop();

Log.Information("created Folder in {sw}", sw.ElapsedMilliseconds);
var paths = new string[] { "544550", "2822613339", "About", "About.xml" };

sw.Restart();
var file = folder[paths];

//await foreach ()

sw.Stop();
if (file != null)
{
    Log.Information("File found in {sw}\n{@json}", sw.ElapsedMilliseconds, file);
}
else
{
    Log.Warning("File not found");
}

Console.ReadLine();

class Test
{
    public int Age { get; set; }
    public string Name { get; set; }
}