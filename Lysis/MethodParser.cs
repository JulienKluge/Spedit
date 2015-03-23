using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SourcePawn;
using System.Diagnostics;

namespace Lysis
{
    public class MethodParser
    {
        private class LIR
        {
            public LBlock entry;
            public List<LInstruction> instructions = new List<LInstruction>();
            public Dictionary<uint, LBlock> targets = new Dictionary<uint, LBlock>();
            public uint entry_pc = 0;
            public uint exit_pc = 0;
            public int argDepth = 0;

            public bool isTarget(uint offs)
            {
                //Debug.Assert(offs >= entry_pc && offs < exit_pc);
                return targets.ContainsKey(offs);
            }

            public LBlock blockOfTarget(uint offs)
            {
                //Debug.Assert(offs >= entry_pc && offs < exit_pc);
                //Debug.Assert(targets[offs] != null);
                return targets[offs];
            }
        }

        private SourcePawnFile file_;
        private uint pc_;
        private uint current_pc_;
        private LIR lir_ = new LIR();

        private int readInt32()
        {
            pc_ += 4;
            return BitConverter.ToInt32(file_.code.bytes, (int)pc_ - 4);
        }

        private uint readUInt32()
        { 
            pc_ += 4;
            return BitConverter.ToUInt32(file_.code.bytes, (int)pc_ - 4);
        }

        private SPOpcode readOp()
        {
            return (SPOpcode)readUInt32();
        }

        private LInstruction add(LInstruction ins)
        {
            ins.setPc(current_pc_);
            lir_.instructions.Add(ins);
            return ins;
        }

        private LBlock prepareJumpTarget(uint offset)
        {
            if (!lir_.targets.ContainsKey(offset))
                lir_.targets[offset] = new LBlock(offset);
            return lir_.targets[offset];
        }

        private int trackStack(int offset)
        {
            if (offset < 0)
                return offset;
            if (offset > lir_.argDepth)
                lir_.argDepth = offset;
            return offset;
        }

