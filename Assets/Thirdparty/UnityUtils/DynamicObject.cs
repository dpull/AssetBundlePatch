using System;
using System.Reflection;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace dpull
{
    public static class DynamicObject
    {
        ///<summary>
        /// <para>作用 根据基类 和 子类型的type字符串创建一个子类的实例</para>
        /// <remarks>要确保要创建的对象类型和 T在一个程序集中</remarks>
        /// <param name="typename">继承于Ｔ的类名字</param>
        /// <param name="types">该类的构造函数参数</param>
        /// <param name="parameters">该类的构造函数参数</param>
        /// <returns>成功返回 类实例，失败返回null</returns>
        ///</summary>
        public static T Create<T>(string typename, Type[] types, object[] parameters)
        {
            Assembly assembly = typeof(T).Assembly;
            Type type = assembly.GetType(typename);
            if (type == null)
                return default(T);
            
            
            ConstructorInfo constructor = type.GetConstructor(types);
            if (constructor == null)
                return default(T);
            
            T result = (T)constructor.Invoke(parameters);
            return result;
        }
        
        public static T Clone<T>(T item)
        {
            MemoryStream ms = new MemoryStream();
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(ms, item);
            ms.Seek(0, SeekOrigin.Begin);
            
            return (T)bf.Deserialize(ms);
        }
        
        /// <summary>
        /// 这个限制很强的一个模板函数
        /// 生成镜像，把原类型中的属性转化为字符串赋值给目标类
        /// 目标类中字段或者属性是public，且都是字符串
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TDestination"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public static void MirrorTo<TSource, TDestination>(TSource source, /*ref*/TDestination destination)
        {            
            Type sourceType = typeof(TSource);
            Type destinationType = typeof(TDestination);      
            
            FieldInfo[] fields = destinationType.GetFields();
            foreach (FieldInfo field in fields)
            {
                string destinationValue = GetObjectValue(source, sourceType, field.Name).ToString();         
                field.SetValue(destination, destinationValue);
            }
            
            PropertyInfo[] propertys = destinationType.GetProperties();
            foreach (PropertyInfo property in propertys)
            {
                if (!property.CanWrite)
                    continue;
                
                string destinationValue = GetObjectValue(source, sourceType, property.Name).ToString();
                property.SetValue(destination, destinationValue, null);
            }
        }
        
        static object GetObjectValue(object obj, Type objType, string name)
        {
            object result = null;
            PropertyInfo property = objType.GetProperty(name);
            if (property != null)
            {
                result = property.GetValue(obj, null);
            }
            else
            {
                FieldInfo field = objType.GetField(name);
                if (field == null)
                    throw new ArgumentException(string.Format("{0}不存在类型{1}", objType.Name, name));
                
                result = field.GetValue(obj);
            }
            return result;
        }
    }    
}