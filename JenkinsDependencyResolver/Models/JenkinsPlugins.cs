using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace JenkinsDependencyResolver.Models
{
    public partial class JenkinsPlugins
    {
        [JsonProperty("connectionCheckUrl")]
        public Uri ConnectionCheckUrl { get; set; }

        [JsonProperty("core")]
        public Core Core { get; set; }

        [JsonProperty("deprecations")]
        public Dictionary<string, Deprecation> Deprecations { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("plugins")]
        public Dictionary<string, PluginValue> Plugins { get; set; }

        [JsonProperty("signature")]
        public Signature Signature { get; set; }

        [JsonProperty("updateCenterVersion")]
        [JsonConverter(typeof(ParseStringConverter))]
        public long UpdateCenterVersion { get; set; }

        [JsonProperty("warnings")]
        public Warning[] Warnings { get; set; }
    }

    public partial class Core
    {
        [JsonProperty("buildDate")]
        public string BuildDate { get; set; }

        [JsonProperty("name")]
        public Name Name { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("sha256")]
        public string Sha256 { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public partial class Deprecation
    {
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }

    public partial class PluginValue
    {
        [JsonProperty("buildDate")]
        public string BuildDate { get; set; }

        [JsonProperty("dependencies")]
        public Dependency[] Dependencies { get; set; }

        [JsonProperty("developers")]
        public Developer[] Developers { get; set; }

        [JsonProperty("excerpt")]
        public string Excerpt { get; set; }

        [JsonProperty("gav")]
        public string Gav { get; set; }

        [JsonProperty("labels")]
        public string[] Labels { get; set; }

        [JsonProperty("minimumJavaVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string MinimumJavaVersion { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("popularity")]
        public long Popularity { get; set; }

        [JsonProperty("previousTimestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? PreviousTimestamp { get; set; }

        [JsonProperty("previousVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string PreviousVersion { get; set; }

        [JsonProperty("releaseTimestamp")]
        public DateTimeOffset ReleaseTimestamp { get; set; }

        [JsonProperty("requiredCore")]
        public string RequiredCore { get; set; }

        [JsonProperty("scm", NullValueHandling = NullValueHandling.Ignore)]
        public string Scm { get; set; }

        [JsonProperty("sha1")]
        public string Sha1 { get; set; }

        [JsonProperty("sha256")]
        public string Sha256 { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("wiki")]
        public Uri Wiki { get; set; }

        [JsonProperty("compatibleSinceVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string CompatibleSinceVersion { get; set; }
    }

    public partial class Dependency
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("optional")]
        public bool Optional { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }
    }

    public partial class Developer
    {
        [JsonProperty("developerId", NullValueHandling = NullValueHandling.Ignore)]
        public string DeveloperId { get; set; }

        [JsonProperty("email", NullValueHandling = NullValueHandling.Ignore)]
        public string Email { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }

    public partial class Signature
    {
        [JsonProperty("certificates")]
        public string[] Certificates { get; set; }

        [JsonProperty("correct_digest")]
        public string CorrectDigest { get; set; }

        [JsonProperty("correct_digest512")]
        public string CorrectDigest512 { get; set; }

        [JsonProperty("correct_signature")]
        public string CorrectSignature { get; set; }

        [JsonProperty("correct_signature512")]
        public string CorrectSignature512 { get; set; }
    }

    public partial class Warning
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public Name Type { get; set; }

        [JsonProperty("url")]
        public Uri Url { get; set; }

        [JsonProperty("versions")]
        public Version[] Versions { get; set; }
    }

    public partial class Version
    {
        [JsonProperty("lastVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string LastVersion { get; set; }

        [JsonProperty("pattern")]
        public string Pattern { get; set; }

        [JsonProperty("firstVersion", NullValueHandling = NullValueHandling.Ignore)]
        public string FirstVersion { get; set; }
    }

    public enum Name { Core, Plugin };

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                NameConverter.Singleton,
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }

    internal class NameConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(Name) || t == typeof(Name?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            switch (value)
            {
                case "core":
                    return Name.Core;
                case "plugin":
                    return Name.Plugin;
            }
            throw new Exception("Cannot unmarshal type Name");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (Name)untypedValue;
            switch (value)
            {
                case Name.Core:
                    serializer.Serialize(writer, "core");
                    return;
                case Name.Plugin:
                    serializer.Serialize(writer, "plugin");
                    return;
            }
            throw new Exception("Cannot marshal type Name");
        }

        public static readonly NameConverter Singleton = new NameConverter();
    }

    internal class ParseStringConverter : JsonConverter
    {
        public override bool CanConvert(Type t) => t == typeof(long) || t == typeof(long?);

        public override object ReadJson(JsonReader reader, Type t, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null) return null;
            var value = serializer.Deserialize<string>(reader);
            long l;
            if (Int64.TryParse(value, out l))
            {
                return l;
            }
            throw new Exception("Cannot unmarshal type long");
        }

        public override void WriteJson(JsonWriter writer, object untypedValue, JsonSerializer serializer)
        {
            if (untypedValue == null)
            {
                serializer.Serialize(writer, null);
                return;
            }
            var value = (long)untypedValue;
            serializer.Serialize(writer, value.ToString());
            return;
        }

        public static readonly ParseStringConverter Singleton = new ParseStringConverter();
    }
}

