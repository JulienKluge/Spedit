// vim set: ts=4 sw=4 tw=99 et:
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using SourcePawn;

namespace Lysis
{
    public enum Opcode
    {
        LoadLocal,
        StoreLocal,
        LoadLocalRef,
        StoreLocalRef,
        Load,
        Constant,
        StackAddress,
        Store,
        IndexAddress,
        Move,
        PushReg,
        PushConstant,
        Pop,
        Stack,
        Return,
        Jump,
        JumpCondition,
        AddConstant,
        MulConstant,
        ZeroGlobal,
        IncGlobal,
        IncLocal,
        DecLocal,
        IncI,
        IncReg,
        DecI,
        DecReg,
        Fill,
        Bounds,
        SysReq,
        Swap,
        PushStackAddress,
        DebugBreak,
        Goto,
        PushLocal,
        Exchange,
        Binary,
        PushGlobal,
        StoreGlobal,
        LoadGlobal,
        Call,
        EqualConstant,
        LoadIndex,
        Unary,
        StoreGlobalConstant,
        StoreLocalConstant,
        ZeroLocal,
        Heap,
        MemCopy,
        Switch
    }

    public abstract class LInstruction
    {
        private uint pc_;

        public LInstruction()
        {
        }

        public abstract Opcode op
        {
            get;
        }

        public abstract void print(TextWriter tw);

        public virtual bool isControl()
        {
            return false;
        }

        public void setPc(uint pc)
        {
            pc_ = pc;
        }

        public uint pc
        {
            get { return pc_; }
        }

        public static string RegisterName(Register reg)
        {
            return (reg == Register.Pri) ? "pri" : "alt";
        }
    }

    
    public abstract class LControlInstruction : LInstruction
    {
        protected LBlock[] successors_;

        public LControlInstruction(params LBlock[] blocks)
        {
            successors_ = blocks;
        }

        public virtual void replaceSuccessor(int i, LBlock block)
        {
            successors_[i] = block;
        }

        public virtual int numSuccessors
        {
            get { return successors_.Length; }
        }

        public virtual LBlock getSuccessor(int i)
        {
            return successors_[i];
        }

        public override bool isControl()
        {
            return true;
        }
    }

    public abstract class LInstructionReg : LInstruction
    {
        private Register reg_;

        public LInstructionReg(Register reg)
        {
            reg_ = reg;
        }

        public Register reg
        {
            get { return reg_; }
        }
    }

    public abstract class LInstructionStack : LInstruction
    {
        private int offs_;

        public LInstructionStack(int offset)
        {
            offs_ = offset;
        }

        public int offset
        {
            get { return offs_; }
        }
    }

    public abstract class LInstructionRegStack : LInstruction
    {
        private Register reg_;
        private int offs_;

        public LInstructionRegStack(Register reg, int offset)
        {
            reg_ = reg;
            offs_ = offset;
        }

        public Register reg
        {
            get { return reg_; }
        }

        public int offset
        {
            get { return offs_; }
        }
    }

    public abstract class LInstructionJump : LControlInstruction
    {
        private uint target_offs_;

        public LInstructionJump(uint target_offs, params LBlock[] targets)
            : base(targets)
        {
            target_offs_ = target_offs;
        }

        public uint target_offs
        {
            get { return target_offs_; }
        }
    }

    public class LLoadLocal : LInstructionRegStack
    {
        public LLoadLocal(int offset, Register reg)
            : base(reg, offset)
        {
        }

        public override Opcode op
        {
            get { return Opcode.LoadLocal; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("load.s." + RegisterName(reg) + " " + offset);
        }
    }

    public class LLoadLocalRef : LInstructionRegStack
    {
        public LLoadLocalRef(int offset, Register reg)
            : base(reg, offset)
        {
        }

        public override Opcode op
        {
            get { return Opcode.LoadLocalRef; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("lref.s." + RegisterName(reg) + " " + offset);
        }
    }

    public class LStoreLocal : LInstructionRegStack
    {
        public LStoreLocal(Register reg, int offset)
            : base(reg, offset)
        {
        }

        public override Opcode op
        {
            get { return Opcode.StoreLocal; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("stor.s." + RegisterName(reg) + " " + offset);
        }
    }

    public class LStoreLocalRef : LInstructionRegStack
    {
        public LStoreLocalRef(Register reg, int offset)
            : base(reg, offset)
        {
        }

        public override Opcode op
        {
            get { return Opcode.StoreLocalRef; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("sref.s." + RegisterName(reg) + " " + offset);
        }
    }

    public class LLoad : LInstruction
    {
        private int bytes_;

