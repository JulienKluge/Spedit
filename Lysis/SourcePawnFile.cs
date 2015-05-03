using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using Lysis;

namespace SourcePawn
{
    public enum SPOpcode : uint
    {
        invalid,
        load_pri,
        load_alt,
        load_s_pri,
        load_s_alt,
        lref_pri,
        lref_alt,
        lref_s_pri,
        lref_s_alt,
        load_i,
        lodb_i,
        const_pri,
        const_alt,
        addr_pri,
        addr_alt,
        stor_pri,
        stor_alt,
        stor_s_pri,
        stor_s_alt,
        sref_pri,
        sref_alt,
        sref_s_pri,
        sref_s_alt,
        stor_i,
        strb_i,
        lidx,
        lidx_b,
        idxaddr,
        idxaddr_b,
        align_pri,
        align_alt,
        lctrl,
        sctrl,
        move_pri,
        move_alt,
        xchg,
        push_pri,
        push_alt,
        push_r,
        push_c,
        push,
        push_s,
        pop_pri,
        pop_alt,
        stack,
        heap,
        proc,
        ret,
        retn,
        call,
        call_pri,
        jump,
        jrel,
        jzer,
        jnz,
        jeq,
        jneq,
        jless,
        jleq,
        jgrtr,
        jgeq,
        jsless,
        jsleq,
        jsgrtr,
        jsgeq,
        shl,
        shr,
        sshr,
        shl_c_pri,
        shl_c_alt,
        shr_c_pri,
        shr_c_alt,
        smul,
        sdiv,
        sdiv_alt,
        umul,
        udiv,
        udiv_alt,
        add,
        sub,
        sub_alt,
        and,
        or,
        xor,
        not,
        neg,
        invert,
        add_c,
        smul_c,
        zero_pri,
        zero_alt,
        zero,
        zero_s,
        sign_pri,
        sign_alt,
        eq,
        neq,
        less,
        leq,
        grtr,
        geq,
        sless,
        sleq,
        sgrtr,
        sgeq,
        eq_c_pri,
        eq_c_alt,
        inc_pri,
        inc_alt,
        inc,
        inc_s,
        inc_i,
        dec_pri,
        dec_alt,
        dec,
        dec_s,
        dec_i,
        movs,
        cmps,
        fill,
        halt,
        bounds,
        sysreq_pri,
        sysreq_c,
        file,
        line,
        symbol,
        srange,
        jump_pri,
        switch_,
        casetbl,
        swap_pri,
        swap_alt,
        push_adr,
        nop,
        sysreq_n,
        symtag,
        dbreak,
        push2_c,
        push2,
        push2_s,
        push2_adr,
        push3_c,
        push3,
        push3_s,
        push3_adr,
        push4_c,
        push4,
        push4_s,
        push4_adr,
        push5_c,
        push5,
        push5_s,
        push5_adr,
        load_both,
        load_s_both,
        const_,
        const_s,
        sysreq_d,
        sysreq_nd,
        tracker_push_c,
        tracker_pop_setheap,
        genarray,
        genarray_z,
        stradjust_pri,
        stackadjust,
        endproc,
        fabs,
        float_,
        floatadd,
        floatsub,
        floatmul,
        floatdiv,
        rnd_to_nearest,
        rnd_to_floor,
        rnd_to_ceil,
        rnd_to_zero,
        floatcmp
    }

    public class SourcePawnFile : PawnFile
    {
        public const uint MAGIC = 0x53504646;
        private const byte IDENT_VARIABLE = 1;
        private const byte IDENT_REFERENCE = 2;
        private const byte IDENT_ARRAY = 3;
        private const byte IDENT_REFARRAY = 4;
        private const byte IDENT_FUNCTION = 9;
        private const byte IDENT_VARARGS = 11;

        private enum Compression : uint
        {
            None,
            Gzip
        }

        private struct Header
        {
            public uint magic;
            public uint version;
            public Compression compression;
            public int disksize;
            public int imagesize;
            public int sections;
            public int stringtab;
            public int dataoffs;
        }

        public struct DebugHeader
        {
            public int numFiles;
            public int numLines;
            public int numSyms;
        }

        private struct Section
        {
            public int dataoffs;
            public int size;

