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
        private static readonly Dictionary<string, TraceSource> Sources = new Dictionary<string, TraceSource>();
        private SourceLevels Level = SourceLevels.Information; //the same for all sources at the moment. TODO: per-component tracing setting.

        public static List<TraceListener> ExtraListeners = new List<TraceListener>();

        public Trace(string sourceName)
        {
            if (Sources.ContainsKey(sourceName))
                s = Sources[sourceName];
            else
            {
                s = new TraceSource(sourceName, Level);
                s.Listeners.Clear();
                s.Listeners.Add(GetTraceListener(sourceName));
                if (Config.GetFlag(Config.WRITE_TRACE_TO_CONSOLE))
                    s.Listeners.Add(new ConsoleTraceListener());
                ExtraListeners.ForEach(l => s.Listeners.Add(l));
                Sources.Add(sourceName, s);
                Verbose("Logging started.");
            }
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
                string formatted = FormatMessage(message);
                s.TraceEvent(type, id, formatted);
                s.Flush();
                id++;
        }

        private string FormatMessage(string message)
        {
            return String.Format("[{0}] {1}", DateTime.Now.ToString(), message);
        }

        private TraceListener GetTraceListener(string sourceName)
        {
            TraceListener t = new TextWriterTraceListener(GetLogName(sourceName));
            t.Filter = new EventTypeFilter(Level);
            return t;
        }

        private string GetLogName(string sourceName)
        {
            string path = Config.Get(Config.LOG_PATH);
            string name = sourceName + ".log";
            return System.IO.Path.Combine(path, name);
        }

        public void Dispose()
        {
            Close();
        }
    }
}
