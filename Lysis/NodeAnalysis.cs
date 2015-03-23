using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SourcePawn;

namespace Lysis
{
    public static class NodeAnalysis
    {
        public static void RemoveGuards(NodeGraph graph)
        {
            for (int i = graph.numBlocks - 1; i >= 0; i--)
            {
                NodeBlock block = graph[i];
                for (NodeList.reverse_iterator iter = block.nodes.rbegin(); iter.more(); )
                {
                    if (iter.node.guard)
                    {
                        //Debug.Assert(iter.node.idempotent);
                        iter.node.removeFromUseChains();
                        block.nodes.remove(iter);
                        continue;
                    }
                    iter.next();
                }
            }
        }

        private static void RemoveDeadCodeInBlock(NodeBlock block)
        {
            for (NodeList.reverse_iterator iter = block.nodes.rbegin(); iter.more(); )
            {
                if (iter.node.type == NodeType.DeclareLocal)
                {
                    DDeclareLocal decl = (DDeclareLocal)iter.node;
                    if (decl.var == null &&
                        (decl.uses.Count == 0 ||
                         (decl.uses.Count == 1 && decl.value != null)))
                    {
                        // This was probably just a stack temporary.
                        if (decl.uses.Count == 1)
                        {
                            DUse use = decl.uses.First.Value;
                            use.node.replaceOperand(use.index, decl.value);
                        }
                        iter.node.removeFromUseChains();
                        block.nodes.remove(iter);
                        continue;
                    }
                }

                if ((iter.node.type == NodeType.Store &&
                     iter.node.getOperand(0).type == NodeType.Heap &&
                     iter.node.getOperand(0).uses.Count == 1))
                {
                    iter.node.removeFromUseChains();
                    block.nodes.remove(iter);
                }

                if (!iter.node.idempotent || iter.node.guard || iter.node.uses.Count > 0)
                {
                    iter.next();
                    continue;
                }

                iter.node.removeFromUseChains();
                block.nodes.remove(iter);
            }
        }

        // We rely on accurate use counts to rename nodes in a readable way,
        // so we provide a phase for updating use info.
        public static void RemoveDeadCode(NodeGraph graph)
        {
            for (int i = graph.numBlocks - 1; i >= 0; i--)
                RemoveDeadCodeInBlock(graph[i]);
        }

        private static bool IsArray(TypeSet ts)
        {
            if (ts == null)
                return false;
            if (ts.numTypes != 1)
                return false;
            TypeUnit tu = ts[0];
            if (tu.kind == TypeUnit.Kind.Array)
                return true;
            if (tu.kind == TypeUnit.Kind.Reference && tu.inner.kind == TypeUnit.Kind.Array)
                return true;
            return false;
        }

        private static DNode GuessArrayBase(DNode op1, DNode op2)
        {
            if (op1.usedAsArrayIndex)
                return op2;
            if (op2.usedAsArrayIndex)
                return op1;
            if (op1.type == NodeType.ArrayRef ||
                op1.type == NodeType.LocalRef ||
                IsArray(op1.typeSet))
            {
                return op1;
            }
            if (op2.type == NodeType.ArrayRef ||
                op2.type == NodeType.LocalRef ||
                IsArray(op2.typeSet))
            {
                return op2;
            }
            return null;
        }

        private static bool IsReallyLikelyArrayCompute(DNode node, DNode abase)
        {
            if (abase.type == NodeType.ArrayRef)
                return true;
            if (IsArray(abase.typeSet))
                return true;
            foreach (DUse use in node.uses)
            {
                if (use.node.type == NodeType.Store || use.node.type == NodeType.Load)
                    return true;
            }
            return false;
        }

        private static bool IsArrayOpCandidate(DNode node)
        {
            if (node.type == NodeType.Load || node.type == NodeType.Store)
                return true;
            if (node.type == NodeType.Binary)
            {
                DBinary bin = (DBinary)node;
                return bin.spop == SPOpcode.add;
            }
            return false;
        }

