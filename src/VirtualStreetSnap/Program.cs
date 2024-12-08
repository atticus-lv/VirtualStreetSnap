using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace VirtualStreetSnap;

sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        Trace.Listeners.Add(new TextWriterTraceListener("VirtualStreetSnap.log"));
        Trace.AutoFlush = true;
        TextWriter originalConsoleOutput = Console.Out;
        MultiTextWriter multiTextWriter = new MultiTextWriter(originalConsoleOutput, new TraceTextWriter());
        Console.SetOut(multiTextWriter);

        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}

public class MultiTextWriter : TextWriter
{
    private readonly TextWriter _consoleWriter;
    private readonly TextWriter _traceWriter;

    public MultiTextWriter(TextWriter consoleWriter, TextWriter traceWriter)
    {
        _consoleWriter = consoleWriter;
        _traceWriter = traceWriter;
    }

    public override Encoding Encoding => Encoding.UTF8;

    public override void WriteLine(string value)
    {
        _consoleWriter.WriteLine(value);
        _traceWriter.WriteLine(value);
    }

    public override void Write(string value)
    {
        _consoleWriter.Write(value);
        _traceWriter.Write(value);
    }
}

public class TraceTextWriter : TextWriter
{
    public override Encoding Encoding => Encoding.UTF8;

    public override void WriteLine(string value)
    {
        Trace.WriteLine(value);
    }

    public override void Write(string value)
    {
        Trace.Write(value);
    }
}