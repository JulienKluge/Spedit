using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Lysis
{
    // Currently, we expect that Pawn emits reducible control flow graphs.
    // This seems like a reasonable assumption as the language does not have
    // "goto", however I'm not familiar with the compiler enough to make a better assertion.
    public static class BlockAnalysis
    {
        // Return a reverse post-order listing of reachable blocks.
        public static LBlock[] Order(LBlock entry)
        {
            // Postorder traversal without recursion.
            Stack<LBlock> pending = new Stack<LBlock>();
            Stack<int> successors = new Stack<int>();
            Stack<LBlock> done = new Stack<LBlock>();

            LBlock current = entry;
            int nextSuccessor = 0;

            for (; ; )
            {
                if (!current.marked)
                {
                    current.mark();
                    if (nextSuccessor < current.numSuccessors)
                    {
                        pending.Push(current);
                        successors.Push(nextSuccessor);
                        current = current.getSuccessor(nextSuccessor);
                        nextSuccessor = 0;
                        continue;
                    }

                    done.Push(current);
                }

                if (pending.Count == 0)
                    break;

                current = pending.Pop();
                current.unmark();
                nextSuccessor = successors.Pop() + 1;
            }

            List<LBlock> blocks = new List<LBlock>();

            while (done.Count > 0)
            {
                current = done.Pop();
                current.unmark();
                current.setId(blocks.Count);
                blocks.Add(current);
            }

            return blocks.ToArray();
        }

        // Split critical edges in the graph. A critical edge is an edge which
        // is neither its successor's only predecessor, nor its predecessor's
        // only successor. Critical edges must be split to prevent copy-insertion
        // and code motion from affecting other edges. It is probably not strictly
        // needed here.
        public static void SplitCriticalEdges(LBlock[] blocks)
        {
            for (int i = 0; i < blocks.Length; i++)
            {
                LBlock block = blocks[i];
                if (block.numSuccessors < 2)
                    continue;
                for (int j = 0; j < block.numSuccessors; j++)
                {
                    LBlock target = block.getSuccessor(j);
                    if (target.numPredecessors < 2)
                        continue;

                    // Create a new block inheriting from the predecessor.
                    LBlock split = new LBlock(block.pc);
                    LGoto ins = new LGoto(target);
                    LInstruction[] instructions = { ins };
                    split.setInstructions(instructions);
                    block.replaceSuccessor(j, split);
                    target.replacePredecessor(block, split);
                    split.addPredecessor(block);
                }
            }
        }

        private class RBlock
        {
            public List<RBlock> predecessors = new List<RBlock>();
            public List<RBlock> successors = new List<RBlock>();
            public int id;

            public RBlock(int id)
            {
                this.id = id;
            }
        }

        // From Engineering a Compiler (Cooper, Torczon)
        public static bool IsReducible(LBlock[] blocks)
        {
            // Copy the graph into a temporary mutable structure.
            RBlock[] rblocks = new RBlock[blocks.Length];
            for (int i = 0; i < blocks.Length; i++)
                rblocks[i] = new RBlock(i);

            for (int i = 0; i < blocks.Length; i++)
            {
                LBlock block = blocks[i];
                RBlock rblock = rblocks[i];
                for (int j = 0; j < block.numPredecessors; j++)
                    rblock.predecessors.Add(rblocks[block.getPredecessor(j).id]);
                for (int j = 0; j < block.numSuccessors; j++)
                    rblock.successors.Add(rblocks[block.getSuccessor(j).id]);
            }

            // Okay, start reducing.
            LinkedList<RBlock> queue = new LinkedList<RBlock>(rblocks);
            for (;;)
            {
                List<RBlock> deleteQueue = new List<RBlock>();
                foreach (RBlock rblock in queue)
                {
                    // Transformation T1, remove self-edges.
                    if (rblock.predecessors.Contains(rblock))
                        rblock.predecessors.Remove(rblock);
                    if (rblock.successors.Contains(rblock))
                        rblock.successors.Remove(rblock);

                    // Transformation T2, remove blocks with one predecessor,
                    // reroute successors' predecessors.
                    if (rblock.predecessors.Count == 1)
                    {
                        // Mark this node for removal since C# sucks and can't delete during iteration.
                        deleteQueue.Add(rblock);

                        RBlock predecessor = rblock.predecessors[0];

                        // Delete the edge from pred -> me
                        predecessor.successors.Remove(rblock);

                        for (int i = 0; i < rblock.successors.Count; i++)
                        {
                            RBlock successor = rblock.successors[i];
                            //Debug.Assert(successor.predecessors.Contains(rblock));
                            successor.predecessors.Remove(rblock);
                            if (!successor.predecessors.Contains(predecessor))
                                successor.predecessors.Add(predecessor);

                            if (!predecessor.successors.Contains(successor))
                                predecessor.successors.Add(successor);
                        }
                    }
                }

                if (deleteQueue.Count == 0)
                    break;

                foreach (RBlock rblock in deleteQueue)
                    queue.Remove(rblock);
            }

            // If the graph reduced to one node, it was reducible.
            return queue.Count == 1;
        }

        private static bool CompareBitArrays(BitArray b1, BitArray b2)
        {
            //Debug.Assert(b1 != b2 && b1.Count == b2.Count);
            for (int i = 0; i < b1.Length; i++)
            {
                if (b1[i] != b2[i])
                    return false;
            }
            return true;
        }

        public static void ComputeDominators(LBlock[] blocks)
        {
            BitArray[] doms = new BitArray[blocks.Length];
            for (int i = 0; i < doms.Length; i++)
                doms[i] = new BitArray(doms.Length);

            doms[0].Set(0, true);

            for (int i = 1; i < blocks.Length; i++)
            {
                for (int j = 0; j < blocks.Length; j++)
                    doms[i].SetAll(true);
            }

            // Compute dominators.
            bool changed;
            do
            {
                changed = false;
                for (int i = 1; i < blocks.Length; i++)
                {
                    LBlock block = blocks[i];
                    for (int j = 0; j < block.numPredecessors; j++)
                    {
                        LBlock pred = block.getPredecessor(j);
                        BitArray u = new BitArray(doms[i]);
                        doms[block.id].And(doms[pred.id]);
                        doms[block.id].Set(block.id, true);
                        if (!CompareBitArrays(doms[block.id], u))
                            changed = true;
                    }
                }
            }
            while (changed);

            // Turn the bit vectors into lists.
            for (int i = 0; i < blocks.Length; i++)
            {
                LBlock block = blocks[i];
                List<LBlock> list = new List<LBlock>();
                for (int j = 0; j < blocks.Length; j++)
                {
                    if (doms[block.id][j])
                        list.Add(blocks[j]);
                }
                block.setDominators(list.ToArray());
            }
        }

        private static bool StrictlyDominatesADominator(LBlock from, LBlock dom)
        {
            for (int i = 0; i < from.dominators.Length; i++)
            {
                LBlock other = from.dominators[i];
                if (other == from || other == dom)
                    continue;

                if (other.dominators.Contains(dom))
                    return true;
            }
            return false;
        }

        // The immediate dominator or idom of a node n is the unique node that
        // strictly dominates n but does not strictly dominate any other node
        // that strictly dominates n.
        private static void ComputeImmediateDominator(LBlock block)
        {
            for (int i = 0; i < block.dominators.Length; i++)
            {
                LBlock dom = block.dominators[i];
                if (dom == block)
                    continue;

                if (!StrictlyDominatesADominator(block, dom))
                {
                    block.setImmediateDominator(dom);
                    return;
                }
            }
            //Debug.Assert(false, "not reached");
        }

        public static void ComputeImmediateDominators(LBlock[] blocks)
        {
            blocks[0].setImmediateDominator(blocks[0]);

            for (int i = 1; i < blocks.Length; i++)
                ComputeImmediateDominator(blocks[i]);
        }

        public static void ComputeDominatorTree(LBlock[] blocks)
        {
            List<LBlock>[] idominated = new List<LBlock>[blocks.Length];
            for (int i = 0; i < blocks.Length; i++)
                idominated[i] = new List<LBlock>();

            for (int i = 1; i < blocks.Length; i++)
            {
                LBlock block = blocks[i];
                idominated[block.idom.id].Add(block);
            }

            for (int i = 0; i < blocks.Length; i++)
                blocks[i].setImmediateDominated(idominated[i].ToArray());
        }

        private static LBlock SkipContainedLoop(LBlock block, LBlock header)
        {
            while (block.loop != null && block.loop == block)
            {
                if (block.loop != null)
                    block = block.loop;
                if (block == header)
                    break;
                block = block.getLoopPredecessor();
            }
            return block;
        }

        private class LoopBodyWorklist
        {
            private Stack<LBlock> stack_ = new Stack<LBlock>();
            private LBlock backedge_;

            public LoopBodyWorklist(LBlock backedge)
            {
                backedge_ = backedge;
            }

            public void scan(LBlock block)
            {
                for (int i = 0; i < block.numPredecessors; i++)
                {
                    LBlock pred = block.getPredecessor(i);

                    // Has this block already been scanned?
                    if (pred.loop == backedge_.loop)
                        continue;

                    pred = SkipContainedLoop(pred, backedge_.loop);

                    // If this assert hits, there is probably a |break| keyword.
                    //Debug.Assert(pred.loop == null || pred.loop == backedge_.loop);
                    if (pred.loop != null)
                        continue;

                    stack_.Push(pred);
                }
            }
            public bool empty
            {
                get { return stack_.Count() == 0; }
            }
            public LBlock pop()
            {
                return stack_.Pop();
            }
        }

        private static void MarkLoop(LBlock backedge)
        {
            var worklist = new LoopBodyWorklist(backedge);

            worklist.scan(backedge);
            while (!worklist.empty)
            {
                LBlock block = worklist.pop();
                worklist.scan(block);
                block.setInLoop(backedge.loop);
            }
        }

        public static void FindLoops(LBlock[] blocks)
        {
            // Mark backedges and headers.
            for (int i = 1; i < blocks.Length; i++)
            {
                LBlock block = blocks[i];
                for (int j = 0; j < block.numSuccessors; j++)
                {
                    LBlock successor = block.getSuccessor(j);
                    if (successor.id < block.id)
                    {
                        successor.setLoopHeader(block);
                        block.setInLoop(successor);
                        break;
                    }
                }
            }

            for (int i = 0; i < blocks.Length; i++)
            {
                LBlock block = blocks[i];
                if (block.backedge != null)
                    MarkLoop(block.backedge);
            }
        }

        public static NodeBlock GetSingleTarget(NodeBlock block)
        {
            if (block.nodes.last.type != NodeType.Jump)
                return null;
            DJump jump = (DJump)block.nodes.last;
            return jump.target;
        }

        public static NodeBlock GetEmptyTarget(NodeBlock block)
        {
            if (block.nodes.last != block.nodes.first)
                return null;
            return GetSingleTarget(block);
        }

        private static NodeBlock FindJoinBlock(NodeGraph graph, NodeBlock block)
        {
            if (block.nodes.last.type == NodeType.JumpCondition && block.lir.idominated.Length == 3)
                return graph[block.lir.idominated[2].id];
            if (block.lir.idom == null)
                return null;
            if (block.lir.idom == block.lir)
                return null;
            return FindJoinBlock(graph, graph[block.lir.idom.id]);
        }

        private static NodeBlock InLoop(NodeGraph graph, NodeBlock block)
        {
            while (true)
            {
                if (block.lir.backedge != null)
                    return block;
                NodeBlock next = graph[block.lir.idom.id];
                if (block == next)
                    return null;
                block = next;
            }
        }

        public static NodeBlock EffectiveTarget(NodeBlock block)
        {
            NodeBlock target = block;
            for (;;)
            {
                block = GetEmptyTarget(block);
                if (block == null)
                    return target;
                target = block;
            }
        }
    }
}
