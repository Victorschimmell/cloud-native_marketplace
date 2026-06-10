namespace Backend.Application.Common.Abstractions;

public interface IIdGenerator
{
    Guid NewGuid();
}
