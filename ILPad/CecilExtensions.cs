using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil;

namespace ILPad
{
    static class CecilExtensions
    {
        public static string GetDeclarationString(this TypeDefinition type)
        {
            string accessibility = GetAccessibility(type);
            StringBuilder builder = new StringBuilder();
            builder.Append(accessibility).Append(" ");
            if (type.IsAbstract && type.IsSealed)
            {
                builder.Append("static ");
            }
            else if (type.IsAbstract && !type.IsInterface)
            {
                builder.Append("abstract ");
            }
            else if (type.IsSealed && !type.IsValueType)
            {
                builder.Append("sealed ");
            }
            builder.Append(GetKind(type)).Append(" ");
            builder.Append(type);
            return builder.ToString();
        }

        public static string GetDeclarationString(this MethodDefinition method)
        {
            StringBuilder builder = new StringBuilder();
            string accessibility = GetAccessibility(method);
            builder.Append(accessibility).Append(" ");
            builder.Append(method.IsFinal ? "sealed " : "");
            builder.Append(method.IsVirtual ? "virtual " : "");
            builder.Append(method.IsStatic ? "static " : "");
            builder.Append(method);
            return builder.ToString();
        }

        private static string GetAccessibility(MethodDefinition method)
        {
            if (method.IsPublic)
            {
                return "public";
            }
            else if (method.IsFamily)
            {
                return "protected";
            }
            else if (method.IsAssembly)
            {
                return "internal";
            }
            else if (method.IsFamilyOrAssembly)
            {
                return "protected internal";
            }
            else if (method.IsPrivate)
            {
                return "private";
            }

            throw new NotSupportedException();
        }

        private static string GetKind(TypeDefinition type)
        {
            if (type.IsEnum)
            {
                return "enum";
            }
            else if (type.IsValueType)
            {
                return "struct";
            }
            else if (type.IsInterface)
            {
                return "interface";
            }
            else
            {
                return "class";
            }
        }

        private static string GetAccessibility(TypeDefinition method)
        {
            if (method.IsPublic)
            {
                return "public";
            }
            else if (method.IsNestedFamily)
            {
                return "protected";
            }
            else if (method.IsNestedAssembly)
            {
                return "internal";
            }
            else if (method.IsNestedFamilyAndAssembly)
            {
                return "protected internal";
            }
            else if (method.IsNestedPrivate)
            {
                return "private";
            }
            else if (method.IsNotPublic)
            {
                return "internal";
            }

            throw new NotSupportedException();
        }
    }

}
