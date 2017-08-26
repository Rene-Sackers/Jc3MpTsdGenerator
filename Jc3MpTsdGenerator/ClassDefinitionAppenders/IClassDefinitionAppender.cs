using Jc3MpTsdGenerator.Models;

namespace Jc3MpTsdGenerator.ClassDefinitionAppenders
{
    public interface IClassDefinitionAppender
    {
        void AppendDefinition(ApiDefinition apiDefinition, Class @class, DefinitionWriterState writer);
    }
}