            public Section(int dataoffs, int size)
            {
                this.dataoffs = dataoffs;
                this.size = size;
            }
        }

        private static VariableType FromIdent(byte ident)
        {
            switch (ident)
            {
                case IDENT_VARIABLE:
                    return VariableType.Normal;
                case IDENT_REFERENCE:
                    return VariableType.Reference;
                case IDENT_ARRAY:
                    return VariableType.Array;
                case IDENT_REFARRAY:
                    return VariableType.ArrayReference;
                case IDENT_VARARGS:
                    return VariableType.Variadic;
                default:
                    return VariableType.Normal;
            }
        }

        private static byte[] Slice(byte[] bytes, int offset, int length)
        {
            byte[] shadow = new byte[length];
            for (int i = 0; i < length; i++)
                shadow[i] = bytes[offset + i];
            return shadow;
        }

        private static string ReadString(byte[] bytes, int offset, int dataoffs)
        {
            int count = offset;
            for (; count < bytes.Length; count++)
            {
                if (bytes[count] == 0)
                {
                    break;
                }
                if ((dataoffs - 1) == count)
                {
                    break;
                }
            }
            return System.Text.Encoding.UTF8.GetString(bytes, offset, count - offset);
        }

        public class Code
        {
            private byte[] code_;
            private int flags_;
            private int version_;

            public Code(byte[] code, int flags, int version)
            {
                code_ = code;
                flags_ = flags;
                version_ = version;
            }

            public byte[] bytes
            {
                get { return code_; }
            }
            public int flags
            {
                get { return flags_; }
            }
            public int version
            {
                get { return version_; }
            }
        }

        public class Data
        {
            private byte[] data_;
            private int memory_;

            public Data(byte[] data, int memory)
            {
                data_ = data;
                memory_ = memory;
            }

            public byte[] bytes
            {
                get { return data_; }
            }
            public int memory
            {
                get { return memory_; }
            }
        }

        public class PubVar
        {
            private uint address_;
            private string name_;

            public PubVar(string name, uint address)
            {
                name_ = name;
                address_ = address;
            }

            public string name
            {
                get { return name_; }
            }
            public uint address
            {
                get { return address_; }
            }
        }

        public class DebugFile
        {
            private uint address_;
            private string name_;

            public DebugFile(string name, uint address)
            {
                name_ = name;
                address_ = address;
            }

            public string name
            {
                get { return name_; }
            }
            public uint address
            {
                get { return address_; }
            }
        }

        public class DebugLine
        {
            private uint address_;
            private int line_;

            public DebugLine(int line, uint address)
            {
                line_ = line;
                address_ = address;
            }

            public int line
            {
                get { return line_; }
            }
            public uint address
            {
                get { return address_; }
            }
        }

        /// <summary>
        /// File proper
        /// </summary>

        private Header header_;
        private bool debugUnpacked_;
        private Dictionary<string, Section> sections_;
        private Code code_;
        private Data data_;
        private PubVar[] pubvars_;
        private Native[] natives_;
        private DebugFile[] debugFiles_;
        private DebugLine[] debugLines_;
        private DebugHeader debugHeader_;
        private Tag[] tags_;
        private Variable[] variables_;

