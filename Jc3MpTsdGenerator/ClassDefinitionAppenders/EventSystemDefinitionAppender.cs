using System.Linq;
using Jc3MpTsdGenerator.Extensions;
using Jc3MpTsdGenerator.Helpers;
using Jc3MpTsdGenerator.Models;

namespace Jc3MpTsdGenerator.ClassDefinitionAppenders
{
    public class EventSystemDefinitionAppender : IClassDefinitionAppender
    {
        private const string EventSystemClassName = "EventSystem";

        public void AppendDefinition(ApiDefinition apiDefinition, Class @class, DefinitionWriterState writer)
        {
            if (@class.ClassName != EventSystemClassName)
                return;

            apiDefinition.Events.ToList().ForEach(@event =>  AppendEventDefinition(@event, writer));
        }

        private static string SanitizeEventParameterName(string name)
        {
            if (name == "entity_manager->component<VehicleScriptingComponent>(vehicle_handle)")
                return "vehicle";

            if (name.Contains("."))
                return name.Split('.').Last();

            return name;
        }

        private static void AppendEventDefinition(Event @event, DefinitionWriterState writer)
        {
            if (!string.IsNullOrWhiteSpace(@event.Description))
            {
                writer.WriteLine("/**");
                writer.WriteCommentBlockContent(@event.Description);
                writer.WriteLine(" */");
            }

            @event.Parameters.ToList().ForEach(parameter => parameter.Name = SanitizeEventParameterName(parameter.Name));

            writer.WriteLine($"Add(name: '{@event.Name}', handler: {MethodParameterRendering.RenderMethodParameters(@event.Parameters)} => any): void;");
            writer.WriteBlankLine();
        }
    }
}