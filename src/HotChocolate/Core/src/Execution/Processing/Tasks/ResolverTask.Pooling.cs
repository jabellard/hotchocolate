using System.Collections.Immutable;

namespace HotChocolate.Execution.Processing.Tasks;

internal sealed partial class ResolverTask
{
    /// <summary>
    /// Initializes this task after it is retrieved from its pool.
    /// </summary>
    public void Initialize(
        OperationContext operationContext,
        ISelection selection,
        ObjectResult parentResult,
        int responseIndex,
        object? parent,
        Path path,
        IImmutableDictionary<string, object?> scopedContextData)
    {
        _operationContext = operationContext;
        _selection = selection;
        _resolverContext.Initialize(
            operationContext,
            selection,
            parentResult,
            responseIndex,
            parent,
            path,
            scopedContextData);
        ParentResult = parentResult;
    }

    /// <summary>
    /// Resets the resolver task before returning it to the pool.
    /// </summary>
    /// <returns></returns>
    internal bool Reset()
    {
        _completionStatus = ExecutionTaskStatus.Completed;
        _operationContext = default!;
        _selection = default!;
        _resolverContext.Clean();
        ParentResult = default!;
        Status = ExecutionTaskStatus.WaitingToRun;
        IsSerial = false;
        IsRegistered = false;
        Next = null;
        Previous = null;
        State = null;
        _taskBuffer.Clear();
        return true;
    }
}