        public SourcePawnFile(byte[] binary)
        {
            BinaryReader reader = new BinaryReader(new MemoryStream(binary));
            header_.magic = reader.ReadUInt32();
            if (header_.magic != MAGIC)
                throw new Exception("bad magic - not SourcePawn file");
            header_.version = reader.ReadUInt16();
            header_.compression = (Compression)reader.ReadByte();
            header_.disksize = (int)reader.ReadUInt32();
            header_.imagesize = (int)reader.ReadUInt32();
            header_.sections = (int)reader.ReadByte();
            header_.stringtab = (int)reader.ReadUInt32();
            header_.dataoffs = (int)reader.ReadUInt32();

            sections_ = new Dictionary<string, Section>();

            // There was a brief period of incompatibility, where version == 0x0101
            // and the packing changed, at the same time .dbg.ntvarg was introduced.
            // Once the incompatibility was noted, version was bumped to 0x0102.
            debugUnpacked_ = (header_.version == 0x0101) && !sections_.ContainsKey(".dbg.natives");

            switch (header_.compression)
            {
                case Compression.Gzip:
                {
                    byte[] bits = new byte[header_.imagesize];
                    for (int i = 0; i < header_.dataoffs; i++)
                        bits[i] = binary[i];

                    int uncompressedSize = header_.imagesize - header_.dataoffs;
                    int compressedSize = header_.disksize - header_.dataoffs;
                    MemoryStream ms = new MemoryStream(binary, header_.dataoffs + 2, compressedSize - 2);
                    DeflateStream gzip = new DeflateStream(ms, CompressionMode.Decompress);

					int actualSize = gzip.Read(bits, header_.dataoffs, uncompressedSize);
					//Debug.Assert(actualSize == uncompressedSize, "uncompressed size mismatch, bad file?");

                    binary = bits;
                    break;
                }
            }

            // Read sections.
            for (int i = 0; i < header_.sections; i++)
            {
                int nameOffset = (int)reader.ReadUInt32();
                int dataoffs = (int)reader.ReadUInt32();
                int size = (int)reader.ReadUInt32();
                string name = ReadString(binary, header_.stringtab + nameOffset, header_.dataoffs);
                sections_[name] = new Section(dataoffs, size);
            }

            if (sections_.ContainsKey(".code"))
            {
                Section sc = sections_[".code"];
                BinaryReader br = new BinaryReader(new MemoryStream(binary, sc.dataoffs, sc.size));
                uint codesize = br.ReadUInt32();
                byte cellsize = br.ReadByte();
                byte codeversion = br.ReadByte();
                ushort flags = br.ReadUInt16();
                uint main = br.ReadUInt32();
                uint codeoffs = br.ReadUInt32();
                byte[] codeBytes = Slice(binary, sc.dataoffs + (int)codeoffs, (int)codesize);
                code_ = new Code(codeBytes, (int)flags, (int)codeversion);
            }

            if (sections_.ContainsKey(".data"))
            {
                Section sc = sections_[".data"];
                BinaryReader br = new BinaryReader(new MemoryStream(binary, sc.dataoffs, sc.size));
                uint datasize = br.ReadUInt32();
                uint memsize = br.ReadUInt32();
                uint dataoffs = br.ReadUInt32();
                byte[] dataBytes = Slice(binary, sc.dataoffs + (int)dataoffs, (int)datasize);
                data_ = new Data(dataBytes, (int)memsize);
            }

            if (sections_.ContainsKey(".publics"))
            {
                Section sc = sections_[".publics"];
                BinaryReader br = new BinaryReader(new MemoryStream(binary, sc.dataoffs, sc.size));
                int numPublics = sc.size / 8;
                publics_ = new Public[numPublics];
                for (int i = 0; i < numPublics; i++)
                {
                    uint address = br.ReadUInt32();
                    uint nameOffset = br.ReadUInt32();
                    string name = ReadString(binary, sections_[".names"].dataoffs + (int)nameOffset, header_.dataoffs);
                    publics_[i] = new Public(name, address);
                }
            }

            if (sections_.ContainsKey(".pubvars"))
            {
                Section sc = sections_[".pubvars"];
                BinaryReader br = new BinaryReader(new MemoryStream(binary, sc.dataoffs, sc.size));
                int numPubVars = sc.size / 8;
                pubvars_ = new PubVar[numPubVars];
                for (int i = 0; i < numPubVars; i++)
                {
                    uint address = br.ReadUInt32();
                    uint nameOffset = br.ReadUInt32();
                    string name = ReadString(binary, sections_[".names"].dataoffs + (int)nameOffset, header_.dataoffs);
                    pubvars_[i] = new PubVar(name, address);
                }
            }

            if (sections_.ContainsKey(".natives"))
            {
                Section sc = sections_[".natives"];
                BinaryReader br = new BinaryReader(new MemoryStream(binary, sc.dataoffs, sc.size));
                int numNatives = sc.size / 4;
                natives_ = new Native[numNatives];
                for (int i = 0; i < numNatives; i++)
                {
                    uint nameOffset = br.ReadUInt32();
                    string name = ReadString(binary, sections_[".names"].dataoffs + (int)nameOffset, header_.dataoffs);
                    natives_[i] = new Native(name, i);
                }
            }

            if (sections_.ContainsKey(".tags"))
            {
                Section sc = sections_[".tags"];
                BinaryReader br = new BinaryReader(new MemoryStream(binary, sc.dataoffs, sc.size));
                int numTags = sc.size / 8;
                tags_ = new Tag[numTags];
                for (int i = 0; i < numTags; i++)
                {
                    uint tag_id = br.ReadUInt32();
                    uint nameOffset = br.ReadUInt32();
                    string name = ReadString(binary, sections_[".names"].dataoffs + (int)nameOffset, header_.dataoffs);
                    tags_[i] = new Tag(name, tag_id);
                }
            }

            if (sections_.ContainsKey(".dbg.info"))
            {
                Section sc = sections_[".dbg.info"];
                BinaryReader br = new BinaryReader(new MemoryStream(binary, sc.dataoffs, sc.size));
                debugHeader_.numFiles = (int)br.ReadUInt32();
                debugHeader_.numLines = (int)br.ReadUInt32();
                debugHeader_.numSyms = (int)br.ReadUInt32();
            }

            if (sections_.ContainsKey(".dbg.files") && debugHeader_.numFiles > 0)
            {
                Section sc = sections_[".dbg.files"];
                BinaryReader br = new BinaryReader(new MemoryStream(binary, sc.dataoffs, sc.size));
                debugFiles_ = new DebugFile[debugHeader_.numFiles];
                for (int i = 0; i < debugHeader_.numFiles; i++)
                {
                    uint address = br.ReadUInt32();
                    uint nameOffset = br.ReadUInt32();
                    string name = ReadString(binary, sections_[".dbg.strings"].dataoffs + (int)nameOffset, header_.dataoffs);
                    debugFiles_[i] = new DebugFile(name, nameOffset);
                }
            }

            if (sections_.ContainsKey(".dbg.lines") && debugHeader_.numLines > 0)
            {
                Section sc = sections_[".dbg.lines"];
                BinaryReader br = new BinaryReader(new MemoryStream(binary, sc.dataoffs, sc.size));
                debugLines_ = new DebugLine[debugHeader_.numLines];
                for (int i = 0; i < debugHeader_.numLines; i++)
                {
                    uint address = br.ReadUInt32();
                    uint line = br.ReadUInt32();
                    debugLines_[i] = new DebugLine((int)line, address);
                }
            }

            if (sections_.ContainsKey(".dbg.symbols") && debugHeader_.numSyms > 0)
            {
                Section sc = sections_[".dbg.symbols"];
                BinaryReader br = new BinaryReader(new MemoryStream(binary, sc.dataoffs, sc.size));
                List<Variable> locals = new List<Variable>();
                List<Variable> globals = new List<Variable>();
                List<Function> functions = new List<Function>();
                for (int i = 0; i < debugHeader_.numSyms; i++)
                {
                    int addr = br.ReadInt32();
                    short tagid = br.ReadInt16();
                    uint codestart = br.ReadUInt32();
                    uint codeend = br.ReadUInt32();
                    byte ident = br.ReadByte();
                    Scope vclass = (Scope)br.ReadByte();
                    ushort dimcount = br.ReadUInt16();
                    uint nameOffset = br.ReadUInt32();
                    string name = ReadString(binary, sections_[".dbg.strings"].dataoffs + (int)nameOffset, header_.dataoffs);

                    if (ident == IDENT_FUNCTION)
                    {
                        Tag tag = tagid >= tags_.Length ? null : tags_[tagid];
                        Function func = new Function((uint)addr, codestart, codeend, name, tag);
                        functions.Add(func);
                    }
                    else
                    {
                        VariableType type = FromIdent(ident);
                        Dimension[] dims = null;
                        if (dimcount > 0)
                        {
                            dims = new Dimension[dimcount];
                            for (int dim = 0; dim < dimcount; dim++)
                            {
                                short dim_tagid = br.ReadInt16();
                                Tag dim_tag = dim_tagid >= tags_.Length ? null : tags_[dim_tagid];
                                uint size = br.ReadUInt32();
                                dims[dim] = new Dimension(dim_tagid, dim_tag, (int)size);
                            }
                        }

                        Tag tag = tagid >= tags_.Length ? null : tags_[tagid];
                        Variable var = new Variable(addr, tagid, tag, codestart, codeend, type, vclass, name, dims);
                        if (vclass == Scope.Global)
                            globals.Add(var);
                        else
                            locals.Add(var);
                    }
                }

                globals.Sort(delegate(Variable var1, Variable var2)
                {
                    return var1.address - var2.address;
                });
                functions.Sort(delegate(Function fun1, Function fun2)
                {
                    return (int)(fun1.address - fun2.address);
                });

                variables_ = locals.ToArray();
                globals_ = globals.ToArray();
                functions_ = functions.ToArray();
            }

            if (sections_.ContainsKey(".dbg.natives"))
            {
                Section sc = sections_[".dbg.natives"];
                BinaryReader br = new BinaryReader(new MemoryStream(binary, sc.dataoffs, sc.size));
                uint nentries = br.ReadUInt32();
                for (int i = 0; i < (int)nentries; i++)
                {
                    uint index = br.ReadUInt32();
                    uint nameOffset = br.ReadUInt32();
                    string name = ReadString(binary, sections_[".dbg.strings"].dataoffs + (int)nameOffset, header_.dataoffs);
                    short tagid = br.ReadInt16();
                    Tag tag = tagid >= tags_.Length ? null : tags_[tagid];
                    ushort nargs = br.ReadUInt16();

                    Argument[] args = new Argument[nargs];
                    for (ushort arg = 0; arg < nargs; arg++)
                    {
                        byte ident = br.ReadByte();
                        short arg_tagid = br.ReadInt16();
                        ushort dimcount = br.ReadUInt16();
                        uint argNameOffset = br.ReadUInt32();
                        string argName = ReadString(binary, sections_[".dbg.strings"].dataoffs + (int)argNameOffset, header_.dataoffs);
                        Tag argTag = arg_tagid >= tags_.Length ? null : tags_[arg_tagid];
                        VariableType type = FromIdent(ident);

                        Dimension[] dims = null;
                        if (dimcount > 0)
                        {
                            dims = new Dimension[dimcount];
                            for (int dim = 0; dim < dimcount; dim++)
                            {
                                short dim_tagid = br.ReadInt16();
                                Tag dim_tag = dim_tagid >= tags_.Length ? null : tags_[dim_tagid];
                                uint size = br.ReadUInt32();
                                dims[dim] = new Dimension(dim_tagid, dim_tag, (int)size);
                            }
                        }

                        args[arg] = new Argument(type, argName, arg_tagid, argTag, dims);
                    }

                    if ((int)index >+ natives_.Length)
                        continue;

                    natives_[index].setDebugInfo(tagid, tag, args);
                }
            }

            // For every function, attempt to build argument information.
            for (int i = 0; i < functions_.Length; i++)
            {
                Function fun = functions_[i];
                int argOffset = 12;
                var args = new List<Argument>();
                do
                {
                    Variable var = lookupVariable(fun.address, argOffset);
                    if (var == null)
                        break;
                    Argument arg = new Argument(var.type, var.name, (int)var.tag.tag_id, var.tag, var.dims);
                    args.Add(arg);
                    argOffset += 4;
                } while (true);
                fun.setArguments(args);
            }
        }

