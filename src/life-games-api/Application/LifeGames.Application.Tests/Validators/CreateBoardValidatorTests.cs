using FluentAssertions;
using FluentValidation.TestHelper;
using LifeGames.Application.DTOs;
using LifeGames.Application.Handlers;
using LifeGames.Application.Validators;

namespace LifeGames.Application.Tests.Validators;

public class CreateBoardValidatorTests
{
    private readonly CreateBoardValidator _validator;

    public CreateBoardValidatorTests()
    {
        _validator = new CreateBoardValidator();
    }

    [Fact]
    public void Validate_ValidCommand_PassesValidation()
    {
        var command = new CreateBoardCommand("Test Board", [new CellDto(0, 0)]);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NullCells_FailsValidation()
    {
        var command = new CreateBoardCommand("Test Board", null!);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Cells)
            .WithErrorMessage("Cells collection cannot be null");
    }

    [Fact]
    public void Validate_EmptyCells_PassesValidation()
    {
        var command = new CreateBoardCommand("Test Board", []);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NullName_PassesValidation()
    {
        var command = new CreateBoardCommand(null, [new CellDto(0, 0)]);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NameExceeds200Characters_FailsValidation()
    {
        var longName = new string('a', 201);
        var command = new CreateBoardCommand(longName, [new CellDto(0, 0)]);

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorMessage("Board name cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_NameExactly200Characters_PassesValidation()
    {
        var name = new string('a', 200);
        var command = new CreateBoardCommand(name, [new CellDto(0, 0)]);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NameLessThan200Characters_PassesValidation()
    {
        var command = new CreateBoardCommand("Short Name", [new CellDto(0, 0)]);

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
