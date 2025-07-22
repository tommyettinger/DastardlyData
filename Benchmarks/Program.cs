using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Benchmarks;

public class Program {
    static void Main(string[] args) {
        /*
        // Configure how you want your benchmarks to output additional data, Serilog, Savage.Logs, and ZLogger are all good ways to do this
        // Savage.Logs
        var loggySettings = LogPipelineSettings.Default();
        loggyLogger = new LogPipeline().Initialize(loggySettings);

         // Serilog
         Serilog.Log.Logger = new LoggerConfiguration()
                                    .WriteTo.Console()
                                    .CreateLogger();
        // ZLogger
        using var factory = LoggerFactory.Create(logging => {
            logging.SetMinimumLevel(LogLevel.Trace);

            // Add ZLogger provider to ILoggingBuilder
            logging.AddZLoggerConsole();
        });
        */

        Summary? summary;
        if (TryParseArguments(args, out Mode runMode, out string argumentErrorMessage) is false) Console.WriteLine(argumentErrorMessage);
        else {
            switch (runMode) {
                case Mode.TestLatency:
                    summary = BenchmarkRunner.Run<Latency>();
                    break;

                case Mode.TestThroughput:
                    summary = BenchmarkRunner.Run<Throughput>();
                    break;
            }
        }

        Console.ReadLine();
    }

    enum Mode {
        None,
        TestLatency,
        TestThroughput
    }
    
    static bool TryParseArguments(string[] arguments, out Mode runMode, out string argumentErrorMessage) {
        runMode = Mode.None;
        argumentErrorMessage = "the only expected arguments are '-latency' '-l', or '-throughput' '-t'";

        if (arguments.Length == 0)
            return true;

        if (arguments.Length > 1) 
            return false;

        switch (arguments[0]) {
            default:
                return false;

            case "-l":
            case "-latency":
                runMode = Mode.TestLatency;
                return true;


            case "-t":
            case "-throughput":
                runMode = Mode.TestThroughput;
                return true;
        }
    }
}