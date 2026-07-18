using System.Text.Json;
using YildizCanAdmin.Shared;

namespace YildizCanAdmin.Web.Services;

// script.js'e gömülü 60 yerleşik sorunun panel kopyası (Data/builtin-questions.json).
// Uygulamadaki sorular değişirse scratchpad extract betiğiyle yeniden üretilir.
public sealed class BuiltinCatalog
{
    public IReadOnlyDictionary<string, List<BuiltinQuestion>> Topics { get; }

    public BuiltinCatalog(IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "Data", "builtin-questions.json");
        var json = File.ReadAllText(path);
        Topics = JsonSerializer.Deserialize<Dictionary<string, List<BuiltinQuestion>>>(
            json, new JsonSerializerOptions(JsonSerializerDefaults.Web)) ?? [];
    }

    public List<BuiltinQuestion> ForTopic(string topic)
        => Topics.TryGetValue(topic, out var list) ? list : [];
}
