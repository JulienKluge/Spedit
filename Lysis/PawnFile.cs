using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace Lysis
{
    public class Public
    {
        private uint address_;
        private string name_;

        public Public(string name, uint address)
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

	public abstract class PawnFile
	{
        protected Function[] functions_;
        protected Public[] publics_;
        protected Variable[] globals_;

        public static PawnFile FromFile(string path)
        {
            FileStream fs = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            List<byte> bytes = new List<byte>();
            int b;
            while ((b = fs.ReadByte()) >= 0)
                bytes.Add((byte)b);
            byte[] vec = bytes.ToArray();
            uint magic = BitConverter.ToUInt32(vec, 0);
            if (magic == SourcePawn.SourcePawnFile.MAGIC)
                return new SourcePawn.SourcePawnFile(vec);
            throw new Exception("not a .smx file!");
        }
        
        public abstract string stringFromData(int address);
        public abstract float floatFromData(int address);
        public abstract int int32FromData(int address);

        public Function lookupFunction(uint pc)
        {
            for (int i = 0; i < functions_.Length; i++)
            {
                Function f = functions_[i];
                if (pc >= f.codeStart && pc < f.codeEnd)
                    return f;
            }
            return null;
        }
        public Public lookupPublic(string name)
        {
            for (int i = 0; i < publics_.Length; i++)
            {
                if (publics_[i].name == name)
                    return publics_[i];
            }
            return null;
        }

        public Public lookupPublic(uint addr)
        {
            for (int i = 0; i < publics_.Length; i++)
            {
                if (publics_[i].address == addr)
                    return publics_[i];
            }
            return null;
        }
        
        public Function[] functions
        {
            get { return functions_; }
        }
        public Public[] publics
        {
            get { return publics_; }
        }
        public Variable[] globals
        {
            get { return globals_; }
        }
        public abstract byte[] DAT
        {
            get;
        }
	}
}

