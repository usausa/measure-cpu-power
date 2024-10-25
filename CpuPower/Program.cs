using System.CommandLine;
using System.CommandLine.NamingConventionBinder;

using CpuPower;

using LibreHardwareMonitor.Hardware;

var rootCommand = new RootCommand("CPU power measurement tool");
rootCommand.AddOption(new Option<int>(["--loop", "-l"], () => 10, "Loop count"));
rootCommand.AddOption(new Option<int>(["--interval", "-i"], () => 1000, "Interval ms"));
rootCommand.AddOption(new Option<bool>(["--progress", "-p"], "Show progress"));
rootCommand.Handler = CommandHandler.Create(RootCommandHandler);

return await rootCommand.InvokeAsync(args).ConfigureAwait(false);

static void RootCommandHandler(
    int loop,
    int interval,
    bool progress)
{
    var table = new float?[loop];

    var visitor = new UpdateVisitor();
    var computer = new Computer
    {
        IsCpuEnabled = true
    };
    computer.Open();
    computer.Accept(visitor);

    var cpu = computer.Hardware.SelectMany(EnumerableHardware).FirstOrDefault(x => x.HardwareType == HardwareType.Cpu);
    if (cpu is null)
    {
        return;
    }

    Console.WriteLine(cpu.Name);

    for (var i = 0; i < loop; i++)
    {
        Thread.Sleep(interval);

        computer.Accept(visitor);
        var sensor = EnumerableSensors(cpu)
            .Where(x => x.SensorType == SensorType.Power)
            .FirstOrDefault(x => x.Name.Contains("Package", StringComparison.OrdinalIgnoreCase));
        table[i] = sensor?.Value;

        if (progress)
        {
            Console.WriteLine($"{DateTime.Now:HH:mm:ss}: {table[i]:F2}");
        }
    }

    Console.WriteLine($"Min: {table.Min():F2}");
    Console.WriteLine($"Max: {table.Max():F2}");
    Console.WriteLine($"Avg: {table.Average():F2}");
}

static IEnumerable<IHardware> EnumerableHardware(IHardware hardware)
{
    yield return hardware;

    foreach (var subHardware in hardware.SubHardware)
    {
        yield return subHardware;
    }
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
