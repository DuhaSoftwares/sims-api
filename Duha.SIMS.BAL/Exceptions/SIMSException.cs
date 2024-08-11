using Duha.SIMS.DomainModels.Base;

namespace Duha.SIMS.BAL.Exceptions
{
    public class SIMSException : System.Exception
    {
        public ExceptionTypeDM ExceptionType { get; }

        public SIMSException(ExceptionTypeDM exceptionType, string message)
            : base(message)
        {
            ExceptionType = exceptionType;
        }
    }
}
