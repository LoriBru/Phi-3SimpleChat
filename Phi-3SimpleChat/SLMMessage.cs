using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phi_3SimpleChat
{
    public enum SLMMessageType
    {
        User,
        Assistant
    }

    public class SLMMessage
    {
        public string Text { get; set; }

        public DateTime Time { get; private set; }

        public SLMMessageType Type { get; set; }

        public SLMMessage(string text, DateTime dateTime, SLMMessageType type)
        {
            Text = text;
            Time = dateTime;
            Type = type;
        }

        public override string ToString()
        {
            return Time.ToString() + " " + Text;
        }
    }
}
