using System;
using System.Runtime.Serialization;

namespace Common.DAL.EF
{
    [Serializable]
    public class DalEFException : Exception
    {
        public DalEFException()
        {
        }

        public DalEFException(string message)
            : base(message)
        {
        }

        public DalEFException(string format, params object[] args)
            : base(string.Format(format, args))
        {
        }


        public DalEFException(string message, Exception innerException )
            : base(message, innerException )
        {
        }

        public DalEFException(string format, Exception innerException, params object[] args)
            : base(string.Format(format, args), innerException)
        {
        }

        protected DalEFException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
