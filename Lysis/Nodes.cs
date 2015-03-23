using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SourcePawn;

namespace Lysis
{
    public enum NodeType
    {
        Sentinel,
        Constant,
        DeclareLocal,
        DeclareStatic,
        LocalRef,
        Jump,
        JumpCondition,
        SysReq,
        Binary,
        BoundsCheck,
        ArrayRef,
        Store,
        Load,
        Return,
        Global,
        String,
        Boolean,
        Float,
        Function,
        Character,
        Call,
        TempName,
        Phi,
        Unary,
        IncDec,
        Heap,
        MemCopy,
        InlineArray,
        Switch
    }

    public abstract class NodeVisitor
    {
        public virtual void visit(DConstant node) { }
        public virtual void visit(DDeclareLocal local) { }
        public virtual void visit(DDeclareStatic local) { }
        public virtual void visit(DLocalRef lref) { }
        public virtual void visit(DJump jump) { }
        public virtual void visit(DJumpCondition jcc) { }
        public virtual void visit(DSysReq sysreq) { }
        public virtual void visit(DBinary binary) { }
        public virtual void visit(DBoundsCheck check) { }
        public virtual void visit(DArrayRef aref) { }
        public virtual void visit(DStore store) { }
        public virtual void visit(DLoad load) { }
        public virtual void visit(DReturn ret) { }
        public virtual void visit(DGlobal global) { }
        public virtual void visit(DString node) { }
        public virtual void visit(DCall call) { }
        public virtual void visit(DPhi phi) { }
        public virtual void visit(DBoolean phi) { }
        public virtual void visit(DCharacter phi) { }
        public virtual void visit(DFloat phi) { }
        public virtual void visit(DFunction phi) { }
        public virtual void visit(DUnary phi) { }
        public virtual void visit(DIncDec phi) { }
        public virtual void visit(DHeap phi) { }
        public virtual void visit(DMemCopy phi) { }
        public virtual void visit(DInlineArray ia) { }
        public virtual void visit(DSwitch switch_) { }
    }

    public class DUse
    {
        private DNode node_;
        private int index_;

        public DUse(DNode node, int index)
        {
            node_ = node;
            index_ = index;
        }

        public DNode node
        {
            get { return node_; }
        }
        public int index
        {
            get { return index_; }
        }
    }

    public abstract class DNode
    {
        private NodeBlock block_;
        private LinkedList<DUse> uses_ = new LinkedList<DUse>();
        private DNode next_;
        private DNode prev_;
        private bool usedAsArrayIndex_ = false;
        private bool usedAsReference_ = false;
        private TypeSet typeSet_ = null;

        protected void addUse(DNode other, int i)
        {
            uses_.AddLast(new DUse(other, i));
        }

        public void initOperand(int i, DNode node)
        {
            if (node != null)
                node.addUse(this, i);
            setOperand(i, node);
        }
        public void replaceOperand(int i, DNode node)
        {
            if (getOperand(i) == node)
                return;

            if (getOperand(i) != null)
                getOperand(i).removeUse(i, this);
            initOperand(i, node);
        }
        public void replaceAllUsesWith(DNode node)
        {
            DUse[] copies = uses.ToArray();
            foreach (DUse use in copies)
                use.node.replaceOperand(use.index, node);
        }
        public void removeUse(int index, DNode node)
        {
            DUse use = null;
            foreach (DUse u in uses)
            {
                if (u.index == index && u.node == node)
                {
                    use = u;
                    break;
                }
            }
            //Debug.Assert(use != null);
            uses.Remove(use);
        }

        // Remove this node from all use chains.
        public void removeFromUseChains()
        {
            for (int i = 0; i < numOperands; i++)
                replaceOperand(i, null);
        }

        public void setBlock(NodeBlock block)
        {
            block_ = block;
        }

