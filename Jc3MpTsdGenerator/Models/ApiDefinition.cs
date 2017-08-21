using System.Collections.Generic;
using Newtonsoft.Json;

namespace Jc3MpTsdGenerator.Models
{
    public class Property
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("read_only")]
        public bool ReadOnly { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("deprecated")]
        public bool? Deprecated { get; set; }

        [JsonProperty("return_type")]
        public string ReturnType { get; set; }

        [JsonProperty("has_parameters")]
        public bool? HasParameters { get; set; }

        [JsonProperty("parameters")]
        public IList<Parameter> Parameters { get; set; }
    }

    public class Function
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("return_type")]
        public string ReturnType { get; set; }

        [JsonProperty("has_parameters")]
        public bool HasParameters { get; set; }

        [JsonProperty("parameters")]
        public IList<Parameter> Parameters { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("example")]
        public string Example { get; set; }

        [JsonProperty("unstable")]
        public bool? Unstable { get; set; }
    }

    public class Parameter
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("optional")]
        public bool Optional { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }

    public class Constructor
    {
        [JsonProperty("parameters")]
        public IList<Parameter> Parameters { get; set; }

        [JsonProperty("example_args")]
        public string ExampleArgs { get; set; }
    }

    public class Class
    {
        [JsonProperty("data_type")]
        public string DataType { get; set; }

        [JsonProperty("class_name")]
        public string ClassName { get; set; }

        [JsonProperty("has_constructor")]
        public bool HasConstructor { get; set; }

        [JsonProperty("custom_properties")]
        public bool CustomProperties { get; set; }

        [JsonProperty("auto_destroy")]
        public bool AutoDestroy { get; set; }

        [JsonProperty("constructible")]
        public bool Constructible { get; set; }

        [JsonProperty("properties")]
        public IList<Property> Properties { get; set; }

        [JsonProperty("functions")]
        public IList<Function> Functions { get; set; }

        [JsonProperty("has_properties")]
        public bool HasProperties { get; set; }

        [JsonProperty("has_functions")]
        public bool HasFunctions { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("__filled")]
        public bool Filled { get; set; }

        [JsonProperty("constructor")]
        public Constructor Constructor { get; set; }

        [JsonProperty("instance_of")]
        public string InstanceOf { get; set; }
    }

    public class Event
    {
        [JsonProperty("is_event")]
        public bool IsEvent { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("parameters")]
        public IList<Parameter> Parameters { get; set; }

        [JsonProperty("has_parameters")]
        public bool HasParameters { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("example")]
        public string Example { get; set; }
    }

    public class ApiDefinition
    {
        [JsonProperty("classes")]
        public IList<Class> Classes { get; set; }

        [JsonProperty("events")]
        public IList<Event> Events { get; set; }
    }
}