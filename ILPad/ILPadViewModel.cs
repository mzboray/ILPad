using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.MSBuild;
using ReactiveUI;
using System.Reactive.Linq;

namespace ILPad
{
    class ILPadViewModel : ReactiveObject
    {
        private static readonly string Indent = "    ";
        private static readonly MetadataReference Mscorlib = new MetadataFileReference(typeof(object).Assembly.Location);
        private static readonly MetadataReference SystemDll = new MetadataFileReference(typeof(Uri).Assembly.Location);
        private static readonly MetadataReference SystemCore = new MetadataFileReference(typeof(Enumerable).Assembly.Location);
        private static readonly MetadataReference CSharp = new MetadataFileReference(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location);

        private string _sourceText, _outputText;
        private bool _isError, _isWorking;
        private const string InitialCode = @"using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main()
    {
        // Enter your code here.
    }
}";
        public ILPadViewModel()
        {
            _sourceText = InitialCode;
            CompilerOptions = new CompilerOptionsViewModel();
            var command = ReactiveCommand.Create();
            command.Subscribe(async _ =>
            {
                await Task.Run(() =>
                {
                    var tree = SyntaxFactory.ParseSyntaxTree(SourceText);
                    var newRoot = Formatter.Format(tree.GetRoot(), MSBuildWorkspace.Create());
                    SourceText = newRoot.GetText().ToString();
                });
            });
            CleanupCommand = command;
            this.WhenAny(vm => vm.SourceText, c => string.IsNullOrEmpty(c.Value))
                .Throttle(TimeSpan.FromSeconds(1)).Subscribe(async _ =>
            {
                await GenerateCode();
            });
        }

        public ICommand CleanupCommand { get; set; }

        public CompilerOptionsViewModel CompilerOptions { get; set; }

        public string SourceText
        {
            get { return _sourceText; }
            set { this.RaiseAndSetIfChanged(ref _sourceText, value); }
        }

        public string OutputText
        {
            get { return _outputText; }
            set { this.RaiseAndSetIfChanged(ref _outputText, value); }
        }

        public bool IsError
        {
            get { return _isError; }
            set { this.RaiseAndSetIfChanged(ref _isError, value); }
        }

        public bool IsWorking
        {
            get { return _isWorking; }
            set { this.RaiseAndSetIfChanged(ref _isWorking, value); }
        }

        public async Task GenerateCode()
        {
            IsWorking = true;
            try
            {
                var result = await Task.Run(() =>
                {
                    var tree = SyntaxFactory.ParseSyntaxTree(SourceText);
                    var compilation = CSharpCompilation.Create("RoslynDynamicAssembly", options: CompilerOptions.GetOptions())
                    .AddReferences(Mscorlib, SystemDll, SystemCore, CSharp).AddSyntaxTrees(tree);
                    using (var ilStream = new MemoryStream())
                    using (var pdbStream = new MemoryStream())
                    {
                        var emitResult = compilation.Emit(ilStream, pdbStream: pdbStream);
                        if (!emitResult.Success)
                        {
                            StringBuilder errors = new StringBuilder();
                            foreach (var diagnostic in emitResult.Diagnostics)
                            {
                                errors.Append(diagnostic.ToString()).AppendLine();
                            }
                            return Tuple.Create(false, errors.ToString());
                        }

                        ilStream.Position = 0;
                        var assembly = AssemblyDefinition.ReadAssembly(ilStream);
                        using (var stringWriter = new StringWriter())
                        using (var writer = new IndentedTextWriter(stringWriter, Indent))
                        {
                            foreach (var type in assembly.MainModule.Types.Skip(1))
                            {
                                WriteType(writer, type);
                            }

                            return Tuple.Create(true, stringWriter.GetStringBuilder().ToString());
                        }                        
                    }

                });

                IsError = !result.Item1;
                OutputText = result.Item2;
            }
            finally
            {
                IsWorking = false;
            }
        }

        private void WriteType(IndentedTextWriter writer, TypeDefinition type)
        {
            writer.WriteLine(type.GetDeclarationString());
            writer.Indent++;
            if (type.HasFields)
            {
                writer.WriteLine("// Fields: ");
                foreach (var field in type.Fields)
                {
                    writer.WriteLine(field);
                }
                writer.WriteLine();
            }

            if (type.HasProperties)
            {
                writer.WriteLine("// Properties: ");
                foreach (var prop in type.Properties)
                {
                    writer.WriteLine(prop);
                }
                writer.WriteLine();
            }

            if (type.HasEvents)
            {
                writer.WriteLine("// Events: ");
                foreach (var evt in type.Events)
                {
                    writer.WriteLine(evt);
                }
                writer.WriteLine();
            }

            if (type.HasMethods)
            {
                writer.WriteLine("// Methods: ");
                foreach (var method in type.Methods)
                {
                    writer.WriteLine(method.GetDeclarationString());

                    if (method.Body == null)
                        continue;

                    writer.Indent++;
                    foreach (var il in method.Body.Instructions)
                    {
                        writer.WriteLine("IL_{0}: {1} {2}", il.Offset.ToString("X").PadLeft(4, '0').ToLower(), il.OpCode, il.Operand);
                    }
                    writer.Indent--;
                    writer.WriteLine();
                }
            }

            if (type.HasNestedTypes)
            {
                writer.WriteLine("// Nested Types: ");
                foreach(var nestedType in type.NestedTypes)
                {
                    WriteType(writer, nestedType);
                }
                writer.WriteLine();
            }
            writer.Indent--;
        }
    }
}
