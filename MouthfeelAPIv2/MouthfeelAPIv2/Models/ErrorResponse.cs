using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MouthfeelAPIv2.Models
{
    public class ErrorResponse : Exception
    {
        public HttpStatusCode? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }

        public ErrorResponse(HttpStatusCode errorCode, string errorMessage)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
        }
    }
}
