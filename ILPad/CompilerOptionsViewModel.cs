using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace ILPad
{
    public class CompilerOptionsViewModel : INotifyPropertyChanged
    {
        private bool _optimize = false;
        public bool Optimize
        {
            get
            {
                return _optimize;
            }
            set
            {
                if (value != _optimize)
                {
                    _optimize = value;
                    OnPropertyChanged("Optimize");
                }
            }
        }

        private bool _allowUnsafe = false;
        public bool AllowUnsafe
        {
            get
            {
                return _allowUnsafe;
            }
            set
            {
                if (value != _allowUnsafe)
                {
                    _allowUnsafe = value;
                    OnPropertyChanged("AllowUnsafe");
                }
            }
        }

        public CSharpCompilationOptions GetOptions()
        {
            return new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, optimize: Optimize, allowUnsafe: AllowUnsafe);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