        public LLoad(int bytes)
        {
            bytes_ = bytes;
        }

        private int bytes
        {
            get { return bytes_; }
        }

        public override Opcode op
        {
            get { return Opcode.Load; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("load.i." + bytes + "   ; pri = [pri]");
        }
    }

    public class LConstant : LInstructionReg
    {
        private int val_;

        public LConstant(int val, Register reg) : base(reg)
        {
            val_ = val;
        }

        public int val
        {
            get { return val_; }
        }

        public override Opcode op
        {
            get { return Opcode.Constant; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("const." + RegisterName(reg) + " " + val);
        }
    }


    public class LStackAddress : LInstructionRegStack
    {
        public LStackAddress(int offset, Register reg)
            : base(reg, offset)
        {
        }

        public override Opcode op
        {
            get { return Opcode.StackAddress; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("addr." + RegisterName(reg) + " " + offset);
        }
    }

    public class LStore : LInstruction
    {
        private int bytes_;

        public LStore(int bytes)
        {
            bytes_ = bytes;
        }

        public int bytes
        {
            get { return bytes_; }
        }

        public override Opcode op
        {
            get { return Opcode.Store; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("stor.i." + bytes + "   ; [alt] = pri");
        }
    }

    public class LIndexAddress : LInstruction
    {
        private int shift_;

        public LIndexAddress(int shift)
        {
            shift_ = shift;
        }

        public int shift
        {
            get { return shift_; }
        }

        public override Opcode op
        {
            get { return Opcode.IndexAddress; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("idxaddr " + shift + " ; pri=alt+(pri<<" + shift + ")");
        }
    }

    public class LMove : LInstructionReg
    {
        public LMove(Register reg)
            : base(reg)
        {
        }

        public override Opcode op
        {
            get { return Opcode.Move; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("move." + RegisterName(reg), ", " +
                (reg == Register.Pri ? RegisterName(Register.Alt) : RegisterName(Register.Pri)));
        }
    }

    public class LPushReg : LInstructionReg
    {
        public LPushReg(Register reg)
            : base(reg)
        {
        }

        public override Opcode op
        {
            get { return Opcode.PushReg; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("push." + RegisterName(reg));
        }
    }

    public class LIncReg : LInstructionReg
    {
        public LIncReg(Register reg)
            : base(reg)
        {
        }

        public override Opcode op
        {
            get { return Opcode.IncReg; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("inc." + RegisterName(reg));
        }
    }

    public class LDecReg : LInstructionReg
    {
        public LDecReg(Register reg)
            : base(reg)
        {
        }

        public override Opcode op
        {
            get { return Opcode.DecReg; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("dec." + RegisterName(reg));
        }
    }

    public class LPushConstant : LInstruction
    {
        private int val_;

        public LPushConstant(int val)
        {
            val_ = val;
        }

        public int val
        {
            get { return val_; }
        }

        public override Opcode op
        {
            get { return Opcode.PushConstant; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("push.c " + val);
        }
    }

    public class LPop : LInstructionReg
    {
        public LPop(Register reg)
            : base(reg)
        {
        }

        public override Opcode op
        {
            get { return Opcode.Pop; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("pop." + RegisterName(reg));
        }
    }

    public class LStack : LInstruction
    {
        private int val_;

        public LStack(int val)
        {
            val_ = val;
        }

        public int amount
        {
            get { return val_; }
        }

        public override Opcode op
        {
            get { return Opcode.Stack; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("stack " + amount);
        }
    }

    public class LReturn : LControlInstruction
    {
        public LReturn() : base()
        {
        }

        public override Opcode op
        {
            get { return Opcode.Return; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("return");
        }
    }

    public class LGoto : LControlInstruction
    {
        public LGoto(LBlock target)
            : base(target)
        {
        }

        public LBlock target
        {
            get { return getSuccessor(0); }
        }

        public override Opcode op
        {
            get { return Opcode.Goto; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("goto block" + target.id);
        }
    }

    public class LJump : LInstructionJump
    {
        public LJump(LBlock target, uint target_offs)
            : base(target_offs, target)
        {
        }

        public override Opcode op
        {
            get { return Opcode.Jump; }
        }

        public LBlock target
        {
            get { return getSuccessor(0); }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("jump block" + target.id);
        }
    }

    public class LJumpCondition : LInstructionJump
    {
        private SPOpcode op_;

        public LJumpCondition(SPOpcode op, LBlock true_target, LBlock false_target, uint target_offs)
            : base(target_offs, true_target, false_target)
        {
            op_ = op;
        }

