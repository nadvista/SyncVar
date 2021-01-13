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
        public static List<MMB> objects;
        static void Main(string[] args)
        {
            objects = new List<MMB>();
           

            A a = new A("firstA",1);
            B b = new B(50,50);
            B b1 = new B(20,20);

            SyncSerializedValue[] array = GetFieldsWithAttribute(typeof(SyncVarAttribute));
            byte[] serialized = Serialize(array);
            SyncSerializedValue[] darray = (SyncSerializedValue[])Deserialize(serialized);

            a.value = 2;
            a.name = "secondA";
            b.speed = 20; b.value = 20;
            b1.speed = 52; b1.value = 53;
            PasteFieldsBack(darray);
            

        }
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

        static SyncSerializedValue[] GetFieldsWithAttribute(Type attributeType)
        {
            List<SyncSerializedValue> fieldSyncInfo = new List<SyncSerializedValue>();
            for (int i = 0; i < objects.Count; i++)
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
        static void PasteFieldsBack(SyncSerializedValue[] fieldsData)
        {
            foreach (var syncFieldData in fieldsData)
            {
                var arrayElement = objects[syncFieldData.indexInArray];
                Type arrayElementType = arrayElement.GetType();
                arrayElementType.GetField(syncFieldData.varName).SetValue(arrayElement,syncFieldData.value);
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

    class MMB
    {
        public MMB()
        {
            Program.objects.Add(this);
        }
    }
    class A : MMB
    {
        public string name;
        [SyncVar] public int value;

        public A(string n,int v) : base()
        {
            name = n;
            value = v;
        }
    }
    class B : MMB
    {
        [SyncVar] public int speed;
        [SyncVar] public  int value;
        public B(int s, int v) : base()
        {
            speed = s;
            value = v;
        }
    }


    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    class SyncVarAttribute : Attribute
    { }

}