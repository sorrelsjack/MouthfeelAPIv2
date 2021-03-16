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
        public string DescriptiveErrorCode { get; set; }

        public ErrorResponse(HttpStatusCode errorCode, string errorMessage, string descriptiveErrorCode = null)
        {
            ErrorCode = errorCode;
            ErrorMessage = errorMessage;
            DescriptiveErrorCode = descriptiveErrorCode;
        }
    }
}
