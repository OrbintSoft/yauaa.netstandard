using Antlr4.Runtime.Tree;
using System.Collections.Concurrent;
using System.Runtime.Serialization;

namespace OrbintSoft.Yauaa.Analyzer.Parse.UserAgentNS.SerializableHelpers
{
    internal class SerializableParseTreeProperty<T> : ParseTreeProperty<T>, ISerializable
    {
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("annotations", annotations, typeof(ConcurrentDictionary<IParseTree, T>));            
        }

        public SerializableParseTreeProperty(SerializationInfo info, StreamingContext context)
        {
            annotations = (ConcurrentDictionary<IParseTree, T>)info.GetValue("annotations", typeof(ConcurrentDictionary<IParseTree, T>));
        }

        public SerializableParseTreeProperty()
        {

        }
    }
}
