using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.InteropServices;

namespace NetLua.VM
{
  enum OpCode : byte
  {
    MOV = 0x00,
    LOADK,
    SETGLOBAL,
    GETGLOBAL,
    SETUPVAL,
    GETUPVAL,
    GETTABLE,
    SETTABLE,
    NEWTABLE,
    ADD,
    SUB,
    MUL,
    DIV,
    POW,
    UNM,
    NOT,
    CONCAT,
    JMP,
    EQ,
    LT,
    LE,
    CALL,
    RET,
    VARARG
  }

  struct Instruction
  {
    public OpCode OpCode;

    public byte A;
    public byte B;
    public byte C;

    public short sBx;
    public ushort Bx;

    int line, col;
    string filename;

    public Instruction(OpCode opc, int A, int B, int C, int sBx, uint Bx, int line, int col, string filename)
    {
      OpCode = opc;
      this.A = (byte)A;
      this.B = (byte)B;
      this.C = (byte)C;
      this.sBx = (short)sBx;
      this.Bx = (ushort)Bx;
      this.line = line;
      this.col = col;
      this.filename = filename;
    }

    public Instruction(OpCode opc, int A)
      : this(opc, A, 0, 0, 0, 0, 0, 0, "")
    { }

    public Instruction(OpCode opc, int A, int B)
      : this(opc, A, B, 0, 0, 0, 0, 0, "")
    { }

    public Instruction(OpCode opc, int A, int B, int C)
      : this(opc, A, B, C, 0, 0, 0, 0, "")
    { }
  }
}
