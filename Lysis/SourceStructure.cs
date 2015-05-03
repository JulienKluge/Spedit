using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SourcePawn;

namespace Lysis
{
    public enum ControlType
    {
        If,
        Return,
        Statement,
        WhileLoop,
        DoWhileLoop,
        Switch
    };

    public abstract class ControlBlock
    {
        NodeBlock source_;
        public abstract ControlType type { get; }

        public ControlBlock(NodeBlock source)
        {
            source_ = source;
        }

        public NodeBlock source
        {
            get { return source_; }
        }
    }

    public enum LogicOperator
    {
        Or,
        And
    };

    public class LogicChain
    {
        public class Node
        {
            private DNode expression_;
            private LogicChain subChain_;

            public Node(DNode expression)
            {
                expression_ = expression;
            }
            public Node(LogicChain subChain)
            {
                subChain_ = subChain;
            }

            public DNode expression
            {
                get
                {
                    //Debug.Assert(!isSubChain);
                    return expression_;
                }
            }
            public bool isSubChain
            {
                get { return subChain_ != null; }
            }
            public LogicChain subChain
            {
                get { return subChain_; }
            }
        }

        private LogicOperator op_;
        private List<Node> nodes_ = new List<Node>();

        public LogicChain(LogicOperator op)
        {
            op_ = op;
        }

        public void append(DNode expression)
        {
            nodes_.Add(new Node(expression));
        }
        public void append(LogicChain subChain)
        {
            nodes_.Add(new Node(subChain));
        }

        public LogicOperator op
        {
            get { return op_; }
        }
        public List<Node> nodes
        {
            get { return nodes_; }
        }
    };

    public class IfBlock : ControlBlock
    {
        ControlBlock trueArm_;
        ControlBlock falseArm_;
        ControlBlock join_;
        LogicChain logic_;
        bool invert_;

        public IfBlock(NodeBlock source, bool invert, ControlBlock trueArm, ControlBlock join)
          : base(source)
        {
            trueArm_ = trueArm;
            falseArm_ = null;
            join_ = join;
            invert_ = invert;
        }

        public IfBlock(NodeBlock source, bool invert, ControlBlock trueArm, ControlBlock falseArm, ControlBlock join)
          : base(source)
        {
            trueArm_ = trueArm;
            falseArm_ = falseArm;
            join_ = join;
            invert_ = invert;
        }
        public IfBlock(NodeBlock source, LogicChain chain, ControlBlock trueArm, ControlBlock join)
            : base(source)
        {
            trueArm_ = trueArm;
            falseArm_ = null;
            join_ = join;
            invert_ = invert;
            logic_ = logic;
        }

        public IfBlock(NodeBlock source, LogicChain chain, ControlBlock trueArm, ControlBlock falseArm, ControlBlock join)
            : base(source)
        {
            trueArm_ = trueArm;
            falseArm_ = falseArm;
            join_ = join;
            invert_ = invert;
            logic_ = logic;
        }

        public override ControlType type
        {
            get { return ControlType.If; }
        }
        public ControlBlock trueArm
        {
            get { return trueArm_; }
        }
        public ControlBlock falseArm
        {
            get { return falseArm_; }
        }
        public ControlBlock join
        {
            get { return join_; }
        }
        public bool invert
        {
            get { return invert_; }
        }
        public LogicChain logic
        {
            get { return logic_; }
        }
    }

    public class WhileLoop : ControlBlock
    {
        ControlBlock body_;
        ControlBlock join_;
        LogicChain logic_;
        ControlType type_;

        public WhileLoop(ControlType type, NodeBlock source, ControlBlock body, ControlBlock join)
          : base(source)
        {
            body_ = body;
            join_ = join;
            type_ = type;
        }

        public WhileLoop(ControlType type, LogicChain logic, ControlBlock body, ControlBlock join)
            : base(null)
        {
            body_ = body;
            join_ = join;
            logic_ = logic;
            type_ = type;
        }

