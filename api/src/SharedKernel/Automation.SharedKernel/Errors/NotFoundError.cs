using FluentResults;

namespace Automation.SharedKernel.Errors;

public class NotFoundError(string message) : Error(message);



