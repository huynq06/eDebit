using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Web.Services.Protocols;

namespace Utils
{
    public class CustomException : Exception
    {
        public CustomException()
            : base()
        {
        }

        public CustomException(string message)
            : base(message)
        {
        }

        public CustomException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class CustomTimeoutException : TimeoutException
    {
        public CustomTimeoutException()
            : base()
        {
        }

        public CustomTimeoutException(string message)
            : base(message)
        {
        }

        public CustomTimeoutException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class CustomWebException : WebException
    {
        public CustomWebException()
            : base()
        {
        }

        public CustomWebException(string message)
            : base(message)
        {
        }

        public CustomWebException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public CustomWebException(string message, WebExceptionStatus status)
            : base(message, status)
        {
        }

        public CustomWebException(string message, Exception innerException, WebExceptionStatus status, WebResponse response)
            : base(message, innerException, status, response)
        {
        }
    }

    public class CustomSoapException : SoapException
    {
        public CustomSoapException()
            : base()
        {
        }
    }
}
