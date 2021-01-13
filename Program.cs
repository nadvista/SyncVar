using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Reflection;
using System.Collections.Generic;

namespace SyncVarSystem
{
    class SyncVar
    {
        static byte[] Serialize (object o)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream();
            bf.Serialize(ms, o);
            byte[] answer = ms.GetBuffer();
            ms.Close();
            return answer;
        }
        static object Deserialize(byte[] o)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream ms = new MemoryStream(o);
            object answer = bf.Deserialize(ms);
            ms.Close();
            return answer;
        }

        static SyncSerializedValue[] GetFieldsWithAttribute(Type attributeType,object[] objects)
        {
            List<SyncSerializedValue> fieldSyncInfo = new List<SyncSerializedValue>();
            for (int i = 0; i < objects.Length; i++)
            {
                Type objectType = objects[i].GetType();
                var allObjectTypeFields = objectType.GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
                for (int j = 0; j < allObjectTypeFields.Length; j++)
                {
                    var field = allObjectTypeFields[j];
                    var attribute = field.GetCustomAttribute(attributeType);
                    if (attribute != null)
                    {
                        SyncSerializedValue value = new SyncSerializedValue(field.GetValue(objects[i]),field.Name,i);
                        fieldSyncInfo.Add(value);
                    }
                }
            }
            return fieldSyncInfo.ToArray();
        }
        static void PasteFieldsBack(SyncSerializedValue[] fieldsData,object[] objects)
        {
            foreach (var syncFieldData in fieldsData)
            {
                var arrayElement = objects[syncFieldData.indexInArray];
                Type arrayElementType = arrayElement.GetType();
                var test = arrayElementType.GetField(syncFieldData.varName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                test. SetValue(arrayElement,syncFieldData.value);
            }
        }

    }
    [System.Serializable]
    class SyncSerializedValue
    {

        public string varName;
        public object value;
        public int indexInArray;
        

        public SyncSerializedValue(object value, string varName, int indexInClass)
        {
            this.value = value;
            this.varName = varName;
            this.indexInArray = indexInClass;
        }
    }

    

    [AttributeUsage(AttributeTargets.Field)]
    class SyncVarAttribute : Attribute
    { }

}