        public Code code
        {
            get { return code_; }
        }

        public Data data
        {
            get { return data_; }
        }

        public PubVar[] pubvars
        {
            get { return pubvars_; }
        }
        public Native[] natives
        {
            get { return natives_; }
        }

        public string lookupFile(uint address)
        {
            if (debugFiles_ == null)
                return null;

            int high = debugFiles_.Length;
            int low = -1;

            while (high - low > 1)
            {
                int mid = (low + high) >> 1;
                if (debugFiles_[mid].address <= address)
                    low = mid;
                else
                    high = mid;
            }
            if (low == -1)
                return null;
            return debugFiles_[low].name;
        }

        public int lookupLine(uint address)
        {
            if (debugLines_ == null)
                return -1;

            int high = debugLines_.Length;
            int low = -1;

            while (high - low > 1)
            {
                int mid = (low + high) >> 1;
                if (debugLines_[mid].address <= address)
                    low = mid;
                else
                    high = mid;
            }
            if (low == -1)
                return -1;
            return debugLines_[low].line;
        }

        public Variable lookupDeclarations(uint pc, ref int i, Scope scope = Scope.Local)
        {
            for (i++; i < variables_.Length; i++)
            {
                Variable var = variables_[i];
                if (pc != var.codeStart)
                    continue;
                if (var.scope == scope)
                    return var;
            }
            return null;
        }

