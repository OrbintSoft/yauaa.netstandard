using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrbintSoft.Yauaa.WebSample.AspNetCore.Exceptions
{
    public class InvalidHtmlAttributeException : Exception
    {
        public InvalidHtmlAttributeException() : base()
        {

        }

        public InvalidHtmlAttributeException(string message) : base(message)
        {

        }

        public InvalidHtmlAttributeException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