        private static bool CollapseArrayReferences(NodeBlock block)
        {
            bool changed = false;

            for (NodeList.reverse_iterator iter = block.nodes.rbegin(); iter.more(); iter.next())
            {
                DNode node = iter.node;

                if (node.type == NodeType.Store || node.type == NodeType.Load)
                {
                    if (node.getOperand(0).type != NodeType.ArrayRef && IsArray(node.getOperand(0).typeSet))
                    {
                        DConstant index0 = new DConstant(0);
                        DArrayRef aref0 = new DArrayRef(node.getOperand(0), index0, 0);
                        block.nodes.insertBefore(node, index0);
                        block.nodes.insertBefore(node, aref0);
                        node.replaceOperand(0, aref0);
                        continue;
                    }
                }

                if (node.type != NodeType.Binary)
                    continue;

                DBinary binary = (DBinary)node;
                if (binary.spop != SPOpcode.add)
                    continue;

                if (binary.lhs.type == NodeType.LocalRef)
                {
                    //Debug.Assert(true);
                }

                // Check for an array index.
                DNode abase = GuessArrayBase(binary.lhs, binary.rhs);
                if (abase == null)
                    continue;
                DNode index = (abase == binary.lhs) ? binary.rhs : binary.lhs;

                if (!IsReallyLikelyArrayCompute(binary, abase))
                    continue;

                // Multi-dimensional arrays are indexed like:
                // x[y] => x + x[y]
                //
                // We recognize this and just remove the add, ignoring the
                // underlying representation of the array.
                if (index.type == NodeType.Load && index.getOperand(0) == abase)
                {
                    node.replaceAllUsesWith(index);
                    node.removeFromUseChains();
                    block.nodes.remove(iter);
                    changed = true;
                    continue;
                }

                // Otherwise, create a new array reference.
                DArrayRef aref = new DArrayRef(abase, index);
                iter.node.replaceAllUsesWith(aref);
                iter.node.removeFromUseChains();
                block.nodes.remove(iter);
                block.nodes.insertBefore(iter.node, aref);
                changed = true;
            }

            return changed;
        }

        // Find adds that should really be array references.
        public static void CollapseArrayReferences(NodeGraph graph)
        {
            bool changed;
            do
            {
                changed = false;
                for (int i = graph.numBlocks - 1; i >= 0; i--)
                    changed |= CollapseArrayReferences(graph[i]);
            } while (changed);
        }

        public static void CoalesceLoadsAndDeclarations(NodeGraph graph)
        {
            for (int i = 0; i < graph.numBlocks; i++)
            {
                NodeBlock block = graph[i];
                for (NodeList.iterator iter = block.nodes.begin(); iter.more(); )
                {
                    DNode node = iter.node;

                    if (node.type == NodeType.DeclareLocal)
                    {
                        // Peephole next = store(this, expr)
                        DDeclareLocal local = (DDeclareLocal)node;
                        if (node.next.type == NodeType.Store)
                        {
                            DStore store = (DStore)node.next;
                            if (store.getOperand(0) == local)
                            {
                                local.replaceOperand(0, store.getOperand(1));
                                store.removeFromUseChains();
                                iter.next();
                                block.nodes.remove(iter);
                                continue;
                            }
                        }
                    }

                    iter.next();
                }
            }
        }