        private LInstruction readInstruction(SPOpcode op)
        {
            switch (op)
            {
                case SPOpcode.load_pri:
                case SPOpcode.load_alt:
                {
                    Register reg = (op == SPOpcode.load_pri) ? Register.Pri : Register.Alt;
                    return new LLoadGlobal(readInt32(), reg);
                }

                case SPOpcode.load_s_pri:
                case SPOpcode.load_s_alt:
                {
                    Register reg = (op == SPOpcode.load_s_pri) ? Register.Pri : Register.Alt;
                    return new LLoadLocal(trackStack(readInt32()), reg);
                }

                case SPOpcode.lref_s_pri:
                case SPOpcode.lref_s_alt:
                {
                    Register reg = (op == SPOpcode.lref_s_pri) ? Register.Pri : Register.Alt;
                    return new LLoadLocalRef(trackStack(readInt32()), reg);
                }

                case SPOpcode.stor_s_pri:
                case SPOpcode.stor_s_alt:
                {
                    Register reg = (op == SPOpcode.stor_s_pri) ? Register.Pri : Register.Alt;
                    return new LStoreLocal(reg, trackStack(readInt32()));
                }

                case SPOpcode.sref_s_pri:
                case SPOpcode.sref_s_alt:
                {
                    Register reg = (op == SPOpcode.sref_s_pri) ? Register.Pri : Register.Alt;
                    return new LStoreLocalRef(reg, trackStack(readInt32()));
                }

                case SPOpcode.load_i:
                    return new LLoad(4);

                case SPOpcode.lodb_i:
                    return new LLoad(readInt32());

                case SPOpcode.const_pri:
                case SPOpcode.const_alt:
                {
                    Register reg = (op == SPOpcode.const_pri) ? Register.Pri : Register.Alt;
                    return new LConstant(readInt32(), reg);
                }

                case SPOpcode.addr_pri:
                case SPOpcode.addr_alt:
                {
                    Register reg = (op == SPOpcode.addr_pri) ? Register.Pri : Register.Alt;
                    return new LStackAddress(trackStack(readInt32()), reg);
                }

                case SPOpcode.stor_pri:
                case SPOpcode.stor_alt:
                {
                    Register reg = (op == SPOpcode.stor_pri) ? Register.Pri : Register.Alt;
                    return new LStoreGlobal(readInt32(), reg);
                }

                case SPOpcode.stor_i:
                    return new LStore(4);

                case SPOpcode.strb_i:
                    return new LStore(readInt32());

                case SPOpcode.lidx:
                    return new LLoadIndex(4);

                case SPOpcode.lidx_b:
                    return new LLoadIndex(readInt32());

                case SPOpcode.idxaddr:
                    return new LIndexAddress(2);

                case SPOpcode.idxaddr_b:
                    return new LIndexAddress(readInt32());

                case SPOpcode.move_pri:
                case SPOpcode.move_alt:
                {
                    Register reg = (op == SPOpcode.move_pri) ? Register.Pri : Register.Alt;
                    return new LMove(reg);
                }

                case SPOpcode.xchg:
                    return new LExchange();

                case SPOpcode.push_pri:
                case SPOpcode.push_alt:
                {
                    Register reg = (op == SPOpcode.push_pri) ? Register.Pri : Register.Alt;
                    return new LPushReg(reg);
                }

                case SPOpcode.push_c:
                    return new LPushConstant(readInt32());

                case SPOpcode.push:
                    return new LPushGlobal(readInt32());

                case SPOpcode.push_s:
                    return new LPushLocal(trackStack(readInt32()));

                case SPOpcode.pop_pri:
                case SPOpcode.pop_alt:
                {
                    Register reg = (op == SPOpcode.pop_pri) ? Register.Pri : Register.Alt;
                    return new LPop(reg);
                }

                case SPOpcode.stack:
                    return new LStack(readInt32());

                case SPOpcode.retn:
                    return new LReturn();

                case SPOpcode.call:
                    return new LCall(readInt32());

                case SPOpcode.jump:
                {
                    uint offset = readUInt32();
                    return new LJump(prepareJumpTarget(offset), offset);
                }

                case SPOpcode.jeq:
                case SPOpcode.jneq:
                case SPOpcode.jnz:
                case SPOpcode.jzer:
                case SPOpcode.jsgeq:
                case SPOpcode.jsless:
                case SPOpcode.jsgrtr:
                case SPOpcode.jsleq:
                {
                    uint offset = readUInt32();
                    if (offset == pc_)
                        return new LJump(prepareJumpTarget(offset), offset);
                    return new LJumpCondition(op, prepareJumpTarget(offset), prepareJumpTarget(pc_), offset);
                }

                case SPOpcode.sdiv_alt:
                case SPOpcode.sub_alt:
                    return new LBinary(op, Register.Alt, Register.Pri);

                case SPOpcode.add:
                case SPOpcode.and:
                case SPOpcode.or:
                case SPOpcode.smul:
                case SPOpcode.shr:
                case SPOpcode.shl:
                case SPOpcode.sub:
                case SPOpcode.sshr:
                case SPOpcode.xor:
                    return new LBinary(op, Register.Pri, Register.Alt);

                case SPOpcode.not:
                case SPOpcode.invert:
                    return new LUnary(op, Register.Pri);

                case SPOpcode.add_c:
                    return new LAddConstant(readInt32());

                case SPOpcode.smul_c:
                    return new LMulConstant(readInt32());

                case SPOpcode.zero_pri:
                case SPOpcode.zero_alt:
                {
                    Register reg = (op == SPOpcode.zero_pri) ? Register.Pri : Register.Alt;
                    return new LConstant(0, reg);
                }

                case SPOpcode.zero_s:
                    return new LZeroLocal(trackStack(readInt32()));

                case SPOpcode.zero:
                    return new LZeroGlobal(readInt32());

                case SPOpcode.eq:
                case SPOpcode.neq:
                case SPOpcode.sleq:
                case SPOpcode.sgeq:
                case SPOpcode.sgrtr:
                case SPOpcode.sless:
                    return new LBinary(op, Register.Pri, Register.Alt);

                case SPOpcode.eq_c_pri:
                case SPOpcode.eq_c_alt:
                {
                    Register reg = (op == SPOpcode.eq_c_pri) ? Register.Pri : Register.Alt;
                    return new LEqualConstant(reg, readInt32());
                }

                case SPOpcode.inc:
                    return new LIncGlobal(readInt32());
                    
                case SPOpcode.inc_s:
                    return new LIncLocal(trackStack(readInt32()));

                case SPOpcode.dec_s:
                    return new LDecLocal(trackStack(readInt32()));

                case SPOpcode.inc_i:
                    return new LIncI();

                case SPOpcode.inc_pri:
                case SPOpcode.inc_alt:
                    {
                        Register reg = (op == SPOpcode.inc_pri) ? Register.Pri : Register.Alt;
                        return new LIncReg(reg);
                    }

                case SPOpcode.dec_pri:
                case SPOpcode.dec_alt:
                {
                    Register reg = (op == SPOpcode.dec_pri) ? Register.Pri : Register.Alt;
                    return new LDecReg(reg);
                }

                case SPOpcode.dec_i:
                    return new LDecI();

                case SPOpcode.fill:
                    return new LFill(readInt32());

                case SPOpcode.bounds:
                    return new LBounds(readInt32());

                case SPOpcode.swap_pri:
                case SPOpcode.swap_alt:
                {
                    Register reg = (op == SPOpcode.swap_pri) ? Register.Pri : Register.Alt;
                    return new LSwap(reg);
                }

                case SPOpcode.push_adr:
                    return new LPushStackAddress(trackStack(readInt32()));

                case SPOpcode.sysreq_n:
                {
                    int index = readInt32();
                    add(new LPushConstant(readInt32()));
                    return new LSysReq(file_.natives[index]);
                }
                    
                case SPOpcode.dbreak:
                    return new LDebugBreak();

                case SPOpcode.endproc:
                    return null;

                case SPOpcode.push2_s:
                {
                    add(new LPushLocal(trackStack(readInt32())));
                    return new LPushLocal(trackStack(readInt32()));
                }

                case SPOpcode.push2_adr:
                {
                    add(new LPushStackAddress(trackStack(readInt32())));
                    return new LPushStackAddress(trackStack(readInt32()));
                }

                case SPOpcode.push2_c:
                {
                    add(new LPushConstant(readInt32()));
                    return new LPushConstant(readInt32());
                }

                case SPOpcode.push2:
                {
                    add(new LPushGlobal(readInt32()));
                    return new LPushGlobal(readInt32());
                }

                case SPOpcode.push3_s:
                {
                    add(new LPushLocal(trackStack(readInt32())));
                    add(new LPushLocal(trackStack(readInt32())));
                    return new LPushLocal(trackStack(readInt32()));
                }

                case SPOpcode.push3_adr:
                {
                    add(new LPushStackAddress(trackStack(readInt32())));
                    add(new LPushStackAddress(trackStack(readInt32())));
                    return new LPushStackAddress(trackStack(readInt32()));
                }

                case SPOpcode.push3_c:
                {
                    add(new LPushConstant(readInt32()));
                    add(new LPushConstant(readInt32()));
                    return new LPushConstant(readInt32());
                }

                case SPOpcode.push3:
                {
                    add(new LPushGlobal(readInt32()));
                    add(new LPushGlobal(readInt32()));
                    return new LPushGlobal(readInt32());
                }

                case SPOpcode.push4_s:
                {
                    add(new LPushLocal(trackStack(readInt32())));
                    add(new LPushLocal(trackStack(readInt32())));
                    add(new LPushLocal(trackStack(readInt32())));
                    return new LPushLocal(trackStack(readInt32()));
                }

                case SPOpcode.push4_adr:
                {
                    add(new LPushStackAddress(trackStack(readInt32())));
                    add(new LPushStackAddress(trackStack(readInt32())));
                    add(new LPushStackAddress(trackStack(readInt32())));
                    return new LPushStackAddress(trackStack(readInt32()));
                }

                case SPOpcode.push4_c:
                {
                    add(new LPushConstant(readInt32()));
                    add(new LPushConstant(readInt32()));
                    add(new LPushConstant(readInt32()));
                    return new LPushConstant(readInt32());
                }

                case SPOpcode.push4:
                {
                    add(new LPushGlobal(readInt32()));
                    add(new LPushGlobal(readInt32()));
                    add(new LPushGlobal(readInt32()));
                    return new LPushGlobal(readInt32());
                }

                case SPOpcode.push5_s:
                {
                    add(new LPushLocal(trackStack(readInt32())));
                    add(new LPushLocal(trackStack(readInt32())));
                    add(new LPushLocal(trackStack(readInt32())));
                    add(new LPushLocal(trackStack(readInt32())));
                    return new LPushLocal(trackStack(readInt32()));
                }

                case SPOpcode.push5_c:
                {
                    add(new LPushConstant(readInt32()));
                    add(new LPushConstant(readInt32()));
                    add(new LPushConstant(readInt32()));
                    add(new LPushConstant(readInt32()));
                    return new LPushConstant(readInt32());
                }

                case SPOpcode.push5_adr:
                {
                    add(new LPushStackAddress(trackStack(readInt32())));
                    add(new LPushStackAddress(trackStack(readInt32())));
                    add(new LPushStackAddress(trackStack(readInt32())));
                    add(new LPushStackAddress(trackStack(readInt32())));
                    return new LPushStackAddress(trackStack(readInt32()));
                }

                case SPOpcode.push5:
                {
                    add(new LPushGlobal(readInt32()));
                    add(new LPushGlobal(readInt32()));
                    add(new LPushGlobal(readInt32()));
                    add(new LPushGlobal(readInt32()));
                    return new LPushGlobal(readInt32());
                }

                case SPOpcode.load_both:
                {
                    add(new LLoadLocal(readInt32(), Register.Pri));
                    return new LLoadLocal(readInt32(), Register.Alt);
                }

                case SPOpcode.load_s_both:
                {
                    add(new LLoadLocal(trackStack(readInt32()), Register.Pri));
                    return new LLoadLocal(trackStack(readInt32()), Register.Alt);
                }

                case SPOpcode.const_:
                {
                    return new LStoreGlobalConstant(readInt32(), readInt32());
                }

                case SPOpcode.const_s:
                {
                    return new LStoreLocalConstant(trackStack(readInt32()), readInt32());
                }

                case SPOpcode.heap:
                {
                    return new LHeap(readInt32());
                }

                case SPOpcode.movs:
                {
                    return new LMemCopy(readInt32());
                }

                case SPOpcode.switch_:
                {
                    uint table = readUInt32();
                    uint savePc = pc_;
                    pc_ = table;

                    SPOpcode casetbl = (SPOpcode)readUInt32();
                    //Debug.Assert(casetbl == SPOpcode.casetbl);

                    int ncases = readInt32();
                    uint defaultCase = readUInt32();
                    var cases = new List<SwitchCase>();
                    for (int i = 0; i < ncases; i++)
                    {
                        int value = readInt32();
                        uint pc = readUInt32();
                        LBlock target = prepareJumpTarget(pc);
                        cases.Add(new SwitchCase(value, target));
                    }
                    pc_ = savePc;
                    return new LSwitch(prepareJumpTarget(defaultCase), cases);
                }

                case SPOpcode.casetbl:
                {
                    int ncases = readInt32();
                    pc_ += (uint)ncases * 8 + 4;
                    return new LDebugBreak();
                }

                default:
                {
                    throw new OpCodeNotKnownException("Unrecognized opcode " + op);
                }
            }
        }