        public LinkedList<DUse> uses
        {
            get { return uses_; }
        }
        public NodeBlock block
        {
            get { return block_; }
        }
        public DNode next
        {
            get { return next_; }
            set
            {
                if ((next_ != null && next_.type == NodeType.Store) ||
                    (value != null && value.type == NodeType.Store))
                {
                    //Debug.Assert(true);
                }
                next_ = value;
            }
        }
        public DNode prev
        {
            get { return prev_; }
            set { prev_ = value; }
        }
        public bool usedAsArrayIndex
        {
            get { return usedAsArrayIndex_; }
        }
        public void setUsedAsArrayIndex()
        {
            usedAsArrayIndex_ = true;
        }
        public bool usedAsReference
        {
            get { return usedAsReference_; }
        }
        public void setUsedAsReference()
        {
            usedAsReference_ = true;
        }
        private TypeSet ensureTypeSet()
        {
            if (typeSet_ == null)
                typeSet_ = new TypeSet();
            return typeSet_;
        }
        public void addType(TypeUnit tu)
        {
            //Debug.Assert(tu != null);
            ensureTypeSet().addType(tu);
        }
        public void addTypes(TypeSet ts)
        {
            ensureTypeSet().addTypes(ts);
        }
        public TypeSet typeSet
        {
            get { return ensureTypeSet(); }
        }

        public virtual bool guard
        {
            get { return false; }
        }
        public virtual bool idempotent
        {
            get { return true; }
        }
        public virtual bool controlFlow
        {
            get { return false; }
        }
        public abstract NodeType type { get; }
        public abstract int numOperands { get; }
        public abstract DNode getOperand(int i);
        protected abstract void setOperand(int i, DNode node);
        public abstract void accept(NodeVisitor visitor);

        public virtual DNode applyType(SourcePawnFile file, Tag tag, VariableType type)
        {
            return this;
        }
    }

    public abstract class DNullaryNode : DNode
    {
        public override int numOperands
        {
            get { return 0; }
        }
        public override DNode getOperand(int i)
        {
            throw new Exception("not reached");
        }
        protected override void setOperand(int i, DNode node)
        {
            throw new Exception("not reached");
        }
    }

    public abstract class DUnaryNode : DNode
    {
        private DNode operand_;

        public DUnaryNode(DNode operand)
        {
            initOperand(0, operand);
        }

        public override int numOperands
        {
            get { return 1; }
        }
        public override DNode getOperand(int i)
        {
            //Debug.Assert(i == 0);
            return operand_;
        }
        protected override void setOperand(int i, DNode node)
        {
            //Debug.Assert(i == 0);
            operand_ = node;
        }
    }

    public abstract class DBinaryNode : DNode
    {
        private DNode[] operands_ = new DNode[2];

        public DBinaryNode(DNode operand1, DNode operand2)
        {
            initOperand(0, operand1);
            initOperand(1, operand2);
        }

        public override int numOperands
        {
            get { return 2; }
        }
        public override DNode getOperand(int i)
        {
            return operands_[i];
        }
        protected override void setOperand(int i, DNode node)
        {
            operands_[i] = node;
        }
        public DNode lhs
        {
            get { return getOperand(0); }
        }
        public DNode rhs
        {
            get { return getOperand(1); }
        }
    }

    public class DDeclareLocal : DUnaryNode
    {
        private uint pc_;
        private int offset_;
        private Variable var_;

        public DDeclareLocal(uint pc, DNode value) : base(value)
        {
            pc_ = pc;
        }

        public void setOffset(int offset)
        {
            offset_ = offset;
        }
        public void setVariable(Variable var)
        {
            var_ = var;
        }

