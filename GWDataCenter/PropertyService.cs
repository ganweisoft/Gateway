﻿﻿// Copyright (c) 2004-2025 Shenzhen Ganwei Software Technology Co., Ltd
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
namespace GWDataCenter
{
    public sealed class CallbackOnDispose : IDisposable
    {
        Action callback;
        public CallbackOnDispose(Action callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");
            this.callback = callback;
        }
        public void Dispose()
        {
            Action action = Interlocked.Exchange(ref callback, null);
            if (action != null)
            {
                action();
            }
        }
    }
    public delegate void PropertyChangedEventHandler(object sender, PropertyChangedEventArgs e);
    public class PropertyChangedEventArgs : EventArgs
    {
        Properties properties;
        string key;
        object newValue;
        object oldValue;
        public Properties Properties
        {
            get
            {
                return properties;
            }
        }
        public string Key
        {
            get
            {
                return key;
            }
        }
        public object NewValue
        {
            get
            {
                return newValue;
            }
        }
        public object OldValue
        {
            get
            {
                return oldValue;
            }
        }
        public PropertyChangedEventArgs(Properties properties, string key, object oldValue, object newValue)
        {
            this.properties = properties;
            this.key = key;
            this.oldValue = oldValue;
            this.newValue = newValue;
        }
    }
    public class Properties
    {
        class SerializedValue
        {
            string content;
            public string Content
            {
                get { return content; }
            }
            public T Deserialize<T>()
            {
                XmlSerializer serializer = new XmlSerializer(typeof(T));
                return (T)serializer.Deserialize(new StringReader(content));
            }
            public SerializedValue(string content)
            {
                this.content = content;
            }
        }
        Dictionary<string, object> properties = new Dictionary<string, object>();
        public string this[string property]
        {
            get
            {
                return Convert.ToString(Get(property), CultureInfo.InvariantCulture);
            }
            set
            {
                Set(property, value);
            }
        }
        public string[] Elements
        {
            get
            {
                lock (properties)
                {
                    List<string> ret = new List<string>();
                    foreach (KeyValuePair<string, object> property in properties)
                        ret.Add(property.Key);
                    return ret.ToArray();
                }
            }
        }
        public object Get(string property)
        {
            lock (properties)
            {
                object val;
                properties.TryGetValue(property, out val);
                return val;
            }
        }
        public void Set<T>(string property, T value)
        {
            if (property == null)
                throw new ArgumentNullException("property");
            if (value == null)
                throw new ArgumentNullException("value");
            T oldValue = default(T);
            lock (properties)
            {
                if (!properties.ContainsKey(property))
                {
                    properties.Add(property, value);
                }
                else
                {
                    oldValue = Get<T>(property, value);
                    properties[property] = value;
                }
            }
            OnPropertyChanged(new PropertyChangedEventArgs(this, property, oldValue, value));
        }
        public bool Contains(string property)
        {
            lock (properties)
            {
                return properties.ContainsKey(property);
            }
        }
        public int Count
        {
            get
            {
                lock (properties)
                {
                    return properties.Count;
                }
            }
        }
        public bool Remove(string property)
        {
            lock (properties)
            {
                return properties.Remove(property);
            }
        }
        public override string ToString()
        {
            lock (properties)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("[Properties:{");
                foreach (KeyValuePair<string, object> entry in properties)
                {
                    sb.Append(entry.Key);
                    sb.Append("=");
                    sb.Append(entry.Value);
                    sb.Append(",");
                }
                sb.Append("}]");
                return sb.ToString();
            }
        }
        public static Properties ReadFromAttributes(XmlReader reader)
        {
            Properties properties = new Properties();
            if (reader.HasAttributes)
            {
                for (int i = 0; i < reader.AttributeCount; i++)
                {
                    reader.MoveToAttribute(i);
                    properties[reader.Name] = reader.Value;
                }
                reader.MoveToElement();
            }
            return properties;
        }
        internal void ReadProperties(XmlReader reader, string endElement)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == endElement)
                        {
                            return;
                        }
                        break;
                    case XmlNodeType.Element:
                        string propertyName = reader.LocalName;
                        if (propertyName == "Properties")
                        {
                            propertyName = reader.GetAttribute(0);
                            Properties p = new Properties();
                            p.ReadProperties(reader, "Properties");
                            properties[propertyName] = p;
                        }
                        else if (propertyName == "Array")
                        {
                            propertyName = reader.GetAttribute(0);
                            properties[propertyName] = ReadArray(reader);
                        }
                        else if (propertyName == "SerializedValue")
                        {
                            propertyName = reader.GetAttribute(0);
                            properties[propertyName] = new SerializedValue(reader.ReadInnerXml());
                        }
                        else
                        {
                            properties[propertyName] = reader.HasAttributes ? reader.GetAttribute(0) : null;
                        }
                        break;
                }
            }
        }
        ArrayList ReadArray(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return new ArrayList(0);
            ArrayList l = new ArrayList();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.EndElement:
                        if (reader.LocalName == "Array")
                        {
                            return l;
                        }
                        break;
                    case XmlNodeType.Element:
                        l.Add(reader.HasAttributes ? reader.GetAttribute(0) : null);
                        break;
                }
            }
            return l;
        }
        public void WriteProperties(XmlWriter writer)
        {
            lock (properties)
            {
                List<KeyValuePair<string, object>> sortedProperties = new List<KeyValuePair<string, object>>(properties);
                sortedProperties.Sort((a, b) => StringComparer.OrdinalIgnoreCase.Compare(a.Key, b.Key));
                foreach (KeyValuePair<string, object> entry in sortedProperties)
                {
                    object val = entry.Value;
                    if (val is Properties)
                    {
                        writer.WriteStartElement("Properties");
                        writer.WriteAttributeString("name", entry.Key);
                        ((Properties)val).WriteProperties(writer);
                        writer.WriteEndElement();
                    }
                    else if (val is Array || val is ArrayList)
                    {
                        writer.WriteStartElement("Array");
                        writer.WriteAttributeString("name", entry.Key);
                        foreach (object o in (IEnumerable)val)
                        {
                            writer.WriteStartElement("Element");
                            WriteValue(writer, o);
                            writer.WriteEndElement();
                        }
                        writer.WriteEndElement();
                    }
                    else if (TypeDescriptor.GetConverter(val).CanConvertFrom(typeof(string)))
                    {
                        writer.WriteStartElement(entry.Key);
                        WriteValue(writer, val);
                        writer.WriteEndElement();
                    }
                    else if (val is SerializedValue)
                    {
                        writer.WriteStartElement("SerializedValue");
                        writer.WriteAttributeString("name", entry.Key);
                        writer.WriteRaw(((SerializedValue)val).Content);
                        writer.WriteEndElement();
                    }
                    else
                    {
                        writer.WriteStartElement("SerializedValue");
                        writer.WriteAttributeString("name", entry.Key);
                        XmlSerializer serializer = new XmlSerializer(val.GetType());
                        serializer.Serialize(writer, val, null);
                        writer.WriteEndElement();
                    }
                }
            }
        }
        void WriteValue(XmlWriter writer, object val)
        {
            if (val != null)
            {
                if (val is string)
                {
                    writer.WriteAttributeString("value", val.ToString());
                }
                else
                {
                    TypeConverter c = TypeDescriptor.GetConverter(val.GetType());
                    writer.WriteAttributeString("value", c.ConvertToInvariantString(val));
                }
            }
        }
        public void Save(string fileName)
        {
            using (XmlTextWriter writer = new XmlTextWriter(fileName, Encoding.UTF8))
            {
                writer.Formatting = Formatting.Indented;
                writer.WriteStartElement("Properties");
                WriteProperties(writer);
                writer.WriteEndElement();
            }
        }
        public static Properties Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return null;
            }
            using (XmlTextReader reader = new XmlTextReader(fileName))
            {
                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        switch (reader.LocalName)
                        {
                            case "Properties":
                                Properties properties = new Properties();
                                properties.ReadProperties(reader, "Properties");
                                return properties;
                        }
                    }
                }
            }
            return null;
        }
        public T Get<T>(string property, T defaultValue)
        {
            lock (properties)
            {
                object o;
                if (!properties.TryGetValue(property, out o))
                {
                    properties.Add(property, defaultValue);
                    return defaultValue;
                }
                if (o is string && typeof(T) != typeof(string))
                {
                    TypeConverter c = TypeDescriptor.GetConverter(typeof(T));
                    try
                    {
                        o = c.ConvertFromInvariantString(o.ToString());
                    }
                    catch (Exception ex)
                    {
                        o = defaultValue;
                    }
                    properties[property] = o;
                }
                else if (o is ArrayList && typeof(T).IsArray)
                {
                    ArrayList list = (ArrayList)o;
                    Type elementType = typeof(T).GetElementType();
                    Array arr = System.Array.CreateInstance(elementType, list.Count);
                    TypeConverter c = TypeDescriptor.GetConverter(elementType);
                    try
                    {
                        for (int i = 0; i < arr.Length; ++i)
                        {
                            if (list[i] != null)
                            {
                                arr.SetValue(c.ConvertFromInvariantString(list[i].ToString()), i);
                            }
                        }
                        o = arr;
                    }
                    catch (Exception ex)
                    {
                        o = defaultValue;
                    }
                    properties[property] = o;
                }
                else if (!(o is string) && typeof(T) == typeof(string))
                {
                    TypeConverter c = TypeDescriptor.GetConverter(typeof(T));
                    if (c.CanConvertTo(typeof(string)))
                    {
                        o = c.ConvertToInvariantString(o);
                    }
                    else
                    {
                        o = o.ToString();
                    }
                }
                else if (o is SerializedValue)
                {
                    try
                    {
                        o = ((SerializedValue)o).Deserialize<T>();
                    }
                    catch (Exception ex)
                    {
                        o = defaultValue;
                    }
                    properties[property] = o;
                }
                try
                {
                    return (T)o;
                }
                catch (NullReferenceException)
                {
                    return defaultValue;
                }
            }
        }
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
    public static class PropertyService
    {
        static string propertyFileName;
        static string propertyXmlRootNodeName;
        static string configDirectory;
        static string dataDirectory;
        static Properties properties;
        public static bool Initialized
        {
            get { return properties != null; }
        }
        public static void InitializeServiceForUnitTests()
        {
            properties = null;
            InitializeService(null, null, null);
        }
        public static void InitializeService(string configDirectory, string dataDirectory, string propertiesName)
        {
            if (properties != null)
                throw new InvalidOperationException("Service is already initialized.");
            properties = new Properties();
            PropertyService.configDirectory = configDirectory;
            PropertyService.dataDirectory = dataDirectory;
            propertyXmlRootNodeName = propertiesName;
            propertyFileName = propertiesName + ".xml";
            properties.PropertyChanged += new PropertyChangedEventHandler(PropertiesPropertyChanged);
        }
        public static string ConfigDirectory
        {
            get
            {
                return configDirectory;
            }
        }
        public static string DataDirectory
        {
            get
            {
                return dataDirectory;
            }
        }
        public static string Get(string property)
        {
            return properties[property];
        }
        public static T Get<T>(string property, T defaultValue)
        {
            return properties.Get(property, defaultValue);
        }
        public static void Set<T>(string property, T value)
        {
            properties.Set(property, value);
        }
        public static void Load()
        {
            if (properties == null)
                throw new InvalidOperationException("Service is not initialized.");
            if (string.IsNullOrEmpty(configDirectory) || string.IsNullOrEmpty(propertyXmlRootNodeName))
                throw new InvalidOperationException("No file name was specified on service creation");
            if (!Directory.Exists(configDirectory))
            {
                Directory.CreateDirectory(configDirectory);
            }
            if (!LoadPropertiesFromStream(Path.Combine(configDirectory, propertyFileName)))
            {
                LoadPropertiesFromStream(Path.Combine(DataDirectory, "options", propertyFileName));
            }
        }
        public static bool LoadPropertiesFromStream(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return false;
            }
            try
            {
                using (LockPropertyFile())
                {
                    using (XmlTextReader reader = new XmlTextReader(fileName))
                    {
                        while (reader.Read())
                        {
                            if (reader.IsStartElement())
                            {
                                if (reader.LocalName == propertyXmlRootNodeName)
                                {
                                    properties.ReadProperties(reader, propertyXmlRootNodeName);
                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch (XmlException ex)
            {
            }
            return false;
        }
        public static void Save()
        {
            if (string.IsNullOrEmpty(configDirectory) || string.IsNullOrEmpty(propertyXmlRootNodeName))
                throw new InvalidOperationException("No file name was specified on service creation");
            using (MemoryStream ms = new MemoryStream())
            {
                XmlTextWriter writer = new XmlTextWriter(ms, Encoding.UTF8);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartElement(propertyXmlRootNodeName);
                properties.WriteProperties(writer);
                writer.WriteEndElement();
                writer.Flush();
                ms.Position = 0;
                string fileName = Path.Combine(configDirectory, propertyFileName);
                using (LockPropertyFile())
                {
                    using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        ms.WriteTo(fs);
                    }
                }
            }
        }
        public static IDisposable LockPropertyFile()
        {
            Mutex mutex = new Mutex(false, "PropertyServiceSave-30F32619-F92D-4BC0-BF49-AA18BF4AC313");
            mutex.WaitOne();
            return new CallbackOnDispose(
                delegate
                {
                    mutex.ReleaseMutex();
                    mutex.Close();
                });
        }
        static void PropertiesPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(null, e);
            }
        }
        public static event PropertyChangedEventHandler PropertyChanged;
    }
}
