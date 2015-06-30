using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetLua.VM
{
  class ExpandableList : List<LuaObject>
  {
    public new LuaObject this[int i]
    {
      get
      {
        if (i > Count)
        {
          int diff = i - Count;
          for (int j = 0; j < diff; j++)
          {
            Add(LuaObject.Nil);
          }
          return LuaObject.Nil;
        }
        else
        {
          return base[i];
        }
      }

      set
      {
        if (i > Count)
        {
          int diff = i - Count;
          for (int j = 0; j < diff; j++)
          {
            Add(LuaObject.Nil);
          }
          base[i] = value;
        }
        else
        {
          base[i] = value;
        }
      }
    }
  }

  class VirtualMachine
  {
    LuaObject G = LuaObject.NewTable();

    public LuaObject Globals
    {
      get
      {
        return G;
      }
    }

    public Function BuildFunction(Ast.FunctionDefinition def)
    {
      Function f = new Function();
      f.vm = this;
      f.isVarargs = def.isVarargs;
      f.numParams = def.Arguments.Count;
      Closure c = new Closure();
      c.proto = f;
      Dictionary<string, int> locals = new Dictionary<string, int>();
      List<Instruction> instructions = new List<Instruction>();
      List<LuaObject> literals = new List<LuaObject>();

      foreach(Ast.Argument arg in def.Arguments)
      {
        locals.Add(arg.Name, locals.Count);
      }


      foreach(Ast.IStatement statement in def.Body.Statements)
      {
        if(statement is Ast.LocalAssignment)
        {
          var locass = statement as Ast.LocalAssignment;
          int start = locals.Count;
          foreach(string name in locass.Names)
          {
            if(!locals.ContainsKey(name))
              locals.Add(name, locals.Count);
          }

          for(int i = 0; i < locass.Names.Count; i++)
          {
            var val = locass.Values[i];
            int register = locals[locass.Names[i]];

            if(val is Ast.ILiteral)
            {
              var lit = val as Ast.ILiteral;
              literals.Add(lit.GetValue());
              var instr = new Instruction(OpCode.LOADK, register, literals.Count - 1);

              instructions.Add(instr);
            }
            else if(val is Ast.VarargsLiteral)
            {
              var varargs = val as Ast.VarargsLiteral;
              if (!def.isVarargs)
                throw new LuaException(varargs.filename, varargs.lineNumber, varargs.columnNumber, "Function is not variadic");

              var instr = new Instruction(OpCode.VARARG, register, 0);
              instructions.Add(instr);
            }
            else if(val is Ast.Variable)
            {
              var variable = val as Ast.Variable;
              if(locals.ContainsKey(variable.Name))
              {
                int localRegister = locals[variable.Name];
                var instr = new Instruction(OpCode.MOV, register, localRegister);
                instructions.Add(instr);
              }
              else
              {
                int literalIndex = literals.Count;
                literals.Add(LuaObject.FromString(variable.Name));
                var instr = new Instruction(OpCode.GETGLOBAL, register, literalIndex);
                instructions.Add(instr);
              }
            }
            // TODO: Implement function call
          }
        }
      }

      f.closure = c;
      f.code = instructions.ToArray();
      f.R = new LuaObject[locals.Count];
      f.K = new LuaObject[literals.Count];
      
      return f;
    }

    public LuaArguments Exec(Function f, LuaArguments args)
    {
      int PC = 0;
      var R = f.R;
      var K = f.K;
      var U = f.closure.upvals;
      while(PC < f.code.Length)
      {
        var instruction = f.code[PC];
        var A = instruction.A;
        var B = instruction.B;
        var C = instruction.C;
        var sBx = instruction.sBx;
        var Bx = instruction.Bx;

        switch(instruction.OpCode)
        {
          case OpCode.MOV:
            R[A] = R[B];
            break;
          case OpCode.LOADK:
            R[A] = K[B];
            break;
          case OpCode.SETGLOBAL:
            G[K[Bx]] = R[A];
            break;
          case OpCode.GETGLOBAL:
            R[A] = G[K[Bx]];
            break;
          case OpCode.SETUPVAL:
            U[A] = R[B];
            break;
          case OpCode.GETUPVAL:
            R[A] = U[B];
            break;
          case OpCode.SETTABLE:
            R[A][R[B]] = R[C];
            break;
          case OpCode.GETTABLE:
            R[A] = R[B][R[C]];
            break;
          case OpCode.NEWTABLE:
            R[A] = LuaObject.NewTable();
            break;

          case OpCode.ADD:
            R[A] = R[B] + R[C];
            break;
          case OpCode.SUB:
            R[A] = R[B] - R[C];
            break;
          case OpCode.MUL:
            R[A] = R[B] * R[C];
            break;
          case OpCode.DIV:
            R[A] = R[B] / R[C];
            break;
          case OpCode.POW:
            R[A] = R[B] ^ R[C];
            break;
          case OpCode.CONCAT:
            R[A] = LuaEvents.concat_event(R[B], R[C]);
            break;
          case OpCode.UNM:
            R[A] = -R[B];
            break;
          case OpCode.NOT:
            R[A] = !R[B];
            break;
          case OpCode.JMP:
            if (R[A])
              PC += sBx;
            break;
          case OpCode.EQ:
            R[A] = R[B] == R[C];
            break;
          case OpCode.LT:
            R[A] = R[B] < R[C];
            break;
          case OpCode.LE:
            R[A] = R[B] <= R[C];
            break;

          case OpCode.CALL:
            {
              LuaArguments passArgs = new LuaArguments();
              for (int i = 1; i < B; i++)
              {
                passArgs.Add(R[A + i]);
              }
              LuaArguments ret = R[A].Call(passArgs);
              for (int i = 0; i < C - 1; i++)
              {
                R[A + C] = ret[i];
              }
              break;
            }
          case OpCode.RET:
            {
              LuaArguments passArgs = new LuaArguments();
              for (int i = 0; i < B - 1; i++)
              {
                passArgs.Add(R[A + i]);
              }
              return passArgs;
            }

          case OpCode.VARARG:
            R[A] = args[f.numParams + B];
            break;
        }
        PC++;
      }
      return Lua.Return();
    }
  }

  class Closure
  {
    public Function proto;
    public LuaObject[] upvals;
  }

  class Function
  {
    public LuaObject[] R;
    public LuaObject[] K;
    public Instruction[] code;
    public Function[] functions;
    public Closure closure;
    public int numParams;
    public bool isVarargs;

    public VirtualMachine vm;

    LuaArguments Exec(LuaArguments args)
    {
      return vm.Exec(this, args);
    }

    public LuaFunction ToLFunction()
    {
      return Exec;
    }
  }
}
