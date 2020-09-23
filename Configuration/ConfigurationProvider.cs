﻿using System;

namespace Alexr03.Common.Configuration
{
    public abstract class ConfigurationProvider<T>
    {
        protected string ConfigName { get; set; }
        public bool GenerateIfNonExisting { get; set; } = true;

        protected ConfigurationProvider() : this(typeof(T).Name)
        {
        }

        protected ConfigurationProvider(string configName)
        {
            ConfigName = configName;
        }

        public abstract T GetConfiguration();
        public abstract bool SetConfiguration(T config);
        
        public bool SaveConfiguration(T config)
        {
            return SetConfiguration(config);
        }

        protected T GetTObject()
        {
            Console.WriteLine("Generating default config");
            if (typeof(T).IsValueType || typeof(T) == typeof(string)) return default;
            return (T) Activator.CreateInstance(typeof(T));
        }
    }
}