        public override ControlType type
        {
            get { return type_; }
        }
        public ControlBlock body
        {
            get { return body_; }
        }
        public ControlBlock join
        {
            get { return join_; }
        }
        public LogicChain logic
        {
            get { return logic_; }
        }
    }

    public class SwitchBlock : ControlBlock
    {
        public class Case
        {
            private int value_;
            private ControlBlock target_;

            public Case(int value, ControlBlock target)
            {
                value_ = value;
                target_ = target;
            }

            public int value
            {
                get { return value_; }
            }
            public ControlBlock target
            {
                get { return target_; }
            }
        }

        ControlBlock defaultCase_;
        List<Case> cases_;
        ControlBlock join_;

        public SwitchBlock(NodeBlock source, ControlBlock defaultCase, List<Case> cases, ControlBlock join)
          : base(source)
        {
            defaultCase_ = defaultCase;
            cases_ = cases;
            join_ = join;
        }

        public override ControlType type
        {
            get { return ControlType.Switch; }
        }
        public int numCases
        {
            get { return cases_.Count; }
        }
        public ControlBlock defaultCase
        {
            get { return defaultCase_; }
        }
        public Case getCase(int i)
        {
            return cases_[i];
        }
        public ControlBlock join
        {
            get { return join_; }
        }
    }

    public class ReturnBlock : ControlBlock
    {
        public ReturnBlock(NodeBlock source)
          : base(source)
        {
        }

        public override ControlType type
        {
            get { return ControlType.Return; }
        }
    }

    public class StatementBlock : ControlBlock
    {
        ControlBlock next_;

        public StatementBlock(NodeBlock source, ControlBlock next)
            : base(source)
        {
                        next_ = next;
        }

        public override ControlType type
        {
            get { return ControlType.Statement; }
        }
        public ControlBlock next
        {
            get { return next_; }
        }
    }

    public class SourceStructureBuilder
    {
        private NodeGraph graph_;
        private Stack<NodeBlock> joinStack_ = new Stack<NodeBlock>();

        public SourceStructureBuilder(NodeGraph graph)
        {
            graph_ = graph;
        }

        private void pushScope(NodeBlock block)
        {
            joinStack_.Push(block);
        }
        private NodeBlock popScope()
        {
            return joinStack_.Pop();
        }
        private bool isJoin(NodeBlock block)
        {
            for (int i = joinStack_.Count - 1; i >= 0; i--)
            {
                if (joinStack_.ElementAt(i) == block)
                    return true;
            }
            return false;
        }

        private static bool HasSharedTarget(NodeBlock pred, DJumpCondition jcc)
        {
            NodeBlock trueTarget = BlockAnalysis.EffectiveTarget(jcc.trueTarget);
            if (trueTarget.lir.numPredecessors == 1)
                return false;
            if (pred.lir.idominated.Length > 3)
                return true;

            // Hack... sniff out the case we care about, the true target
            // probably having a conditional.
            if (trueTarget.lir.instructions.Length == 2 &&
                trueTarget.lir.instructions[0].op == Opcode.Constant &&
                trueTarget.lir.instructions[1].op == Opcode.Jump)
            {
                return true;
            }

            // Because of edge splitting, there will always be at least 4
            // dominators for the immediate dominator of a shared block.
            return false;
        }

        private static LogicOperator ToLogicOp(DJumpCondition jcc)
        {
            NodeBlock trueTarget = BlockAnalysis.EffectiveTarget(jcc.trueTarget);
            bool targetIsTruthy = false;
            if (trueTarget.lir.instructions[0] is LConstant)
            {
                LConstant constant = (LConstant)trueTarget.lir.instructions[0];
                targetIsTruthy = (constant.val == 1);
            }

            // jump on true -> 1 == ||
            // jump on false -> 0 == &&
            // other combinations are nonsense, so assert.
            ////Debug.Assert((jcc.spop == SPOpcode.jnz && targetIsTruthy) ||
            //             (jcc.spop == SPOpcode.jzer && !targetIsTruthy));
            LogicOperator logicop = (jcc.spop == SPOpcode.jnz && targetIsTruthy)
                                    ? LogicOperator.Or
                                    : LogicOperator.And;
            return logicop;
        }

