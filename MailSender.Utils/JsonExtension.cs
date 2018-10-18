using Newtonsoft.Json;

namespace MailSender.Utils
{
    public static class JsonExtension
    {
        public static string ToJson<T>(this T source)
        {
            return JsonConvert.SerializeObject(source, Formatting.Indented);
        }


        public static T FromJson<T>(this string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }

    public static class StringExtension
    {
        public static int ToInt(this string source, int defaultValue = 0)
        {
            if (int.TryParse(source, out var i))
                return i;
            return defaultValue;
        }
    }
}

