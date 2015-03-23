using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Lysis
{
    public class LGraph
    {
        public LBlock entry;
        public LBlock[] blocks;
        public int nargs;
    }

    public class LBlock
    {
        private uint pc_;
        private LInstruction[] instructions_;
        private List<LBlock> predecessors_ = new List<LBlock>();
        private bool marked_ = false;
        private int id_;
        private LBlock backedge_ = null;
        private LBlock loop_ = null;

        // Immediate dominator.
        private LBlock idom_ = null;

        // List of blocks which dominate this block.
        private LBlock[] dominators_ = null;

        // List of blocks that are immediately dominated by this block.
        private LBlock[] idominated_ = null;

        public LBlock(uint pc)
        {
            pc_ = pc;
        }

        public void setInstructions(LInstruction[] instructions)
        {
            instructions_ = instructions;
        }

        public void addPredecessor(LBlock pred)
        {
            //Debug.Assert(!predecessors_.Contains(pred));
            predecessors_.Add(pred);
        }

        public void mark()
        {
            //Debug.Assert(!marked_);
            marked_ = true;
        }

        public void unmark()
        {
            //Debug.Assert(marked_);
            marked_ = false;
        }

        public bool marked
        {
            get { return marked_; }
        }

        public void setId(int id)
        {
            id_ = id;
        }
        public void setLoopHeader(LBlock backedge)
        {
            backedge_ = backedge;
            loop_ = this;
        }
        public void setInLoop(LBlock loop)
        {
            loop_ = loop;
        }

        public void setImmediateDominator(LBlock idom)
        {
            idom_ = idom;
        }
        public void setDominators(LBlock[] dominators)
        {
            dominators_ = dominators;
        }
        public void setImmediateDominated(LBlock[] idominated)
        {
            idominated_ = idominated;
        }

        public int id
        {
            get { return id_; }
        }
        public LBlock idom
        {
            get { return idom_; }
        }
        public uint pc
        {
            get { return pc_; }
        }
        public LInstruction[] instructions
        {
            get { return instructions_; }
        }

        public LBlock backedge
        {
            get { return backedge_; }
        }
        public int numPredecessors
        {
            get { return predecessors_.Count; }
        }
        public int numSuccessors
        {
            get { return last.numSuccessors; }
        }
        public LBlock getSuccessor(int successor)
        {
            return last.getSuccessor(successor);
        }
        public LBlock getPredecessor(int predecessor)
        {
            return predecessors_[predecessor];
        }
        public LBlock getLoopPredecessor()
        {
            //Debug.Assert(loop_ == this);
            //Debug.Assert(numPredecessors == 2);
            if (getPredecessor(0).id < id)
            {
                //Debug.Assert(getPredecessor(1).id >= id);
                return getPredecessor(0);
            }
            //Debug.Assert(getPredecessor(1).id < id);
            return getPredecessor(1);
        }
        public LBlock[] dominators
        {
            get { return dominators_; }
        }
        public LBlock[] idominated
        {
            get { return idominated_; }
        }
        public LBlock loop
        {
            get { return loop_; }
        }

        public void replaceSuccessor(int pos, LBlock split)
        {
            last.replaceSuccessor(pos, split);
        }

        public void replacePredecessor(LBlock from, LBlock split)
        {
            //Debug.Assert(predecessors_.Contains(from));
            for (int i = 0; i < numPredecessors; i++)
            {
                if (getPredecessor(i) == from)
                {
                    predecessors_[i] = split;
                    break;
                }
            }
            //Debug.Assert(!predecessors_.Contains(from));
        }

        public LControlInstruction last
        {
            get { return (LControlInstruction)instructions[instructions.Length - 1]; }
        }
    }

    public enum VariableType : uint
    {
        Normal,
        Reference,
        Array,
        ArrayReference,
        Variadic
    }

    public class Tag
    {
        private uint tag_id_;
        private string name_;

        public Tag(string name, uint tag_id)
        {
            tag_id_ = tag_id;
            name_ = name;
        }

        public uint tag_id
        {
            get { return tag_id_; }
        }
        public string name
        {
            get { return name_; }
        }
    }

    public class Dimension
    {
        int tag_id_;
        Tag tag_;
        int size_;

        public Dimension(int tag_id, Tag tag, int size)
        {
            tag_id_ = tag_id;
            tag_ = tag;
            size_ = size;
        }

        public Tag tag
        {
            get { return tag_; }
        }
        public int size
        {
            get { return size_; }
        }
    }

    public class Argument
    {
        VariableType type_;
        string name_;
        int tag_id_;
        Tag tag_;
        Dimension[] dims_;

        public Argument(VariableType type, string name, int tag_id, Tag tag, Dimension[] dims)
        {
            type_ = type;
            name_ = name;
            tag_id_ = tag_id;
            tag_ = tag;
            dims_ = dims;
        }

        public VariableType type
        {
            get { return type_; }
        }
        public string name
        {
            get { return name_; }
        }
        public Tag tag
        {
            get { return tag_; }
        }
        public Dimension[] dimensions
        {
            get { return dims_; }
        }
    }

    public class Signature
    {
        protected string name_;
        protected uint tag_id_;
        protected Tag tag_;
        protected Argument[] args_;

        public Signature(string name)
        {
            name_ = name;
        }

        public Tag returnType
        {
            get { return tag_; }
        }
        public uint tag_id
        {
            get { return tag_id_; }
        }
        public string name
        {
            get { return name_; }
        }
        public Argument[] args
        {
            get { return args_; }
        }
        public void setTag(Tag tag)
        {
            tag_ = tag;
        }
    }

    public class Native : Signature
    {
        int index_;

        public Native(string name, int index)
          : base(name)
        {
            index_ = index;
        }

        public void setDebugInfo(int tag_id, Tag tag, Argument[] args)
        {
            tag_id_ = (uint)tag_id;
            tag_ = tag;
            args_ = args;
        }

        public int index
        {
            get { return index_; }
        }
    }

    public enum Register : uint
    {
        Pri,
        Alt
    }

    public enum Scope : uint
    {
        Global,
        Local,
        Static
    }

    public class Variable
    {
        int addr_;
        uint tag_id_;
        Tag tag_;
        uint codeStart_;
        uint codeEnd_;
        VariableType type_;
        Scope scope_;
        string name_;
        Dimension[] dims_;

        public Variable(int addr, int tag_id, Tag tag, uint codeStart,
                        uint codeEnd, VariableType type, Scope scope,
                        string name, Dimension[] dims = null)
        {
            addr_ = addr;
            tag_id_ = (uint)tag_id;
            tag_ = tag;
            codeStart_ = codeStart;
            codeEnd_ = codeEnd;
            type_ = type;
            scope_ = scope;
            name_ = name;
            dims_ = dims;
        }

        public int address
        {
            get { return addr_; }
        }
        public uint codeStart
        {
            get { return codeStart_; }
        }
        public uint codeEnd
        {
            get { return codeEnd_; }
        }
        public string name
        {
            get { return name_; }
        }
        public VariableType type
        {
            get { return type_; }
        }
        public Scope scope
        {
            get { return scope_; }
        }
        public Tag tag
        {
            get { return tag_; }
        }
        public uint tag_id
        {
            get { return tag_id_; }
        }
        public Dimension[] dims
        {
            get { return dims_; }
        }
        public void setTag(Tag tag)
        {
            tag_ = tag;
        }
    }

    public class Function : Signature
    {
        uint addr_;
        uint codeStart_;
        uint codeEnd_;

        public Function(uint addr, uint codeStart, uint codeEnd, string name, Tag tag)
          : base(name)
        {
            addr_ = addr;
            codeStart_ = codeStart;
            codeEnd_ = codeEnd;
            tag_ = tag;
        }
        
        public Function(uint addr, uint codeStart, uint codeEnd, string name, uint tag_id)
          : base(name)
        {
            addr_ = addr;
            codeStart_ = codeStart;
            codeEnd_ = codeEnd;
            tag_id_ = tag_id;
        }

        public void setArguments(List<Argument> from)
        {
            args_ = from.ToArray();
        }
        public uint address
        {
            get { return addr_; }
        }
        public uint codeStart
        {
            get { return codeStart_; }
        }
        public uint codeEnd
        {
            get { return codeEnd_; }
        }
    }
}
