﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web.Configuration;
using Serilog;
using Serilog.Events;

namespace Alexr03.Common.Logging
{
    public class Logger
    {
        private readonly string _logBaseLocation = "./Components/{0}/Logs/{1}/{2}/{2}.log";
        public string Application { get; }
        public Serilog.Core.Logger InternalLogger { get; }
        private string LogLocation { get; }
        private Type Type { get; set; }

        public Logger(string application, Type type = null)
        {
            Application = application;
            Type = type;

            var consoleOutputTemplate =
                $"[{application}" + " {Timestamp:HH:mm:ss.ff} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            var loggerConfiguration = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: consoleOutputTemplate)
                .MinimumLevel.Debug();
            // if (Utilities.IsRunningOnTcAdmin)
            // {
            //     var arCommonSettings = ModuleConfiguration.GetModuleConfiguration(Globals.ModuleId, "ArCommonSettings").Parse<ArCommonSettings>();
            //     loggerConfiguration.MinimumLevel.Is(arCommonSettings.MinimumLogLevel);
            //     _logBaseLocation = _logBaseLocation.Replace("./", Path.Combine(Utility.GetLogPath(), "../"));
            // }

            if (Type != null)
            {
                var assemblyName = Type.Assembly.GetName().Name;
                LogLocation =
                    Path.Combine(
                        _logBaseLocation
                            .Replace("{0}", assemblyName)
                            .Replace("{1}", Type.Namespace?.Replace(assemblyName, "").Trim('.'))
                            .Replace("{2}", application));
                loggerConfiguration.WriteTo.File(LogLocation, rollingInterval: RollingInterval.Day, shared: true);
            }
            else
            {
                LogLocation = $"./Components/Misc/Logs/{application}/{application}.log";
                loggerConfiguration.WriteTo.File(LogLocation, rollingInterval: RollingInterval.Day, shared: true);
            }
            
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings.Get("TCAdmin.LogPath")))
            {
                LogLocation = LogLocation.Replace("./", Path.Combine(ConfigurationManager.AppSettings["TCAdmin.LogPath"], "../"));
            }

            InternalLogger = loggerConfiguration.CreateLogger();
        }
        
        public static Logger Create(Type type)
        {
            var logger = new Logger(type.Name, type);
            return logger;
        }

        public static Logger Create<T>(string application)
        {
            var logger = new Logger(application, typeof(T));
            return logger;
        }

        public static Logger Create<T>()
        {
            var logger = new Logger(typeof(T).Name, typeof(T));
            return logger;
        }
        
        public void LogMessage(string message)
        {
            LogMessage(LogEventLevel.Information, message);
        }

        public void Information(string message)
        {
            LogMessage(LogEventLevel.Information, message);
        }

        public void Debug(string message)
        {
            LogMessage(LogEventLevel.Debug, message);
        }
        
        public void Error(string message)
        {
            LogMessage(LogEventLevel.Error, message);
        }
        
        public void Fatal(string message)
        {
            LogMessage(LogEventLevel.Fatal, message);
        }
        
        public void Verbose(string message)
        {
            LogMessage(LogEventLevel.Verbose, message);
        }
        
        public void Warning(string message)
        {
            LogMessage(LogEventLevel.Warning, message);
        }

        public List<FileInfo> GetLogFiles()
        {
            var directoryInfo = new FileInfo(LogLocation).Directory;
            directoryInfo?.Create();
            return directoryInfo?.GetFiles().ToList();
        }

        public FileInfo GetCurrentLogFile()
        {
            var fileInfos = GetLogFiles().OrderByDescending(x => x.LastWriteTimeUtc).ToList();
            if (fileInfos.Count >= 1)
            {
                return fileInfos[0];
            }

            return null;
        }

        public void LogMessage(LogEventLevel logLevel, string message)
        {
            InternalLogger.Write(logLevel, message);
            LogReceived?.Invoke(logLevel, InternalLogger, message, Application);
        }
        
        public void LogMessage(LogLevel logLevel, string message)
        {
            InternalLogger.Write((LogEventLevel)logLevel, message);
            LogReceived?.Invoke((LogEventLevel)logLevel, InternalLogger, message, Application);
        }

        public void LogException(Exception exception)
        {
            InternalLogger.Write(LogEventLevel.Error, exception, exception.Message);
            LogExceptionReceived?.Invoke(LogEventLevel.Error, InternalLogger, exception, Application);
        }

        public static event LogRaised LogReceived;

        public static event LogExceptionRaised LogExceptionReceived;

        public delegate void LogRaised(LogEventLevel logLevel, Serilog.Core.Logger logger, string message,
            string application);

        public delegate void LogExceptionRaised(LogEventLevel logLevel, Serilog.Core.Logger logger, Exception exception,
            string application);
    }
    
    public enum LogLevel
    {
        Verbose,
        Debug,
        Information,
        Warning,
        Error,
        Fatal
    }
}