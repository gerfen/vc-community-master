﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VirtoCommerce.Foundation;
using VirtoCommerce.Foundation.AppConfig.Model;
using VirtoCommerce.Foundation.AppConfig.Repositories;
using VirtoCommerce.Foundation.Frameworks;
using VirtoCommerce.Foundation.Frameworks.Extensions;
using VirtoCommerce.Framework.Web.Modularity;
using VirtoCommerce.Framework.Web.Settings;

namespace VirtoCommerce.CoreModule.Web.Settings
{
    public class SettingsManager : ISettingsManager
    {
        #region Constants
        public const string SettingsCacheKey = "S:{0}";
        #endregion

        private readonly IModuleManifestProvider _manifestProvider;
        private readonly Func<IAppConfigRepository> _repositoryFactory;
        private readonly CacheHelper _cacheHelper;

        public SettingsManager(IModuleManifestProvider manifestProvider, Func<IAppConfigRepository> repositoryFactory, ICacheRepository cache)
        {
            _manifestProvider = manifestProvider;
            _repositoryFactory = repositoryFactory;
            this._cacheHelper = new CacheHelper(cache);
        }

        #region ISettingsManager Members

        public ModuleDescriptor[] GetModules()
        {
            var moduleDescriptors = GetModuleManifestsWithSettings()
                .Select(ConvertToModuleDescriptor)
                .ToArray();

            return moduleDescriptors;
        }

        public SettingDescriptor[] GetSettings(string moduleId)
        {
            var result = new List<SettingDescriptor>();

            var manifest = GetModuleManifestsWithSettings().FirstOrDefault(m => m.Id == moduleId);

            if (manifest != null && manifest.Settings != null)
            {
                var settingNames = manifest.Settings
                    .SelectMany(g => g.Settings)
                    .Select(s => s.Name)
                    .Distinct()
                    .ToArray();

                if (settingNames.Any())
                {
                    var existingSettings = LoadSettings(settingNames);

                    foreach (var group in manifest.Settings)
                    {
                        if (group.Settings != null)
                        {
                            result.AddRange(group.Settings.Select(s => ConvertToSettingsDescriptor(group.Name, s, existingSettings)));
                        }
                    }
                }
            }

            return result.ToArray();
        }

        public void SaveSettings(SettingDescriptor[] settings)
        {
            if (settings != null)
            {
                var settingNames = settings
                    .Select(s => s.Name)
                    .Distinct()
                    .ToArray();

                var existingSettings = LoadSettings(settingNames);

                using (var repository = _repositoryFactory())
                {
                    foreach (var settingDescriptor in settings)
                    {
                        SaveSetting(repository, settingDescriptor, existingSettings);
                    }

                    repository.UnitOfWork.Commit();
                }

                _cacheHelper.Remove(
                    CacheHelper.CreateCacheKey(Constants.SettingsCachePrefix, string.Format(SettingsCacheKey, "all")));
            }
        }

        public T[] GetArray<T>(string name, T[] defaultValue)
        {
            var result = defaultValue;

            var repositorySetting = LoadSetting(name);
            if (repositorySetting != null)
            {
                result = repositorySetting.SettingValues
                    .Select(v => (T)v.RawValue())
                    .ToArray();
            }
            else
            {
                var manifestSetting = GetSettingFromManifest(name);
                if (manifestSetting != null)
                {
                    if (manifestSetting.ArrayValues != null)
                    {
                        result = manifestSetting.ArrayValues
                            .Select(v => (T)manifestSetting.RawValue(v))
                            .ToArray();
                    }
                    else if (manifestSetting.DefaultValue != null)
                    {
                        result = new[] { (T)manifestSetting.RawDefaultValue() };
                    }
                }
            }

            return result;
        }

        public T GetValue<T>(string name, T defaultValue)
        {
            var result = defaultValue;

            var values = GetArray(name, new[] { defaultValue });
            if (values.Any())
            {
                result = values.First();
            }

            return result;
        }

        public void SetValue<T>(string name, T value)
        {
            var type = typeof(T);
            var objectValue = (object)value;
            var descriptor = new SettingDescriptor { Name = name };

            if (type.IsArray)
            {
                descriptor.IsArray = true;
                descriptor.ValueType = ConvertToModuleSettingType(type.GetElementType());

                if (objectValue != null)
                {
                    descriptor.ArrayValues =
                        ((IEnumerable)value).OfType<object>()
                        .Select(v => v == null ? null : string.Format(CultureInfo.InvariantCulture, "{0}", v))
                        .ToArray();
                }
            }
            else
            {
                descriptor.ValueType = ConvertToModuleSettingType(type);
                descriptor.Value = objectValue == null ? null : string.Format(CultureInfo.InvariantCulture, "{0}", value);
            }

            SaveSettings(new[] { descriptor });
        }

        #endregion


        private IEnumerable<ModuleManifest> GetModuleManifestsWithSettings()
        {
            return _manifestProvider.GetModuleManifests().Values
                .Where(m => m.Settings != null && m.Settings.Any());
        }

        private IEnumerable<ModuleSetting> GetAllManifestSettings()
        {
            return GetModuleManifestsWithSettings()
                .SelectMany(m => m.Settings)
                .SelectMany(g => g.Settings);
        }

