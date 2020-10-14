﻿using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Alexr03.Common.TCAdmin.Web.Binders;
using TCAdmin.Interfaces.Database;
using TCAdmin.SDK.Objects;

namespace Alexr03.Common.TCAdmin.Objects
{
    [ModelBinder(typeof(DynamicTypeBaseBinder))]
    public class DynamicTypeBase : ObjectBase
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // Needs to be public as it uses System.Activator to create the object.
        public DynamicTypeBase()
        {
            this.UseApplicationDataField = true;
        }

        public DynamicTypeBase(string tableName) : this()
        {
            this.TableName = tableName;
            this.KeyColumns = new[] {"id"};
            this.SetValue("id", -1);
            this.ValidateKeys();
        }

        public int Id
        {
            get => this.GetIntegerValue("id");
            set => this.SetValue("id", value);
        }

        protected string TypeName
        {
            get => this.GetStringValue("typeName");
            set => this.SetValue("typeName", value);
        }

        public Type Type => Type.GetType(TypeName);

        public bool HasConfiguration
        {
            get
            {
                try
                {
                    return ConfigurationId != 0 && Configuration != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        public string ConfigurationModuleId
        {
            get => this.GetStringValue("configurationModuleId");
            set => this.SetValue("configurationModuleId", value);
        }

        protected int ConfigurationId
        {
            get => this.GetIntegerValue("configurationId");
            set => this.SetValue("configurationId", value);
        }

        public virtual ModuleConfiguration Configuration =>
            new ModuleConfiguration(ConfigurationId, ConfigurationModuleId);

        public object Create(object args = null)
        {
            return Create<object>(args);
        }

        public T Create<T>(object args = null)
        {
            if (args == null)
            {
                return (T) Activator.CreateInstance(Type);
            }

            return (T) Activator.CreateInstance(Type, args);
        }

        public DynamicTypeBase FindByType(Type type)
        {
            var typeName = $"{type}, {type.Assembly.GetName().Name}";
            var whereList = new WhereList
            {
                {"typeName", ColumnOperator.Like, typeName}
            };
            var dynamicTypes = new DynamicTypeBase(TableName).GetObjectList(whereList).Cast<DynamicTypeBase>().ToList();
            return dynamicTypes.Any() ? dynamicTypes[0] : null;
        }

        public static DynamicTypeBase FindByType(string tableName, Type type)
        {
            var typeName = $"{type}, {type.Assembly.GetName().Name}";
            var whereList = new WhereList
            {
                {"typeName", ColumnOperator.Like, typeName}
            };
            var dynamicTypes = new DynamicTypeBase(tableName).GetObjectList(whereList).Cast<DynamicTypeBase>().ToList();
            return dynamicTypes.Any() ? dynamicTypes[0] : null;
        }

        public static object GetDynamicCurrentTypeBase(Type type, string idParam = "id")
        {
            if (!global::TCAdmin.SDK.Utility.IsWebEnvironment())
            {
                throw new Exception("Is not web environment");
            }

            if (!HttpContext.Current.Request.RequestContext.RouteData.Values.TryGetValue(idParam, out var id)) return null;
            var idInteger = int.Parse(id.ToString());
            return Activator.CreateInstance(type, idInteger);
        }

        public static T GetDynamicCurrentTypeBase<T>(string idParam = "id")
        {
            return (T) GetDynamicCurrentTypeBase(typeof(T), idParam);
        }
    }
}