using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Serialization;
using CW_JP_PUZZLES.Models;

namespace CW_JP_PUZZLES.Data
{
    public static class XmlConfigProvider
    {
        public static void SaveToFile<T>(T data, string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                serializer.Serialize(writer, data);
            }
        }
        public static T LoadFromFile<T>(string filePath) where T : new()
        {
            if (!File.Exists(filePath))
            {
                return new T();
            }

            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using (StreamReader reader = new StreamReader(filePath))
            {
                return (T)serializer.Deserialize(reader)!;
            }
        }
    }
}