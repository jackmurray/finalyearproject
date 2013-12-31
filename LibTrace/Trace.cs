using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LibConfig;
using LibUtil;

namespace LibTrace
{
    public class Trace : IDisposable
    {
        private TraceSource s;
        private int id = 0;
        private static readonly Dictionary<string, Trace> Objects = new Dictionary<string, Trace>();
        private static SourceLevels StartLevel = Config.GetTraceLevel(); //the same for all sources at the moment. TODO: per-component tracing setting.

        public static List<TraceListener> ExtraListeners = new List<TraceListener>();
        public static bool Initialised = false;

        protected Trace(string sourceName)
        {
            s = new TraceSource(sourceName, StartLevel);
            s.Listeners.Clear();
            s.Listeners.Add(GetTraceListener(sourceName));
            if (Config.GetFlag(Config.WRITE_TRACE_TO_CONSOLE))
                s.Listeners.Add(new ConsoleTraceListener());
            ExtraListeners.ForEach(l => s.Listeners.Add(l));
            
            Information("Logging started at level " + s.Switch.Level);
        }

        public static Trace GetInstance(string sourceName)
        {
#if !TEST
            if (!Trace.Initialised)
                throw new InvalidOperationException("Tracing has not been initialised yet!");
#endif
            if (Objects.ContainsKey(sourceName))
                return Objects[sourceName];

            Trace t = new Trace(sourceName);
            Objects.Add(sourceName, t);
            return t;
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
            if (LibUtil.Util.IsRunningOnMono && !ShouldLog(StartLevel, type)) //There's a bug in mono where trace event filtering doesn't work properly, so we have to do it ourselves.
                return;

            string formatted = FormatMessage(message);
            s.TraceEvent(type, id, formatted);
            s.Flush();
            id++;
        }

        /// <summary>
        /// Check if we should log a given type. Only for use in the mono workaround.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private bool ShouldLog(SourceLevels level, TraceEventType type)
        {
            return ((int)level & (int)type) != 0;
        }

        private string FormatMessage(string message)
        {
            DateTime dt = DateTime.Now;
            return String.Format("[{0}:{1:000}] {2}", dt.ToString(), dt.Millisecond, message);
        }

        private TraceListener GetTraceListener(string sourceName)
        {
            TraceListener t = new TextWriterTraceListener(GetLogName(sourceName));
            return t;
        }

        private string GetLogName(string sourceName)
        {
            string path = Util.ResolvePath(Config.Get(Config.LOG_PATH));
            string name = sourceName + ".log";
            return System.IO.Path.Combine(path, name);
        }

        public void Dispose()
        {
            Close();
        }

        public static void SetLevel(SourceLevels level)
        {
            Trace.StartLevel = level;
            foreach (var kvp in Objects)
            {
                kvp.Value.s.Switch.Level = level;
            }
        }
    }
}
