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

using Generator.IO;

namespace Generator.Decoder.CSharp {
	sealed class CSharpDecoderTableSerializer : DecoderTableSerializer {
		public string ClassName { get; }

		public CSharpDecoderTableSerializer(GenTypes genTypes, string className, DecoderTableSerializerInfo info)
			: base(genTypes, CSharpIdentifierConverter.Create(), info) {
			ClassName = className;
		}

		public void Serialize(FileWriter writer) {
			writer.WriteFileHeader();
			writer.WriteLine($"#if {CSharpConstants.DecoderDefine}");
			writer.WriteLine($"namespace {CSharpConstants.DecoderNamespace} {{");
			using (writer.Indent()) {
				writer.WriteLine($"static partial class {ClassName} {{");
				using (writer.Indent()) {
					writer.WriteLineNoIndent($"#if {CSharpConstants.HasSpanDefine}");
					writer.WriteLine("static System.ReadOnlySpan<byte> GetSerializedTables() =>");
					writer.WriteLineNoIndent("#else");
					writer.WriteLine("static byte[] GetSerializedTables() =>");
					writer.WriteLineNoIndent("#endif");
					using (writer.Indent()) {
						writer.WriteLine("new byte[] {");
						using (writer.Indent())
							SerializeCore(writer);
						writer.WriteLine("};");
					}

					writer.WriteLine($"const int MaxIdNames = {info.TablesToSerialize.Length};");
					foreach (var name in info.TableIndexNames) {
						var constName = idConverter.Constant($"{name}Index");
						writer.WriteLine($"const uint {constName} = {GetInfo(name).Index};");
					}

				}
				writer.WriteLine("}");
			}
			writer.WriteLine("}");
			writer.WriteLine("#endif");
		}
	}
}