        private static void CoalesceLoadStores(NodeBlock block)
        {
            for (NodeList.reverse_iterator riter = block.nodes.rbegin(); riter.more(); riter.next())
            {
                if (riter.node.type != NodeType.Store)
                    continue;

                DStore store = (DStore)riter.node;

                DNode coalesce = null;
                if (store.rhs.type == NodeType.Binary)
                {
                    DBinary rhs = (DBinary)store.rhs;
                    if (rhs.lhs.type == NodeType.Load)
                    {
                        DLoad load = (DLoad)rhs.lhs;
                        if (load.from == store.lhs)
                        {
                            coalesce = rhs.rhs;
                        }
                        else if (load.from.type == NodeType.ArrayRef &&
                                 store.lhs.type == NodeType.Load)
                        {
                            DArrayRef aref = (DArrayRef)load.from;
                            load = (DLoad)store.lhs;
                            if (aref.abase == load &&
                                aref.index.type == NodeType.Constant &&
                                ((DConstant)aref.index).value == 0)
                            {
                                coalesce = rhs.rhs;
                                store.replaceOperand(0, aref);
                            }
                        }
                    }
                    if (coalesce != null)
                        store.makeStoreOp(rhs.spop);
                }
                else if (store.rhs.type == NodeType.Load &&
                         store.rhs.getOperand(0) == store.lhs)
                {
                    // AWFUL PATTERN MATCHING AHEAD.
                    // This *looks* like a dead store, but there is probably
                    // something in between the load and store that changes
                    // the reference. We assume this has to be an incdec.
                    if (store.prev.type == NodeType.IncDec &&
                        store.prev.getOperand(0) == store.rhs)
                    {
                        // This detects a weird case in ucp.smx:
                        // v0 = ArrayRef
                        // v1 = Load(v0)
                        // --   Dec(v1)
                        // --   Store(v0, v1)
                        // This appears to be:
                        //   *ref = (--*ref)
                        // But, this should suffice:
                        //   --*ref
                        store.removeFromUseChains();
                        block.nodes.remove(riter);
                        //Debug.Assert(riter.node.type == NodeType.IncDec);
                        riter.node.replaceOperand(0, riter.node.getOperand(0).getOperand(0));
                    }
                }

                if (coalesce != null)
                    store.replaceOperand(1, coalesce);
            }
        }

        public static void CoalesceLoadStores(NodeGraph graph)
        {
            for (int i = 0; i < graph.numBlocks; i++)
                CoalesceLoadStores(graph[i]);
        }

        private static Signature SignatureOf(DNode node)
        {
            if (node.type == NodeType.Call)
                return ((DCall)node).function;
            return ((DSysReq)node).native;
        }

        private static bool AnalyzeHeapNode(NodeBlock block, DHeap node)
        {
            // Easy case: compiler needed a lvalue.
            if (node.uses.Count == 2)
            {
                DUse lastUse = node.uses.Last();
                DUse firstUse = node.uses.First();
                if ((lastUse.node.type == NodeType.Call ||
                     lastUse.node.type == NodeType.SysReq) &&
                    firstUse.node.type == NodeType.Store &&
                    firstUse.index == 0)
                {
                    lastUse.node.replaceOperand(lastUse.index, firstUse.node.getOperand(1));
                    return true;
                }

                if ((lastUse.node.type == NodeType.Call ||
                     lastUse.node.type == NodeType.SysReq) &&
                    firstUse.node.type == NodeType.MemCopy &&
                    firstUse.index == 0)
                {
                    // heap -> memcpy always reads from DAT + constant
                    DMemCopy memcopy = (DMemCopy)firstUse.node;
                    DConstant cv = (DConstant)memcopy.from;
                    DInlineArray ia = new DInlineArray(cv.value, memcopy.bytes);
                    block.nodes.insertAfter(node, ia);
                    lastUse.node.replaceOperand(lastUse.index, ia);

                    // Give the inline array some type information.
                    Signature signature = SignatureOf(lastUse.node);
                    TypeUnit tu = TypeUnit.FromArgument(signature.args[lastUse.index]);
                    ia.addType(tu);
                    return true;
                }
            }

            return false;
        }

        private static void AnalyzeHeapUsage(NodeBlock block)
        {
            for (NodeList.reverse_iterator riter = block.nodes.rbegin(); riter.more(); riter.next())
            {
                if (riter.node.type == NodeType.Heap)
                {
                    if (AnalyzeHeapNode(block, (DHeap)riter.node))
                        block.nodes.remove(riter);
                }
            }
        }

        public static void AnalyzeHeapUsage(NodeGraph graph)
        {
            for (int i = 0; i < graph.numBlocks; i++)
                AnalyzeHeapUsage(graph[i]);
        }
    }
}
