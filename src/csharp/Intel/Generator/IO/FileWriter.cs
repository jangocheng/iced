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
using System.IO;

namespace Generator.IO {
	sealed class FileWriter : IDisposable {
		readonly TextWriter writer;
		readonly TargetLanguage targetLanguage;
		readonly string numberPrefix;
		readonly string numberByteFormat;
		readonly string singleLineCommentPrefix;
		readonly (string begin, string end) multiLineComment;
		bool needSpace;
		bool needIndent;
		int indentCount;
		string indentString;

		public FileWriter(TargetLanguage targetLanguage, TextWriter writer) {
			this.writer = writer;
			this.targetLanguage = targetLanguage;

			switch (targetLanguage) {
			case TargetLanguage.CSharp:
			case TargetLanguage.Rust:
				numberPrefix = "0x";
				numberByteFormat = "X2";
				singleLineCommentPrefix = "// ";
				multiLineComment = ("/*", "*/");
				break;

			default:
				throw new InvalidOperationException();
			}

			indentString = null!;
			needIndent = true;
			InitializeIndent();
		}

		void InitializeIndent() => indentString = GetIndentString(indentCount);

		static string GetIndentString(int indentCount) =>
			indentCount switch {
				1 => "\t",
				2 => "\t\t",
				3 => "\t\t\t",
				4 => "\t\t\t\t",
				5 => "\t\t\t\t\t",
				6 => "\t\t\t\t\t\t",
				7 => "\t\t\t\t\t\t\t",
				8 => "\t\t\t\t\t\t\t\t",
				_ => new string('\t', indentCount),
			};

		public readonly struct Indenter : IDisposable {
			readonly FileWriter writer;
			readonly int indent;

			public Indenter(FileWriter writer, int indent) {
				this.writer = writer;
				this.indent = indent;
				writer.IndentPrivate(indent);
			}

			public void Dispose() => writer.UnindentPrivate(indent);
		}

		public Indenter Indent() => new Indenter(this, 1);
		public Indenter Indent(int indent) => new Indenter(this, indent);

		void IndentPrivate(int indent) {
			if (indent < 0)
				throw new ArgumentOutOfRangeException(nameof(indent));
			indentCount += indent;
			InitializeIndent();
		}

		void UnindentPrivate(int indent) {
			if (indent < 0)
				throw new ArgumentOutOfRangeException(nameof(indent));
			indentCount -= indent;
			if (indentCount < 0)
				throw new InvalidOperationException();
			InitializeIndent();
		}

		void WriteIndent() {
			if (needIndent) {
				needIndent = false;
				writer.Write(indentString);
			}
		}

		static readonly string[] licenseHeader = new string[] {
			"Copyright (C) 2018-2019 de4dot@gmail.com",
			"",
			"Permission is hereby granted, free of charge, to any person obtaining",
			"a copy of this software and associated documentation files (the",
			"\"Software\"), to deal in the Software without restriction, including",
			"without limitation the rights to use, copy, modify, merge, publish,",
			"distribute, sublicense, and/or sell copies of the Software, and to",
			"permit persons to whom the Software is furnished to do so, subject to",
			"the following conditions:",
			"",
			"The above copyright notice and this permission notice shall be",
			"included in all copies or substantial portions of the Software.",
			"",
			"THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND,",
			"EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF",
			"MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.",
			"IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY",
			"CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,",
			"TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE",
			"SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.",
		};
		static readonly string[] generatorFullFileMessage = new string[] {
			"⚠️This file was generated by GENERATOR!🦹‍♂️",
		};
		static readonly string[] generatorPartialFileMessage = new string[] {
			"⚠️This was generated by GENERATOR!🦹‍♂️",
		};

		void WriteCLangMultiLineComment(string[] lines) {
			WriteLine(multiLineComment.begin);
			foreach (var line in lines)
				WriteLine(line);
			WriteLine(multiLineComment.end);
		}

		void WriteCLangSingleLineComment(string[] lines) {
			foreach (var msg in lines)
				WriteLine($"{singleLineCommentPrefix}{msg}");
		}

		void WriteCLangHeader() {
			WriteCLangMultiLineComment(licenseHeader);
			WriteLine();
			WriteCLangSingleLineComment(generatorFullFileMessage);
			WriteLine();
		}

		public void WritePartialGeneratedComment() => WriteCLangSingleLineComment(generatorPartialFileMessage);

		public void WriteFileHeader() {
			WriteCLangHeader();
			if (targetLanguage == TargetLanguage.CSharp) {
				WriteLine("#nullable enable");
				WriteLine();
			}
		}

		public void WriteLine(string s) {
			Write(s);
			WriteLine();
		}

		public void WriteByte(byte value) => WriteNumberComma(value.ToString(numberByteFormat));

		void WriteNumberComma(string number) {
			if (needSpace)
				Write(" ");
			needSpace = true;
			Write(numberPrefix);
			Write(number);
			Write(",");
		}

		public void WriteCompressedUInt32(uint value) {
			for (;;) {
				uint v = value;
				if (v < 0x80)
					WriteByte((byte)value);
				else
					WriteByte((byte)(value | 0x80));
				value >>= 7;
				if (value == 0)
					break;
			}
		}

		public void WriteCommentLine(string s) {
			Write(singleLineCommentPrefix);
			Write(s);
			WriteLine();
		}

		public void Write(string s) {
			WriteIndent();
			WriteNoIndent(s);
		}

		public void WriteLine() {
			writer.WriteLine();
			needSpace = false;
			needIndent = true;
		}

		public void WriteNoIndent(string s) => writer.Write(s);

		public void WriteLineNoIndent(string s) {
			WriteNoIndent(s);
			WriteLine();
		}

		public void Dispose() => writer.Dispose();
	}
}
