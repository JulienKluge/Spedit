using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using SourcePawn;

namespace Lysis
{
    class SourceBuilder
    {
        private PawnFile file_;
        private StringBuilder out_;
        private string indent_;

        public SourceBuilder(PawnFile file, StringBuilder tw)
        {
            file_ = file;
            out_ = tw;
            indent_ = "";
        }

        private void increaseIndent()
        {
            indent_ += "\t";
        }

        private void decreaseIndent()
        {
            if (indent_.Length > 0)
            {
                indent_ = indent_.Remove(indent_.Length - 1);
            }
        }

        private void outputLine(string text)
        {
            out_.AppendLine(indent_ + text);
        }
        private void outputStart(string text)
        {
            out_.Append(indent_ + text);
        }

        static public string spop(SPOpcode op)
        {
            switch (op)
            {
                case SPOpcode.add:
                    return "+";
                case SPOpcode.sub:
                case SPOpcode.sub_alt:
                    return "-";
                case SPOpcode.less:
                case SPOpcode.sless:
                case SPOpcode.jsless:
                    return "<";
                case SPOpcode.grtr:
                case SPOpcode.sgrtr:
                case SPOpcode.jsgrtr:
                    return ">";
                case SPOpcode.leq:
                case SPOpcode.sleq:
                case SPOpcode.jsleq:
                    return "<=";
                case SPOpcode.geq:
                case SPOpcode.sgeq:
                case SPOpcode.jsgeq:
                    return ">=";
                case SPOpcode.eq:
                case SPOpcode.jeq:
                case SPOpcode.jzer:
                    return "==";
                case SPOpcode.jnz:
                case SPOpcode.jneq:
                case SPOpcode.neq:
                    return "!=";
                case SPOpcode.and:
                    return "&";
                case SPOpcode.not:
                    return "!";
                case SPOpcode.or:
                    return "|";
                case SPOpcode.sdiv_alt:
                    return "/";
                case SPOpcode.smul:
                    return "*";
                case SPOpcode.shr:
                    return ">>";
                case SPOpcode.shl:
                    return "<<";
                case SPOpcode.invert:
                    return "~";
                case SPOpcode.xor:
                    return "^";
                case SPOpcode.sshr:
                    return ">>>";

                default:
                    throw new Exception("NYI");
            }
        }

        private string buildTag(PawnType type)
        {
            if (type.type == CellType.Bool)
                return "bool ";
            if (type.type == CellType.Float)
                return "float ";
            if (type.type == CellType.Tag)
                return buildTag(type.tag);
            return "";
        }
        private string buildConTag(PawnType type)
        {
            if (type.type == CellType.Bool)
                return "view_as<bool>";
            if (type.type == CellType.Float)
                return "view_as<float>";
            if (type.type == CellType.Tag)
                return buildConTag(type.tag);
            return "";
        }

        private string buildTag(Tag tag)
        {
            if (tag.name == "_")
            {
                return "int ";
            }
            if (tag.name == "Float")
            {
                return "float ";
            }
            if (tag.name == "String")
            {
                return "char "; //no array def. cause syntax is: char str[]
            }
            return tag.name + " ";
        }
        private string buildConTag(Tag tag)
        {
            if (tag.name == "_")
            {
                return "view_as<int>";
            }
            return "view_as<" + tag.name + ">";
        }

        private void writeSignature(NodeBlock entry)
        {
            Function f = file_.lookupFunction(entry.lir.pc);
            Debug.Assert(f != null);

            if (file_.lookupPublic(entry.lir.pc) != null)
                out_.Append("public ");

            if (f != null)
            {
                out_.Append(buildTag(f.returnType) + f.name);
            }
            else
            {
                out_.Append("function" + f.address);
            }

            out_.Append("(");
            for (int i = 0; i < f.args.Length; i++)
            {
                out_.Append(buildArgDeclaration(f.args[i]));
                if (i != f.args.Length - 1)
                    out_.Append(", ");
            }

            out_.Append(")" + Environment.NewLine);
        }

        private string buildConstant(DConstant node)
        {
            string prefix = "";
            if (node.typeSet.numTypes == 1)
            {
                TypeUnit tu = node.typeSet[0];
                if (tu.kind == TypeUnit.Kind.Cell && tu.type.type == CellType.Tag)
                    prefix = tu.type.tag.name + " ";
            }
            return prefix + node.value.ToString();
        }

