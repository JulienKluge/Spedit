using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using SourcePawn;

namespace Lysis
{
    public class NodeRenamer
    {
        private NodeGraph graph_;

        private void renameBlock(NodeBlock block)
        {
            for (NodeList.iterator iter = block.nodes.begin(); iter.more();)
            {
                DNode node = iter.node;
                switch (node.type)
                {
                    case NodeType.TempName:
                    case NodeType.Jump:
                    case NodeType.JumpCondition:
                    case NodeType.Store:
                    case NodeType.Return:
                    case NodeType.IncDec:
                    case NodeType.DeclareStatic:
                    case NodeType.Switch:
                    {
                        iter.next();
                        continue;
                    }

                    case NodeType.DeclareLocal:
                    {
                        DDeclareLocal decl = (DDeclareLocal)node;
                        if (decl.var == null)
                        {
                            if (decl.uses.Count <= 1)
                            {
                                // This was probably just a stack temporary.
                                if (decl.uses.Count == 1)
                                {
                                    DUse use = decl.uses.First.Value;
                                    use.node.replaceOperand(use.index, decl.value);
                                }
                                block.nodes.remove(iter);
                                continue;
                            }
                            DTempName name = new DTempName(graph_.tempName());
                            node.replaceAllUsesWith(name);
                            name.init(decl.value);
                            block.nodes.replace(iter, name);
                        }
                        iter.next();
                        continue;
                    }

                    case NodeType.SysReq:
                    case NodeType.Call:
                    {
                        // Calls are statements or expressions, so we can't
                        // remove them if they have no uses.
                        if (node.uses.Count <= 1)
                        {
                            if (node.uses.Count == 1)
                                block.nodes.remove(iter);
                            else
                                iter.next();
                            continue;
                        }
                        break;
                    }

                    case NodeType.Constant:
                    {
                        // Constants can be deeply copied.
                        block.nodes.remove(iter);
                        continue;
                    }

                    default:
                    {
                        if (node.uses.Count <= 1)
                        {
                            // This node has one or zero uses, so instead of
                            // renaming it, we remove it from the instruction
                            // stream. This way the source printer will deep-
                            // print it instead of using its 'SSA' name.
                            block.nodes.remove(iter);
                            continue;
                        }

                        break;
                    }
                }

                // If we've reached here, the expression has more than one use
                // and we have to wrap it in some kind of name, lest we
                // duplicate it in the expression tree which may be illegal.
                DTempName replacement = new DTempName(graph_.tempName());
                node.replaceAllUsesWith(replacement);
                replacement.init(node);
                block.nodes.replace(iter, replacement);
                iter.next();
            }
        }

        public NodeRenamer(NodeGraph graph)
        {
            graph_ = graph;
        }

        public void rename()
        {
            for (int i = 0; i < graph_.numBlocks; i++)
                renameBlock(graph_[i]);
        }
    }
}