        private void readAll()
        {
            lir_.entry_pc = pc_;

            if (readOp() != SPOpcode.proc)
                throw new Exception("invalid method, first op must be PROC");

            while (pc_ < (uint)file_.code.bytes.Length)
            {
                current_pc_ = pc_;
                SPOpcode op = readOp();
                if (op == SPOpcode.proc)
                    break;
                add(readInstruction(op));
            }

            lir_.exit_pc = pc_;
        }

        private class BlockBuilder
        {
            private List<LInstruction> pending_ = new List<LInstruction>();
            private LBlock block_ = null;
            private LIR lir_;

            private void transitionBlocks(LBlock next)
            {
                //Debug.Assert(pending_.Count == 0 || block_ != null);
                if (block_ != null)
                {
                    //Debug.Assert(pending_[pending_.Count - 1].isControl());
                    //Debug.Assert(block_.pc >= lir_.entry_pc && block_.pc < lir_.exit_pc);
                    block_.setInstructions(pending_.ToArray());
                    pending_.Clear();
                }
                block_ = next;
            }

            public BlockBuilder(LIR lir)
            {
                lir_ = lir;
                block_ = lir_.entry;
            }

            public LBlock parse()
            {
                for (int i = 0; i < lir_.instructions.Count; i++)
                {
                    LInstruction ins = lir_.instructions[i];

                    if (lir_.isTarget(ins.pc))
                    {
                        // This instruction is the target of a basic block, so
                        // finish the old one.
                        LBlock next = lir_.blockOfTarget(ins.pc);

                        // Multiple instructions could be at the same pc,
                        // because of decomposition, so make sure we're not
                        // transitioning to the same block.
                        if (block_ != next)
                        {
                            // Add implicit control flow to make further algorithms easier.
                            if (block_ != null)
                            {
                                //Debug.Assert(!pending_[pending_.Count - 1].isControl());
                                pending_.Add(new LGoto(next));
                                next.addPredecessor(block_);
                            }

                            // Fallthrough to the next block.
                            transitionBlocks(next);
                        }
                    }

                    // If there is no block present, we assume this is dead code.
                    if (block_ == null)
                        continue;

                    pending_.Add(ins);

                    switch (ins.op)
                    {
                        case Opcode.Return:
                        {
                            // A return terminates the current block.
                            transitionBlocks(null);
                            break;
                        }

                        case Opcode.Jump:
                        {
                            LJump jump = (LJump)ins;
                            jump.target.addPredecessor(block_);
                            transitionBlocks(null);
                            break;
                        }

                        case Opcode.JumpCondition:
                        {
                            LJumpCondition jcc = (LJumpCondition)ins;
                            jcc.trueTarget.addPredecessor(block_);
                            jcc.falseTarget.addPredecessor(block_);

                            // The next iteration will pick the false target up.
                            //Debug.Assert(lir_.instructions[i + 1].pc == jcc.falseTarget.pc);
                            transitionBlocks(null);
                            break;
                        }

                        case Opcode.Switch:
                        {
                            LSwitch switch_ = (LSwitch)ins;
                            for (int j = 0; j < switch_.numSuccessors; j++)
                                switch_.getSuccessor(j).addPredecessor(block_);
                            transitionBlocks(null);
                            break;
                        }
                    }
                }
                return lir_.entry;
            }
        }

