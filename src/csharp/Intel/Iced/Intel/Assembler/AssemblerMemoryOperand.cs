/*
Copyright (C) 2018-2019 de4dot@gmail.com

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/
using System;
using System.Diagnostics;

namespace Iced.Intel
{
	/// <summary>
	/// Defines an assembly memory operand used with <see cref="Assembler"/>.
	/// </summary>
	[DebuggerDisplay("{Base} + {Index} * {Scale} + {Displacement}")]
	public readonly struct AssemblerMemoryOperand {
		
		/// <summary>
		/// Creates a new instance.
		/// </summary>
		/// <param name="size">Size of the operand.</param>
		/// <param name="prefix">Register prefix.</param>
		/// <param name="base">Base register.</param>
		/// <param name="index">Index register.</param>
		/// <param name="scale">Scale of the index.</param>
		/// <param name="displacement">Displacement.</param>
		/// <param name="flags">Flags attached to this operand.</param>
		public AssemblerMemoryOperand(MemoryOperandSize size, Register prefix, Register @base, Register index, int scale, long displacement, AssemblerOperandFlags flags) {
			Size = size;
			Prefix = prefix;
			Base = @base;
			Index = index;
			Scale = scale;
			Displacement = displacement;
			Flags = flags;
		}

		/// <summary>
		/// Gets the size of the operand.
		/// </summary>
		public readonly MemoryOperandSize Size;

		/// <summary>
		/// Gets the register used as a prefix.
		/// </summary>
		public readonly Register Prefix;

		/// <summary>
		/// Gets the register used as a base.
		/// </summary>
		public readonly Register Base;

		/// <summary>
		/// Gets the register used as an index.
		/// </summary>
		public readonly Register Index;

		/// <summary>
		/// Gets the scale applied to the index register.
		/// </summary>
		public readonly int Scale;

		/// <summary>
		/// Gets the displacement.
		/// </summary>
		public readonly long Displacement;
		
		/// <summary>
		/// Gets the mask associated with this operand.
		/// </summary>
		public readonly AssemblerOperandFlags Flags;

		/// <summary>
		/// Gets a boolean indicating if this memory operand is a memory access without a base/index and with a displacement bigger than 32-bit.
		/// </summary>
		public bool IsDisplacement64BitOnly => Base == Register.None && Index == Register.None && (Displacement < int.MinValue || Displacement > int.MaxValue);

		/// <summary>
		/// Gets the size of the displacement.
		/// </summary>
		public int DisplacementSize {
			get {
				if (Displacement == 0) return 0;

				if (Displacement >= sbyte.MinValue && Displacement <= sbyte.MaxValue) return 1;
				if (Displacement >= short.MinValue && Displacement <= short.MaxValue) return 2;
				if (Displacement >= int.MinValue && Displacement <= int.MaxValue) return 4;
				return 8;
			}
		}

		/// <summary>
		/// Adds a memory operand with an new base or index.
		/// </summary>
		/// <param name="left">The memory operand.</param>
		/// <param name="right">The base or index.</param>
		/// <returns></returns>
		public static AssemblerMemoryOperand operator +(AssemblerMemoryOperand left, AssemblerRegister right) {
			var hasBase = left.Base != Register.None;
			return new AssemblerMemoryOperand(left.Size, Register.None, hasBase ? left.Base : right.Value, hasBase ? right.Value : left.Index, left.Scale, left.Displacement, left.Flags);
		}

		/// <summary>
		/// Adds a memory operand with an new base or index.
		/// </summary>
		/// <param name="left">The base or index.</param>
		/// <param name="right">The memory operand.</param>
		/// <returns></returns>
		public static AssemblerMemoryOperand operator +(AssemblerRegister left, AssemblerMemoryOperand right) {
			var hasBase = right.Base != Register.None;
			return new AssemblerMemoryOperand(right.Size, Register.None, hasBase ? right.Base : left.Value, hasBase ? left.Value : right.Index, right.Scale, right.Displacement, right.Flags);
		}

		/// <summary>
		/// Adds a displacement to a memory operand.
		/// </summary>
		/// <param name="left">The memory operand.</param>
		/// <param name="displacement">displacement.</param>
		/// <returns></returns>
		public static AssemblerMemoryOperand operator +(AssemblerMemoryOperand left, int displacement) {
			return new AssemblerMemoryOperand(left.Size, Register.None, left.Base, left.Index, left.Scale, displacement, left.Flags);
		}

		/// <summary>
		/// Converts implicitly an <see cref="AssemblerMemoryOperand"/> to a <see cref="MemoryOperand"/>.
		/// </summary>
		/// <param name="v">The memory operand.</param>
		/// <returns></returns>
		public static implicit operator MemoryOperand(AssemblerMemoryOperand v) {
			return new MemoryOperand(v.Base, v.Index, v.Scale, (int)v.Displacement, v.DisplacementSize);
		}
	}
}
