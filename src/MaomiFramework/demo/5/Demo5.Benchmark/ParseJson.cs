using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

[SimpleJob(RuntimeMoniker.Net70)]
[SimpleJob(RuntimeMoniker.NativeAot70)]
[MemoryDiagnoser]
[ThreadingDiagnoser]
[MarkdownExporter, AsciiDocExporter, HtmlExporter, CsvExporter, RPlotExporter]
public class ParseJson
{
	private ReadOnlySequence<byte> sequence;

	[Params("100.json", "1000.json", "10000.json")]
	public string FileName;

	[GlobalSetup]
	public async Task Setup()
	{
		var text = File.ReadAllText(Path.Combine(Environment.CurrentDirectory, $"json/{FileName}"));
		var bytes = Encoding.UTF8.GetBytes(text);
		sequence = new ReadOnlySequence<byte>(bytes);
	}

	[Benchmark]
	public void Utf8JsonReader()
	{
		var reader = new Utf8JsonReader(sequence, new JsonReaderOptions());
		U8Read(ref reader);
	}

	private static void U8Read(ref Utf8JsonReader reader)
	{
		while (reader.Read())
		{
			if (reader.TokenType is JsonTokenType.StartArray)
			{
				U8ReadArray(ref reader);
			}
			else if (reader.TokenType is JsonTokenType.EndObject) break;
			else if (reader.TokenType is JsonTokenType.PropertyName)
			{
				reader.Read();
				if (reader.TokenType is JsonTokenType.StartArray)
				{
					// 进入数组处理
					U8ReadArray(ref reader);
				}
				else if (reader.TokenType is JsonTokenType.StartObject)
				{
					U8Read(ref reader);
				}
				else
				{
				}
			}
		}
	}

	private static void U8ReadArray(ref Utf8JsonReader reader)
	{
		while (reader.Read())
		{
			if (reader.TokenType is JsonTokenType.EndArray) break;
			switch (reader.TokenType)
			{
				case JsonTokenType.StartObject:
					U8Read(ref reader);
					break;
				// [...,[],...]
				case JsonTokenType.StartArray:
					U8ReadArray(ref reader);
					break;
			}
		}
	}

	[Benchmark]
	public void JsonNode()
	{
		var reader = new Utf8JsonReader(sequence, new JsonReaderOptions());
		var nodes = System.Text.Json.Nodes.JsonNode.Parse(ref reader, null);
		if (nodes is JsonObject o)
		{
			JNRead(o);
		}
		else if (nodes is JsonArray a)
		{
			JNArray(a);
		}
	}

	private static void JNRead(JsonObject obj)
	{
		foreach (var item in obj)
		{
			var v = item.Value;
			if (v is JsonObject o)
			{
				JNRead(o);
			}
			else if (v is JsonArray a)
			{
				JNArray(a);
			}
			else if (v is JsonValue value)
			{
				var el = value.GetValue<JsonElement>();
				JNValue(el);
			}
		}
	}

	private static void JNArray(JsonArray obj)
	{
		foreach (var v in obj)
		{
			if (v is JsonObject o)
			{
				JNRead(o);
			}
			else if (v is JsonArray a)
			{
				JNArray(a);
			}
			else if (v is JsonValue value)
			{
				var el = value.GetValue<JsonElement>();
				JNValue(el);
			}
		}
	}

	private static void JNValue(JsonElement obj)
	{
	}
}