        private ModuleSetting GetSettingFromManifest(string name)
        {
            return GetAllManifestSettings().FirstOrDefault(s => s.Name == name);
        }

        private static void SaveSetting(IRepository repository, SettingDescriptor descriptor, ICollection<Setting> existingSettings)
        {
            var name = descriptor.Name;

            // Create new setting or attach existing
            var setting = existingSettings.FirstOrDefault(s => s.Name == name);
            if (setting == null)
            {
                setting = new Setting { Name = name };
                repository.Add(setting);
                existingSettings.Add(setting);
            }
            else
            {
                repository.Attach(setting);
            }

            setting.SettingValueType = ConvertToSettingValueType(descriptor.ValueType);
            setting.IsEnum = descriptor.IsArray;

            var values = new List<string>();

            if (descriptor.IsArray)
            {
                if (descriptor.ArrayValues != null)
                {
                    values.AddRange(descriptor.ArrayValues);
                }
            }
            else
            {
                values.Add(descriptor.Value);
            }

            // Add new values
            for (var i = setting.SettingValues.Count; i < values.Count; i++)
            {
                setting.SettingValues.Add(new SettingValue());
            }

            // Remove old values
            while (setting.SettingValues.Count > values.Count)
            {
                setting.SettingValues.RemoveAt(values.Count);
            }

            // Set values
            for (var i = 0; i < values.Count; i++)
            {
                var settingValue = setting.SettingValues[i];
                settingValue.ValueType = setting.SettingValueType;
                SetSettingValue(settingValue, ModuleSetting.RawValue(descriptor.ValueType, values[i]));
            }
        }

        private static void SetSettingValue(SettingValue settingValue, object value)
        {
            switch (settingValue.ValueType)
            {
                case SettingValue.TypeBoolean:
                    settingValue.BooleanValue = (bool)value;
                    break;
                case SettingValue.TypeInteger:
                    settingValue.IntegerValue = (int)value;
                    break;
                case SettingValue.TypeDecimal:
                    settingValue.DecimalValue = (decimal)value;
                    break;
                case SettingValue.TypeShortText:
                    settingValue.ShortTextValue = (string)value;
                    break;
            }
        }

        private static string ConvertToSettingValueType(string valueType)
        {
            switch (valueType)
            {
                case ModuleSetting.TypeBoolean:
                    return SettingValue.TypeBoolean;
                case ModuleSetting.TypeInteger:
                    return SettingValue.TypeInteger;
                case ModuleSetting.TypeDecimal:
                    return SettingValue.TypeDecimal;
                default:
                    return SettingValue.TypeShortText;
            }
        }

        private static string ConvertToModuleSettingType(Type valueType)
        {
            if (valueType == typeof(bool))
                return ModuleSetting.TypeBoolean;

            if (valueType == typeof(int))
                return ModuleSetting.TypeInteger;

            if (valueType == typeof(decimal))
                return ModuleSetting.TypeDecimal;

            return ModuleSetting.TypeString;
        }

        private static ModuleDescriptor ConvertToModuleDescriptor(ModuleManifest manifest)
        {
            return new ModuleDescriptor
            {
                Id = manifest.Id,
                Title = manifest.Title,
            };
        }

        Setting LoadSetting(string name)
        {
            return LoadSettings(name).FirstOrDefault();
        }

        private List<Setting> LoadSettings(params string[] settingNames)
        {
            var settings = LoadAllSettings();
            return settings == null ? null : settings.Where(s => settingNames.Contains(s.Name)).ToList();
        }

        private List<Setting> LoadAllSettings()
        {
            return _cacheHelper.Get(
                CacheHelper.CreateCacheKey(Constants.SettingsCachePrefix, string.Format(SettingsCacheKey, "all")),
                this.LoadAllSettingsFromDatabase,
                TimeSpan.FromSeconds(120));   // TODO: have a setting
        }

        private List<Setting> LoadAllSettingsFromDatabase()
        {
            using (var repository = _repositoryFactory())
            {
                return repository.Settings
                    .Expand(s => s.SettingValues)
                    .ToList();
            }
        }

        private static SettingDescriptor ConvertToSettingsDescriptor(string groupName, ModuleSetting setting, IEnumerable<Setting> existingSettings)
        {
            var result = new SettingDescriptor
            {
                GroupName = groupName,
                Name = setting.Name,
                Value = setting.DefaultValue,
                ValueType = setting.ValueType,
                AllowedValues = setting.AllowedValues,
                DefaultValue = setting.DefaultValue,
                IsArray = setting.IsArray,
                ArrayValues = setting.ArrayValues,
                Title = setting.Title,
                Description = setting.Description,
            };

            var existingSetting = existingSettings
                .FirstOrDefault(s => s.Name == setting.Name);

            if (existingSetting != null)
            {
                var existingValues = existingSetting.SettingValues
                    .Select(v => v.ToString(CultureInfo.InvariantCulture))
                    .ToArray();

                if (setting.IsArray)
                {
                    result.ArrayValues = existingValues;
                }
                else
                {
                    if (existingValues.Any())
                    {
                        result.Value = existingValues.First();
                    }
                }
            }

            return result;
        }
    }
}