        private static NodeBlock SingleTarget(NodeBlock block)
        {
            DJump jump = (DJump)block.nodes.last;
            return jump.target;
        }

        private static void AssertInnerJoinValidity(NodeBlock join, NodeBlock earlyExit)
        {
            DJumpCondition jcc = (DJumpCondition)join.nodes.last;
            //Debug.Assert(BlockAnalysis.EffectiveTarget(jcc.trueTarget) == earlyExit || join == SingleTarget(earlyExit));
        }

        private LogicChain buildLogicChain(NodeBlock block, NodeBlock earlyExitStop, out NodeBlock join)
        {
            DJumpCondition jcc = (DJumpCondition)block.nodes.last;
            LogicChain chain = new LogicChain(ToLogicOp(jcc));

            // Grab the true target, which will be either the "1" or "0"
            // branch of the AND/OR expression.
            NodeBlock earlyExit = BlockAnalysis.EffectiveTarget(jcc.trueTarget);

            NodeBlock exprBlock = block;
            do
            {
                do
                {
                    DJumpCondition childJcc = (DJumpCondition)exprBlock.nodes.last;
                    if (BlockAnalysis.EffectiveTarget(childJcc.trueTarget) != earlyExit)
                    {
                        // Parse a sub-expression.
                        NodeBlock innerJoin;
                        LogicChain rhs = buildLogicChain(exprBlock, earlyExit, out innerJoin);
                        AssertInnerJoinValidity(innerJoin, earlyExit);
                        chain.append(rhs);
                        exprBlock = innerJoin;
                        childJcc = (DJumpCondition)exprBlock.nodes.last;
                    }
                    else
                    {
                        chain.append(childJcc.getOperand(0));
                    }
                    exprBlock = childJcc.falseTarget;
                } while (exprBlock.nodes.last.type == NodeType.JumpCondition);

                do
                {
                    // We have reached the end of a sequence - a block containing
                    // a Constant and a Jump to the join point of the sequence.
                    //Debug.Assert(exprBlock.lir.instructions[0].op == Opcode.Constant);

                    // The next block is the join point.
                    NodeBlock condBlock = SingleTarget(exprBlock);
                    var last = condBlock.nodes.last;
                    DJumpCondition condJcc;
                    try
                    {
                        condJcc = (DJumpCondition)last;
                    }
                    catch (Exception e)
                    {
                        throw new LogicChainConversionException(e.Message);
                    }

                    join = condBlock;

                    // If the cond block is the tagret of the early stop, we've
                    // gone a tad too far. This is the case for a simple
                    // expression like (a && b) || c.
                    if (earlyExitStop != null && SingleTarget(earlyExitStop) == condBlock)
                        return chain;

                    // If the true connects back to the early exit stop, we're
                    // done.
                    if (BlockAnalysis.EffectiveTarget(condJcc.trueTarget) == earlyExitStop)
                        return chain;

                    // If the true target does not have a shared target, we're
                    // done parsing the whole logic chain.
                    if (!HasSharedTarget(condBlock, condJcc))
                        return chain;

                    // Otherwise, there is another link in the chain. This link
                    // joins the existing chain to a new subexpression, which
                    // actually starts hanging off the false branch of this
                    // conditional.
                    earlyExit = BlockAnalysis.EffectiveTarget(condJcc.trueTarget);

                    // Build the right-hand side of the expression.
                    NodeBlock innerJoin;
                    LogicChain rhs = buildLogicChain(condJcc.falseTarget, earlyExit, out innerJoin);
                    AssertInnerJoinValidity(innerJoin, earlyExit);

                    // Build the full expression.
                    LogicChain root = new LogicChain(ToLogicOp(condJcc));
                    root.append(chain);
                    root.append(rhs);
                    chain = root;

                    // If the inner join's false target is a conditional, the
                    // outer expression may continue.
                    DJumpCondition innerJcc = (DJumpCondition)innerJoin.nodes.last;
                    if (innerJcc.falseTarget.nodes.last.type == NodeType.JumpCondition)
                    {
                        exprBlock = innerJcc.falseTarget;
                        break;
                    }

                    // Finally, the new expression block is always the early exit
                    // block. It's on the "trueTarget" edge of the expression,
                    // whereas incoming into this loop it's on the "falseTarget"
                    // edge, but this does not matter.
                    exprBlock = earlyExit;
                } while (true);
            } while (true);
        }

