using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate.Execution.Instrumentation;
using Microsoft.Extensions.ObjectPool;

namespace HotChocolate.Execution.Processing.Tasks;

internal sealed partial class ResolverTask : IExecutionTask
{
    private readonly ObjectPool<ResolverTask> _objectPool;
    private readonly MiddlewareContext _resolverContext = new();
    private readonly List<ResolverTask> _taskBuffer = new();
    private OperationContext _operationContext = default!;
    private ISelection _selection = default!;
    private ExecutionTaskStatus _completionStatus = ExecutionTaskStatus.Completed;
    private Task? _task;

    public ResolverTask(ObjectPool<ResolverTask> objectPool)
    {
        _objectPool = objectPool ?? throw new ArgumentNullException(nameof(objectPool));
    }

    /// <summary>
    /// Gets access to the resolver context for this task.
    /// </summary>
    internal MiddlewareContext ResolverContext => _resolverContext;

    /// <summary>
    /// Gets access to the diagnostic events.
    /// </summary>
    private IExecutionDiagnosticEvents DiagnosticEvents => _operationContext.DiagnosticEvents;

    /// <summary>
    /// Gets the selection for which a resolver is executed.
    /// </summary>
    internal ISelection Selection => _selection;

    /// <inheritdoc />
    public ExecutionTaskKind Kind => ExecutionTaskKind.Parallel;

    /// <inheritdoc />
    public ExecutionTaskStatus Status { get; private set; }

    /// <inheritdoc />
    public IExecutionTask? Next { get; set; }

    /// <inheritdoc />
    public IExecutionTask? Previous { get; set; }

    /// <summary>
    /// Gets access to the internal result map into which the task will write the result.
    /// </summary>
    public ObjectResult ParentResult { get; private set; } = default!;

    /// <inheritdoc />
    public object? State { get; set; }

    /// <inheritdoc />
    public bool IsSerial { get; set; }

    /// <inheritdoc />
    public bool IsRegistered { get; set; }

    /// <inheritdoc />
    public void BeginExecute(CancellationToken cancellationToken)
    {
        Status = ExecutionTaskStatus.Running;
        _task = ExecuteAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task WaitForCompletionAsync(CancellationToken cancellationToken)
        => _task ?? Task.CompletedTask;
}
