using SpringCard.LibCs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test
{
    class Program
    {
        static void LoggerReaderCallback(LoggerReader.Entry entry)
        {
            entry.WriteToConsole();
        }

        static void Main(string[] args)
        {
            string ConsoleTestFile = @"d:\dev\softs\software.companion-service\_output\service.log";
            string FileTestFile1 = @"D:\dev\interne\internal.multiprod\_output\log\multiprod-20230119.log";
            string FileTestFile2 = @"D:\dev\interne\internal.multiprod\_output\log\multiprod-20230117.log";

            LoggerReader loggerReader = new LoggerReader(new LoggerReader.Callback(LoggerReaderCallback));

            loggerReader.ReadFile(ConsoleTestFile, LoggerReader.InputFormat.SpringCardVerboseConsole);
            //loggerReader.ReadFile(FileTestFile1);
            //loggerReader.ReadFile(FileTestFile2);
            //loggerReader.ReadAndWatchFile(ConsoleTestFile);

            Logger.ReadArgs(args);
            Logger.Fatal("Hello, world");
            Logger.Error("Hello, world");
            Logger.Warning("Hello, world");
            Logger.Info("Hello, world");
            Logger.Trace("Hello, world");
            Logger.Debug("Hello, world");

            Logger logger = new Logger();
            logger.fatal("Hello, world");
            logger.error("Hello, world");
            logger.warning("Hello, world");
            logger.info("Hello, world");
            logger.trace("Hello, world");
            logger.debug("Hello, world");

            Console.WriteLine("Press enter to exit.");
            Console.ReadLine();
        }
    }
}
