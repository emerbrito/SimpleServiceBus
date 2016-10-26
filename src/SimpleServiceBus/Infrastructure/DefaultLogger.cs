using SimpleServiceBus.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Infrastructure
{
    class DefaultLogger : ILogger
    {
        public void Debug(object message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        public void Debug(string format, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(FormatMessage(format, args));
        }

        public void Debug(object message, Exception exception)
        {
            System.Diagnostics.Debug.WriteLine(FormatMessage(message.ToString(), exception));
        }

        public void Debug(string format, Exception exception, params object[] args)
        {
            System.Diagnostics.Debug.WriteLine(FormatMessage(format, exception, args));
        }

        public void Error(object message)
        {
            System.Diagnostics.Trace.TraceError(message.ToString());
        }

        public void Error(string format, params object[] args)
        {
            System.Diagnostics.Trace.TraceError(FormatMessage(format, args));
        }

        public void Error(object message, Exception exception)
        {
            System.Diagnostics.Trace.TraceError(FormatMessage(message.ToString(), exception));
        }

        public void Error(string format, Exception exception, params object[] args)
        {
            System.Diagnostics.Trace.TraceError(FormatMessage(format, exception, args));
        }

        public void Fatal(object message)
        {
            System.Diagnostics.Trace.Fail(message.ToString());
        }

        public void Fatal(string format, params object[] args)
        {
            System.Diagnostics.Trace.Fail(FormatMessage(format, args));
        }

        public void Fatal(object message, Exception exception)
        {
            System.Diagnostics.Trace.Fail(FormatMessage(message.ToString(), exception));
        }

        public void Fatal(string format, Exception exception, params object[] args)
        {
            System.Diagnostics.Trace.Fail(FormatMessage(format, exception, args));
        }

        public void Info(object message)
        {
            System.Diagnostics.Trace.TraceInformation(message.ToString());
        }

        public void Info(string format, params object[] args)
        {
            System.Diagnostics.Trace.TraceInformation(format, args);
        }

        public void Info(object message, Exception exception)
        {
            System.Diagnostics.Trace.TraceInformation(FormatMessage(message.ToString(), exception));
        }

        public void Info(string format, Exception exception, params object[] args)
        {
            System.Diagnostics.Trace.TraceInformation(FormatMessage(format, exception, args));
        }

        public void Trace(object message)
        {
            System.Diagnostics.Trace.WriteLine(message);
        }

        public void Trace(string format, params object[] args)
        {
            System.Diagnostics.Trace.WriteLine(FormatMessage(format, args));
        }

        public void Trace(string format, Exception exception, params object[] args)
        {
            System.Diagnostics.Trace.WriteLine(FormatMessage(format, exception, args));
        }

        public void Warn(object message)
        {
            System.Diagnostics.Trace.TraceWarning(message.ToString());
        }

        public void Warn(string format, params object[] args)
        {
            System.Diagnostics.Trace.TraceWarning(format, args);
        }

        public void Warn(object message, Exception exception)
        {
            System.Diagnostics.Trace.TraceWarning(FormatMessage(message.ToString(), exception));
        }

        public void Warn(string format, Exception exception, params object[] args)
        {
            System.Diagnostics.Trace.TraceWarning(FormatMessage(format, exception, args));
        }

        private string FormatMessage(string format, params object[] args)
        {

            if (args != null && args.Length > 0)
            {
                return string.Format(format, args);
            }

            return format;

        }

        private string FormatMessage(string format, Exception exception, params object[] args)
        {

            var builder = new StringBuilder(FormatMessage(format, args));
            if(exception != null)
            {
                builder.Append(" - EXCEPTION: ")
                    .Append(exception.ToString());
            }

            return builder.ToString();

        }


    }
}
