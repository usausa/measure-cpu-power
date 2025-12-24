// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable MemberCanBeProtected.Global
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
namespace CpuPower;

using System;

using LibreHardwareMonitor.Hardware;

using Smart.CommandLine.Hosting;

public sealed class RootCommandHandler : ICommandHandler
{
    [Option<int>("--loop", "-l", Description = "Loop count", DefaultValue = 10)]
    public int Loop { get; set; }

    [Option<int>("--interval", "-i", Description = "Loop count", DefaultValue = 1000)]
    public int Interval { get; set; }

    [Option("--progress", "-p", Description = "Show progress")]
    public bool Progress { get; set; }

    public async ValueTask ExecuteAsync(CommandContext context)
    {
        var table = new float?[Loop];

        var visitor = new UpdateVisitor();
        var computer = new Computer
        {
            IsCpuEnabled = true
        };
        computer.Open();
        computer.Accept(visitor);

        var cpu = computer.Hardware.SelectMany(EnumerableHardware).FirstOrDefault(static x => x.HardwareType == HardwareType.Cpu);
        if (cpu is null)
        {
            return;
        }

        Console.WriteLine(cpu.Name);

        for (var i = 0; i < Loop; i++)
        {
            await Task.Delay(Interval);

            computer.Accept(visitor);
            var sensor = EnumerableSensors(cpu)
                .Where(static x => x.SensorType == SensorType.Power)
                .FirstOrDefault(static x => x.Name.Contains("Package", StringComparison.OrdinalIgnoreCase));
            table[i] = sensor?.Value;

            if (Progress)
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss}: {table[i]:F2}");
            }
        }

        Console.WriteLine($"Min: {table.Min():F2}");
        Console.WriteLine($"Max: {table.Max():F2}");
        Console.WriteLine($"Avg: {table.Average():F2}");
    }

    private static IEnumerable<IHardware> EnumerableHardware(IHardware hardware)
    {
        yield return hardware;

        foreach (var subHardware in hardware.SubHardware)
        {
            yield return subHardware;
        }
    }

    private static IEnumerable<ISensor> EnumerableSensors(IHardware hardware)
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
}
