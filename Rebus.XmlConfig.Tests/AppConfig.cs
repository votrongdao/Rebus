﻿using System;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace Rebus.XmlConfig.Tests
{
    /// <summary>
    /// Awesome hack, found here: http://stackoverflow.com/a/6151688/6560
    /// </summary>
    public abstract class AppConfig : IDisposable
    {
        public static AppConfig Change(string path)
        {
            return new ChangeAppConfig(path);
        }

        public abstract void Dispose();

        class ChangeAppConfig : AppConfig
        {
            private readonly string _oldConfig =
                AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();

            private bool _disposedValue;

            public ChangeAppConfig(string path)
            {
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", path);
                ResetConfigMechanism();
            }

            public override void Dispose()
            {
                if (!_disposedValue)
                {
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", _oldConfig);
                    ResetConfigMechanism();


                    _disposedValue = true;
                }
                GC.SuppressFinalize(this);
            }

            private static void ResetConfigMechanism()
            {
                typeof(ConfigurationManager)
                    .GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static)
                    .SetValue(null, 0);

                typeof(ConfigurationManager)
                    .GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static)
                    .SetValue(null, null);

                typeof(ConfigurationManager)
                    .Assembly.GetTypes()
                    .Where(x => x.FullName == "System.Configuration.ClientConfigPaths")
                    .First()
                    .GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static)
                    .SetValue(null, null);
            }
        }
    }
}