namespace MyCleanTemplate.Domain.Exceptions.Base
{
    public abstract class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message)
        {
            
        }
    }
}
