﻿using System;
using System.IO;
using System.Text;
using Alexr03.Common.Logging;
using Newtonsoft.Json;

namespace Alexr03.Common.Configuration
{
    public class LocalConfiguration<T> : ConfigurationProvider<T>
    {
        private readonly Logger _logger = Logger.Create<LocalConfiguration<T>>();
        private const string ConfigBaseLocation = "./Components/{0}/Configurations/{1}/";
        private readonly string _configLocation;
        private readonly Type _type = typeof(T);
        private readonly string _assemblyName = typeof(T).Assembly.GetName().Name;

        public LocalConfiguration() : this(typeof(T).Name)
        {
        }

        public LocalConfiguration(string configName) : base(configName)
        {
            ConfigName = !ConfigName.EndsWith(".json") ? ConfigName + ".json" : ConfigName;
            _configLocation =
                Path.Combine(
                    ConfigBaseLocation.Replace("{0}", _assemblyName)
                        .Replace("{1}", _type.Namespace?.Replace(_assemblyName, "").Trim('.')), ConfigName);
        }

        public override T GetConfiguration()
        {
            try
            {
                var fileInfo = new FileInfo(_configLocation);
                fileInfo.Directory?.Create();
                if (!fileInfo.Exists)
                {
                    _logger.Information($"Config '{fileInfo.Name}' does not exist. Auto generating.");
                    var defaultT = GetTObject();
                    SetConfiguration(defaultT);
                    return defaultT;
                }

                var fileContents = File.ReadAllText(fileInfo.FullName);
                var deserializeObject = JsonConvert.DeserializeObject<T>(fileContents);
                return deserializeObject;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
            }

            return GetTObject();
        }

        public override bool SetConfiguration(T config)
        {
            try
            {
                var serializedObject = JsonConvert.SerializeObject(config, Formatting.Indented);
                File.WriteAllText(_configLocation, serializedObject, Encoding.Default);

                _logger.Information("Saved new Configuration: " + ConfigName);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogException(e);
                return false;
            }
        }
    }
}