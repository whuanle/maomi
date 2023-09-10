using Maomi.I18n;
using Microsoft.Extensions.Configuration;
using System.Buffers;
using System.Text;
using System.Text.Json;

public class Program
{
    static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
    static void Main()
    {
        const string json =
            """
            {
                "Name": "工良",
                "Netwotk1": "IPV4",
                "Netwotk2": "IPV6"
            }
            """;
        jsonSerializerOptions.Converters.Add(new EnumStringConverterFactory());
        jsonSerializerOptions.Converters.Add(new CustomDateTimeConverter("yyyy/MM/dd HH:mm:ss"));
        var obj = JsonSerializer.Deserialize<Model>(json, jsonSerializerOptions);

        // 注意，不能直接读取文件为字节数组，因为文件有 bom 头
        var text = Encoding.UTF8.GetBytes(File.ReadAllText("read.json"));
        var dic = ReadJsonHelper.Read(new ReadOnlySequence<byte>(text), new JsonReaderOptions { AllowTrailingCommas = true });

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(dic.ToDictionary(x => x.Key, x => x.Value.ToString()))
            .Build();
    }
}