        public override Opcode op
        {
            get { return Opcode.JumpCondition; }
        }

        public SPOpcode spop
        {
            get { return op_; }
        }
        public LBlock trueTarget
        {
            get { return getSuccessor(0); }
        }
        public LBlock falseTarget
        {
            get { return getSuccessor(1); }
        }

        public override void print(TextWriter tw)
        {
            switch (op_)
            {
                case SPOpcode.jnz:
                    tw.Write("jnz ");
                    break;
                case SPOpcode.jzer:
                    tw.Write("jzero ");
                    break;
                case SPOpcode.jsgeq:
                    tw.Write("jsgeq ");
                    break;
                case SPOpcode.jsgrtr:
                    tw.Write("jsgrtr ");
                    break;
                case SPOpcode.jsleq:
                    tw.Write("jsleq ");
                    break;
                case SPOpcode.jsless:
                    tw.Write("jsless ");
                    break;
                case SPOpcode.jeq:
                    tw.Write("jeq ");
                    break;
                case SPOpcode.jneq:
                    tw.Write("jneq ");
                    break;
                default:
                    throw new Exception("unrecognized spop");
            }
            tw.Write("block" + trueTarget.id + " (block" + falseTarget.id + ")");
        }
    }

    public class LAddConstant : LInstruction
    {
        private int amount_;

        public LAddConstant(int amount)
        {
            amount_ = amount;
        }

        public int amount
        {
            get { return amount_; }
        }

        public override Opcode op
        {
            get { return Opcode.AddConstant; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("add.pri " + amount);
        }
    }

    public class LMulConstant : LInstruction
    {
        private int amount_;

        public LMulConstant(int amount)
        {
            amount_ = amount;
        }

        public int amount
        {
            get { return amount_; }
        }

        public override Opcode op
        {
            get { return Opcode.MulConstant; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("mul.pri " + amount);
        }
    }

    public class LZeroGlobal : LInstruction
    {
        private int address_;

        public LZeroGlobal(int address)
        {
            address_ = address;
        }

        public int address
        {
            get { return address_; }
        }

        public override Opcode op
        {
            get { return Opcode.ZeroGlobal; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("zero " + address);
        }
    }

    public class LZeroLocal : LInstruction
    {
        private int address_;

        public LZeroLocal(int address)
        {
            address_ = address;
        }

        public int address
        {
            get { return address_; }
        }

        public override Opcode op
        {
            get { return Opcode.ZeroLocal; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("zero.s " + address);
        }
    }

    public class LIncGlobal : LInstruction
    {
        private int address_;

        public LIncGlobal(int address)
        {
            address_ = address;
        }

        public int address
        {
            get { return address_; }
        }

        public override Opcode op
        {
            get { return Opcode.IncGlobal; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("inc " + address);
        }
    }

    public class LIncLocal : LInstructionStack
    {
        public LIncLocal(int offset)
            : base(offset)
        {
        }

        public override Opcode op
        {
            get { return Opcode.IncLocal; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("inc.s " + offset);
        }
    }

    public class LDecLocal : LInstructionStack
    {
        public LDecLocal(int offset)
            : base(offset)
        {
        }

        public override Opcode op
        {
            get { return Opcode.DecLocal; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("dec.s " + offset);
        }
    }

    public class LIncI : LInstruction
    {
        public LIncI()
        {
        }

        public override Opcode op
        {
            get { return Opcode.IncI; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("inc.i    ; [pri] = [pri] + 1");
        }
    }

    public class LDecI : LInstruction
    {
        public LDecI()
        {
        }

        public override Opcode op
        {
            get { return Opcode.DecI; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("dec.i    ; [pri] = [pri] + 1");
        }
    }

    public class LFill : LInstruction
    {
        private int amount_;

        public LFill(int amount)
        {
            amount_ = amount;
        }

        public int amount
        {
            get { return amount_; }
        }

        public override Opcode op
        {
            get { return Opcode.Fill; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("fill alt, " + amount_);
        }
    }

    public class LBounds : LInstruction
    {
        private int amount_;

        public LBounds(int amount)
        {
            amount_ = amount;
        }

        public int amount
        {
            get { return amount_; }
        }

        public override Opcode op
        {
            get { return Opcode.Bounds; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("bounds.pri " + amount_);
        }
    }

    public class LSysReq : LInstruction
    {
        private Native native_;

        public LSysReq(Native native)
        {
            native_ = native;
        }

        public override Opcode op
        {
            get { return Opcode.SysReq; }
        }

        public Native native
        {
            get { return native_; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("sysreq " + native.name);
        }
    }