        public Variable lookupVariable(uint pc, int offset, Scope scope = Scope.Local)
        {
            for (int i = 0; i < variables_.Length; i++)
            {
                Variable var = variables_[i];
                if ((pc >= var.codeStart && pc < var.codeEnd) &&
                    (offset == var.address && var.scope == scope))
                {
                    return var;
                }
            }
            return null;
        }

        public Variable lookupGlobal(int address)
        {
            for (int i = 0; i < globals_.Length; i++)
            {
                Variable var = globals_[i];
                if (var.address == address)
                    return var;
            }
            return null;
        }

        public override string stringFromData(int address)
        {
            return ReadString(data.bytes, address, header_.dataoffs);
        }
        public override int int32FromData(int address)
        {
            if ((address - 4) <= data.bytes.Length)
            {
                return BitConverter.ToInt32(data.bytes, address);
            }
            else
            {
                return 0;
            }
        }
        public override float floatFromData(int address)
        {
            return BitConverter.ToSingle(data.bytes, address);
        }
        public override byte[] DAT
        {
            get { return data.bytes; }
        }
    }

    public static class OpcodeHelpers
    {
        public static SPOpcode ConditionToJump(SPOpcode spop, bool onTrue)
        {
            switch (spop)
            {
                case SPOpcode.sleq:
                    return onTrue ? SPOpcode.jsleq : SPOpcode.jsgrtr;
                case SPOpcode.sless:
                    return onTrue ? SPOpcode.jsless : SPOpcode.jsgeq;
                case SPOpcode.sgrtr:
                    return onTrue ? SPOpcode.jsgrtr : SPOpcode.jsleq;
                case SPOpcode.sgeq:
                    return onTrue ? SPOpcode.jsgeq : SPOpcode.jsless;
                case SPOpcode.eq:
                    return onTrue ? SPOpcode.jeq : SPOpcode.jneq;
                case SPOpcode.neq:
                    return onTrue ? SPOpcode.jneq : SPOpcode.jeq;
                case SPOpcode.not:
                    return onTrue ? SPOpcode.jzer : SPOpcode.jnz;
                default:
                    //Debug.Assert(false);
                    break;
            }
            return spop;
        }

