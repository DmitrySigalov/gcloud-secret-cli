using Newtonsoft.Json;

namespace Google.Cloud.SecretManager.Client.Common;

public static class JsonSerializationHelper
{
    public static TObject Deserialize<TObject>(string source)
    {
        if (source == null)
        {
            return default(TObject);
        }

        return JsonConvert.DeserializeObject<TObject>(source);
    }

    public static string Serialize<TObject>(TObject source)
    {
        return JsonConvert.SerializeObject(source, Formatting.Indented);
    }
}