        private NodeBlock findJoinOfSimpleIf(NodeBlock block, DJumpCondition jcc)
        {
            //Debug.Assert(block.nodes.last == jcc);
            //Debug.Assert(block.lir.idominated[0] == jcc.falseTarget.lir || block.lir.idominated[0] == jcc.trueTarget.lir);
            //Debug.Assert(block.lir.idominated[1] == jcc.falseTarget.lir || block.lir.idominated[1] == jcc.trueTarget.lir);
            if (block.lir.idominated.Length == 2)
            {
                if (jcc.trueTarget != BlockAnalysis.EffectiveTarget(jcc.trueTarget))
                    return jcc.trueTarget;
                if (jcc.falseTarget != BlockAnalysis.EffectiveTarget(jcc.falseTarget))
                    return jcc.falseTarget;
                return null;
            }
            return graph_[block.lir.idominated[2].id];
        }

        private IfBlock traverseComplexIf(NodeBlock block, DJumpCondition jcc)
        {
            // Degenerate case: using || or &&, or any combination thereof,
            // will generate a chain of n+1 conditional blocks, where each
            // |n| has a target to a shared "success" block, setting a
            // phony variable. We decompose this giant mess into the intended
            // sequence of expressions.
            NodeBlock join;
            LogicChain chain = buildLogicChain(block, null, out join);

            DJumpCondition finalJcc = (DJumpCondition)join.nodes.last;
            //Debug.Assert(finalJcc.spop == SPOpcode.jzer);
          
            // The final conditional should have the normal dominator
            // properties: 2 or 3 idoms, depending on the number of arms.
            // Because of critical edge splitting, we may have 3 idoms
            // even if there are only actually two arms.
            NodeBlock joinBlock = findJoinOfSimpleIf(join, finalJcc);

            // If an AND chain reaches its end, the result is 1. jzer tests
            // for zero, so this is effectively testing (!success).
            // If an OR expression reaches its end, the result is 0. jzer
            // tests for zero, so this is effectively testing if (failure).
            //
            // In both cases, the true target represents a failure, so flip
            // the targets around.
            NodeBlock trueBlock = finalJcc.falseTarget;
            NodeBlock falseBlock = finalJcc.trueTarget;

            // If there is no join block, both arms terminate control flow,
            // eliminate one arm and use the other as a join point.
            if (joinBlock == null)
                joinBlock = falseBlock;

            if (join.lir.idominated.Length == 2 ||
                BlockAnalysis.EffectiveTarget(falseBlock) == joinBlock)
            {
                if (join.lir.idominated.Length == 3)
                    joinBlock = BlockAnalysis.EffectiveTarget(falseBlock);

                // One-armed structure.
                pushScope(joinBlock);
                ControlBlock trueArm1 = traverseBlock(trueBlock);
                popScope();

                ControlBlock joinArm1 = traverseBlock(joinBlock);
                return new IfBlock(block, chain, trueArm1, joinArm1);
            }

            //Debug.Assert(join.lir.idominated.Length == 3);

            pushScope(joinBlock);
            ControlBlock trueArm2 = traverseBlock(trueBlock);
            ControlBlock falseArm = traverseBlock(falseBlock);
            popScope();

            ControlBlock joinArm2 = traverseBlock(joinBlock);
            return new IfBlock(block, chain, trueArm2, falseArm, joinArm2);
        }

