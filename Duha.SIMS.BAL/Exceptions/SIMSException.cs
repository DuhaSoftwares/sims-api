using Duha.SIMS.DomainModels.Base;
using Duha.SIMS.ServiceModels.Enums;

namespace Duha.SIMS.BAL.Exceptions
{
    public class SIMSException : System.Exception
    {
        public ExceptionTypeDM ExceptionType { get; }
        public ApiErrorTypeSM _apiErrorType { get; }

        public SIMSException(ExceptionTypeDM exceptionType, string message)
            : base(message)
        {
            ExceptionType = exceptionType;
        }
        public SIMSException(ApiErrorTypeSM apiErrorType, string message)
            : base(message)
        {
            _apiErrorType = apiErrorType;
        }
    }
}
