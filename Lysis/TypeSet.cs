using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Lysis
{
    public enum CellType
    {
        None,
        Bool,
        Float,
        Character,
        Tag,
        Function
    };

    public struct PawnType
    {
        private CellType type_;
        private Tag tag_;

        public bool equalTo(PawnType other)
        {
            return type_ == other.type_ && tag_ == other.tag_;
        }
        public CellType type
        {
            get { return type_; }
        }
        public Tag tag
        {
            get { return tag_; }
        }
        public bool isString
        {
            get { return type_ == CellType.Tag && tag_.name == "String"; }
        }

        public PawnType(Tag tag)
        {
            if (tag.name == "_")
            {
                type_ = CellType.None;
                tag_ = null;
            }
            else if (tag.name == "Float")
            {
                type_ = CellType.Float;
                tag_ = null;
            }
            else if (tag.name == "bool")
            {
                type_ = CellType.Bool;
                tag_ = null;
            }
            else if (SourcePawn.OpcodeHelpers.IsFunctionTag(tag))
            {
                type_ = CellType.Function;
                tag_ = null;
            }
            else
            {
                type_ = CellType.Tag;
                tag_ = tag;
            }
        }
        public PawnType(CellType type)
        {
            type_ = type;
            tag_ = null;
        }
    };

    public class TypeUnit
    {
        public enum Kind
        {
            Cell,
            Reference,
            Array
        };

        private Kind kind_;
        private PawnType type_;         // kind_ == Cell or Array
        private int dims_;              // kind_ == Array
        private TypeUnit ref_;          // kind_ == Reference

        public TypeUnit(PawnType type)
        {
            kind_ = Kind.Cell;
            type_ = type;
        }
        public TypeUnit(PawnType type, int dims)
        {
            kind_ = Kind.Array;
            type_ = type;
            dims_ = dims;
        }
        public TypeUnit(TypeUnit other)
        {
            kind_ = Kind.Reference;
            ref_ = other;
        }
        public Kind kind
        {
            get { return kind_; }
        }
        public int dims
        {
            get
            {
                //Debug.Assert(kind_ == Kind.Array);
                return dims_;
            }
        }
        public PawnType type
        {
            get
            {
                //Debug.Assert(kind_ == Kind.Cell || kind_ == Kind.Array);
                return type_;
            }
        }
        public TypeUnit inner
        {
            get
            {
                //Debug.Assert(kind_ == Kind.Reference);
                return ref_;
            }
        }

        public TypeUnit load()
        {
            if (kind_ == Kind.Cell)
                return null;
            if (kind_ == Kind.Reference)
            {
                if (ref_.kind == Kind.Array)
                    return ref_.load();
                return ref_;
            }
            //Debug.Assert(kind_ == Kind.Array);
            if (dims_ == 1)
            {
                if (type_.isString)
                    return new TypeUnit(new PawnType(CellType.Character));
                return new TypeUnit(type_);
            }
            return new TypeUnit(new TypeUnit(type_, dims_ - 1));
        }

        public bool equalTo(TypeUnit other)
        {
            if (kind_ != other.kind_)
                return false;
            if (kind_ == Kind.Array && dims_ != other.dims_)
                return false;
            if (kind_ == Kind.Reference)
            {
                if ((ref_ == null) != (other.ref_ == null))
                    return false;
                if (ref_ != null && !ref_.equalTo(other.ref_))
                    return false;
            }
            else
            {
                if (!type_.equalTo(other.type_))
                    return false;
            }
            return true;
        }

        public static TypeUnit FromTag(Tag tag)
        {
            return new TypeUnit(new PawnType(tag));
        }

        public static TypeUnit FromVariable(Variable var)
        {
            switch (var.type)
            {
                case VariableType.Normal:
                    return FromTag(var.tag);
                case VariableType.Array:
                    return new TypeUnit(new PawnType(var.tag), var.dims.Length);
                case VariableType.Reference:
                {
                    TypeUnit tu = new TypeUnit(new PawnType(var.tag));
                    return new TypeUnit(tu);
                }
                case VariableType.ArrayReference:
                {
                    TypeUnit tu = new TypeUnit(new PawnType(var.tag), var.dims.Length);
                    return new TypeUnit(tu);
                }
            }
            return null;
        }

        public static TypeUnit FromArgument(Argument arg)
        {
            switch (arg.type)
            {
                case VariableType.Normal:
                    return FromTag(arg.tag);
                case VariableType.Array:
                case VariableType.ArrayReference:
                    return new TypeUnit(new PawnType(arg.tag), arg.dimensions.Length);
            }
            return null;
        }
    };

    public class TypeSet
    {
        private List<TypeUnit> types_ = null;

        public int numTypes
        {
            get { return types_ == null ? 0 : types_.Count; }
        }
        public TypeUnit this[int i]
        {
            get { return types_[i]; }
        }

        public void addType(TypeUnit tu)
        {
            if (types_ == null)
            {
                types_ = new List<TypeUnit>();
            }
            else
            {
                for (int i = 0; i < types_.Count; i++)
                {
                    if (types_[i].equalTo(tu))
                        return;
                }
            }
            types_.Add(tu);
        }
        public void addTypes(TypeSet other)
        {
            for (int i = 0; i < other.numTypes; i++)
                addType(other[i]);
        }
    };
}
