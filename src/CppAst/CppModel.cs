using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Text;

namespace CppAst
{
    public abstract class CppElement
    {
        public CppSourceSpan Span;

        public string Comment { get; set; }

        public ICppContainer Parent { get; internal set; }
    }
    
    public struct CppSourceLocation
    {
        public CppSourceLocation(string file, int offset, int line, int column)
        {
            File = file;
            Offset = offset;
            Line = line;
            Column = column;
        }

        public string File;

        public int Offset;

        public int Line;

        public int Column;

        public override string ToString()
        {
            return $"{File}({Line}, {Column})";
        }
    }

    public struct CppSourceSpan
    {
        public CppSourceSpan(CppSourceLocation start, CppSourceLocation end)
        {
            Start = start;
            End = end;
        }

        public CppSourceLocation Start;

        public CppSourceLocation End;

        public override string ToString()
        {
            return $"{Start}";
        }
    }

    public class CppGlobalDeclarationContainer : CppElement, ICppGlobalDeclarationContainer
    {
        public CppGlobalDeclarationContainer()
        {
            Macros = new List<CppMacro>();
            Fields = new CppContainerList<CppField>(this);
            Functions = new CppContainerList<CppFunction>(this);
            Enums = new CppContainerList<CppEnum>(this);
            Classes = new CppContainerList<CppClass>(this);
            Typedefs = new CppContainerList<CppTypedef>(this);
            Namespaces = new CppContainerList<CppNamespace>(this);
        }

        public List<CppMacro> Macros { get;  }

        public CppContainerList<CppField> Fields { get; }

        public CppContainerList<CppFunction> Functions { get; }

        public CppContainerList<CppEnum> Enums { get; }

        public CppContainerList<CppClass> Classes { get; }

        public CppContainerList<CppTypedef> Typedefs { get; }

        public CppContainerList<CppNamespace> Namespaces { get; }
    }

    public class CppCompilation : CppGlobalDeclarationContainer
    {
        public CppCompilation()
        {
            Diagnostics = new CppDiagnosticBag();

            System = new CppGlobalDeclarationContainer();
        }
        
        public CppDiagnosticBag Diagnostics { get; }

        public bool HasErrors => Diagnostics.HasErrors;

        public CppGlobalDeclarationContainer System { get; }
    }

    public class CppNamespace : CppElement, ICppMember, ICppGlobalDeclarationContainer
    {
        public CppNamespace(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Fields = new CppContainerList<CppField>(this);
            Functions = new CppContainerList<CppFunction>(this);
            Enums = new CppContainerList<CppEnum>(this);
            Classes = new CppContainerList<CppClass>(this);
            Typedefs = new CppContainerList<CppTypedef>(this);
            Namespaces = new CppContainerList<CppNamespace>(this);
        }
        
        public string Name { get; set; }

        public override string ToString()
        {
            return $"namespace {Name} {{...}}";
        }

        protected bool Equals(CppNamespace other)
        {
            return Equals(Parent, other.Parent) && Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CppNamespace) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Parent != null ? Parent.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public CppContainerList<CppField> Fields { get; }

        public CppContainerList<CppFunction> Functions { get; }
        public CppContainerList<CppEnum> Enums { get; }

        public CppContainerList<CppClass> Classes { get; }
        public CppContainerList<CppTypedef> Typedefs { get; }

        public CppContainerList<CppNamespace> Namespaces { get; }
    }

    public abstract class CppType : CppElement
    {
        protected CppType(CppTypeKind typeKind)
        {
            TypeKind = typeKind;
        }
        
        public CppTypeKind TypeKind { get; }

        protected bool Equals(CppType other)
        {
            return TypeKind == other.TypeKind;
        }

        public virtual bool IsEquivalent(CppType other)
        {
            return Equals((object)other);
        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is CppType type && Equals(type);
        }

        public override int GetHashCode()
        {
            return (int) TypeKind;
        }
    }
    
