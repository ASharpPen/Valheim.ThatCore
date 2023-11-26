using System;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using ThatCore.Logging;

namespace ThatCore.Extensions;

public static class CodeMatcherExtensions
{
    public static CodeMatcher GetPosition(this CodeMatcher codeMatcher, out int position)
    {
        position = codeMatcher.Pos;
        return codeMatcher;
    }

    public static CodeMatcher InsertAndAdvance(this CodeMatcher codeMatcher, OpCode opcode, object operand = null)
    {
        codeMatcher.InsertAndAdvance(new CodeInstruction(opcode, operand));
        return codeMatcher;
    }

    public static CodeMatcher InsertAndAdvance(this CodeMatcher codeMatcher, MethodInfo method)
    {
        codeMatcher.InsertAndAdvance(new CodeInstruction(OpCodes.Call, method));
        return codeMatcher;
    }

    /// <summary>
    /// Shortcut for CodeMatcher.InsertAndAdvance(AccessTools.Method(typeof(T), methodName)).
    /// </summary>
    public static CodeMatcher InsertAndAdvance<T>(this CodeMatcher codeMatcher, string methodName)
    {
        return codeMatcher.InsertAndAdvance(AccessTools.Method(typeof(T), methodName));
    }

    /// <summary>
    /// Shortcut for CodeMatcher.InsertAndAdvance(Transpilers.EmitDelegate(action)).
    /// </summary>
    public static CodeMatcher InsertAndAdvance<T>(this CodeMatcher codeMatcher, T action)
        where T : Delegate
    {
        return codeMatcher.InsertAndAdvance(Transpilers.EmitDelegate(action));
    }

    public static CodeMatcher GetOpcode(this CodeMatcher codeMatcher, out OpCode opcode)
    {
        opcode = codeMatcher.Opcode;
        return codeMatcher;
    }

    public static CodeMatcher GetInstruction(this CodeMatcher codeMatcher, out CodeInstruction instruction)
    {
        instruction = codeMatcher.Instruction;
        return codeMatcher;
    }

    public static CodeMatcher GetOperand(this CodeMatcher codeMatcher, out object operand)
    {
        operand = codeMatcher.Operand;
        return codeMatcher;
    }

    public static CodeMatcher Print(this CodeMatcher codeMatcher, int before, int after)
    {
        if (!Log.DevelopmentEnabled)
        {
            return codeMatcher;
        }

        for (int i = -before; i <= after; ++i)
        {
            int currentOffset = i;
            int index = codeMatcher.Pos + currentOffset;

            if (index <= 0)
            {
                continue;
            }

            if (index >= codeMatcher.Length)
            {
                break;
            }

            try
            {
                var line = codeMatcher.InstructionAt(currentOffset);
                Log.Development?.Log($"[{currentOffset}] " + line.ToString());
            }
            catch (Exception e)
            {
                Log.Error?.Log(e.Message);
            }
        }
      
        return codeMatcher;
    }
}
