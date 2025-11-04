using LabApi.Events.Arguments.ServerEvents;
using LabApi.Features.Enums;

using LabExtended.API;

using LabExtended.Commands.Interfaces;
using LabExtended.Commands.Utilities;

using LabExtended.Core;
using LabExtended.Utilities.Update;

using NorthwoodLib.Pools;

using UnityEngine;

namespace LabExtended.Commands.Runners;

/// <summary>
/// Runs continuable commands.
/// </summary>
public class ContinuableCommandRunner : ICommandRunner
{
    /// <summary>
    /// Gets the <see cref="ContinuableCommandRunner"/> singleton.
    /// </summary>
    public static ContinuableCommandRunner Singleton { get; } = new();

    private ContinuableCommandBase? command;
    private bool eventStatus = false;

    /// <inheritdoc cref="ICommandRunner.Create"/>
    public ICommandRunner Create(CommandContext context)
        => this;

    /// <inheritdoc cref="ICommandRunner.ShouldPool"/>
    public bool ShouldPool(CommandContext ctx)
        => ctx.Response is null || !ctx.Response.IsContinued || ctx.Sender?.activeRunner is not ContinuableCommandRunner;

    /// <inheritdoc cref="ICommandRunner.ShouldContinue"/>
    public bool ShouldContinue(CommandExecutingEventArgs args, ExPlayer sender)
    {
        if (sender.activeRunner is not ContinuableCommandRunner continuableCommandRunner
            || continuableCommandRunner.command is null)
            return false;

        args.IsAllowed = false;

        var lineArgs = ListPool<string>.Shared.Rent(args.Arguments);
        var lineFull = string.Join(" ", args.Arguments.Array);

        var newContext = new CommandContext
        {
            Args = lineArgs,
            Line = lineFull,
            Sender = sender,
            Runner = continuableCommandRunner,
            
            Type = args.CommandType,
            
            Command = continuableCommandRunner.command.CommandData,
            Overload = continuableCommandRunner.command.Overload,
            Instance = continuableCommandRunner.command
        };
        
        continuableCommandRunner.command.PreviousContext = continuableCommandRunner.command.Context;
        continuableCommandRunner.command.Context = newContext;

        try
        {
            continuableCommandRunner.command.OnContinued();
        }
        catch (Exception ex)
        {
            ApiLog.Error("CommandManager", ex);

            newContext.Response = new(false, false, false, null!, newContext.FormatExceptionResponse(ex));
        }

        HandleResponse(newContext);
        
        newContext.InvokeExecuted();
        return true;
    }

    /// <inheritdoc cref="ICommandRunner.Run"/>
    public void Run(CommandContext ctx, object[] buffer)
    {
        if (ctx is null)
            throw new ArgumentNullException(nameof(ctx));
        
        if (buffer is null)
            throw new ArgumentNullException(nameof(buffer));

        var prevRunner = ctx.Sender.activeRunner;

        void FinishCommand()
        {
            if (ctx.Sender.activeRunner is CoroutineCommandRunner or AsyncCommandRunner)
                ctx.Sender.activeRunner = prevRunner;
            
            HandleResponse(ctx);
        
            ctx.InvokeExecuted();
        }

        try
        {
            var result = ctx.Overload.Method(ctx.Instance, buffer);

            if (result is IEnumerator<float> coroutine)
            {
                var runner = new CoroutineCommandRunner(coroutine, FinishCommand);

                prevRunner = ctx.Sender.activeRunner;

                ctx.Sender.activeRunner = runner;
                
                runner.Start();
                return;
            }
            
            if (result is Task task)
            {
                var runner = new AsyncCommandRunner(task, FinishCommand);
                
                prevRunner = ctx.Sender.activeRunner;
                
                ctx.Sender.activeRunner = runner;
                
                runner.Start();
                return;
            }
        }
        catch (Exception ex)
        {
            ctx.Response = new(false, false, false, null!, ctx.FormatExceptionResponse(ex));

            ApiLog.Error("CommandManager", ex);
        }
        
        FinishCommand();
    }

    private void HandleResponse(CommandContext ctx)
    {
        if (ctx.WriteResponse(out var continuable))
        {
            if (ctx.Sender.activeRunner is ContinuableCommandRunner continuableCommandRunner)
            {
                ctx.Sender.activeRunner = continuableCommandRunner;

                continuableCommandRunner.command = continuable;
                continuableCommandRunner.Start();
            }
            else
            {
                continuableCommandRunner = new();
                continuableCommandRunner.command = continuable;

                ctx.Sender.activeRunner = continuableCommandRunner;
                
                continuableCommandRunner.Start();
            }
        }
        else
        {
            if (ctx.Sender.activeRunner is ContinuableCommandRunner continuableCommandRunner)
            {
                continuableCommandRunner.Stop();

                ctx.Sender.activeRunner = null;
            }
        }
    }

    private void Start()
    {
        command!.RemainingTime = command.CommandData.TimeOut ?? 0f;

        if (command.RemainingTime > 0f && !eventStatus)
        {
            PlayerUpdateHelper.Component.OnUpdate += Update;

            eventStatus = true;
        }
    }

    private void Stop()
    {
        if (command != null && eventStatus)
        {
            PlayerUpdateHelper.Component.OnUpdate -= Update;

            eventStatus = false;
        }

        command = null;
    }

    private void Update()
    {
        if (command != null)
        {
            command.RemainingTime -= Time.deltaTime;

            if (command.RemainingTime > 0f)
            {
                command.OnUpdate();
                return;
            }
            
            RunTimeOut();
            Stop();
        }
    }

    private void RunTimeOut()
    {
        var emptyArgs = ListPool<string>.Shared.Rent();
        
        var newContext = new CommandContext
        {
            Args = emptyArgs,
            
            Line = string.Empty,
            
            Sender = command.Context.Sender,
            Runner = command.Context.Runner,
            
            Type = CommandType.Console,
            
            Command = command.CommandData,
            Overload = command.Overload,
            Instance = command
        };
        
        command.PreviousContext = command.Context;
        command.Context = newContext;

        try
        {
            command.OnTimedOut();
        }
        catch (Exception ex)
        {
            ApiLog.Error("Command API", ex);
            
            newContext.Response = new(false, false, false, null, newContext.FormatExceptionResponse(ex));
        }
        
        HandleResponse(newContext);
        
        newContext.InvokeExecuted();
    }
}