    public class LSwap : LInstructionReg
    {
        public LSwap(Register reg)
            : base(reg)
        {
        }

        public override Opcode op
        {
            get { return Opcode.Swap; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("swap." + RegisterName(reg));
        }
    }

    public class LPushStackAddress : LInstructionStack
    {
        public LPushStackAddress(int offset)
            : base(offset)
        {
        }

        public override Opcode op
        {
            get { return Opcode.PushStackAddress; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("push.adr " + offset);
        }
    }

    public class LDebugBreak : LInstruction
    {
        public LDebugBreak()
        {
        }

        public override Opcode op
        {
            get { return Opcode.DebugBreak; }
        }

        public override void print(TextWriter tw)
        {
            tw.Write("break");
        }
    }

    public class LPushLocal : LInstruction
    {
        private int offset_;

        public LPushLocal(int offset)
        {
            offset_ = offset;
        }

        public int offset
        {
            get { return offset_; }
        }
        public override Opcode op
        {
            get { return Opcode.PushLocal; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("push.s " + offset);
        }
    }

    public class LExchange : LInstruction
    {
        public LExchange()
        {
        }

        public override Opcode op
        {
            get { return Opcode.Exchange; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("xchg");
        }
    }

    public class LUnary : LInstruction
    {
        private SPOpcode spop_;
        private Register reg_;

        public LUnary(SPOpcode op, Register reg)
        {
            spop_ = op;
            reg_ = reg;
        }

        public SPOpcode spop
        {
            get { return spop_; }
        }
        public override Opcode op
        {
            get { return Opcode.Unary; }
        }
        public Register reg
        {
            get { return reg_; }
        }
        public override void print(TextWriter tw)
        {
            switch (spop)
            {
                case SPOpcode.not:
                    tw.Write("not");
                    break;
                case SPOpcode.invert:
                    tw.Write("invert");
                    break;

                default:
                    throw new Exception("unexpected op");
            }
        }
    }

    public class LBinary : LInstruction
    {
        private SPOpcode spop_;
        private Register lhs_;
        private Register rhs_;

        public LBinary(SPOpcode op, Register lhs, Register rhs)
        {
            spop_ = op;
            lhs_ = lhs;
            rhs_ = rhs;
        }

        public SPOpcode spop
        {
            get { return spop_; }
        }
        public override Opcode op
        {
            get { return Opcode.Binary; }
        }
        public Register lhs
        {
            get { return lhs_; }
        }
        public Register rhs
        {
            get { return rhs_; }
        }
        public override void print(TextWriter tw)
        {
            switch (spop)
            {
                case SPOpcode.add:
                    tw.Write("add");
                    break;
                case SPOpcode.sub:
                    tw.Write("sub");
                    break;
                case SPOpcode.eq:
                    tw.Write("eq");
                    break;
                case SPOpcode.neq:
                    tw.Write("neq");
                    break;
                case SPOpcode.sleq:
                    tw.Write("sleq");
                    break;
                case SPOpcode.sgeq:
                    tw.Write("sgeq");
                    break;
                case SPOpcode.sgrtr:
                    tw.Write("sgrtr");
                    break;
                case SPOpcode.and:
                    tw.Write("and");
                    break;
                case SPOpcode.or:
                    tw.Write("or");
                    break;
                case SPOpcode.smul:
                    tw.Write("smul");
                    break;
                case SPOpcode.sdiv_alt:
                    tw.Write("sdiv.alt");
                    break;
                case SPOpcode.shr:
                    tw.Write("shr");
                    break;
                case SPOpcode.shl:
                    tw.Write("shl");
                    break;
                case SPOpcode.sub_alt:
                    tw.Write("sub.alt");
                    break;
                case SPOpcode.sless:
                    tw.Write("sless");
                    break;
                case SPOpcode.xor:
                    tw.Write("xor");
                    break;
                case SPOpcode.sshr:
                    tw.Write("sshr");
                    break;

                default:
                    throw new Exception("unexpected op");
            }
        }
    }

    public class LPushGlobal : LInstruction
    {
        private int address_;

        public LPushGlobal(int address)
        {
            address_ = address;
        }

        public int address
        {
            get { return address_; }
        }
        public override Opcode op
        {
            get { return Opcode.PushGlobal; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("push " + address);
        }
    }

    public class LStoreGlobal : LInstructionReg
    {
        private int address_;

        public LStoreGlobal(int address, Register reg)
            : base(reg)
        {
            address_ = address;
        }

        public int address
        {
            get { return address_; }
        }
        public override Opcode op
        {
            get { return Opcode.StoreGlobal; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("stor." + RegisterName(reg) + " " + address);
        }
    }