        private IfBlock traverseIf(NodeBlock block, DJumpCondition jcc)
        {
            if (HasSharedTarget(block, jcc))
                return traverseComplexIf(block, jcc);

            NodeBlock trueTarget = (jcc.spop == SPOpcode.jzer) ? jcc.falseTarget : jcc.trueTarget;
            NodeBlock falseTarget = (jcc.spop == SPOpcode.jzer) ? jcc.trueTarget : jcc.falseTarget;
            NodeBlock joinTarget = findJoinOfSimpleIf(block, jcc);

            // If there is no join block (both arms terminate control flow),
            // eliminate one arm and use the other as a join point.
            if (joinTarget == null)
                joinTarget = falseTarget;

            // If the false target is equivalent to the join point, eliminate
            // it.
            if (BlockAnalysis.EffectiveTarget(falseTarget) == joinTarget)
                falseTarget = null;

            // If the true target is equivalent to the join point, promote
            // the false target to the true target and undo the inversion.
            bool invert = false;
            if (BlockAnalysis.EffectiveTarget(trueTarget) == joinTarget)
            {
                trueTarget = falseTarget;
                falseTarget = null;
                invert ^= true;
            }

            // If there is always a true target and a join target.
            pushScope(joinTarget);
            ControlBlock trueArm = traverseBlock(trueTarget);
            popScope();

            ControlBlock joinArm = traverseJoin(joinTarget);
            if (falseTarget == null)
                return new IfBlock(block, invert, trueArm, joinArm);

            pushScope(joinTarget);
            ControlBlock falseArm = traverseBlock(falseTarget);
            popScope();

            return new IfBlock(block, invert, trueArm, falseArm, joinArm);
        }

        private ControlType findLoopJoinAndBody(NodeBlock header, NodeBlock effectiveHeader,
                                                out NodeBlock join, out NodeBlock body, out NodeBlock cond)
        {
            //Debug.Assert(effectiveHeader.lir.numSuccessors == 2);

            LBlock succ1 = effectiveHeader.lir.getSuccessor(0);
            LBlock succ2 = effectiveHeader.lir.getSuccessor(1);

            if (succ1.loop != header.lir || succ2.loop != header.lir)
            {
                //Debug.Assert(succ1.loop == header.lir || succ2.loop == header.lir);
                if (succ1.loop != header.lir)
                {
                    join = graph_[succ1.id];
                    body = graph_[succ2.id];
                }
                else
                {
                    join = graph_[succ2.id];
                    body = graph_[succ1.id];
                }
                cond = header;

                // If this is a self-loop, it is more correct to decompose it
                // to a do-while loop. This may not be source accurate in the
                // case of something like |while (x);| but it catches many more
                // source-accurate cases.
                if (header == effectiveHeader &&
                    BlockAnalysis.GetEmptyTarget(body) == header)
                {
                    body = null;
                    return ControlType.DoWhileLoop;
                }

                return ControlType.WhileLoop;
            }
            else
            {
                // Neither successor of the header exits the loop, so this is
                // probably a do-while loop. For now, assume it's simple.
                //Debug.Assert(header == effectiveHeader);
                LBlock backedge = header.lir.backedge;
                if (BlockAnalysis.GetEmptyTarget(graph_[backedge.id]) == header)
                {
                    // Skip an empty block sitting in between the backedge and
                    // the condition.
                    //Debug.Assert(backedge.numPredecessors == 1);
                    backedge = backedge.getPredecessor(0);
                }

                //Debug.Assert(backedge.numSuccessors == 2);
                succ1 = backedge.getSuccessor(0);
                succ2 = backedge.getSuccessor(1);

                body = header;
                cond = graph_[backedge.id];
                if (succ1.loop != header.lir)
                {
                    join = graph_[succ1.id];
                }
                else
                {
                    //Debug.Assert(succ2.loop != header.lir);
                    join = graph_[succ2.id];
                }
                return ControlType.DoWhileLoop;
            }
        }