        public static bool IsFunctionTag(Tag tag)
        {
            switch (tag.name)
            {
                case "Function":
                case "ConCmd":
                case "Timer":
                case "NativeCall":
                case "SocketErrorCB":
                case "SocketReceiveCB":
                case "SocketDisconnectCB":
                case "SocketConnectCB":
                    return true;
            }
            return false;
        }

        public static SPOpcode Invert(SPOpcode spop)
        {
            switch (spop)
            {
                case SPOpcode.jsleq:
                    return SPOpcode.jsgrtr;
                case SPOpcode.jsless:
                    return SPOpcode.jsgeq;
                case SPOpcode.jsgrtr:
                    return SPOpcode.jsleq;
                case SPOpcode.jsgeq:
                    return SPOpcode.jsless;
                case SPOpcode.jeq:
                    return SPOpcode.jneq;
                case SPOpcode.jneq:
                    return SPOpcode.jeq;
                case SPOpcode.jnz:
                    return SPOpcode.jzer;
                case SPOpcode.jzer:
                    return SPOpcode.jnz;
                case SPOpcode.sleq:
                    return SPOpcode.sgrtr;
                case SPOpcode.sless:
                    return SPOpcode.sgeq;
                case SPOpcode.sgrtr:
                    return SPOpcode.sleq;
                case SPOpcode.sgeq:
                    return SPOpcode.sless;
                case SPOpcode.eq:
                    return SPOpcode.neq;
                case SPOpcode.neq:
                    return SPOpcode.eq;
                default:
                    //Debug.Assert(false);
                    break;
            }
            return spop;
        }
    }
}
