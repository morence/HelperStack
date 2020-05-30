using System;
using System.Web.Script.Serialization;

namespace EBayAPI.Infrastructure
{
    public class JsonHelper
    {
        public static T Deserialize<T>(string jsonStr)
        {
            try
            {
                var serializer = new  JavaScriptSerializer();
                return serializer.Deserialize<T>(jsonStr);
            }
            catch (Exception ex)
            {
                return default(T);
            }
        }

        public static string Serialize<T>(T obj)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                return serializer.Serialize(obj);
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }
    }
}
