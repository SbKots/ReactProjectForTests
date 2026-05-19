using Microsoft.AspNetCore.Mvc;
using TestManagement.Api.Application.DTOs;
using TestManagement.Api.Application.Interfaces;

namespace TestManagement.Api.Presentation.Controllers;

[ApiController]
[Route("api/tests")]
public sealed class TestsController(
    ITestService testService,
    ITestTakingService testTakingService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TestSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TestSummaryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var tests = await testService.GetAllAsync(cancellationToken);
        return Ok(tests);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TestDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestDetailsDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var test = await testService.GetByIdAsync(id, cancellationToken);
        return Ok(test);
    }

    [HttpGet("{id:guid}/take")]
    [ProducesResponseType(typeof(TestForTakingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestForTakingDto>> GetForTaking(Guid id, CancellationToken cancellationToken)
    {
        var test = await testService.GetForTakingAsync(id, cancellationToken);
        return Ok(test);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TestDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TestDetailsDto>> Create(
        CreateTestRequest request,
        CancellationToken cancellationToken)
    {
        var createdTest = await testService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = createdTest.Id }, createdTest);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(TestDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestDetailsDto>> Update(
        Guid id,
        UpdateTestRequest request,
        CancellationToken cancellationToken)
    {
        var updatedTest = await testService.UpdateAsync(id, request, cancellationToken);
        return Ok(updatedTest);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await testService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/submissions")]
    [ProducesResponseType(typeof(TestSubmissionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TestSubmissionResultDto>> Submit(
        Guid id,
        SubmitTestRequest request,
        CancellationToken cancellationToken)
    {
        var result = await testTakingService.SubmitAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