    public static class CppTypeExtension
    {
        public static string GetName(this CppType type)
        {
            if (type is ICppMember member) return member.Name;
            return type.ToString();
        }
    }

    public enum CppTypeKind
    {
        Primitive,
        Pointer,
        Reference,
        Array,
        Qualified,
        Function,
        Typedef,
        StructOrClass,
        Enum,
        TemplateInstance,
        TemplateParameterType,
        Unexposed,
    }

    public sealed class CppTemplateInstanceType : CppType
    {
        public CppTemplateInstanceType(CppType templateType) : base(CppTypeKind.TemplateInstance)
        {
            TemplateType = templateType ?? throw new ArgumentNullException(nameof(templateType));
            TemplateArguments = new List<CppType>();
        }

        public CppType TemplateType { get; }

        public List<CppType> TemplateArguments { get; }

        private bool Equals(CppTemplateInstanceType other)
        {
            return base.Equals(other) && TemplateType.Equals(other.TemplateType) && TemplateArguments.SequenceEqual(other.TemplateArguments);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CppTemplateInstanceType) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ TemplateType.GetHashCode();
                hashCode = (hashCode * 397) ^ TemplateArguments.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(TemplateType);
            builder.Append("<");
            for (var i = 0; i < TemplateArguments.Count; i++)
            {
                var templateArgument = TemplateArguments[i];
                if (i > 0) builder.Append(", ");
                builder.Append(templateArgument.GetName());
            }