    public class LLoadGlobal : LInstructionReg
    {
        private int address_;

        public LLoadGlobal(int address, Register reg)
            : base(reg)
        {
            address_ = address;
        }

        public int address
        {
            get { return address_; }
        }
        public override Opcode op
        {
            get { return Opcode.LoadGlobal; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("load." + RegisterName(reg) + " " + address);
        }
    }

    public class LCall : LInstruction
    {
        private int address_;

        public LCall(int address)
        {
            address_ = address;
        }

        public int address
        {
            get { return address_; }
        }
        public override Opcode op
        {
            get { return Opcode.Call; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("call");
        }
    }

    public class LEqualConstant : LInstructionReg
    {
        private int value_;

        public LEqualConstant(Register reg, int value)
            : base(reg)
        {
            value_ = value;
        }

        public int value
        {
            get { return value_; }
        }
        public override Opcode op
        {
            get { return Opcode.EqualConstant; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("eq.c." + RegisterName(reg) + " " + value);
        }
    }

    public class LLoadIndex : LInstruction
    {
        private int shift_;

        public LLoadIndex(int shift)
        {
            shift_ = shift;
        }

        public int shift
        {
            get { return shift_; }
        }
        public override Opcode op
        {
            get { return Opcode.LoadIndex; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("lidx." + shift + " ; [pri=alt+(pri<<" + shift + ")]");
        }
    }

    public class LStoreGlobalConstant : LInstruction
    {
        private int address_;
        private int value_;

        public LStoreGlobalConstant(int address, int value)
        {
            address_ = address;
            value_ = value;
        }

        public int address
        {
            get { return address_; }
        }
        public int value
        {
            get { return value_; }
        }
        public override Opcode op
        {
            get { return Opcode.StoreGlobalConstant; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("const [" + address + "]" + " = value");
        }
    }

    public class LStoreLocalConstant : LInstruction
    {
        private int address_;
        private int value_;

        public LStoreLocalConstant(int address, int value)
        {
            address_ = address;
            value_ = value;
        }

        public int address
        {
            get { return address_; }
        }
        public int value
        {
            get { return value_; }
        }
        public override Opcode op
        {
            get { return Opcode.StoreLocalConstant; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("const.s [" + address + "]" + " = value");
        }
    }

    public class LHeap : LInstruction
    {
        private int amount_;

        public LHeap(int amount)
        {
            amount_ = amount;
        }

        public int amount
        {
            get { return amount_; }
        }
        public override Opcode op
        {
            get { return Opcode.Heap; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("heap " + amount);
        }
    }

    public class LMemCopy : LInstruction
    {
        private int bytes_;

        public LMemCopy(int bytes)
        {
            bytes_ = bytes;
        }

        public int bytes
        {
            get { return bytes_; }
        }
        public override Opcode op
        {
            get { return Opcode.MemCopy; }
        }
        public override void print(TextWriter tw)
        {
            tw.Write("movs " + bytes);
        }
    }

    public class SwitchCase
    {
        private int value_;
        public LBlock target;

        public SwitchCase(int value, LBlock target)
        {
            value_ = value;
            this.target = target;
        }

        public int value
        {
            get { return value_; }
        }
    }

    public class LSwitch : LControlInstruction
    {
        private LBlock defaultCase_;
        private List<SwitchCase> cases_;

        public LSwitch(LBlock defaultCase, List<SwitchCase> cases)
        {
            defaultCase_ = defaultCase;
            cases_ = cases;
        }
        public LBlock defaultCase
        {
            get { return defaultCase_; }
        }
        public override Opcode op
        {
            get { return Opcode.Switch; }
        }
        public override void replaceSuccessor(int i, LBlock block)
        {
            if (i == 0)
                defaultCase_ = block;
            if (cases_.Count >= i && i > 0)
            {
                cases_[i - 1].target = block;
            }
        }
        public override int numSuccessors
        {
            get { return cases_.Count + 1; }
        }
        public override LBlock getSuccessor(int i)
        {
            if (i == 0)
                return defaultCase_;
            return cases_[i - 1].target;
        }
        public int numCases
        {
            get { return cases_.Count; }
        }
        public SwitchCase getCase(int i)
        {
            return cases_[i];
        }
        public override void print(TextWriter tw)
        {
            string text = defaultCase.id + (numCases > 0 ? "," : "");
            for (int i = 0; i < numCases; i++)
            {
                text += getCase(i).target.id;
                if (i != numCases - 1)
                    text += ",";
            }
            tw.Write("switch.pri -> " + text);
        }
    }
}
