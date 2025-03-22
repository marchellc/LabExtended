using System.Reflection;
using System.Reflection.Emit;

using HarmonyLib;
using LabExtended.Core;
using LabExtended.Core.Pooling.Pools;
using LabExtended.Extensions;
using NorthwoodLib.Pools;

namespace LabExtended.Utilities.Transpilers;

/// <summary>
/// Used to help with transpiler patches.
/// </summary>
public class TranspilerContext
{
    private struct CodeMatch
    {
        public OpCode ExpectedCode;
        public object? ExpectedOperand;
        public int? Offset;
    }

    private CodeMatch? prevMatch;
    private CodeMatch? afterMatch;
    
    /// <summary>
    /// Gets a list of declared labels.
    /// </summary>
    public Dictionary<string, Label> Labels { get; private set; } = DictionaryPool<string, Label>.Shared.Rent();

    /// <summary>
    /// Gets a list of declared locals.
    /// </summary>
    public Dictionary<string, LocalBuilder> Locals { get; private set; } =
        DictionaryPool<string, LocalBuilder>.Shared.Rent();

    /// <summary>
    /// Gets the modified instructions list.
    /// </summary>
    public List<CodeInstruction> Instructions { get; private set; } = ListPool<CodeInstruction>.Shared.Rent();

    /// <summary>
    /// Gets the buffered instructions list.
    /// </summary>
    public List<CodeInstruction> Buffer { get; private set; } = ListPool<CodeInstruction>.Shared.Rent();

    /// <summary>
    /// Gets the assigned ILGenerator.
    /// </summary>
    public ILGenerator? Generator { get; private set; }
    
    /// <summary>
    /// Gets the target method.
    /// </summary>
    public MethodBase? Method { get; private set; }

    /// <summary>
    /// Gets the found index.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Initializes a new TranspilerContext instance.
    /// </summary>
    /// <param name="generator">The ILGenerator instance.</param>
    /// <param name="method">The method that is being patched.</param>
    /// <param name="instructions">The list of original instructions.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext(ILGenerator generator, MethodBase method, IEnumerable<CodeInstruction> instructions)
    {
        if (instructions is null)
            throw new ArgumentNullException(nameof(instructions));

        Generator = generator ?? throw new ArgumentNullException(nameof(generator));
        Method = method ?? throw new ArgumentNullException(nameof(method));

        Instructions.AddRange(instructions);
    }

    /// <summary>
    /// Sets a new code match required after a matching index is found.
    /// </summary>
    /// <param name="expectedCode">The expected OpCode.</param>
    /// <param name="expectedOperand">The expected operand.</param>
    /// <param name="offset">The after index offset.</param>
    public void CheckAfterIndex(OpCode expectedCode, object? expectedOperand = null, int? offset = null)
        => afterMatch = new CodeMatch { ExpectedCode = expectedCode, ExpectedOperand = expectedOperand, Offset = offset };

    /// <summary>
    /// Sets a new code match required before a matching index is found.
    /// </summary>
    /// <param name="expectedCode">The expected OpCode.</param>
    /// <param name="expectedOperand">The expected operand.</param>
    /// <param name="offset">The before index offset.</param>
    public void CheckBeforeIndex(OpCode expectedCode, object? expectedOperand = null, int? offset = null)
        => prevMatch = new CodeMatch { ExpectedCode = expectedCode, ExpectedOperand = expectedOperand, Offset = offset };
    
