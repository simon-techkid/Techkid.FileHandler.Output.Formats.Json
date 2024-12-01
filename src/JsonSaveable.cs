﻿// Techkid.FileHandler by Simon Field

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml.Linq;

namespace Techkid.FileHandler.Output.Formats.Json;

/// <summary>
/// Provides instructions for serializing and transforming JSON data using System.Text.Json.
/// </summary>
public abstract class JsonSaveable<TRecord>(Func<TRecord> doc, string? trackName) :
    SaveableAndTransformableBase<TRecord, List<JsonDocument>, List<JsonDocument>>(doc, trackName)
{

    /// <summary>
    /// The <see cref="JsonSerializerOptions"/> for the exported contents of this JSON document.
    /// </summary>
    protected abstract JsonSerializerOptions JsonOptions { get; }

    protected override byte[] ConvertToBytes()
    {
        JsonArray allTracks = new();
        Document.ForEach(doc => allTracks.Add(doc.RootElement));
        JsonElement encapsulatedTracks = JsonDocument.Parse(allTracks.ToJsonString()).RootElement;
        string document = JsonSerializer.Serialize(encapsulatedTracks, JsonOptions);

        return OutputEncoding.GetBytes(document);
    }

    protected override XDocument TransformToXml()
    {
        XElement root = new("Root");

        foreach (JsonDocument doc in Document)
        {
            JsonElement obj = doc.RootElement;
            XElement element = JsonToXElement(obj);
            root.Add(element);
        }

        return new XDocument(root);
    }

    private static XElement JsonToXElement(JsonElement element)
    {
        XElement xElement;

        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                xElement = new XElement("Object");
                foreach (JsonProperty property in element.EnumerateObject())
                {
                    XElement childElement = JsonToXElement(property.Value);
                    childElement.Name = property.Name;
                    xElement.Add(childElement);
                }
                break;
            case JsonValueKind.Array:
                xElement = new XElement("Array");
                foreach (JsonElement item in element.EnumerateArray())
                {
                    XElement childElement = JsonToXElement(item);
                    xElement.Add(childElement);
                }
                break;
            default:
                xElement = new XElement("Value", element.ToString());
                break;
        }

        return xElement;
    }

    protected override List<JsonDocument> ClearDocument()
    {
        return new();
    }
}