        public uint pc
        {
            get { return pc_; }
        }
        public DNode value
        {
            get { return getOperand(0); }
        }
        public int offset
        {
            get { return offset_; }
        }
        public Variable var
        {
            get { return var_; }
        }
        public override NodeType type
        {
            get { return NodeType.DeclareLocal; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public override bool idempotent
        {
            get { return false; }
        }
        public override DNode applyType(SourcePawnFile file, Tag tag, VariableType type)
        {
            if (value == null)
                return null;
            DNode replacement = value.applyType(file, tag, type);
            if (replacement != value)
                replaceOperand(0, replacement);
            return this;
        }
    }

    public class DDeclareStatic : DNullaryNode
    {
        private Variable var_;

        public DDeclareStatic(Variable var)
        {
            var_ = var;
        }

        public Variable var
        {
            get { return var_; }
        }
        public override NodeType type
        {
            get { return NodeType.DeclareStatic; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public override bool idempotent
        {
            get { return false; }
        }
    }

    public class DConstant : DNullaryNode
    {
        private int value_;
        private uint pc_;

        public DConstant(int value)
        {
            value_ = value;
        }

        public DConstant(int value, uint pc)
        {
            value_ = value;
            pc_ = pc;
        }

        public int value
        {
            get { return value_; }
        }
        public uint pc
        {
            get { return pc_; }
        }
        public override NodeType type
        {
            get { return NodeType.Constant; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public override DNode applyType(SourcePawnFile file, Tag tag, VariableType type)
        {
            switch (type)
            {
                case VariableType.Array:
                case VariableType.ArrayReference:
                case VariableType.Reference:
                case VariableType.Variadic:
                {
                    Variable global = file.lookupGlobal(value);
                    if (global != null)
                        return new DGlobal(global);
                    if (tag.name == "String")
                        return new DString(file.stringFromData(value));
                    break;
                }
            }
            return this;
        }
    }

    public class DLocalRef : DUnaryNode
    {
        public DLocalRef(DDeclareLocal local) : base(local)
        {
        }

        public DDeclareLocal local
        {
            get
            {
                var operand = getOperand(0);
                if (operand is DConstant) //workaround to fix: TypePropagation:ForwardTypePropagation.visit(DLocalRef lref)
                {
                    return null;
                }
                return (DDeclareLocal)operand;
            }
        }
        public string LocalName //WORKAROUND: to allow SourceBuilder:buildLocalRef to work..
        {
            get
            {
                DNode operator_ = getOperand(0);
                if (operator_ is DTempName)
                {
                    return ((DTempName)operator_).name;
                }
                if (operator_ is DConstant)
                {
                    return string.Empty;
                }
                return ((DDeclareLocal)operator_).var.name;
            }
        }
        public override NodeType type
        {
            get { return NodeType.LocalRef; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DJump : DNullaryNode
    {
        private NodeBlock target_;
        private bool isBreak_;

        public DJump(NodeBlock target)
        {
            target_ = target;
        }

        public NodeBlock target
        {
            get { return target_; }
        }
        public override NodeType type
        {
            get { return NodeType.Jump; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public override bool idempotent
        {
            get { return false; }
        }
        public override bool controlFlow
        {
            get { return true; }
        }
        public void setBreak()
        {
            isBreak_ = true;
        }
        public bool isBreak
        {
            get { return isBreak_; }
        }
    }

    public class DJumpCondition : DUnaryNode
    {
        private SPOpcode spop_;
        private NodeBlock trueTarget_;
        private NodeBlock falseTarget_;
        private NodeBlock joinTarget_;

        public DJumpCondition(SPOpcode spop, DNode node, NodeBlock lht, NodeBlock rht)
            : base(node)
        {
            // //Debug.Assert(getOperand(0) != null);
            spop_ = spop;
            trueTarget_ = lht;
            falseTarget_ = rht;
        }

        public void rewrite(SPOpcode spop, DNode node)
        {
            spop_ = spop;
            replaceOperand(0, node);
        }

        public SPOpcode spop
        {
            get { return spop_; }
        }
        public NodeBlock trueTarget
        {
            get { return trueTarget_; }
        }
        public NodeBlock falseTarget
        {
            get { return falseTarget_; }
        }
        public override NodeType type
        {
            get { return NodeType.JumpCondition; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public override bool idempotent
        {
            get { return false; }
        }
        public override bool controlFlow
        {
            get { return true; }
        }
        public NodeBlock joinTarget
        {
            get { return joinTarget_; }
        }
        public void setTrueTarget(NodeBlock block)
        {
            trueTarget_ = block;
        }
        public void setFalseTarget(NodeBlock block)
        {
            falseTarget_ = block;
        }
        public void setJoinTarget(NodeBlock block)
        {
            joinTarget_ = block;
        }
        public void setConditional(SPOpcode op)
        {
            spop_ = op;
        }
    }

    public class DSwitch : DUnaryNode
    {
        private LSwitch lir_;

        public DSwitch(DNode node, LSwitch lir)
            : base(node)
        {
            lir_ = lir;
        }
        
        public override NodeType type
        {
            get { return NodeType.Switch; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public override bool idempotent
        {
            get { return false; }
        }
        public override bool controlFlow
        {
            get { return true; }
        }
        public LBlock defaultCase
        {
            get { return lir_.defaultCase; }
        }
        public int numCases
        {
            get { return lir_.numCases; }
        }
        public SwitchCase getCase(int i)
        {
            return lir_.getCase(i);
        }
    }

    public abstract class DCallNode : DNode
    {
        private DNode[] arguments_;

        public DCallNode(DNode[] arguments)
        {
            arguments_ = new DNode[arguments.Length];
            for (int i = 0; i < arguments.Length; i++)
                initOperand(i, arguments[i]);
        }

        public override int numOperands
        {
            get { return arguments_.Length; }
        }
        public override DNode getOperand(int i)
        {
            return arguments_[i];
        }
        protected override void setOperand(int i, DNode node)
        {
            arguments_[i] = node;
        }
        public override bool idempotent
        {
            get { return false; }
        }
    }

    public class DSysReq : DCallNode
    {
        private Native native_;

        public DSysReq(Native native, DNode[] arguments) : base(arguments)
        {
            native_ = native;
        }

        public Native native
        {
            get { return native_; }
        }
        public override NodeType type
        {
            get { return NodeType.SysReq; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DCall : DCallNode
    {
        private Function function_;

        public DCall(Function function, DNode[] arguments)
            : base(arguments)
        {
            function_ = function;
        }

        public Function function
        {
            get { return function_; }
        }
        public override NodeType type
        {
            get { return NodeType.Call; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DUnary : DUnaryNode
    {
        private SPOpcode spop_;

        public DUnary(SPOpcode op, DNode node) : base(node)
        {
            spop_ = op;
        }

        public SPOpcode spop
        {
            get { return spop_; }
        }
        public override NodeType type
        {
            get { return NodeType.Unary; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DBinary : DBinaryNode
    {
        private SPOpcode spop_;

        public DBinary(SPOpcode op, DNode lhs, DNode rhs) : base(lhs, rhs)
        {
            spop_ = op;
        }

        public SPOpcode spop
        {
            get { return spop_; }
        }
        public override NodeType type
        {
            get { return NodeType.Binary; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DBoundsCheck : DUnaryNode
    {
        public DBoundsCheck(DNode index) : base(index)
        {
        }

        public override NodeType type
        {
            get { return NodeType.BoundsCheck; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public override bool guard
        {
            get { return true; }
        }
    }

    public class DArrayRef : DBinaryNode
    {
        private int shift_;

        public DArrayRef(DNode bas, DNode index, int shift = 2) : base(bas, index)
        {
            shift_ = shift;
        }

        public override NodeType type
        {
            get { return NodeType.ArrayRef; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public DNode abase
        {
            get { return getOperand(0); }
        }
        public DNode index
        {
            get { return getOperand(1); }
        }
    }

    public class DStore : DBinaryNode
    {
        private SPOpcode spop_;

        public DStore(DNode addr, DNode value) : base(addr, value)
        {
            //Debug.Assert(value != null);
            spop_ = SPOpcode.nop;
        }

        public void makeStoreOp(SPOpcode op)
        {
            spop_ = op;
        }

        public override NodeType type
        {
            get { return NodeType.Store; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public override bool idempotent
        {
            get { return false; }
        }
        public SPOpcode spop
        {
            get { return spop_; }
        }
    }

    public class DLoad : DUnaryNode
    {
        public DLoad(DNode addr) : base(addr)
        {
        }

        public override NodeType type
        {
            get { return NodeType.Load; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public DNode from
        {
            get { return getOperand(0); }
        }
    }

    public class DReturn : DUnaryNode
    {
        public DReturn(DNode value) : base(value)
        {
        }

        public override NodeType type
        {
            get { return NodeType.Return; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public override bool idempotent
        {
            get { return false; }
        }
        public override bool controlFlow
        {
            get { return true; }
        }
    }

    public class DGlobal : DNullaryNode
    {
        private Variable var_;

        public DGlobal(Variable var)
        {
            //Debug.Assert(var != null);
            var_ = var;
        }

        public Variable var
        {
            get { return var_; }
        }
        public override NodeType type
        {
            get { return NodeType.Global; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DString : DNullaryNode
    {
        private string value_;

        public DString(string value)
        {
            value_ = value;
        }

        public string value
        {
            get { return value_; }
        }
        public override NodeType type
        {
            get { return NodeType.String; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DBoolean : DNullaryNode
    {
        private bool value_;

        public DBoolean(bool value)
        {
            value_ = value;
        }

        public bool value
        {
            get { return value_; }
        }
        public override NodeType type
        {
            get { return NodeType.Boolean; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DCharacter : DNullaryNode
    {
        private char value_;

        public DCharacter(char value)
        {
            value_ = value;
        }

        public char value
        {
            get { return value_; }
        }
        public override NodeType type
        {
            get { return NodeType.Character; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DFloat : DNullaryNode
    {
        private float value_;

        public DFloat(float value)
        {
            value_ = value;
        }

        public float value
        {
            get { return value_; }
        }
        public override NodeType type
        {
            get { return NodeType.Float; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DFunction : DNullaryNode
    {
        private Function function_;
        private uint pc_;

        public DFunction(uint pc, Function value)
        {
            pc_ = pc;
            function_ = value;
        }

        public uint pc
        {
            get { return pc_; }
        }
        public Function function
        {
            get { return function_; }
        }
        public override NodeType type
        {
            get { return NodeType.Function; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DSentinel : DNullaryNode
    {
        public override NodeType type
        {
            get { return NodeType.Sentinel; }
        }
        public override void accept(NodeVisitor visitor)
        {
        }
    }

    public class DTempName : DUnaryNode
    {
        private string name_;

        public DTempName(string name) : base(null)
        {
            name_ = name;
        }

        public void init(DNode node)
        {
            initOperand(0, node);
        }
        public string name
        {
            get { return name_; }
        }
        public override NodeType type
        {
            get { return NodeType.TempName; }
        }
        public override void accept(NodeVisitor visitor)
        {
        }
        public override bool idempotent
        {
            get { return false; }
        }
    }

    public class DPhi : DNode
    {
        private List<DNode> inputs_ = new List<DNode>();

        public DPhi(DNode node)
        {
            addInput(node);
        }

        public void addInput(DNode node)
        {
            inputs_.Add(null);
            initOperand(inputs_.Count - 1, node);
        }

        public override NodeType type
        {
            get { return NodeType.Phi; }
        }
        public override int numOperands
        {
            get { return inputs_.Count; }
        }
        public override DNode getOperand(int i)
        {
            return inputs_[i];
        }
        protected override void setOperand(int i, DNode node)
        {
            inputs_[i] = node;
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
    }

    public class DIncDec : DUnaryNode
    {
        private int amount_;

        public DIncDec(DNode node, int amount)
            : base(node)
        {
            amount_ = amount;
        }

        public override NodeType type
        {
            get { return NodeType.IncDec; }
        }
        public override bool idempotent
        {
            get { return false; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public int amount
        {
            get { return amount_; }
        }
    }

    public class DHeap : DNullaryNode
    {
        private int amount_;

        public DHeap(int amount)
        {
            amount_ = amount;
        }

        public override NodeType type
        {
            get { return NodeType.Heap; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public int amount
        {
            get { return amount_; }
        }
    }

    public class DMemCopy : DBinaryNode
    {
        private int bytes_;

        public DMemCopy(DNode to, DNode from, int bytes)
            : base(to, from)
        {
            bytes_ = bytes;
        }

        public override NodeType type
        {
            get { return NodeType.MemCopy; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public override bool idempotent
        {
            get { return false; }
        }
        public int bytes
        {
            get { return bytes_; }
        }
        public DNode from
        {
            get { return getOperand(1); }
        }
        public DNode to
        {
            get { return getOperand(0); }
        }
    }

    public class DInlineArray : DNullaryNode
    {
        private int address_;
        private int size_;

        public DInlineArray(int address, int size)
        {
            address_ = address;
            size_ = size;
        }

        public override NodeType type
        {
            get { return NodeType.InlineArray; }
        }
        public override void accept(NodeVisitor visitor)
        {
            visitor.visit(this);
        }
        public int address
        {
            get { return address_; }
        }
        public int size
        {
            get { return size_; }
        }
    }
}
