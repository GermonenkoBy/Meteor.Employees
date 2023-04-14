using Meteor.Common.Core.Exceptions;

namespace Meteor.Employees.Core.Exceptions;

public class MissingCustomerDataException : MeteorException
{
    public MissingCustomerDataException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}