            builder.Append(">");
            return builder.ToString();
        }
    }



    public enum CppTypeQualifier
    {
        Const,

        Volatile,
    }
    
    public enum CppPrimitiveKind
    {
        Void,

        Bool,

        WChar,
        Char,
        Short,
        Int,
        LongLong,

        UnsignedChar,
        UnsignedShort,
        UnsignedInt,
        UnsignedLongLong,

        Float,
        Double,
        LongDouble,
    }

    public sealed class CppPrimitiveType : CppType
    { 
        public static readonly CppPrimitiveType Void = new CppPrimitiveType(CppPrimitiveKind.Void);

        public static readonly CppPrimitiveType Bool = new CppPrimitiveType(CppPrimitiveKind.Bool);

        public static readonly CppPrimitiveType WChar = new CppPrimitiveType(CppPrimitiveKind.WChar);

        public static readonly CppPrimitiveType Char = new CppPrimitiveType(CppPrimitiveKind.Char);

        public static readonly CppPrimitiveType Short = new CppPrimitiveType(CppPrimitiveKind.Short);

        public static readonly CppPrimitiveType Int = new CppPrimitiveType(CppPrimitiveKind.Int);

        public static readonly CppPrimitiveType LongLong = new CppPrimitiveType(CppPrimitiveKind.LongLong);

        public static readonly CppPrimitiveType UnsignedChar = new CppPrimitiveType(CppPrimitiveKind.UnsignedChar);

        public static readonly CppPrimitiveType UnsignedShort = new CppPrimitiveType(CppPrimitiveKind.UnsignedShort);

        public static readonly CppPrimitiveType UnsignedInt = new CppPrimitiveType(CppPrimitiveKind.UnsignedInt);

        public static readonly CppPrimitiveType UnsignedLongLong = new CppPrimitiveType(CppPrimitiveKind.UnsignedLongLong);

        public static readonly CppPrimitiveType Float = new CppPrimitiveType(CppPrimitiveKind.Float);

        public static readonly CppPrimitiveType Double = new CppPrimitiveType(CppPrimitiveKind.Double);

        public static readonly CppPrimitiveType LongDouble = new CppPrimitiveType(CppPrimitiveKind.LongDouble);

        private CppPrimitiveType(CppPrimitiveKind kind) : base(CppTypeKind.Primitive)
        {
            Kind = kind;
        }

        public CppPrimitiveKind Kind { get; }

        public override string ToString()
        {
            switch (Kind)
            {
                case CppPrimitiveKind.Void:
                    return "void";
                case CppPrimitiveKind.WChar:
                    return "wchar";
                case CppPrimitiveKind.Char:
                    return "char";
                case CppPrimitiveKind.Short:
                    return "short";
                case CppPrimitiveKind.Int:
                    return "int";
                case CppPrimitiveKind.LongLong:
                    return "long long";
                case CppPrimitiveKind.UnsignedChar:
                    return "unsigned char";
                case CppPrimitiveKind.UnsignedShort:
                    return "unsigned short";
                case CppPrimitiveKind.UnsignedInt:
                    return "unsigned int";
                case CppPrimitiveKind.UnsignedLongLong:
                    return "unsigned long long";
                case CppPrimitiveKind.Float:
                    return "float";
                case CppPrimitiveKind.Double:
                    return "double";
                case CppPrimitiveKind.LongDouble:
                    return "long double";
                default:
                    throw new InvalidOperationException($"Unhandled PrimitiveKind: {Kind}");
            }
        }

        private bool Equals(CppPrimitiveType other)
        {
            return base.Equals(other) && Kind == other.Kind;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppPrimitiveType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int) Kind;
            }
        }
    }

    public sealed class CppQualifiedType : CppType
    {
        public CppQualifiedType(CppTypeQualifier qualifier, CppType elementType) : base(CppTypeKind.Qualified)
        {
            Qualifier = qualifier;
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public CppTypeQualifier Qualifier { get; }

        public CppType ElementType { get; }

        public override string ToString()
        {
            return $"{Qualifier.ToString().ToLowerInvariant()} {ElementType.GetName()}";
        }

        private bool Equals(CppQualifiedType other)
        {
            return base.Equals(other) && Qualifier == other.Qualifier && ElementType.Equals(other.ElementType);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppQualifiedType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (int) Qualifier;
                hashCode = (hashCode * 397) ^ ElementType.GetHashCode();
                return hashCode;
            }
        }
    }
    
    public sealed class CppArrayType : CppType, IEquatable<CppArrayType>
    {
        public CppArrayType(CppType elementType, int size) : base(CppTypeKind.Array)
        { 
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
            Size = size;
        }

        public CppType ElementType { get; }

        public int Size { get; }

        public bool Equals(CppArrayType other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && ElementType.Equals(other.ElementType) && Size == other.Size;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppArrayType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ ElementType.GetHashCode();
                hashCode = (hashCode * 397) ^ Size;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{ElementType.GetName()}[{Size}]";
        }
    }

    public sealed class CppPointerType : CppType
    {
        public CppPointerType(CppType elementType) : base(CppTypeKind.Pointer)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public CppType ElementType { get; }

        private bool Equals(CppPointerType other)
        {
            return base.Equals(other) && ElementType.Equals(other.ElementType);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppPointerType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ ElementType.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{ElementType.GetName()}*";
        }
    }

    public sealed class CppReferenceType : CppType
    {
        public CppReferenceType(CppType elementType) : base(CppTypeKind.Reference)
        {
            ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
        }

        public CppType ElementType { get; }

        private bool Equals(CppReferenceType other)
        {
            return base.Equals(other) && ElementType.Equals(other.ElementType);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppReferenceType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ ElementType.GetHashCode();
            }
        }

        public override string ToString()
        {
            return $"{ElementType.GetName()}&";
        }
    }

    public sealed class CppFunctionType : CppType
    {
        public CppFunctionType(CppType returnType) : base(CppTypeKind.Function)
        {
            ReturnType = returnType ?? throw new ArgumentNullException(nameof(returnType));
            ParameterTypes = new List<CppParameter>();
        }

        public CppType ReturnType { get; set; }
        
        public List<CppParameter> ParameterTypes { get; }

        private bool Equals(CppFunctionType other)
        {
            return base.Equals(other) && ReturnType.Equals(other.ReturnType) && ParameterTypes.SequenceEqual(other.ParameterTypes);

        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppFunctionType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ ReturnType.GetHashCode();
                foreach (var parameterType in ParameterTypes)
                {
                    hashCode = (hashCode * 397) ^ parameterType.GetHashCode();
                }
                return hashCode;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(ReturnType.GetName());
            builder.Append(" ");
            builder.Append("(*)(");
            for (var i = 0; i < ParameterTypes.Count; i++)
            {
                var param = ParameterTypes[i];
                if (i > 0) builder.Append(", ");
                builder.Append(param);
            }

            builder.Append(")");
            return builder.ToString();
        }
    }


    public interface ICppTemplateOwner
    {
        List<CppTemplateParameterType> TemplateParameters { get; }
    }

    public sealed class CppTemplateParameterType : CppType
    {
        public CppTemplateParameterType(string name) : base(CppTypeKind.TemplateParameterType)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        public string Name { get; }

        private bool Equals(CppTemplateParameterType other)
        {
            return base.Equals(other) && Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppTemplateParameterType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }
        
        public override string ToString()
        {
            return Name.ToString();
        }
    }


    public sealed class CppUnexposedType : CppType
    {
        public CppUnexposedType(string name) : base(CppTypeKind.Unexposed)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }
        public string Name { get; }

        private bool Equals(CppTemplateParameterType other)
        {
            return base.Equals(other) && Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppTemplateParameterType other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Name.GetHashCode();
            }
        }

        public override string ToString()
        {
            return Name.ToString();
        }
    }


    public sealed class CppTypedef : CppType, ICppMemberWithVisibility
    {
        public CppTypedef(string name, CppType type) : base(CppTypeKind.Typedef)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type ?? throw new ArgumentNullException(nameof(type));
        }

        public CppVisibility Visibility { get; set; }

        public string Name { get; set; }

        public CppType Type { get; }

        private bool Equals(CppTypedef other)
        {
            return base.Equals(other) && Name.Equals(other.Name) && Type.Equals(other.Type);
        }

        public override bool IsEquivalent(CppType other)
        {
            // Special case for typedef, as they are aliasing, we don't care about the name
            return Type.IsEquivalent(other);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppTypedef other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Type.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"typedef {Type.GetName()} {Name}";
        }
    }

    public interface ICppContainer
    {
    }

    public interface ICppDeclarationContainer : ICppContainer
    {
        CppContainerList<CppField> Fields { get; }

        CppContainerList<CppFunction> Functions { get; }

        CppContainerList<CppEnum> Enums { get; }

        CppContainerList<CppClass> Classes { get; }

        CppContainerList<CppTypedef> Typedefs { get; }
    }

    public interface ICppGlobalDeclarationContainer : ICppDeclarationContainer
    {
        CppContainerList<CppNamespace> Namespaces { get; }
    }

    [DebuggerTypeProxy(typeof(ContainerListDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public class CppContainerList<TElement> : IList<TElement> where TElement : CppElement, ICppMember
    {
        private ICppContainer _container;
        private readonly List<TElement> _elements;

        public CppContainerList(ICppContainer container)
        {
            _container = container ?? throw new ArgumentNullException(nameof(container));
            _elements = new List<TElement>();
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return _elements.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) _elements).GetEnumerator();
        }

        public void Add(TElement item)
        {
            if (item.Parent != null)
            {
                throw new ArgumentException("The item belongs already to a container");
            }
            item.Parent = _container;
            _elements.Add(item);
        }

        public void Clear()
        {
            foreach (var element in _elements)
            {
                element.Parent = null;
            }

            _elements.Clear();
        }

        public bool Contains(TElement item)
        {
            return _elements.Contains(item);
        }

        public void CopyTo(TElement[] array, int arrayIndex)
        {
            _elements.CopyTo(array, arrayIndex);
        }

        public bool Remove(TElement item)
        {
            if (_elements.Remove(item))
            {
                item.Parent = null;
                return true;
            }
            return false;
        }

        public int Count => _elements.Count;

        public bool IsReadOnly => false;

        public int IndexOf(TElement item)
        {
            return _elements.IndexOf(item);
        }

        public void Insert(int index, TElement item)
        {
            if (item.Parent != null)
            {
                throw new ArgumentException("The item belongs already to a container");
            }

            item.Parent = _container;
            _elements.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            var element = _elements[index];
            element.Parent = null;
            _elements.RemoveAt(index);
        }

        public TElement this[int index]
        {
            get => _elements[index];
            set => _elements[index] = value;
        }
    }

    class ContainerListDebugView<T>
    {
        private ICollection<T> collection;

        public ContainerListDebugView(ICollection<T> collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");
            this.collection = collection;
        }

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get
            {
                T[] array = new T[this.collection.Count];
                this.collection.CopyTo(array, 0);
                return array;
            }
        }
    }

    public interface ICppMember
    {
        //ICppDeclarationContainer Parent { get; }

        string Name { get; set; }
    }

    public interface ICppMemberWithVisibility : ICppMember
    {
        CppVisibility Visibility { get; set; }
    }

    public enum CppClassKind
    {
        Class,
        Struct,
        Union,
    }

    public sealed class CppClass : CppType, ICppMemberWithVisibility, ICppDeclarationContainer, ICppTemplateOwner
    {
        public CppClass(string name) : base(CppTypeKind.StructOrClass)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BaseTypes = new List<CppBaseType>();
            Fields = new CppContainerList<CppField>(this);
            Constructors = new CppContainerList<CppFunction>(this);
            Functions = new CppContainerList<CppFunction>(this);
            Enums = new CppContainerList<CppEnum>(this);
            Classes = new CppContainerList<CppClass>(this);
            Typedefs = new CppContainerList<CppTypedef>(this);
            TemplateParameters = new List<CppTemplateParameterType>();
        }
        
        public CppClassKind ClassKind { get; set; }

        public string Name { get; set; }

        public CppVisibility Visibility { get; set; }

        public bool IsDefinition { get; set; }

        public List<CppBaseType> BaseTypes { get; }

        public CppContainerList<CppField> Fields { get; }

        public CppContainerList<CppFunction> Constructors { get; set; }

        public CppContainerList<CppFunction> Functions { get; }

        public CppContainerList<CppEnum> Enums { get; }

        public CppContainerList<CppClass> Classes { get; }

        public CppContainerList<CppTypedef> Typedefs { get; }

        public List<CppTemplateParameterType> TemplateParameters { get; }

        private bool Equals(CppClass other)
        {
            return base.Equals(other) && Equals(Parent, other.Parent) && Name.Equals(other.Name);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppClass other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Parent != null ? Parent.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Name.GetHashCode();
                return hashCode;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            switch (ClassKind)
            {
                case CppClassKind.Class:
                    builder.Append("class ");
                    break;
                case CppClassKind.Struct:
                    builder.Append("struct ");
                    break;
                case CppClassKind.Union:
                    builder.Append("union ");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            builder.Append(Name);

            if (BaseTypes.Count > 0)
            {
                builder.Append(" : ");
                for (var i = 0; i < BaseTypes.Count; i++)
                {
                    var baseType = BaseTypes[i];
                    if (i > 0) builder.Append(", ");
                    builder.Append(baseType);
                }
            }

            builder.Append(" { ... }");
            return builder.ToString();
        }
    }

    public sealed class CppBaseType : CppElement
    {
        public CppBaseType(CppType baseType)
        {
            Type = baseType ?? throw new ArgumentNullException(nameof(baseType));
        }

        public CppVisibility Visibility { get; set; }

        public bool IsVirtual { get; set; }

        public CppType Type { get; }
        
        public override string ToString()
        {
            var builder = new StringBuilder();
            if (Visibility != CppVisibility.Default && Visibility != CppVisibility.Public)
            {
                builder.Append(Visibility.ToString().ToLowerInvariant()).Append(" ");
            }

            if (IsVirtual)
            {
                builder.Append("virtual ");
            }

            builder.Append(Type.GetName());
            return builder.ToString();
        }
    }

    public enum CppVisibility
    {
        Default,

        Public,

        Protected,

        Private,
    }
    
    public sealed class CppField : CppElement, ICppMemberWithVisibility
    {
        public CppField(CppType type, string name)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name;
        }

        public CppVisibility Visibility { get; set; }

        public CppStorageQualifier StorageQualifier { get; set; }

        public CppType Type { get; }

        public string Name { get; set; }

        public CppValue DefaultValue { get; set; } 

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (Visibility != CppVisibility.Default)
            {
                builder.Append(Visibility.ToString().ToLowerInvariant());
                builder.Append(" ");
            }

            if (StorageQualifier != CppStorageQualifier.None)
            {
                builder.Append(StorageQualifier.ToString().ToLowerInvariant());
                builder.Append(" ");
            }

            builder.Append(Type.GetName());
            builder.Append(" ");
            builder.Append(Name);

            if (DefaultValue?.Value != null)
            {
                builder.Append(" = ");
                builder.Append(DefaultValue);
            }

            return builder.ToString();
        }
    }

    [Flags]
    public enum CppFunctionFlags
    {
        None = 0,

        Const = 1 << 0,

        Defaulted = 1 << 1,

        Pure = 1 << 2,

        Virtual = 1 << 3,
    }

    [Flags]
    public enum CppAttributeFlags
    {
        None = 0,
        DllImport = 1 << 0,
        DllExport = 1 << 0,
    }

    public class CppMacro : CppElement
    {
        public CppMacro(string name)
        {
            Name = name;
            Tokens = new List<CppToken>();
        }

        public string Name { get; set; }

        public List<string> Parameters { get; set; }

        public List<CppToken> Tokens { get; }

        public string Value { get; set; }

        public void UpdateValueFromTokens()
        {
            Value = CppToken.TokensToString(Tokens);
        }
        
        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.Append(Name);
            if (Parameters != null)
            {
                builder.Append("(");
                for (var i = 0; i < Parameters.Count; i++)
                {
                    var parameter = Parameters[i];
                    if (i > 0) builder.Append(", ");
                    builder.Append(parameter);
                }

                builder.Append(")");
            }

            builder.Append(" = ").Append(Value);
            return builder.ToString();
        }
    }
    
    public enum CppTokenKind
    {
        Punctuation,
        Keyword,
        Identifier,
        Literal,
        Comment,
    }

    public class CppToken : CppElement
    {
        public CppToken(CppTokenKind kind, string text)
        {
            Kind = kind;
            Text = text;
        }

        public CppTokenKind Kind { get; set; }

        public string Text { get; set; }
        
        public override string ToString()
        {
            return Text;
        }

        public static string TokensToString(IEnumerable<CppToken> tokens)
        {
            var builder = new StringBuilder();
            CppTokenKind previousKind = 0;
            foreach (var token in tokens)
            {
                // If previous token and new token are identifiers/keyword, we need a space between them
                if (IsTokenIdentifierOrKeyword(previousKind) && IsTokenIdentifierOrKeyword(token.Kind))
                {
                    builder.Append(" ");
                }
                builder.Append(token.Text);
            }

            return builder.ToString();
        }

        private static bool IsTokenIdentifierOrKeyword(CppTokenKind kind)
        {
            return kind == CppTokenKind.Identifier || kind == CppTokenKind.Keyword;
        }
    }

    public sealed class CppFunction : CppElement, ICppMemberWithVisibility, ICppTemplateOwner
    {
        public CppFunction(string name)
        {
            Name = name;
            Parameters = new List<CppParameter>();
            TemplateParameters = new List<CppTemplateParameterType>();
        }

        public CppVisibility Visibility { get; set; }

        public CppAttributeFlags AttributeFlags { get; set; }

        public CppStorageQualifier StorageQualifier { get; set; }

        public CppType ReturnType { get; set; }

        public bool IsConstructor { get; set; }

        public string Name { get; set; }

        public List<CppParameter> Parameters { get; }

        public CppFunctionFlags Flags { get; set; }

        public List<CppTemplateParameterType> TemplateParameters { get; }

        public override string ToString()
        {
            var builder = new StringBuilder();

            if (Visibility != CppVisibility.Default)
            {
                builder.Append(Visibility.ToString().ToLowerInvariant());
                builder.Append(" ");
            }

            if (StorageQualifier != CppStorageQualifier.None)
            {
                builder.Append(StorageQualifier.ToString().ToLowerInvariant());
                builder.Append(" ");
            }

            if ((Flags & CppFunctionFlags.Virtual) != 0)
            {
                builder.Append("virtual ");
            }

            if (!IsConstructor)
            {
                if (ReturnType != null)
                {
                    builder.Append(ReturnType.GetName());
                    builder.Append(" ");
                }
                else
                {
                    builder.Append("void ");
                }
            }

            builder.Append(Name);
            builder.Append("(");
            for (var i = 0; i < Parameters.Count; i++)
            {
                var param = Parameters[i];
                if (i > 0) builder.Append(", ");
                builder.Append(param);
            }

            builder.Append(")");

            if ((Flags & CppFunctionFlags.Const) != 0)
            {
                builder.Append(" const");
            }

            if ((Flags & CppFunctionFlags.Pure) != 0)
            {
                builder.Append(" = 0");
            }
            return builder.ToString();
        }
    }

    public sealed class CppEnum : CppType, ICppMemberWithVisibility, ICppContainer
    {
        public CppEnum(string name) : base(CppTypeKind.Enum)
        {
            Name = name;
            Items = new CppContainerList<CppEnumItem>(this);
        }
        public CppVisibility Visibility { get; set; }

        public string Name { get; set; }

        public bool IsScoped { get; set; }

        public CppType IntegerType { get; set; }

        public CppContainerList<CppEnumItem> Items { get; }

        private bool Equals(CppEnum other)
        {
            return base.Equals(other) && Equals(Parent, other.Parent) && Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppEnum other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Parent != null ? Parent.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"enum {Name} {{...}}";
        }
    }

    public sealed class CppEnumItem : CppElement, ICppMember
    {
        public CppEnumItem(string name, long value)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Value = value;
        }

        public string Name { get; set; }
        
        public long Value { get; set; }

        public override string ToString()
        {
            return $"{Name} = {Value}";
        }
    }

    public sealed class CppParameter : CppElement
    {
        public CppParameter(CppType type, string name)
        {
            Type = type ?? throw new ArgumentNullException(nameof(type));
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public CppType Type { get; }

        public string Name { get; }

        public CppValue DefaultValue { get; set; }

        private bool Equals(CppParameter other)
        {
            return Equals(Type, other.Type) && Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is CppParameter other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Name != null ? Name.GetHashCode() : 0);
            }
        }

        public override string ToString()
        {
            return DefaultValue?.Value != null ? $"{Type.GetName()} {Name} = {DefaultValue}" : $"{Type.GetName()} {Name}";
        }
    }

    public class CppValue : CppElement
    {
        public CppValue(object value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public object Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public enum CppStorageQualifier
    {
        None,
        Extern,
        Static,
    }

    //public static class CppMemberExtensions
    //{
    //    public static string GetFullName(this ICppMember cppMember)
    //    {
    //        var builder = new StringBuilder();
    //        GetFullName(cppMember, builder);
    //        return builder.ToString();

    //    }
    //    private static void GetFullName(ICppMember cppMember, StringBuilder builder)
    //    {
    //        if (cppMember.Parent is ICppMember parentMember)
    //        {
    //            GetFullName(parentMember, builder);
    //            builder.Append("::");
    //        }
    //        builder.Append(cppMember.Name);
    //    }
    //}
}