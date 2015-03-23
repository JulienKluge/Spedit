using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SourcePawn;

namespace Lysis
{
    public class NodeBuilder
    {
        SourcePawnFile file_;
        private LGraph graph_;
        private NodeBlock[] blocks_;

        public NodeBuilder(SourcePawnFile file, LGraph graph)
        {
            file_ = file;
            graph_ = graph;
            blocks_ = new NodeBlock[graph_.blocks.Length];
            for (int i = 0; i < graph_.blocks.Length; i++)
                blocks_[i] = new NodeBlock(graph_.blocks[i]);
        }

        public void traverse(NodeBlock block)
        {
            for (int i = 0; i < block.lir.numPredecessors; i++)
            {
                NodeBlock pred = blocks_[block.lir.getPredecessor(i).id];
                
                // Don't bother with backedges yet.
                if (pred.lir.id >= block.lir.id)
                    continue;

                block.inherit(graph_, pred);
            }

            foreach (LInstruction uins in block.lir.instructions)
            {
                // Attempt to find static declarations. This is really
                // expensive - we could cheapen it by creating per-method
                // lists of statics.
                {
                    int i = -1;
                    do
                    {
                        Variable var = file_.lookupDeclarations(uins.pc, ref i, Scope.Static);
                        if (var == null)
                            break;
                        block.add(new DDeclareStatic(var));
                    } while (true);
                }

                switch (uins.op)
                {
                    case Opcode.DebugBreak:
                        break;

                    case Opcode.Stack:
                    {
                        LStack ins = (LStack)uins;
                        if (ins.amount < 0)
                        {
                            for (int i = 0; i < -ins.amount / 4; i++)
                            {
                                DDeclareLocal local = new DDeclareLocal(ins.pc, null);
                                block.stack.push(local);
                                block.add(local);
                            }
                        }
                        else
                        {
                            for (int i = 0; i < ins.amount / 4; i++)
                                block.stack.pop();
                        }
                        break;
                    }

                    case Opcode.Fill:
                    {
                        LFill ins = (LFill)uins;
                        DNode node = block.stack.alt;
                        DDeclareLocal local = (DDeclareLocal)node;
                        //Debug.Assert(block.stack.pri.type == NodeType.Constant);
                        for (int i = 0; i < ins.amount; i += 4)
                            block.stack.set(local.offset + i, block.stack.pri);
                        break;
                    }

                    case Opcode.Constant:
                    {
                        LConstant ins = (LConstant)uins;
                        DConstant v = new DConstant(ins.val, ins.pc);
                        block.stack.set(ins.reg, v);
                        block.add(v);
                        break;
                    }

                    case Opcode.PushConstant:
                    {
                        LPushConstant ins = (LPushConstant)uins;
                        DConstant v = new DConstant(ins.val);
                        DDeclareLocal local = new DDeclareLocal(ins.pc, v);
                        block.stack.push(local);
                        block.add(v);
                        block.add(local);
                        break;
                    }

                    case Opcode.PushReg:
                    {
                        LPushReg ins = (LPushReg)uins;
                        DDeclareLocal local = new DDeclareLocal(ins.pc, block.stack.reg(ins.reg));
                        block.stack.push(local);
                        block.add(local);
                        break;
                    }

                    case Opcode.Pop:
                    {
                        LPop ins = (LPop)uins;
                        DNode node = block.stack.popAsTemp();
                        block.stack.set(ins.reg, node);
                        break;
                    }

                    case Opcode.StackAddress:
                    {
                        LStackAddress ins = (LStackAddress)uins;
                        DDeclareLocal local = block.stack.getName(ins.offset);
                        block.stack.set(ins.reg, local);
                        break;
                    }

                    case Opcode.PushStackAddress:
                    {
                        LPushStackAddress ins = (LPushStackAddress)uins;
                        DLocalRef lref = new DLocalRef(block.stack.getName(ins.offset));
                        DDeclareLocal local = new DDeclareLocal(ins.pc, lref);
                        block.stack.push(local);
                        block.add(lref);
                        block.add(local);
                        break;
                    }

                    case Opcode.Goto:
                    {
                        LGoto ins = (LGoto)uins;
                        DJump node = new DJump(blocks_[ins.target.id]);
                        block.add(node);
                        break;
                    }

                    case Opcode.Jump:
                    {
                        LJump ins = (LJump)uins;
                        DJump node = new DJump(blocks_[ins.target.id]);
                        block.add(node);
                        break;
                    }

                    case Opcode.JumpCondition:
                    {
                        LJumpCondition ins = (LJumpCondition)uins;
                        NodeBlock lhtarget = blocks_[ins.trueTarget.id];
                        NodeBlock rhtarget = blocks_[ins.falseTarget.id];
                        DNode cmp = block.stack.pri;
                        SPOpcode jmp = ins.spop;
                        if (jmp != SPOpcode.jzer && jmp != SPOpcode.jnz)
                        {
                            SPOpcode newop;
                            switch (ins.spop)
                            {
                                case SPOpcode.jeq:
                                    newop = SPOpcode.neq;
                                    jmp = SPOpcode.jzer;
                                    break;
                                case SPOpcode.jneq:
                                    newop = SPOpcode.eq;
                                    jmp = SPOpcode.jzer;
                                    break;
                                case SPOpcode.jsgeq:
                                    newop = SPOpcode.sless;
                                    jmp = SPOpcode.jzer;
                                    break;
                                case SPOpcode.jsgrtr:
                                    newop = SPOpcode.sleq;
                                    jmp = SPOpcode.jzer;
                                    break;
                                case SPOpcode.jsleq:
                                    newop = SPOpcode.sgrtr;
                                    jmp = SPOpcode.jzer;
                                    break;
                                case SPOpcode.jsless:
                                    newop = SPOpcode.sgeq;
                                    jmp = SPOpcode.jzer;
                                    break;
                                default:
                                    //Debug.Assert(false);
                                    return;
                            }
                            cmp = new DBinary(newop, block.stack.pri, block.stack.alt);
                            block.add(cmp);
                        }
                        DJumpCondition jcc = new DJumpCondition(jmp, cmp, lhtarget, rhtarget);
                        block.add(jcc);
                        break;
                    }

                    case Opcode.LoadLocal:
                    {
                        LLoadLocal ins = (LLoadLocal)uins;
                        DLoad load = new DLoad(block.stack.getName(ins.offset));
                        block.stack.set(ins.reg, load);
                        block.add(load);
                        break;
                    }

                    case Opcode.LoadLocalRef:
                    {
                        LLoadLocalRef ins = (LLoadLocalRef)uins;
                        DLoad load = new DLoad(block.stack.getName(ins.offset));
                        load = new DLoad(load);
                        block.stack.set(ins.reg, load);
                        block.add(load);
                        break;
                    }

                    case Opcode.StoreLocal:
                    {
                        LStoreLocal ins = (LStoreLocal)uins;
                        DStore store = new DStore(block.stack.getName(ins.offset), block.stack.reg(ins.reg));
                        block.add(store);
                        break;
                    }

                    case Opcode.StoreLocalRef:
                    {
                        LStoreLocalRef ins = (LStoreLocalRef)uins;
                        DLoad load = new DLoad(block.stack.getName(ins.offset));
                        DStore store = new DStore(load, block.stack.reg(ins.reg));
                        block.add(store);
                        break;
                    }

                    case Opcode.SysReq:
                    {
                        LSysReq sysreq = (LSysReq)uins;
                        DConstant ins = (DConstant)block.stack.popValue();
                        List<DNode> arguments = new List<DNode>();
                        for (int i = 0; i < ins.value; i++)
                            arguments.Add(block.stack.popName());
                        DSysReq call = new DSysReq(sysreq.native, arguments.ToArray());
                        block.stack.set(Register.Pri, call);
                        block.add(call);
                        break;
                    }

                    case Opcode.AddConstant:
                    {
                        LAddConstant ins = (LAddConstant)uins;
                        DConstant val = new DConstant(ins.amount);
                        DBinary node = new DBinary(SPOpcode.add, block.stack.pri, val);
                        block.stack.set(Register.Pri, node);
                        block.add(val);
                        block.add(node);
                        break;
                    }

                    case Opcode.MulConstant:
                    {
                        LMulConstant ins = (LMulConstant)uins;
                        DConstant val = new DConstant(ins.amount);
                        DBinary node = new DBinary(SPOpcode.smul, block.stack.pri, val);
                        block.stack.set(Register.Pri, node);
                        block.add(val);
                        block.add(node);
                        break;
                    }

                    case Opcode.Bounds:
                    {
                        LBounds ins = (LBounds)uins;
                        DBoundsCheck node = new DBoundsCheck(block.stack.pri);
                        block.add(node);
                        break;
                    }

                    case Opcode.IndexAddress:
                    {
                        LIndexAddress ins = (LIndexAddress)uins;
                        DArrayRef node = new DArrayRef(block.stack.alt, block.stack.pri, ins.shift);
                        block.stack.set(Register.Pri, node);
                        block.add(node);
                        break;
                    }

                    case Opcode.Move:
                    {
                        LMove ins = (LMove)uins;
                        if (ins.reg == Register.Pri)
                            block.stack.set(Register.Pri, block.stack.alt);
                        else
                            block.stack.set(Register.Alt, block.stack.pri);
                        break;
                    }

                    case Opcode.Store:
                    {
                        LStore ins = (LStore)uins;
                        DStore store = new DStore(block.stack.alt, block.stack.pri);
                        block.add(store);
                        break;
                    }

                    case Opcode.Load:
                    {
                        LLoad ins = (LLoad)uins;
                        DLoad load = new DLoad(block.stack.pri);
                        block.stack.set(Register.Pri, load);
                        block.add(load);
                        break;
                    }

                    case Opcode.Swap:
                    {
                        LSwap ins = (LSwap)uins;
                        DNode lhs = block.stack.popAsTemp();
                        DNode rhs = block.stack.reg(ins.reg);
                        DDeclareLocal local = new DDeclareLocal(ins.pc, rhs);
                        block.stack.set(ins.reg, lhs);
                        block.stack.push(local);
                        block.add(local);
                        break;
                    }

                    case Opcode.IncI:
                    {
                        DIncDec inc = new DIncDec(block.stack.pri, 1);
                        block.add(inc);
                        break;
                    }

                    case Opcode.DecI:
                    {
                        DIncDec dec = new DIncDec(block.stack.pri, -1);
                        block.add(dec);
                        break;
                    }

                    case Opcode.IncLocal:
                    {
                        LIncLocal ins = (LIncLocal)uins;
                        DDeclareLocal local = block.stack.getName(ins.offset);
                        DIncDec inc = new DIncDec(local, 1);
                        block.add(inc);
                        break;
                    }

                    case Opcode.IncReg:
                    {
                        LIncReg ins = (LIncReg)uins;
                        DIncDec dec = new DIncDec(block.stack.reg(ins.reg), 1);
                        block.add(dec);
                        break;
                    }

                    case Opcode.DecLocal:
                    {
                        LDecLocal ins = (LDecLocal)uins;
                        DDeclareLocal local = block.stack.getName(ins.offset);
                        DIncDec dec = new DIncDec(local, -1);
                        block.add(dec);
                        break;
                    }

                    case Opcode.DecReg:
                    {
                        LDecReg ins = (LDecReg)uins;
                        DIncDec dec = new DIncDec(block.stack.reg(ins.reg), -1);
                        block.add(dec);
                        break;
                    }

                    case Opcode.Return:
                    {
                        DReturn node = new DReturn(block.stack.pri);
                        block.add(node);
                        break;
                    }

                    case Opcode.PushLocal:
                    {
                        LPushLocal ins = (LPushLocal)uins;
                        DLoad load = new DLoad(block.stack.getName(ins.offset));
                        DDeclareLocal local = new DDeclareLocal(ins.pc, load);
                        block.stack.push(local);
                        block.add(load);
                        block.add(local);
                        break;
                    }

                    case Opcode.Exchange:
                    {
                        DNode node = block.stack.alt;
                        block.stack.set(Register.Alt, block.stack.pri);
                        block.stack.set(Register.Pri, node);
                        break;
                    }

                    case Opcode.Unary:
                    {
                        LUnary ins = (LUnary)uins;
                        DUnary unary = new DUnary(ins.spop, block.stack.reg(ins.reg));
                        block.stack.set(Register.Pri, unary);
                        block.add(unary);
                        break;
                    }

                    case Opcode.Binary:
                    {
                        LBinary ins = (LBinary)uins;
                        DBinary binary = new DBinary(ins.spop, block.stack.reg(ins.lhs), block.stack.reg(ins.rhs));
                        block.stack.set(Register.Pri, binary);
                        block.add(binary);
                        break;
                    }

                    case Opcode.PushGlobal:
                    {
                        LPushGlobal ins = (LPushGlobal)uins;
                        Variable global = file_.lookupGlobal(ins.address);
                        if (global == null)
                            global = file_.lookupVariable(ins.pc, ins.address, Scope.Static);
                        DGlobal dglobal = new DGlobal(global);
                        DNode node = new DLoad(dglobal);
                        DDeclareLocal local = new DDeclareLocal(ins.pc, node);
                        block.stack.push(local);
                        block.add(dglobal);
                        block.add(node);
                        block.add(local);
                        break;
                    }

                    case Opcode.LoadGlobal:
                    {
                        LLoadGlobal ins = (LLoadGlobal)uins;
                        Variable global = file_.lookupGlobal(ins.address);
                        if (global == null)
                            global = file_.lookupVariable(ins.pc, ins.address, Scope.Static);
                        DNode dglobal = new DGlobal(global);
                        DNode node = new DLoad(dglobal);
                        block.stack.set(ins.reg, node);
                        block.add(dglobal);
                        block.add(node);
                        break;
                    }

                    case Opcode.StoreGlobal:
                    {
                        LStoreGlobal ins = (LStoreGlobal)uins;
                        Variable global = file_.lookupGlobal(ins.address);
                        if (global == null)
                            global = file_.lookupVariable(ins.pc, ins.address, Scope.Static);
                        DGlobal node = new DGlobal(global);
                        DStore store = new DStore(node, block.stack.reg(ins.reg));
                        block.add(node);
                        block.add(store);
                        break;
                    }

                    case Opcode.Call:
                    {
                        LCall ins = (LCall)uins;
                        Function f = file_.lookupFunction((uint)ins.address);
                        DConstant args = (DConstant)block.stack.popValue();
                        List<DNode> arguments = new List<DNode>();
                        for (int i = 0; i < args.value; i++)
                            arguments.Add(block.stack.popName());
                        DCall call = new DCall(f, arguments.ToArray());
                        block.stack.set(Register.Pri, call);
                        block.add(call);
                        break;
                    }

                    case Opcode.EqualConstant:
                    {
                        LEqualConstant ins = (LEqualConstant)uins;
                        DConstant c = new DConstant(ins.value);
                        DBinary node = new DBinary(SPOpcode.eq, block.stack.reg(ins.reg), c);
                        block.stack.set(Register.Pri, node);
                        block.add(c);
                        block.add(node);
                        break;
                    }

                    case Opcode.LoadIndex:
                    {
                        LLoadIndex ins = (LLoadIndex)uins;
                        DArrayRef aref = new DArrayRef(block.stack.alt, block.stack.pri, ins.shift);
                        DLoad load = new DLoad(aref);
                        block.stack.set(Register.Pri, load);
                        block.add(aref);
                        block.add(load);
                        break;
                    }

                    case Opcode.ZeroGlobal:
                    {
                        LZeroGlobal ins = (LZeroGlobal)uins;
                        Variable global = file_.lookupGlobal(ins.address);
                        DNode dglobal = new DGlobal(global);
                        DConstant rhs = new DConstant(0);
                        DStore lhs = new DStore(dglobal, rhs);
                        block.add(dglobal);
                        block.add(rhs);
                        block.add(lhs);
                        break;
                    }

                    case Opcode.IncGlobal:
                    {
                        LIncGlobal ins = (LIncGlobal)uins;
                        Variable global = file_.lookupGlobal(ins.address);
                        DNode dglobal = new DGlobal(global);

                        DLoad load = new DLoad(dglobal);
                        DConstant val = new DConstant(1);
                        DBinary add = new DBinary(SPOpcode.add, load, val);
                        DStore store = new DStore(dglobal, add);
                        block.add(load);
                        block.add(val);
                        block.add(add);
                        block.add(store);
                        break;
                    }

                    case Opcode.StoreGlobalConstant:
                    {
                        LStoreGlobalConstant lstore = (LStoreGlobalConstant)uins;
                        Variable var = file_.lookupGlobal(lstore.address);
                        DConstant val = new DConstant(lstore.value);
                        DGlobal global = new DGlobal(var);
                        DStore store = new DStore(global, val);
                        block.add(val);
                        block.add(global);
                        block.add(store);
                        break;
                    }

                    case Opcode.StoreLocalConstant:
                    {
                        LStoreLocalConstant lstore = (LStoreLocalConstant)uins;
                        DDeclareLocal var = block.stack.getName(lstore.address);
                        DConstant val = new DConstant(lstore.value);
                        DStore store = new DStore(var, val);
                        block.add(val);
                        block.add(store);
                        break;
                    }

                    case Opcode.ZeroLocal:
                    {
                        LZeroLocal lstore = (LZeroLocal)uins;
                        DDeclareLocal var = block.stack.getName(lstore.address);
                        DConstant val = new DConstant(0);
                        DStore store = new DStore(var, val);
                        block.add(val);
                        block.add(store);
                        break;
                    }

                    case Opcode.Heap:
                    {
                        LHeap ins = (LHeap)uins;
                        DHeap heap = new DHeap(ins.amount);
                        block.add(heap);
                        block.stack.set(Register.Alt, heap);
                        break;
                    }

                    case Opcode.MemCopy:
                    {
                        LMemCopy ins = (LMemCopy)uins;
                        DMemCopy copy = new DMemCopy(block.stack.alt, block.stack.pri, ins.bytes);
                        block.add(copy);
                        break;
                    }

                    case Opcode.Switch:
                    {
                        LSwitch ins = (LSwitch)uins;
                        DSwitch switch_ = new DSwitch(block.stack.pri, ins);
                        block.add(switch_);
                        break;
                    }

                    default:
                        throw new Exception("unhandled opcode");
                }
            }

            for (int i = 0; i < block.lir.idominated.Length; i++)
            {
                LBlock lir = block.lir.idominated[i];
                traverse(blocks_[lir.id]);
            }
        }

        public NodeBlock[] buildNodes()
        {
            blocks_[0].inherit(graph_, null);
            traverse(blocks_[0]);
            return blocks_;
        }
    }
}
