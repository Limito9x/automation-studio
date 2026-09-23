using FluentResults;

namespace Automation.SharedKernel.Errors;

public class ValidationError(string message) : Error(message);



