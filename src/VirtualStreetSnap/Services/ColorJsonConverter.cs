using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media;

namespace VirtualStreetSnap.Services;

public class ColorJsonConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var colorString = reader.GetString();
        return Color.Parse(colorString!);
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
} 