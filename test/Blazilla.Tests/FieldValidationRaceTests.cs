using FluentValidation;

using Microsoft.AspNetCore.Components.Forms;

namespace Blazilla.Tests;

/// <summary>
/// Tests the threading contract of async validation: results reach the message store on the renderer
/// dispatcher, serialized with every other reader and writer of the store (renders, other validation
/// message stores), and a field result that a full validation has since superseded is discarded
/// instead of overwriting the newer state.
/// </summary>
public class FieldValidationRaceTests : BunitContext
{
    [Fact]
    public async Task FullValidation_AsyncMode_AppliesResultsOnTheRendererDispatcher()
    {
        // Arrange: an async rule that answers synchronously, the shape of most real async-mode validators
        var validator = new GatedValidator();
        var model = new GatedModel { Name = "taken" };
        var editContext = RenderForm(model, validator);
        validator.Answer(false);

        // Act: request validation on the dispatcher and read the store before yielding it
        List<string>? messagesOnReturn = null;
        await InvokeOnRenderer(() =>
        {
            editContext.Validate();
            messagesOnReturn = editContext.GetValidationMessages().ToList();
        });

        // Assert: the results were applied inline on the dispatcher, not handed to a pool thread
        // that would write the store concurrently with whatever the dispatcher runs next
        messagesOnReturn.Should().ContainSingle(GatedValidator.NameTaken);
    }

    [Fact]
    public async Task FieldValidation_CompletingAfterAFullValidation_DoesNotOverwriteIt()
    {
        // Arrange
        var validator = new GatedValidator();
        var model = new GatedModel { Name = "available" };
        var editContext = RenderForm(model, validator);
        var field = new FieldIdentifier(model, nameof(GatedModel.Name));
        var staleFieldCheck = validator.Gate();

        await InvokeOnRenderer(() => editContext.NotifyFieldChanged(field));

        // Act: a full validation runs and passes while the field check is still in flight
        var isValid = await InvokeOnRenderer(() => editContext.ValidateAsync());
        staleFieldCheck.SetResult(false);
        await InvokeOnRenderer(() => { });

        // Assert
        isValid.Should().BeTrue();
        editContext.GetValidationMessages().Should().BeEmpty();
    }

    [Fact]
    public async Task FieldValidation_CompletingAfterDisposal_LeavesNoMessages()
    {
        // Arrange
        var validator = new GatedValidator();
        var model = new GatedModel { Name = "taken" };
        var editContext = RenderForm(model, validator);
        var field = new FieldIdentifier(model, nameof(GatedModel.Name));
        var fieldCheck = validator.Gate();

        await InvokeOnRenderer(() => editContext.NotifyFieldChanged(field));

        // Act: the form goes away before the field check answers
        await DisposeComponentsAsync();
        fieldCheck.SetResult(false);
        await InvokeOnRenderer(() => { });

        // Assert
        editContext.GetValidationMessages().Should().BeEmpty();
    }

    private EditContext RenderForm(GatedModel model, GatedValidator validator)
    {
        var component = Render<TestFormComponent<GatedModel>>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.Validator, validator)
            .Add(p => p.AsyncMode, true));

        return component.Instance.EditContext!;
    }

    private Task InvokeOnRenderer(Action action) => Renderer.Dispatcher.InvokeAsync(action);

    private Task<T> InvokeOnRenderer<T>(Func<Task<T>> action) => Renderer.Dispatcher.InvokeAsync(action);

    private sealed class GatedModel
    {
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Validator whose availability check answers from a queue of tasks; a call with nothing queued
    /// answers "available" immediately.
    /// </summary>
    private sealed class GatedValidator : AbstractValidator<GatedModel>
    {
        public const string NameTaken = "Name is taken.";

        private readonly Queue<Task<bool>> _answers = new();

        public GatedValidator()
        {
            RuleFor(x => x.Name)
                .MustAsync((_, _) => _answers.TryDequeue(out var answer) ? answer : Task.FromResult(true))
                .WithMessage(NameTaken);
        }

        public TaskCompletionSource<bool> Gate()
        {
            var gate = new TaskCompletionSource<bool>();
            _answers.Enqueue(gate.Task);
            return gate;
        }

        public void Answer(bool available) => _answers.Enqueue(Task.FromResult(available));
    }
}
