using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LibTrace
{
    public class Trace
    {
        private TraceSource s;
        private int id = 0;
        private readonly object tracelock = new object();

        public Trace(string sourceName)
        {
            s = new TraceSource(sourceName, SourceLevels.All);
            s.Listeners.Clear();
            GetAllListeners(sourceName).ForEach(l => s.Listeners.Add(l));
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

        private void DoLog(string message, TraceEventType type)
        {
            lock (tracelock)
            {
                s.TraceEvent(type, id, FormatMessage(message));
                s.Flush();
                id++;
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
            return sourceName + "_" + Enum.GetName(typeof(SourceLevels), level) + ".log";
        }
    }
}
