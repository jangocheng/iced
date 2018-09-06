﻿/*
    Copyright (C) 2018 de4dot@gmail.com

    This file is part of Iced.

    Iced is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    Iced is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with Iced.  If not, see <https://www.gnu.org/licenses/>.
*/

#if !NO_ENCODER
using System;
using Iced.Intel;
using Xunit;

namespace Iced.UnitTests.Intel.EncoderTests {
	public sealed class BlockEncoderTest64_call : BlockEncoderTest {
		const int bitness = 64;
		const ulong origRip = 0x8000;
		const ulong newRip = 0x8000000000000000;

		[Fact]
		void Call_near_fwd() {
			var originalData = new byte[] {
				/*0000*/ 0xE8, 0x07, 0x00, 0x00, 0x00,// call 000000000000800Ch
				/*0005*/ 0xB0, 0x00,// mov al,0
				/*0007*/ 0xB8, 0x78, 0x56, 0x34, 0x12,// mov eax,12345678h
				/*000C*/ 0x90,// nop
			};
			var newData = new byte[] {
				/*0000*/ 0xE8, 0x07, 0x00, 0x00, 0x00,// call 800000000000000Ch
				/*0005*/ 0xB0, 0x00,// mov al,0
				/*0007*/ 0xB8, 0x78, 0x56, 0x34, 0x12,// mov eax,12345678h
				/*000C*/ 0x90,// nop
			};
			var expectedInstructionOffsets = new uint[] {
				0x0000,
				0x0005,
				0x0007,
				0x000C,
			};
			var expectedRelocInfos = Array.Empty<RelocInfo>();
			const BlockEncoderOptions options = BlockEncoderOptions.None;
			EncodeBase(bitness, origRip, originalData, newRip, newData, options, expectedInstructionOffsets, expectedRelocInfos);
		}

		[Fact]
		void Call_near_bwd() {
			var originalData = new byte[] {
				/*0000*/ 0x90,// nop
				/*0001*/ 0xE8, 0xFA, 0xFF, 0xFF, 0xFF,// call 0000000000008000h
				/*0006*/ 0xB0, 0x00,// mov al,0
				/*0008*/ 0xB8, 0x78, 0x56, 0x34, 0x12,// mov eax,12345678h
			};
			var newData = new byte[] {
				/*0000*/ 0x90,// nop
				/*0001*/ 0xE8, 0xFA, 0xFF, 0xFF, 0xFF,// call 8000000000000000h
				/*0006*/ 0xB0, 0x00,// mov al,0
				/*0008*/ 0xB8, 0x78, 0x56, 0x34, 0x12,// mov eax,12345678h
			};
			var expectedInstructionOffsets = new uint[] {
				0x0000,
				0x0001,
				0x0006,
				0x0008,
			};
			var expectedRelocInfos = Array.Empty<RelocInfo>();
			const BlockEncoderOptions options = BlockEncoderOptions.None;
			EncodeBase(bitness, origRip, originalData, newRip, newData, options, expectedInstructionOffsets, expectedRelocInfos);
		}

		[Fact]
		void Call_near_other_near() {
			var originalData = new byte[] {
				/*0000*/ 0xE8, 0x07, 0x00, 0x00, 0x00,// call 000000000000800Ch
				/*0005*/ 0xB0, 0x00,// mov al,0
				/*0007*/ 0xB8, 0x78, 0x56, 0x34, 0x12,// mov eax,12345678h
			};
			var newData = new byte[] {
				/*0000*/ 0xE8, 0x08, 0x00, 0x00, 0x00,// call 000000000000800Ch
				/*0005*/ 0xB0, 0x00,// mov al,0
				/*0007*/ 0xB8, 0x78, 0x56, 0x34, 0x12,// mov eax,12345678h
			};
			var expectedInstructionOffsets = new uint[] {
				0x0000,
				0x0005,
				0x0007,
			};
			var expectedRelocInfos = Array.Empty<RelocInfo>();
			const BlockEncoderOptions options = BlockEncoderOptions.None;
			EncodeBase(bitness, origRip, originalData, origRip - 1, newData, options, expectedInstructionOffsets, expectedRelocInfos);
		}

		[Fact]
		void Call_near_other_long() {
			var originalData = new byte[] {
				/*0000*/ 0xE8, 0x07, 0x00, 0x00, 0x00,// call 123456789ABCDE0Ch
				/*0005*/ 0xB0, 0x00,// mov al,0
				/*0007*/ 0xB8, 0x78, 0x56, 0x34, 0x12,// mov eax,12345678h
			};
			var newData = new byte[] {
				/*0000*/ 0xFF, 0x15, 0x0A, 0x00, 0x00, 0x00,// call qword ptr [8000000000000010h]
				/*0006*/ 0xB0, 0x00,// mov al,0
				/*0008*/ 0xB8, 0x78, 0x56, 0x34, 0x12,// mov eax,12345678h
				/*000D*/ 0xCC, 0xCC, 0xCC,
				/*0010*/ 0x0C, 0xDE, 0xBC, 0x9A, 0x78, 0x56, 0x34, 0x12,
			};
			var expectedInstructionOffsets = new uint[] {
				uint.MaxValue,
				0x0006,
				0x0008,
			};
			var expectedRelocInfos = new RelocInfo[] {
				new RelocInfo(RelocKind.Offset64, 0x8000000000000010),
			};
			const BlockEncoderOptions options = BlockEncoderOptions.None;
			const ulong origRip = 0x123456789ABCDE00;
			EncodeBase(bitness, origRip, originalData, newRip, newData, options, expectedInstructionOffsets, expectedRelocInfos);
		}
	}
}
#endif