        private LGraph buildBlocks()
        {
            lir_.entry = new LBlock(lir_.entry_pc);
            BlockBuilder builder = new BlockBuilder(lir_);
            LBlock entry = builder.parse();

            // Get an RPO ordering of the blocks, since we don't have predecessors yet.
            LBlock[] blocks = BlockAnalysis.Order(entry);

            if (!BlockAnalysis.IsReducible(blocks))
                throw new Exception("control flow graph is not reducible");

            // Split critical edges in the graph (is this even needed?)
            BlockAnalysis.SplitCriticalEdges(blocks);

            // Reorder the blocks since we could have introduced new nodes.
            blocks = BlockAnalysis.Order(entry);
            //Debug.Assert(BlockAnalysis.IsReducible(blocks));

            BlockAnalysis.ComputeDominators(blocks);
            BlockAnalysis.ComputeImmediateDominators(blocks);
            BlockAnalysis.ComputeDominatorTree(blocks);
            BlockAnalysis.FindLoops(blocks);

            LGraph graph = new LGraph();
            graph.blocks = blocks;
            graph.entry = blocks[0];
            if (lir_.argDepth > 0)
                graph.nargs = ((lir_.argDepth - 12) / 4) + 1;
            else
                graph.nargs = 0;

            return graph;
        }

        public MethodParser(SourcePawnFile file, uint addr)
        {
            file_ = file;
            pc_ = addr;
        }

        public LGraph parse()
        {
            //Debug.Assert(BitConverter.IsLittleEndian);

            readAll();
            return buildBlocks();
        }
    }
}
