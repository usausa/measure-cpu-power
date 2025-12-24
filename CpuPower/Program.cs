using CpuPower;

using Smart.CommandLine.Hosting;

var builder = CommandHost.CreateBuilder(args);
builder.ConfigureCommands(commands =>
{
    commands.ConfigureRootCommand(root =>
    {
        root.WithDescription("CPU power measurement tool").UseHandler<RootCommandHandler>();
    });
});

var host = builder.Build();
return await host.RunAsync();
