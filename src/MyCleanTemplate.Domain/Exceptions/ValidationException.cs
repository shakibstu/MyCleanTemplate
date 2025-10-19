using MyCleanTemplate.Domain.Exceptions.Base;

namespace MyCleanTemplate.Domain.Exceptions;

public class ValidationException(string message, IDictionary<string, string[]> data) : BadRequestException(message, data);