    /// <summary>
    /// Attempts to find a base index.
    /// </summary>
    /// <param name="expectedOpCode">The expected OpCode.</param>
    /// <param name="expectedOperand">The expected operand.</param>
    /// <param name="offset"></param>
    /// <returns>The found index.</returns>
    /// <exception cref="Exception"></exception>
    public int FindIndex(OpCode expectedOpCode, object? expectedOperand = null, int? offset = null)
    {
        var i = 0;
        
        if (offset.HasValue)
            i += offset.Value;
        
        for (; i < Instructions.Count; i++)
        {
            var instruction = Instructions[i];

            if (instruction.opcode != expectedOpCode)
                continue;
            
            if ((expectedOperand is null && instruction.operand != null)
                || (expectedOperand != null && instruction.operand is null)
                || !instruction.OperandIs(expectedOperand))
                continue;

            if (prevMatch.HasValue)
            {
                var prevIndex = i;
                
                if (prevMatch.Value.Offset.HasValue)
                    prevIndex += prevMatch.Value.Offset.Value;

                if (i >= 0 && i < Instructions.Count)
                {
                    var prevInstruction = Instructions[prevIndex];
                    
                    if (prevMatch.Value.ExpectedCode != prevInstruction.opcode)
                        continue;
                    
                    if ((prevMatch.Value.ExpectedOperand is null && prevInstruction.operand != null)
                        || (prevMatch.Value.ExpectedOperand != null && prevInstruction.operand is null)
                        || !prevInstruction.OperandIs(prevMatch.Value.ExpectedOperand))
                        continue;
                }
            }

            if (afterMatch.HasValue)
            {
                var afterIndex = i;
                
                if (afterMatch.Value.Offset.HasValue)
                    afterIndex += afterMatch.Value.Offset.Value;

                if (afterIndex >= 0 && afterIndex < Instructions.Count)
                {
                    var afterInstruction = Instructions[afterIndex];
                    
                    if (afterMatch.Value.ExpectedCode != afterInstruction.opcode)
                        continue;
                    
                    if ((afterMatch.Value.ExpectedOperand is null && afterInstruction.operand != null)
                        || (afterMatch.Value.ExpectedOperand != null && afterInstruction.operand is null)
                        || !afterInstruction.OperandIs(afterMatch.Value.ExpectedOperand))
                        continue;
                }
            }

            Index = i;
            return Index;
        }

        ApiLog.Error("Transpiler Context",
            $"\nCould not find targeted code in method &1{Method.GetMemberName()}&r!\n" +
                 $"&3Targeted OpCode&r: &6{expectedOpCode}&r\n" +
                 $"&3Targeted Operand&r: &6{expectedOperand?.ToString() ?? "null"}&r\n" +
                 $"&3Targeted Offset&r: &6{offset}&r");
        
        ApiLog.Error("Transpiler Context", StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine($"&3Method Size&r = &6{Instructions.Count}&r");

            if (Instructions.Count > 0)
                x.AppendLine();

            for (var i = 0; i < Instructions.Count; i++)
            {
                var instruction = Instructions[i];

                x.Append($"&3Method Index&r &1{i}&r = &6{instruction.opcode}&r");

                if (instruction.operand != null)
                    x.Append($" &2({instruction.operand})&r");

                x.AppendLine();
            }
        }));
        
        return -1;
    }

    /// <summary>
    /// Declares a new jump label.
    /// </summary>
    /// <param name="labelName">The name of the label.</param>
    /// <param name="addToCurrentIndex">Whether or not to add this label to the instruction at the base index.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public TranspilerContext DeclareLabel(string labelName, bool addToCurrentIndex = false)
    {
        if (string.IsNullOrEmpty(labelName))
            throw new ArgumentNullException(nameof(labelName));

        if (Labels.ContainsKey(labelName))
            throw new Exception($"Label {labelName} is already declared.");

        var label = Generator.DefineLabel();
        
        if (addToCurrentIndex)
            Instructions[Index].labels.Add(label);
        
        Labels.Add(labelName, label);
        return this;
    }

    /// <summary>
    /// Declares a new local variable.
    /// </summary>
    /// <param name="localName">Name of the local variable.</param>
    /// <typeparam name="T">Type of the local variable.</typeparam>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public TranspilerContext DeclareLocal<T>(string localName)
        => DeclareLocal(typeof(T), localName);

    /// <summary>
    /// Declares a new local variable.
    /// </summary>
    /// <param name="localType">Type of the local variable.</param>
    /// <param name="localName">Name of the local variable.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public TranspilerContext DeclareLocal(Type localType, string localName)
    {
        if (localType is null)
            throw new ArgumentNullException(nameof(localType));

        if (string.IsNullOrEmpty(localName))
            throw new ArgumentNullException(nameof(localName));

        if (Locals.ContainsKey(localName))
            throw new Exception($"Local {localName} is already declared.");

        Locals.Add(localName, Generator.DeclareLocal(localType));
        return this;
    }

    /// <summary>
    /// Inserts a new code instruction to the buffer.
    /// </summary>
    /// <param name="instruction">The instruction to insert.</param>
    /// <param name="labelNames">A list of labels.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext Insert(CodeInstruction instruction, params string[] labelNames)
    {
        if (instruction is null)
            throw new ArgumentNullException(nameof(instruction));

        if (labelNames?.Length > 0)
        {
            var labels = new Label[labelNames.Length];

            for (var i = 0; i < labelNames.Length; i++)
            {
                if (!Labels.TryGetValue(labelNames[i], out var label))
                    throw new Exception($"Label {labelNames[i]} is not declared.");
                
                labels[i] = label;
            }
            
            Buffer.Add(instruction.WithLabels(labels));
            return this;
        }
        
        Buffer.Add(instruction);
        return this;
    }

    /// <summary>
    /// Loads a Call opcode for static methods or Callvirt for non-static methods.
    /// <remarks>The Ldarg_0 opcode is pushed automatically if the method is not static.</remarks>
    /// </summary>
    /// <param name="declaringType">The type that contains this method.</param>
    /// <param name="methodName">The name of the method.</param>
    /// <param name="enableZeroLoad">Whether or not to automatically load the current type instance if the target method is a non-static method
    /// defined in the same class as the method that is being patched.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <param name="methodArgs">The method's argument types.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext Call(Type declaringType, string methodName, bool enableZeroLoad = true, Type[]? methodArgs = null, string[]? labelNames = null)
        => Call(AccessTools.Method(declaringType, methodName, methodArgs), enableZeroLoad, labelNames);

    /// <summary>
    /// Loads a Call opcode for static methods or Callvirt for non-static methods.
    /// <remarks>The Ldarg_0 opcode is pushed automatically if the method is not static.</remarks>
    /// </summary>
    /// <param name="target">The target method.</param>
    /// <param name="enableZeroLoad">Whether or not to automatically load the current type instance if the target method is a non-static method
    /// defined in the same class as the method that is being patched.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext Call(MethodInfo target, bool enableZeroLoad = true, params string[] labelNames)
    {
        if (target is null)
            throw new ArgumentNullException(nameof(target));

        if (target.IsStatic)
            return Insert(new(OpCodes.Call, target));

        if (enableZeroLoad && target.DeclaringType == Method.DeclaringType)
            LoadZeroArgument();
        
        return Insert(new(OpCodes.Callvirt, target), labelNames);
    }

    /// <summary>
    /// Loads a value of a property onto the stack.
    /// <remarks>The Ldarg_0 opcode is pushed automatically if the property getter is not static.</remarks>
    /// </summary>
    /// <param name="declaringType">The property's declaring type.</param>
    /// <param name="propertyName">The property's name.</param>
    /// <param name="enableZeroLoad">Whether or not to automatically load the current type instance if the target method is a non-static method
    /// defined in the same class as the method that is being patched.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext GetPropertyValue(Type declaringType, string propertyName, bool enableZeroLoad = true,  params string[] labelNames)
    {
        if (declaringType is null)
            throw new ArgumentNullException(nameof(declaringType));

        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        return Call(AccessTools.PropertyGetter(declaringType, propertyName), enableZeroLoad, labelNames);
    }

    /// <summary>
    /// Sets a value of a property.
    /// <remarks>The Ldarg_0 opcode is pushed automatically if the property setter is not static.</remarks>
    /// </summary>
    /// <param name="declaringType">The property's declaring type.</param>
    /// <param name="propertyName">The property's name.</param>
    /// <param name="enableZeroLoad">Whether or not to automatically load the current type instance if the target method is a non-static method
    /// defined in the same class as the method that is being patched.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext SetPropertyValue(Type declaringType, string propertyName, bool enableZeroLoad = true,  params string[] labelNames)
    {
        if (declaringType is null)
            throw new ArgumentNullException(nameof(declaringType));

        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        return Call(AccessTools.PropertySetter(declaringType, propertyName), enableZeroLoad,  labelNames);
    }

    /// <summary>
    /// Loads a value of a field.
    /// <remarks>The Ldarg_0 opcode is pushed automatically if the field is not static.</remarks>
    /// </summary>
    /// <param name="declaringType">The field's declaring type.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="enableZeroLoad">Whether or not to automatically load the current type instance if the target method is a non-static method
    /// defined in the same class as the method that is being patched.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext LoadFieldValue(Type declaringType, string fieldName, bool enableZeroLoad = true,  params string[] labelNames)
    {
        if (declaringType is null)
            throw new ArgumentNullException(nameof(declaringType));
        
        if (string.IsNullOrEmpty(fieldName))
            throw new ArgumentNullException(nameof(fieldName));

        return LoadFieldValue(AccessTools.Field(declaringType, fieldName), enableZeroLoad, labelNames);
    }

    /// <summary>
    /// Loads a value of a field.
    /// <remarks>The Ldarg_0 opcode is pushed automatically if the field is not static.</remarks>
    /// </summary>
    /// <param name="field">The field to load the value of.</param>
    /// <param name="enableZeroLoad">Whether or not to automatically load the current type instance if the target method is a non-static method
    /// defined in the same class as the method that is being patched.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext LoadFieldValue(FieldInfo field, bool enableZeroLoad = true,  params string[] labelNames)
    {
        if (field is null)
            throw new ArgumentNullException(nameof(field));

        if (enableZeroLoad && !field.IsStatic && field.DeclaringType == Method.DeclaringType)
            LoadZeroArgument();
        
        return Insert(new(OpCodes.Ldfld, field), labelNames);
    }

    /// <summary>
    /// Sets a field's value.
    /// <remarks>The Ldarg_0 opcode is pushed automatically if the field is not static.</remarks>
    /// </summary>
    /// <param name="declaringType">The type that contains the specified field.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="loadInstruction">The instruction used to load the value to set onto the stack (optional).</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext SetFieldValue(Type declaringType, string fieldName, CodeInstruction? loadInstruction = null, string[] labelNames = null)
    {
        if (declaringType is null)
            throw new ArgumentNullException(nameof(declaringType));
        
        if (string.IsNullOrWhiteSpace(fieldName))
            throw new ArgumentNullException(nameof(fieldName));
        
        return SetFieldValue(AccessTools.Field(declaringType, fieldName), loadInstruction, labelNames);
    }

    /// <summary>
    /// Sets a field's value.
    /// <remarks>The Ldarg_0 opcode is pushed automatically if the field is not static.</remarks>
    /// </summary>
    /// <param name="field">The field to set a value of.</param>
    /// <param name="loadInstruction">The instructions to push for loading the value (optional).</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext SetFieldValue(FieldInfo field, CodeInstruction? loadInstruction = null, string[] labelNames = null)
    {
        if (field is null)
            throw new ArgumentNullException(nameof(field));

        if (!field.IsStatic)
            LoadZeroArgument();

        if (loadInstruction != null)
            Insert(loadInstruction);
        
        return Insert(new(OpCodes.Stfld, field), labelNames);
    }

    /// <summary>
    /// Searches for a specific constructor and then pushes the result onto the stack.
    /// </summary>
    /// <param name="type">The type to construct.</param>
    /// <param name="constructorArgs">The constructor's parameters.</param>
    /// <param name="searchForStaticConstructor">Whether or not to include static constructors.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext Construct(Type type, Type[]? constructorArgs = null, bool searchForStaticConstructor = false, string[] labelNames = null)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));

        return Construct(type.DeclaredConstructor(constructorArgs, searchForStaticConstructor), labelNames);
    }

    /// <summary>
    /// Invokes the specified constructor and pushes the result onto the stack.
    /// </summary>
    /// <param name="constructor">The constructor to invoke.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext Construct(ConstructorInfo constructor, params string[] labelNames)
    {
        if (constructor is null)
            throw new ArgumentNullException(nameof(constructor));
        
        return Insert(new(OpCodes.Newobj, constructor), labelNames);
    }
    
    /// <summary>
    /// Boxes the object on the stack to a specified type.
    /// </summary>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <typeparam name="T">The type to box the object to.</typeparam>
    /// <returns>This context.</returns>
    public TranspilerContext Box<T>(params string[] labelNames)
        => Insert(new(OpCodes.Box, typeof(T)), labelNames);

    /// <summary>
    /// Boxes the object on the stack to a specified type.
    /// </summary>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <param name="type">The type to box the object to.</param>
    /// <returns>This context.</returns>
    public TranspilerContext Box(Type type, params string[] labelNames)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        
        return Insert(new(OpCodes.Box, type), labelNames);
    }
    
    /// <summary>
    /// Checks if the object on the stack is the specified type.
    /// </summary>
    /// <typeparam name="T">The type to check.</typeparam>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext CheckInstanceType<T>(params string[] labelNames)
        => Insert(new(OpCodes.Isinst, typeof(T)), labelNames);

    /// <summary>
    /// Checks if the object on the stack is the specified type.
    /// </summary>
    /// <param name="type">The type to check.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public TranspilerContext CheckInstanceType(Type type, params string[] labelNames)
    {
        if (type is null)
            throw new ArgumentNullException(nameof(type));
        
        return Insert(new(OpCodes.Isinst, type), labelNames);
    }

    /// <summary>
    /// Stores a value from the stack into a local variable.
    /// </summary>
    /// <param name="localName">The variable name.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public TranspilerContext StoreInLocal(string localName, params string[] labelNames)
    {
        if (string.IsNullOrEmpty(localName))
            throw new ArgumentNullException(nameof(localName));
        
        if (!Locals.TryGetValue(localName, out var local))
            throw new Exception($"Local {localName} is not declared.");

        return Insert(new(OpCodes.Stloc_S, local.LocalIndex), labelNames);
    }
    
    /// <summary>
    /// Stores a value from the stack into a local variable.
    /// </summary>
    /// <param name="localIndex">The variable index.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    public TranspilerContext StoreInLocal(int localIndex, params string[] labelNames)
        => Insert(new(OpCodes.Stloc, localIndex), labelNames);

    /// <summary>
    /// Pushes a value onto the stack from a local variable.
    /// </summary>
    /// <param name="localName">The local variable's name.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public TranspilerContext LoadFromLocal(string localName, params string[] labelNames)
    {
        if (string.IsNullOrEmpty(localName))
            throw new ArgumentNullException(nameof(localName));
        
        if (!Locals.TryGetValue(localName, out var local))
            throw new Exception($"Local {localName} is not declared.");

        return Insert(new(OpCodes.Ldloc_S, local.LocalIndex), labelNames);
    }
    
    /// <summary>
    /// Pushes a value onto the stack from a local variable.
    /// </summary>
    /// <param name="localIndex">The local variable's index.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    public TranspilerContext LoadFromLocal(int localIndex, params string[] labelNames)  
        => Insert(new(OpCodes.Ldloc, localIndex), labelNames);
    
    /// <summary>
    /// Pushes the Ldc_I4_1 (or Ldc_I4_0 if value is false) opcode onto the stack.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    public TranspilerContext LoadBool(bool value, params string[] labelNames)
        => Insert(new(value ? OpCodes.Ldc_I4_1 : OpCodes.Ldc_I4_0), labelNames);
    
    /// <summary>
    /// Pushes the Ldstr opcode onto the stack.
    /// </summary>
    /// <param name="value">The string to push onto the stack.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    public TranspilerContext LoadString(string value, params string[] labelNames)
        => Insert(new(OpCodes.Ldstr, value), labelNames);

    /// <summary>
    /// Pushes the specified integer onto the stack.
    /// </summary>
    /// <param name="value">The value to push.</param>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    public TranspilerContext LoadInteger(int value, params string[] labelNames)
    {
        switch (value)
        {
            case 0: return Insert(new(OpCodes.Ldc_I4_0), labelNames);
            case 1: return Insert(new(OpCodes.Ldc_I4_1), labelNames);
            case 2: return Insert(new(OpCodes.Ldc_I4_2), labelNames);
            case 3: return Insert(new(OpCodes.Ldc_I4_3), labelNames);
            case 4: return Insert(new(OpCodes.Ldc_I4_4), labelNames);
            case 5: return Insert(new(OpCodes.Ldc_I4_5), labelNames);
            case 6: return Insert(new(OpCodes.Ldc_I4_6), labelNames);
            case 7: return Insert(new(OpCodes.Ldc_I4_7), labelNames);
            case 8: return Insert(new(OpCodes.Ldc_I4_8), labelNames);
            
            default: return Insert(new(OpCodes.Ldc_I4_S, value), labelNames);
        }
    }

    /// <summary>
    /// Pushes the Ldarg_0 opcode onto the stack, loading the current class instance for non-static methods and the method's first
    /// argument for static methods.
    /// </summary>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    public TranspilerContext LoadZeroArgument(params string[] labelNames)
        => Insert(new(OpCodes.Ldarg_0), labelNames);
    
    /// <summary>
    /// Pushes the Ldarg_1 opcode onto the stack, loading the first argument of the method's overload.
    /// </summary>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    public TranspilerContext LoadFirstArgument(params string[] labelNames)
        => Insert(new(OpCodes.Ldarg_1), labelNames);
    
    /// <summary>
    /// Pushes the Ldarg_2 opcode onto the stack, loading the second argument of the method's overload.
    /// </summary>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    public TranspilerContext LoadSecondArgument(params string[] labelNames)
        => Insert(new(OpCodes.Ldarg_2), labelNames);
    
    /// <summary>
    /// Pushes the Ldarg_3 opcode onto the stack, loading the third argument of the method's overload.
    /// </summary>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    public TranspilerContext LoadThirdArgument(params string[] labelNames)
        => Insert(new(OpCodes.Ldarg_3), labelNames);
    
    /// <summary>
    /// Pushes the Ldarg opcode onto the stack, loading the argument at the specified index.
    /// </summary>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <param name="index">The argument's index.</param>
    /// <returns>This context.</returns>
    public TranspilerContext LoadArgumentAt(int index, params string[] labelNames)
        => Insert(new(OpCodes.Ldarg, index), labelNames);
    
    /// <summary>
    /// Pushes the Dup opcode onto the stack, duplicating the current value.
    /// </summary>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    public TranspilerContext Duplicate(params string[] labelNames)
        => Insert(new(OpCodes.Dup), labelNames);
    
    /// <summary>
    /// Pushes the Ret opcode onto the stack, returning from the current method.
    /// </summary>
    /// <param name="labelNames">The labels to add with this instruction.</param>
    /// <returns>This context.</returns>
    public TranspilerContext Return(params string[] labelNames)
        => Insert(new(OpCodes.Ret), labelNames);

    /// <summary>
    /// Jumps to a specific label.
    /// </summary>
    /// <param name="labelName">The name of the label.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public TranspilerContext JumpToLabel(string labelName)
    {
        if (string.IsNullOrEmpty(labelName))
            throw new ArgumentNullException(nameof(labelName));
        
        if (!Labels.TryGetValue(labelName, out var label))
            throw new Exception($"Label {labelName} not found.");
        
        return Insert(new(OpCodes.Br, label));
    }

    /// <summary>
    /// Jumps to a specific label if the value popped from the stack is false, zero or null.
    /// </summary>
    /// <param name="labelName">The name of the label.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public TranspilerContext JumpToLabelIfFalse(string labelName)
    {
        if (string.IsNullOrEmpty(labelName))
            throw new ArgumentNullException(nameof(labelName));
        
        if (!Labels.TryGetValue(labelName, out var label))
            throw new Exception($"Label {labelName} not found.");
        
        return Insert(new(OpCodes.Brfalse_S, label));
    }
    
    /// <summary>
    /// Jumps to a specific label if the value popped from the stack is true, not zero or not null.
    /// </summary>
    /// <param name="labelName">The name of the label.</param>
    /// <returns>This context.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    public TranspilerContext JumpToLabelIfTrue(string labelName)
    {
        if (string.IsNullOrEmpty(labelName))
            throw new ArgumentNullException(nameof(labelName));
        
        if (!Labels.TryGetValue(labelName, out var label))
            throw new Exception($"Label {labelName} not found.");
        
        return Insert(new(OpCodes.Brtrue_S, label));
    }

    /// <summary>
    /// Inserts the buffer into instructions.
    /// </summary>
    /// <param name="removeCount">The amount of instructions to remove.</param>
    /// <returns>The list of instructions.</returns>
    public IEnumerable<CodeInstruction> ReturnInstructions(int? removeCount = null)
    {
        if (Buffer.Count > 0)
        {
            if (removeCount.HasValue)
                Instructions.RemoveRange(Index, removeCount.Value);
            
            Instructions.InsertRange(Index, Buffer);
            
            Buffer.Clear();
        }

        for (var i = 0; i < Instructions.Count; i++)
            yield return Instructions[i];
        
        ListPool<CodeInstruction>.Shared.Return(Instructions);
        
        if (Buffer != null)
            ListPool<CodeInstruction>.Shared.Return(Buffer);
        
        if (Locals != null)
            DictionaryPool<string, LocalBuilder>.Shared.Return(Locals);
        
        if (Labels != null)
            DictionaryPool<string, Label>.Shared.Return(Labels);
        
        Instructions = null;
        Generator = null;
        Method = null;
        Locals = null;
        Labels = null;
        Buffer = null;
    }

    /// <summary>
    /// Prints a debug state into the server console.
    /// </summary>
    public void Print()
    {
        ApiLog.Debug("Transpiler Context", StringBuilderPool.Shared.BuildString(x =>
        {
            x.AppendLine($"\nTranspiler &1{Method.GetMemberName()}&r");
            x.AppendLine($"&3Base Index&r = &6{Index}&r");
            x.AppendLine($"&3Buffer Size&r = &6{Buffer.Count}&r");
            x.AppendLine($"&3Method Size&r = &6{Instructions.Count}&r");
            
            if (Buffer.Count > 0)
                x.AppendLine();

            for (var i = 0; i < Buffer.Count; i++)
            {
                var instruction = Buffer[i];

                x.Append($"&3Buffer Index&r &1{i}&r = &6{instruction.opcode}&r");

                if (instruction.operand != null)
                    x.Append($" &2({instruction.operand})&r");

                x.AppendLine();
            }

            if (Instructions.Count > 0)
                x.AppendLine();

            for (var i = 0; i < Instructions.Count; i++)
            {
                var instruction = Instructions[i];

                x.Append($"&3Method Index&r &1{i}&r = &6{instruction.opcode}&r");

                if (instruction.operand != null)
                    x.Append($" &2({instruction.operand})&r");

                x.AppendLine();
            }
        }));
    }
}