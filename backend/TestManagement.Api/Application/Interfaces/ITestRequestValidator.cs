using TestManagement.Api.Application.DTOs;

namespace TestManagement.Api.Application.Interfaces;

public interface ITestRequestValidator
{
    void Validate(TestWriteRequest request);
}