        private string buildString(string str)
        {
            str = str.Replace("\r", "\\r");
            str = str.Replace("\n", "\\n");
            str = str.Replace("\"", "\\\"");
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] < 32)
                {
                    str = str.Substring(0, i) + "\\x" + ((int)str[i]).ToString().PadLeft(2, '0') + str.Substring(i + 1, str.Length - (i + 1));
                }
            }
            return "\"" + str + "\"";
        }

        private string buildLocalRef(DLocalRef lref)
        {
            return lref.LocalName;
        }

        private string buildArrayRef(DArrayRef aref)
        {
            string lhs = buildExpression(aref.getOperand(0));
            string rhs = buildExpression(aref.getOperand(1));
            return lhs + "[" + rhs + "]";
        }

        private string buildUnary(DUnary unary)
        {
            string rhs = buildExpression(unary.getOperand(0));
            return spop(unary.spop) + rhs;
        }

        private string buildBinary(DBinary binary)
        {
            string lhs = buildExpression(binary.getOperand(0));
            string rhs = buildExpression(binary.getOperand(1));
            return lhs + " " + spop(binary.spop) + " " + rhs;
        }

        private string buildLoadStoreRef(DNode node)
        {
            switch (node.type)
            {
                case NodeType.TempName:
                    {
                        DTempName temp = (DTempName)node;
                        return temp.name;
                    }

                case NodeType.DeclareLocal:
                    {
                        DDeclareLocal local = (DDeclareLocal)node;
                        return local.var.name;
                    }

                case NodeType.ArrayRef:
                    return buildArrayRef((DArrayRef)node);

                case NodeType.LocalRef:
                    {
                        DLocalRef lref = (DLocalRef)node;
                        DDeclareLocal local = lref.local;
                        if (local.var.type == VariableType.ArrayReference || local.var.type == VariableType.Array)
                            return local.var.name + "[0]";
                        if (local.var.type == VariableType.Reference)
                            return local.var.name;
                        throw new Exception("unknown local ref");
                    }

                case NodeType.Global:
                    {
                        DGlobal global = (DGlobal)node;
                        if (global.var == null)
                            return "__unk";
                        return global.var.name;
                    }

                case NodeType.Load:
                    {
                        DLoad load = (DLoad)node;

                        Debug.Assert(load.from.type == NodeType.DeclareLocal ||
                                        load.from.type == NodeType.ArrayRef ||
                                        load.from.type == NodeType.Global);
                        return buildLoadStoreRef(load.from);
                    }

                default:
                    throw new Exception("unknown load");
            }
        }

        private string buildLoad(DLoad load)
        {
            return buildLoadStoreRef(load.getOperand(0));
        }

        private string buildSysReq(DSysReq sysreq)
        {
            string args = "";
            for (int i = 0; i < sysreq.numOperands; i++)
            {
                DNode input = sysreq.getOperand(i);
                string arg = buildExpression(input);
                args += arg;
                if (i != sysreq.numOperands - 1)
                    args += ", ";
            }

            return sysreq.native.name + "(" + args + ")";
        }

        private string buildCall(DCall call)
        {
            string args = "";
            for (int i = 0; i < call.numOperands; i++)
            {
                DNode input = call.getOperand(i);
                string arg = buildExpression(input);
                args += arg;
                if (i != call.numOperands - 1)
                    args += ", ";
            }

            return call.function.name + "(" + args + ")";
        }

        private string buildInlineArray(DInlineArray ia)
        {
            Debug.Assert(ia.typeSet.numTypes == 1);
            TypeUnit tu = ia.typeSet[0];

            Debug.Assert(tu.kind == TypeUnit.Kind.Array);
            Debug.Assert(tu.dims == 1);

            if (tu.type.isString)
            {
                string s = file_.stringFromData(ia.address);
                return buildString(s);
            }

            string text = "{";
            for (int i = 0; i < ia.size / 4; i++)
            {
                if (tu.type.type == CellType.Float)
                {
                    float f = file_.floatFromData(ia.address + i * 4);
                    text += f;
                }
                else
                {
                    int v = file_.int32FromData(ia.address);
                    text += buildTag(tu.type) + v;
                }
                if (i != (ia.size / 4) - 1)
                    text += ",";
            }
            text += "}";
            return text;
        }

        private string buildBoolean(DBoolean node)
        {
            return node.value ? "true" : "false";
        }

        private string buildFloat(DFloat node)
        {
            return node.value.ToString();
        }

        private string buildCharacter(DCharacter node)
        {
            return "'" + node.value + "'";
        }

        private string buildFunction(DFunction node)
        {
            return node.function.name;
        }

        private string buildExpression(DNode node)
        {
            switch (node.type)
            {
                case NodeType.Constant:
                    return buildConstant((DConstant)node);

                case NodeType.Boolean:
                    return buildBoolean((DBoolean)node);

                case NodeType.Float:
                    return buildFloat((DFloat)node);

                case NodeType.Character:
                    return buildCharacter((DCharacter)node);

                case NodeType.Function:
                    return buildFunction((DFunction)node);

                case NodeType.Load:
                    return buildLoad((DLoad)node);

                case NodeType.String:
                    return buildString(((DString)node).value);

                case NodeType.LocalRef:
                    return buildLocalRef((DLocalRef)node);

                case NodeType.ArrayRef:
                    return buildArrayRef((DArrayRef)node);

                case NodeType.Unary:
                    return buildUnary((DUnary)node);

                case NodeType.Binary:
                    return buildBinary((DBinary)node);

                case NodeType.SysReq:
                    return buildSysReq((DSysReq)node);

                case NodeType.Call:
                    return buildCall((DCall)node);

                case NodeType.DeclareLocal:
                    {
                        DDeclareLocal local = (DDeclareLocal)node;
                        return local.var.name;
                    }

                case NodeType.TempName:
                    {
                        DTempName name = (DTempName)node;
                        return name.name;
                    }

                case NodeType.Global:
                    {
                        DGlobal global = (DGlobal)node;
                        return global.var.name;
                    }

                case NodeType.InlineArray:
                    {
                        return buildInlineArray((DInlineArray)node);
                    }

                default:
                    throw new Exception("waT");
            }
        }

        private string buildArgDeclaration(Argument arg)
        {
            string prefix = arg.type == VariableType.Reference
                            ? "&"
                            : "";
            string decl = prefix + buildTag(arg.tag) + arg.name;
            if (arg.dimensions != null)
            {
                for (int i = 0; i < arg.dimensions.Length; i++)
                {
                    Dimension dim = arg.dimensions[i];
                    decl += "[";
                    if (dim.size >= 1)
                    {
                        if (arg.tag != null && arg.tag.name == "String")
                            decl += dim.size * 4;
                        else
                            decl += dim.size;
                    }
                    decl += "]";
                }
            }
            return decl;
        }

        private string buildVarDeclaration(Variable var)
        {
            string prefix = var.type == VariableType.Reference
                            ? "&"
                            : "";
            string decl = prefix + buildTag(var.tag) + var.name;
            if (var.dims != null)
            {
                for (int i = 0; i < var.dims.Length; i++)
                {
                    Dimension dim = var.dims[i];
                    decl += "[";
                    if (dim.size >= 1)
                    {
                        if (var.tag != null && var.tag.name == "String")
                            decl += dim.size * 4;
                        else
                            decl += dim.size;
                    }
                    decl += "]";
                }
            }
            return decl;
        }

        private void writeLocal(DDeclareLocal local)
        {
            // Don't declare arguments.
            if (local.offset >= 0)
                return;

            string decl = buildVarDeclaration(local.var);

            if (local.value == null)
            {
                //outputLine("decl " + decl + ";");
                outputLine(decl + ";");
                return;
            }

            string expr = buildExpression(local.value);
            //outputLine("new " + decl + " = " + expr + ";");
            outputLine(decl + " = " + expr + ";");
        }

        private void writeStatic(DDeclareStatic decl)
        {
            writeGlobal(decl.var);
        }

        private void writeSysReq(DSysReq sysreq)
        {
            outputLine(buildSysReq(sysreq) + ";");
        }

        private void writeCall(DCall call)
        {
            outputLine(buildCall(call) + ";");
        }

        private void writeStore(DStore store)
        {
            string lhs = buildLoadStoreRef(store.getOperand(0));
            string rhs = buildExpression(store.getOperand(1));
            string eq = store.spop == SPOpcode.nop
                        ? "="
                        : spop(store.spop) + "=";
            outputLine(lhs + " " + eq + " " + rhs + ";");
        }

        private void writeReturn(DReturn ret)
        {
            string operand = buildExpression(ret.getOperand(0));
            outputLine("return " + operand + ";");
        }

        private void writeIncDec(DIncDec incdec)
        {
            string lhs = buildLoadStoreRef(incdec.getOperand(0));
            string rhs = incdec.amount == 1 ? "++" : "--";
            outputLine(lhs + rhs + ";");
        }

        private void writeTempName(DTempName name)
        {
            if (name.getOperand(0) != null)
                outputLine("int " + name.name + " = " + buildExpression(name.getOperand(0)) + ";");
            else
                outputLine("int " + name.name + ";");
        }

        private void writeStatement(DNode node)
        {
            switch (node.type)
            {
                case NodeType.DeclareLocal:
                    writeLocal((DDeclareLocal)node);
                    break;

                case NodeType.DeclareStatic:
                    writeStatic((DDeclareStatic)node);
                    break;

                case NodeType.Jump:
                case NodeType.JumpCondition:
                case NodeType.Return:
                case NodeType.Switch:
                    break;

                case NodeType.SysReq:
                    writeSysReq((DSysReq)node);
                    break;

                case NodeType.Call:
                    {
                        writeCall((DCall)node);
                        break;
                    }

                case NodeType.Store:
                    writeStore((DStore)node);
                    break;

                case NodeType.BoundsCheck:
                    break;

                case NodeType.TempName:
                    writeTempName((DTempName)node);
                    break;

                case NodeType.IncDec:
                    writeIncDec((DIncDec)node);
                    break;

                default:
                    throw new Exception(String.Format("unknown op ({0})", node.type.ToString()));
            }
        }

        private string lgop(LogicOperator lop)
        {
            return lop == LogicOperator.And
                          ? "&&"
                          : "||";
        }

        private string buildLogicExpr(LogicChain.Node node)
        {
            if (node.isSubChain)
            {
                string text = buildLogicChain(node.subChain);
                if (node.subChain.nodes.Count == 1)
                    return text;
                return "(" + text + ")";
            }
            return buildExpression(node.expression);
        }

        private string buildLogicChain(LogicChain chain)
        {
            string text = buildLogicExpr(chain.nodes[0]);
            for (int i = 1; i < chain.nodes.Count; i++)
            {
                LogicChain.Node node = chain.nodes[i];
                text += " " + lgop(chain.op) + " " + buildLogicExpr(node);
            }
            return text;
        }

        private void writeStatements(NodeBlock block)
        {
            for (NodeList.iterator iter = block.nodes.begin(); iter.more(); iter.next())
                writeStatement(iter.node);
        }

        private void writeIf(IfBlock block)
        {
            string cond;
            if (block.logic == null)
            {
                writeStatements(block.source);

                DJumpCondition jcc = (DJumpCondition)block.source.nodes.last;

                if (block.invert)
                {
                    if (jcc.getOperand(0).type == NodeType.Unary &&
                        ((DUnary)jcc.getOperand(0)).spop == SPOpcode.not)
                    {
                        cond = buildExpression(jcc.getOperand(0).getOperand(0));
                    }
                    else if (jcc.getOperand(0).type == NodeType.Load)
                    {
                        cond = "!" + buildExpression(jcc.getOperand(0));
                    }
                    else
                    {
                        cond = "!(" + buildExpression(jcc.getOperand(0)) + ")";
                    }
                }
                else
                {
                    cond = buildExpression(jcc.getOperand(0));
                }
            }
            else
            {
                cond = buildLogicChain(block.logic);
                Debug.Assert(!block.invert);
            }

            outputLine("if (" + cond + ")");
            outputLine("{");
            increaseIndent();
            writeBlock(block.trueArm);
            decreaseIndent();
            if (block.falseArm != null &&
                BlockAnalysis.GetEmptyTarget(block.falseArm.source) == null)
            {
                outputLine("}");
                outputLine("else");
                outputLine("{");
                increaseIndent();
                writeBlock(block.falseArm);
                decreaseIndent();
            }
            outputLine("}");
            if (block.join != null)
                writeBlock(block.join);
        }

        private void writeWhileLoop(WhileLoop loop)
        {
            string cond;
            if (loop.logic == null)
            {
                writeStatements(loop.source);

                DJumpCondition jcc = (DJumpCondition)loop.source.nodes.last;
                cond = buildExpression(jcc.getOperand(0));
            }
            else
            {
                cond = buildLogicChain(loop.logic);
            }

            outputLine("while (" + cond + ")");
            outputLine("{");
            increaseIndent();
            writeBlock(loop.body);
            decreaseIndent();
            outputLine("}");
            if (loop.join != null)
                writeBlock(loop.join);
        }

        private void writeDoWhileLoop(WhileLoop loop)
        {
            outputLine("do" + Environment.NewLine + "{");
            increaseIndent();
            if (loop.body != null)
                writeBlock(loop.body);

            string cond;
            if (loop.logic == null)
            {
                writeStatements(loop.source);
                decreaseIndent();

                DJumpCondition jcc = (DJumpCondition)loop.source.nodes.last;
                cond = buildExpression(jcc.getOperand(0));
            }
            else
            {
                decreaseIndent();
                cond = buildLogicChain(loop.logic);
            }

            outputLine("}");
            outputLine("while (" + cond + ");");
            if (loop.join != null)
                writeBlock(loop.join);
        }

        private void writeSwitch(SwitchBlock switch_)
        {
            writeStatements(switch_.source);

            DSwitch last = (DSwitch)switch_.source.nodes.last;
            string cond = buildExpression(last.getOperand(0));
            outputLine("switch (" + cond + ")");
            outputLine("{");
            increaseIndent();
            for (int i = 0; i < switch_.numCases; i++)
            {
                SwitchBlock.Case cas = switch_.getCase(i);
                outputLine("case " + cas.value + ": {");
                increaseIndent();
                writeBlock(cas.target);
                decreaseIndent();
                outputLine("}");
            }
            outputLine("default: {");
            increaseIndent();
            writeBlock(switch_.defaultCase);
            decreaseIndent();
            outputLine("}");

            decreaseIndent();
            outputLine("}");

            if (switch_.join != null)
                writeBlock(switch_.join);
        }

        private void writeStatementBlock(StatementBlock block)
        {
            writeStatements(block.source);
            if (block.next != null)
                writeBlock(block.next);
        }

        private void writeBlock(ControlBlock block)
        {
            if (block == null)
            {
                return;
            }
            switch (block.type)
            {
                case ControlType.If:
                    writeIf((IfBlock)block);
                    break;
                case ControlType.WhileLoop:
                    writeWhileLoop((WhileLoop)block);
                    break;
                case ControlType.DoWhileLoop:
                    writeDoWhileLoop((WhileLoop)block);
                    break;
                case ControlType.Statement:
                    writeStatementBlock((StatementBlock)block);
                    break;
                case ControlType.Return:
                    writeStatements(block.source);
                    writeReturn((DReturn)block.source.nodes.last);
                    break;
                case ControlType.Switch:
                    writeSwitch((SwitchBlock)block);
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
        }

        private bool isArrayEmpty(int address, int bytes)
        {
            for (int i = address + 0; i < address + bytes; i++)
            {
                if (file_.DAT[i] != 0)
                    return false;
            }
            return true;
        }

        private bool isArrayEmpty(int address, int[] dims, int level)
        {
            if (level == dims.Length - 1)
                return isArrayEmpty(address, dims[level] * 4);

            for (int i = 0; i < dims[level]; i++)
            {
                int abase = address + i * 4;
                int inner = file_.int32FromData(abase);
                int final = abase + inner;
                if (!isArrayEmpty(final, dims, level + 1))
                    return false;
            }

            return true;
        }

        private bool isArrayEmpty(Variable var)
        {
            var dims = new int[var.dims.Length];
            for (int i = 0; i < var.dims.Length; i++)
                dims[i] = var.dims[i].size;
            if (var.tag.name == "String")
                dims[dims.Length - 1] /= 4;
            return isArrayEmpty(var.address, dims, 0);
        }

        private void dumpStringArray(int address, int size)
        {
            for (int i = 0; i < size; i++)
            {
                int abase = address + i * 4;
                int inner = file_.int32FromData(abase);
                int final = abase + inner;
                string str = file_.stringFromData(final);
                string text = buildString(str);
                if (i != size - 1)
                    text += ",";
                outputLine(text);
            }
        }

        private void dumpStringArray(Variable var, int address, int level)
        {
            if (level == var.dims.Length - 2)
            {
                dumpStringArray(address, var.dims[level].size);
                return;
            }

            Debug.Assert(false);

            for (int i = 0; i < var.dims[i].size; i++)
            {
                int abase = address + i * 4;
                int inner = file_.int32FromData(abase);
                int final = abase + inner;
                outputLine("{");
                increaseIndent();
                dumpStringArray(var, final, level + 1);
                decreaseIndent();
                if (i == var.dims[i].size - 1)
                    outputLine("}");
                else
                    outputLine("},");
            }
        }

        private void dumpEntireArray(int address, int size)
        {
            string text = "";
            for (int i = 0; i < size; i++)
            {
                int cell = file_.int32FromData(address + i * 4);
                text += cell;
                if (i != size - 1)
                    text += ", ";
            }
            outputLine(text);
        }

        private void dumpArray(int address, int size)
        {
            int first = file_.int32FromData(address);
            for (int i = 1; i < size; i++)
            {
                int cell = file_.int32FromData(address + i * 4);
                if (first != cell)
                {
                    dumpEntireArray(address, size);
                    return;
                }
            }
            outputLine(first + ", ...");
        }

        private void dumpArray(Variable var, int address, int level)
        {
            if (level == var.dims.Length - 1)
            {
                dumpArray(address, var.dims[level].size);
                return;
            }
            int maxI = var.dims.Length;
            for (int i = 0; i < maxI; i++)
            {
                if (var.dims[i].size >= i)
                {
                    break;
                }
                int abase = address + i * 4;
                int inner = file_.int32FromData(abase);
                int final = abase + inner;
                outputLine("{");
                increaseIndent();
                dumpArray(var, final, level + 1);
                decreaseIndent();
                if (i == var.dims[i].size - 1)
                    outputLine("}");
                else
                    outputLine("},");
            }
        }

        private void writeGlobal(Variable var)
        {
            string decl = var.scope == Scope.Global
                                       ? "" //"new"
                                       : "static";
            if (var.tag.name == "Plugin")
            {
                int nameOffset = file_.int32FromData(var.address + 0);
                int descriptionOffset = file_.int32FromData(var.address + 4);
                int authorOffset = file_.int32FromData(var.address + 8);
                int versionOffset = file_.int32FromData(var.address + 12);
                int urlOffset = file_.int32FromData(var.address + 16);
                string name = file_.stringFromData(nameOffset);
                string description = file_.stringFromData(descriptionOffset);
                string author = file_.stringFromData(authorOffset);
                string version = file_.stringFromData(versionOffset);
                string url = file_.stringFromData(urlOffset);

                outputLine("public Plugin myinfo =");
                outputLine("{");
                increaseIndent();
                outputLine("name = " + buildString(name) + ",");
                outputLine("description = " + buildString(description) + ",");
                outputLine("author = " + buildString(author) + ",");
                outputLine("version = " + buildString(version) + ",");
                outputLine("url = " + buildString(url));
                decreaseIndent();
                outputLine("};");
            }
            else if (var.tag.name == "String")
            {
                if (var.dims.Length == 1)
                {
                    string text = decl + " char " + var.name + "[" + var.dims[0].size + "]";
                    string primer = file_.stringFromData(var.address);
                    if (primer.Length > 0)
                        text += " = " + buildString(primer);
                    outputLine(text + ";");
                }
                else
                {
                    string text = decl + " " + buildTag(var.tag) + var.name;
                    if (var.dims != null)
                    {
                        for (int i = 0; i < var.dims.Length; i++)
                            text += "[" + var.dims[i].size + "]";
                    }
                    if (isArrayEmpty(var))
                    {
                        outputLine(text + ";");
                        return;
                    }
                    outputLine(text + " =");
                    outputLine("{");
                    increaseIndent();
                    dumpStringArray(var, var.address, 0);
                    decreaseIndent();
                    outputLine("}");
                }
            }
            else if (var.dims == null || var.dims.Length == 0)
            {
                string text = decl + " " + buildTag(var.tag) + var.name;
                int value = file_.int32FromData(var.address);
                if (value != 0)
                {
                    text += " = " + value;
                }
                outputLine(text + ";");
            }
            else if (isArrayEmpty(var))
            {
                string text = decl + " " + buildTag(var.tag) + var.name;
                if (var.dims != null)
                {
                    for (int i = 0; i < var.dims.Length; i++)
                        text += "[" + var.dims[i].size + "]";
                }
                outputLine(text + ";");
            }
            else
            {
                string text = decl + " " + buildTag(var.tag) + var.name;
                if (var.dims != null)
                {
                    for (int i = 0; i < var.dims.Length; i++)
                        text += "[" + var.dims[i].size + "]";
                }
                outputLine(text + " =");
                outputLine("{");
                increaseIndent();
                dumpArray(var, var.address, 0);
                decreaseIndent();
                outputLine("}");
            }
        }

        public void writeGlobals()
        {
            for (int i = 0; i < file_.globals.Length; i++)
                writeGlobal(file_.globals[i]);
        }

        public void write(ControlBlock root)
        {
            writeSignature(root.source);
            outputLine("{");
            increaseIndent();
            writeBlock(root);
            decreaseIndent();
            outputLine("}");
        }
    }
}
