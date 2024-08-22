using System;
using System.Data;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Reflection;
using System.Globalization;
using System.Xml.Linq;
using System.Linq;

namespace Utils
{
    public class Xml
    {
        #region declare event
        public delegate void OnXmlNotify();
        public static event OnXmlNotify XmlNotify;
        #endregion  declare event
        private Xml()
        {
        }

        #region Serialize given object into stream.
        /// <summary>
        /// Serialize given object into XmlElement.
        /// </summary>
        /// <param name="transformObject">Input object for serialization.</param>
        /// <returns>Returns serialized XmlElement.</returns>
        public static XmlElement Serialize(object transformObject)
        {
            XmlElement serializedElement = null;
            try
            {
                MemoryStream memStream = new MemoryStream();
                XmlSerializer serializer = new XmlSerializer(transformObject.GetType());
                serializer.Serialize(memStream, transformObject);
                memStream.Position = 0;
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(memStream);
                serializedElement = xmlDoc.DocumentElement;
            }
            catch (Exception SerializeException)
            {
                throw SerializeException;
            }
            return serializedElement;
        }
        #endregion // End - Serialize given object into stream.

        #region Deserialize given string into object.
        /// <summary>
        /// Deserialize given XmlElement into object.
        /// </summary>
        /// <param name="xmlElement">xmlElement to deserialize.</param>
        /// <param name="tp">Type of resultant deserialized object.</param>
        /// <returns>Returns deserialized object.</returns>
        public static object Deserialize(XmlElement xmlElement, System.Type tp)
        {
            Object transformedObject = null;
            try
            {
                Stream memStream = StringToStream(xmlElement.OuterXml);
                XmlSerializer serializer = new XmlSerializer(tp);
                transformedObject = serializer.Deserialize(memStream);
            }
            catch (Exception DeserializeException)
            {
                throw DeserializeException;
            }
            return transformedObject;
        }
        #endregion Deserialize given string into object.

        #region Conversion from string to stream.
        /// <summary>
        /// Conversion from string to stream.
        /// </summary>
        /// <param name="str">Input string.</param>
        /// <returns>Returns stream.</returns>
        public static Stream StringToStream(String str)
        {
            MemoryStream memStream = null;
            try
            {
                byte[] buffer = Encoding.UTF8.GetBytes(str);//new byte[str.Length];
                memStream = new MemoryStream(buffer);
            }
            catch (Exception StringToStreamException)
            {
                throw StringToStreamException;
            }
            finally
            {
                memStream.Position = 0;
            }

            return memStream;
        }
        #endregion Conversion from string to stream.

        #region Write object to xml file
        /// <summary>
        /// Serialize and Write object to xml file.
        /// </summary>
        /// <param name="obj">Instance of an object</param>
        /// <param name="fileName">Xml file name</param>
        public static void WriteToFile(object obj, string fileName)
        {
            System.Xml.Serialization.XmlSerializer writer = new System.Xml.Serialization.XmlSerializer(obj.GetType());
            System.IO.StreamWriter file = new System.IO.StreamWriter(fileName);
            writer.Serialize(file, obj);
            file.Close();
        }
        #endregion

        #region Write list object to xml file
        /// <summary>
        /// Write list object to xml file.
        /// </summary>
        /// <typeparam name="T">DataType</typeparam>
        /// <param name="objectList">List object</param>
        /// <param name="fileName">Xml file name</param>
        public static void WriteListToFile<T>(IEnumerable<T> objectList, string fileName)
        {
            if (!File.Exists(fileName))
            {
                if (objectList is IList<T>)
                    WriteToFile(objectList, fileName);
                else
                    WriteToFile(objectList.ToList(), fileName);
            }
            else
            {
                List<T> listObj = ReadFromFileToList<T>(fileName);
                listObj.AddRange(objectList);
                WriteToFile(listObj, fileName);
            }
            //register event
            if (XmlNotify != null)
            {
                XmlNotify();
            }
        }
        #endregion

        #region Read from xml file to list object
        /// <summary>
        /// Read data from xml file to a list object
        /// </summary>
        /// <typeparam name="T">DataType</typeparam>
        /// <param name="fileName">fileName</param>
        /// <returns>List object</returns>
        public static List<T> ReadFromFileToList<T>(string fileName)
        {
            using (FileStream fs = File.OpenRead(fileName))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<T>));
                return (List<T>)serializer.Deserialize(fs);
            }
        }
        #endregion
    }
}
