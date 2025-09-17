using System.Net;

namespace Vanigam.CRM.Client
{
    public static class HttpHelper
    {
        public static bool IsPreconditionFailed(this HttpResponseMessage httpResponseMessage)
        {
            return httpResponseMessage.StatusCode == HttpStatusCode.PreconditionFailed;
            var resultInt = (int)httpResponseMessage.StatusCode;
            return resultInt >= 400;
        }
        public static bool IsSuccess(this HttpResponseMessage httpResponseMessage)
        {
            var resultInt = (int)httpResponseMessage.StatusCode;
            return resultInt < 400;
        }
    }
}

