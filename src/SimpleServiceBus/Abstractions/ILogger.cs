using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleServiceBus.Abstractions
{
    public interface ILogger
    {

        void Trace(object message);
        void Trace(string format, params object[] args);
        void Trace(string format, Exception exception, params object[] args);

        void Debug(object message);
        void Debug(object message, Exception exception);
        void Debug(string format, params object[] args);
        void Debug(string format, Exception exception, params object[] args);

        void Info(object message);
        void Info(object message, Exception exception);
        void Info(string format, params object[] args);
        void Info(string format, Exception exception, params object[] args);

        void Warn(object message);
        void Warn(object message, Exception exception);
        void Warn(string format, params object[] args);
        void Warn(string format, Exception exception, params object[] args);

        void Error(object message);
        void Error(object message, Exception exception);
        void Error(string format, params object[] args);
        void Error(string format, Exception exception, params object[] args);

        void Fatal(object message);
        void Fatal(object message, Exception exception);
        void Fatal(string format, params object[] args);
        void Fatal(string format, Exception exception, params object[] args);

    }
}
