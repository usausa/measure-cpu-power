using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

using CpuPower;

using LibreHardwareMonitor.Hardware;

var rootCommand = new RootCommand("CPU power measurement tool");
rootCommand.AddOption(new Option<int>(["--loop", "-l"], () => 10, "Loop"));
rootCommand.AddOption(new Option<int>(["--interval", "-i"], () => 1000, "Loop"));
rootCommand.Handler = CommandHandler.Create(RootCommandHandler);

return await rootCommand.InvokeAsync(args).ConfigureAwait(false);

static void RootCommandHandler(
    int loop,
    int interval)
{
    var table = new float?[loop];

    var visitor = new UpdateVisitor();
    var computer = new Computer()
    {
        IsCpuEnabled = true
    };
    computer.Open();
    computer.Accept(visitor);

    for (var i = 0; i < loop; i++)
    {
        Thread.Sleep(interval);

        computer.Accept(visitor);
        var sensor = computer.Hardware
            .SelectMany(EnumerableSensors)
            .Where(x => (x.Hardware.HardwareType == HardwareType.Cpu) && (x.SensorType == SensorType.Power))
            .FirstOrDefault(x => x.Name.Contains("Package", StringComparison.OrdinalIgnoreCase));
        table[i] = sensor?.Value;

        // TODO current
    }

    // TODO
    Console.WriteLine($"Min: {table.Min():F2}");
    Console.WriteLine($"Max: {table.Max():F2}");
}

static IEnumerable<ISensor> EnumerableSensors(IHardware hardware)
{
    foreach (var subHardware in hardware.SubHardware)
    {
        foreach (var sensor in EnumerableSensors(subHardware))
        {
            yield return sensor;
        }
    }

    foreach (var sensor in hardware.Sensors)
    {
        yield return sensor;
    }
}
