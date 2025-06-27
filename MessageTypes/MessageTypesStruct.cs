using System.ComponentModel;
using System.Reflection;

namespace LVCMod
{
    readonly struct MessageType
    {
        private readonly MessageTypes _value;
        private readonly string _description;

        public MessageType(MessageTypes value)
        {
            _value = value;
            _description = GetDescription(value);
        }

        public static implicit operator MessageType(MessageTypes value) => new(value);

        public static implicit operator string(MessageType type) => type._description;

        private static string GetDescription(MessageTypes value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();
            return attribute?.Description ?? value.ToString();
        }
    }
}
