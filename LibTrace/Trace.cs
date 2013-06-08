using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LibConfig;

namespace LibTrace
{
    public class Trace : IDisposable
    {
        private TraceSource s;
        private int id = 0;
        private readonly object tracelock = new object();
        private static readonly Dictionary<string, TraceSource> Sources = new Dictionary<string, TraceSource>();
        private bool writeToConsole = false;

        public static IEnumerable<ITraceReceiver> ExtraReceivers = new List<ITraceReceiver>();

        public Trace(string sourceName)
        {
            if (Sources.ContainsKey(sourceName))
                s = Sources[sourceName];
            else
            {
                // ReSharper disable UseObjectOrCollectionInitializer
                s = new TraceSource(sourceName);
                // ReSharper restore UseObjectOrCollectionInitializer
                s.Switch.Level = SourceLevels.All; //Bug in Mono. If you do this in the constructor it does not work.
                s.Listeners.Clear();
                GetAllListeners(sourceName).ForEach(l => s.Listeners.Add(l));
                Verbose("Logging started.");
                Sources.Add(sourceName, s);
            }

            if (Config.GetFlag(Config.WRITE_TRACE_TO_CONSOLE))
                writeToConsole = true;
        }

        public void Critical(string message)
        {
            DoLog(message, TraceEventType.Critical);
        }

        public void Error(string message)
        {
            DoLog(message, TraceEventType.Error);
        }

        public void Warning(string message)
        {
            DoLog(message, TraceEventType.Warning);
        }

        public void Information(string message)
        {
            DoLog(message, TraceEventType.Information);
        }

        public void Verbose(string message)
        {
            DoLog(message, TraceEventType.Verbose);
        }

        public void Close()
        {
            foreach (TraceListener t in s.Listeners)
                t.Flush();
            s.Close();
        }

        private void DoLog(string message, TraceEventType type)
        {
            lock (tracelock)
            {
                string formatted = FormatMessage(message);
                s.TraceEvent(type, id, formatted);
                s.Flush();
                id++;
                if (writeToConsole)
                    Console.WriteLine("{0} {1}", s.Name, formatted);
                foreach (ITraceReceiver r in ExtraReceivers)
                    r.ReceiveTrace(formatted);
            }
        }

        private string FormatMessage(string message)
        {
            return String.Format("[{0}] {1}", DateTime.Now.ToString(), message);
        }

        private List<TraceListener> GetAllListeners(string sourceName)
        {
            var listeners = new List<TraceListener>
                {
                    GetTraceListener(sourceName, SourceLevels.Critical),
                    GetTraceListener(sourceName, SourceLevels.Error),
                    GetTraceListener(sourceName, SourceLevels.Warning),
                    GetTraceListener(sourceName, SourceLevels.Information),
                    GetTraceListener(sourceName, SourceLevels.Verbose)
                };
            return listeners;
        }

        private TraceListener GetTraceListener(string sourceName, SourceLevels level)
        {
            TraceListener t = new TextWriterTraceListener(GetLogName(sourceName, level));
            t.Filter = new EventTypeFilter(level);
            return t;
        }

        private string GetLogName(string sourceName, SourceLevels level)
        {
            string path = Config.Get(Config.LOG_PATH);
            string name = sourceName + "_" + Enum.GetName(typeof(SourceLevels), level) + ".log";
            return System.IO.Path.Combine(path, name);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
