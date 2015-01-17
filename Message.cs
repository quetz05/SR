using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Diagnostics.Contracts;

namespace SR
{
    //[ProtoContract]
    //public class Message<T>
    //{
    //    [ProtoMember(1)]
    //    public string From { get; private set; }
    //    [ProtoMember(2)]
    //    public string To { get; private set; }
    //    [ProtoMember(3)]
    //    public T MessageBody { get; private set; }

    //    public Message()
    //    {

    //    }

    //    public Message(string from, string to, T messageBody)
    //    {
    //        this.From = from;
    //        this.To = to;
    //        this.MessageBody = messageBody;
    //    }

    //    public byte[] Serialize()
    //    {
    //        byte[] msgOut;

    //        using (var stream = new MemoryStream())
    //        {
    //            Serializer.Serialize(stream, this);
    //            msgOut = stream.GetBuffer();
    //        }

    //        return msgOut;
    //    }

    //    public static Message<T> Deserialize(byte[] message)
    //    {
    //        Message<T> msgOut;

    //        using (var stream = new MemoryStream(message))
    //        {
    //            msgOut = Serializer.Deserialize<Message<T>>(stream);
    //        }

    //        return msgOut;
    //    }
    //}
}
