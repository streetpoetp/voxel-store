using System.Text.Json;
using VoxelStore.Tool;

var json = new JsonSerializerOptions { WriteIndented = true };
try
{
    string command = args.Length == 0 ? "scan" : args[0];
    string? rootArg = null;
    bool write = args.Contains("--write", StringComparer.Ordinal);
    int rootIndex = Array.IndexOf(args, "--root");
    if (rootIndex >= 0 && rootIndex + 1 < args.Length)
        rootArg = args[rootIndex + 1];
    string root = StoreEngine.FindRoot(rootArg);
    var engine = new StoreEngine(root);

    switch (command)
    {
        case "scan":
            {
                StoreSnapshot snapshot = engine.Scan(writeDerived: write, writeCompatibility: write);
                Console.WriteLine(JsonSerializer.Serialize(snapshot, json));
                return 0;
            }
        case "verify":
            {
                StoreSnapshot snapshot = engine.Scan(writeDerived: false, writeCompatibility: false);
                Console.WriteLine(JsonSerializer.Serialize(snapshot, json));
                return snapshot.Healthy ? 0 : 2;
            }
        case "plan":
            {
                StoreSnapshot snapshot = engine.Scan(writeDerived: false, writeCompatibility: false);
                Console.WriteLine(JsonSerializer.Serialize(snapshot.NextJob, json));
                return 0;
            }
        case "generate":
            {
                if (args.Length < 2)
                    throw new ArgumentException("generate requires <asset-id>.");
                engine.Generate(args[1]);
                Console.WriteLine($"generated {args[1]}");
                return 0;
            }
        case "seal":
            {
                if (args.Length < 2)
                    throw new ArgumentException("seal requires <asset-id>.");
                engine.Seal(args[1]);
                Console.WriteLine($"sealed {args[1]}");
                return 0;
            }
        case "density":
            {
                if (args.Length < 2)
                    throw new ArgumentException("density requires <asset-id>.");
                ReferenceDensityEvidence evidence = engine.SelectDensity(args[1], write);
                Console.WriteLine(JsonSerializer.Serialize(evidence, json));
                return evidence.SelectedVpu.HasValue ? 0 : 2;
            }
        case "provider":
            {
                Console.WriteLine(JsonSerializer.Serialize(engine.ResolveProvider(), json));
                return 0;
            }
        default:
            throw new ArgumentException($"Unknown command '{command}'.");
    }
}
catch (Exception ex)
{
    Console.Error.WriteLine(ex.ToString());
    return 1;
}