        private ControlBlock traverseLoop(NodeBlock block)
        {
            DNode last = block.nodes.last;

            NodeBlock effectiveHeader = block;
            LogicChain chain = null;
            if (last.type == NodeType.JumpCondition)
            {
                DJumpCondition jcc = (DJumpCondition)last;
                if (HasSharedTarget(block, jcc))
                    chain = buildLogicChain(block, null, out effectiveHeader);
            }

            last = effectiveHeader.nodes.last;
            //Debug.Assert(last.type == NodeType.JumpCondition);

            if (last.type == NodeType.JumpCondition)
            {
                // Assert that the backedge is a straight jump.
                //Debug.Assert(BlockAnalysis.GetSingleTarget(graph_[block.lir.backedge.id]) == block);

                NodeBlock join, body, cond;
                ControlType type = findLoopJoinAndBody(block, effectiveHeader, out join, out body, out cond);
                ControlBlock joinArm = traverseBlock(join);

                ControlBlock bodyArm = null;
                if (body != null)
                {
                    pushScope(block);
                    pushScope(cond);
                    bodyArm = traverseBlockNoLoop(body);
                    popScope();
                    popScope();
                }

                if (chain != null)
                    return new WhileLoop(type, chain, bodyArm, joinArm);
                return new WhileLoop(type, cond, bodyArm, joinArm);
            }

            return null;
        }

        private ControlBlock traverseSwitch(NodeBlock block, DSwitch switch_)
        {
            var dominators = new List<LBlock>();
            for (int i = 0; i < block.lir.idominated.Length; i++)
                dominators.Add(block.lir.idominated[i]);

            dominators.Remove(switch_.defaultCase);
            for (int i = 0; i < switch_.numCases; i++)
                dominators.Remove(switch_.getCase(i).target);

            NodeBlock join = null;
            if (dominators.Count > 0)
            {
                //Debug.Assert(dominators.Count == 1);
                join = graph_[dominators[dominators.Count - 1].id];
            }

            ControlBlock joinArm = null;
            if (join != null)
                joinArm = traverseBlock(join);

            pushScope(join);
            var cases = new List<SwitchBlock.Case>();
            ControlBlock defaultArm = traverseBlock(graph_[switch_.defaultCase.id]);
            for (int i = 0; i < switch_.numCases; i++)
            {
                ControlBlock arm = traverseBlock(graph_[switch_.getCase(i).target.id]);
                cases.Add(new SwitchBlock.Case(switch_.getCase(i).value, arm));
            }
            popScope();

            return new SwitchBlock(block, defaultArm, cases, joinArm);
        }

        private ControlBlock traverseJoin(NodeBlock block)
        {
            if (isJoin(block))
                return null;
            return traverseBlock(block);
        }

        private ControlBlock traverseBlockNoLoop(NodeBlock block)
        {
            if (block == null)
            {
                return null;
            }

            DNode last = block.nodes.last;

            if (last.type == NodeType.JumpCondition)
                return traverseIf(block, (DJumpCondition)last);

            if (last.type == NodeType.Jump)
            {
                DJump jump = (DJump)last;
                NodeBlock target = BlockAnalysis.EffectiveTarget(jump.target);

                ControlBlock next = null;
                if (!isJoin(target))
                    next = traverseBlock(target);

                return new StatementBlock(block, next);
            }

            if (last.type == NodeType.Switch)
                return traverseSwitch(block, (DSwitch)last);

            //Debug.Assert(last.type == NodeType.Return);
            return new ReturnBlock(block);
        }

        private ControlBlock traverseBlock(NodeBlock block)
        {
            if (block != null)
            {
                if (block.lir != null)
                {
                    if (block.lir.backedge != null)
                    {
                        return traverseLoop(block);
                    }
                }
            }
            return traverseBlockNoLoop(block);
        }

        public ControlBlock build()
        {
            return traverseBlock(graph_[0]);
        }
    };
}
