using FluentResults;

namespace Automation.SharedKernel.Errors;

public class ForbiddenError(string message) : Error(message);



