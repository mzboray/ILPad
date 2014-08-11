using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;

namespace ILPad
{
    class GenerateCommand : ICommand
    {
        private ILPadViewModel viewModel;

        public GenerateCommand(ILPadViewModel viewModel)
        {
            this.viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged
        {
            add
            {
                CommandManager.RequerySuggested += value;
            }
            remove
            {
                CommandManager.RequerySuggested -= value;
            }
        }

        public bool CanExecute(object parameter)
        {
            return !viewModel.IsWorking;
        }

        public async void Execute(object parameter)
        {
            await viewModel.GenerateCode();
        }
    }
}
