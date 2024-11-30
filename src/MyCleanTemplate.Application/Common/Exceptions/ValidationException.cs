using MyCleanTemplate.Domain.Exceptions.Base;

namespace MyCleanTemplate.Application.Common.Exceptions;

public class ValidationException(string message, IDictionary<string, string[]> data) : BadRequestException(message, data);