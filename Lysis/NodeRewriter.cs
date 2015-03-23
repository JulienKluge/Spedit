using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SourcePawn;

namespace Lysis
{
    public class NodeRewriter : NodeVisitor
    {
        private NodeGraph graph_;
        private NodeBlock current_;
        private NodeList.iterator iterator_;

        public override void visit(DConstant node)
        {
        }
        public override void visit(DDeclareLocal local)
        {
        }
        public override void visit(DLocalRef lref)
        {
        }
        public override void visit(DJump jump)
        {
        }

        public override void visit(DJumpCondition jcc)
        {
        }

        public override void visit(DSysReq sysreq)
        {
        }

        public override void visit(DBinary binary)
        {
        }

        public override void visit(DBoundsCheck check)
        {
        }
        public override void visit(DArrayRef aref)
        {
#if B
            DNode node = aref.getOperand(0);
            DNode replacement = node.applyType(graph_.file, null, VariableType.ArrayReference);
            if (replacement != node)
            {
                node.block.replace(node, replacement);
                aref.replaceOperand(0, replacement);
            }
#endif
        }
        public override void visit(DStore store)
        {
        }
        public override void visit(DLoad load)
        {
        }
        public override void visit(DReturn ret)
        {
        }
        public override void visit(DGlobal global)
        {
        }
        public override void visit(DString node)
        {
        }

        public override void visit(DPhi phi)
        {
            // Convert a phi into a move on each incoming edge. Declare the
            // temporary name in the dominator.
            NodeBlock idom = graph_[phi.block.lir.idom.id];

            DTempName name = new DTempName(graph_.tempName());
            idom.prepend(name);

            for (int i = 0; i < phi.numOperands; i++)
            {
                DNode input = phi.getOperand(i);
                DStore store = new DStore(name, input);
                NodeBlock pred = graph_[phi.block.lir.getPredecessor(i).id];
                pred.prepend(store);
            }

            phi.replaceAllUsesWith(name);
        }

        public override void visit(DCall call)
        {
            // Operators can be overloaded for floats, and we want these to print
            // normally, so here is some gross peephole stuff. Maybe we should be
            // looking for bytecode patterns instead or something, but that would
            // need a whole-program analysis.
            if (call.function.name.Length < 8)
                return;
            if (call.function.name.Substring(0, 8) != "operator")
                return;

            string op = "";
            for (int i = 8; i < call.function.name.Length; i++)
            {
                if (call.function.name[i] == '(')
                    break;
                op += call.function.name[i];
            }

            SPOpcode spop;
            switch (op)
            {
                case ">":
                    spop = SPOpcode.sgrtr;
                    break;
                case ">=":
                    spop = SPOpcode.sgeq;
                    break;
                case "<":
                    spop = SPOpcode.sless;
                    break;
                case "<=":
                    spop = SPOpcode.sleq;
                    break;
                case "*":
                    spop = SPOpcode.smul;
                    break;
                case "/":
                    spop = SPOpcode.sdiv;
                    break;
                case "!=":
                    spop = SPOpcode.neq;
                    break;
                case "+":
                    spop = SPOpcode.add;
                    break;
                case "-":
                    spop = SPOpcode.sub;
                    break;
                default:
                    throw new Exception(String.Format("unknown operator ({0})", op.ToString()));
            }

            switch (spop)
            {
                case SPOpcode.sgeq:
                case SPOpcode.sleq:
                case SPOpcode.sgrtr:
                case SPOpcode.sless:
                case SPOpcode.smul:
                case SPOpcode.sdiv:
                case SPOpcode.neq:
                case SPOpcode.add:
                case SPOpcode.sub:
                {
                    if (call.numOperands != 2)
                        return;
                    DBinary binary = new DBinary(spop, call.getOperand(0), call.getOperand(1));
                    call.replaceAllUsesWith(binary);
                    call.removeFromUseChains();
                    current_.replace(iterator_, binary);
                    break;
                }
                default:
                    throw new Exception("unknown spop");
            }
        }

        private void rewriteBlock(NodeBlock block)
        {
            current_ = block;
            iterator_ = block.nodes.begin();
            while (iterator_.more())
            {
                // Iterate before accepting so we can replace the node.
                iterator_.node.accept(this);
                iterator_.next();
            }
        }

        public NodeRewriter(NodeGraph graph)
        {
            graph_ = graph;
        }

        public void rewrite()
        {
            // We rewrite nodes in forward order so they are collapsed by the time we see their uses.
            for (int i = 0; i < graph_.numBlocks; i++)
                rewriteBlock(graph_[i]);
        }
    }
}
