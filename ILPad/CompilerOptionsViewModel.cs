using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using ReactiveUI;

namespace ILPad
{
    public class CompilerOptionsViewModel : ReactiveObject
    {
        private bool _optimize = false;
        public bool Optimize
        {
            get { return _optimize; }
            set { this.RaiseAndSetIfChanged(ref _optimize, value); }
        }

        private bool _allowUnsafe = false;
        public bool AllowUnsafe
        {
            get { return _allowUnsafe; }
            set { this.RaiseAndSetIfChanged(ref _allowUnsafe, value); }
        }

        public CSharpCompilationOptions GetOptions()
        {
            return new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimize: Optimize, allowUnsafe: AllowUnsafe);
        }
    }
}
