namespace DieselScriptData
{
    using System.Collections.Generic;
    using System.IO;

    public class DieselScriptData
    {
        #region Fields

        public dynamic Root;

        private readonly BinaryReader br;

        private readonly Stack<long> savedPositions = new Stack<long>();

        private int floatOffset;

        private int idstringOffset;

        private string path;

        private int quaternionOffset;

        private int stringOffset;

        private int tableOffset;

        private int vectorOffset;

        #endregion

        #region Constructors and Destructors

        public DieselScriptData(string basePath, string path)
        {
            this.path = basePath + path;
            using (var fs = new FileStream(this.path, FileMode.Open, FileAccess.Read))
            {
                this.br = new BinaryReader(fs);
                this.ReadHeader();
                this.Root = this.ParseItem();
                this.br.Close();
            }
        }

        #endregion

        #region Methods

        private dynamic ParseItem()
        {
            uint item_type = this.br.ReadUInt32();
            int value = (int)item_type & 0xFFFFFF;
            item_type = (item_type >> 24) & 0xFF;

            switch (item_type)
            {
                case 0: //Nil
                    return null;
                case 1: //False
                    return false;
                case 2: //True
                    return true;
                case 3: //Number
                    return this.ReadFloat(value);
                case 4: //String
                    return this.ReadString(value);
                case 5: //Vector
                    return this.ReadVector(value);
                case 6: //Quaternion
                    return this.ReadQuaternion(value);
                case 7: //IdString
                    return this.ReadIdString(value);
                case 8: //Table
                    return this.ReadTable(value);
                default:
                    return null;
            }
        }

        private float ReadFloat(int index)
        {
            float return_float;
            this.SeekPush();
            this.Seek(this.floatOffset + index * 4);
            return_float = this.br.ReadSingle();
            this.SeekPop();
            return return_float;
        }

        private void ReadHeader()
        {
            this.Seek(12);
            this.floatOffset = this.br.ReadInt32();
            this.Seek(28);
            this.stringOffset = this.br.ReadInt32();
            this.Seek(44);
            this.vectorOffset = this.br.ReadInt32();
            this.Seek(60);
            this.quaternionOffset = this.br.ReadInt32();
            this.Seek(76);
            this.idstringOffset = this.br.ReadInt32();
            this.Seek(92);
            this.tableOffset = this.br.ReadInt32();
            this.Seek(100);
        }

        private ulong ReadIdString(int index)
        {
            ulong return_idstring;
            this.SeekPush();
            this.Seek(this.idstringOffset + index * 8);
            return_idstring = this.br.ReadUInt64();
            this.SeekPop();
            return return_idstring;
        }

        private float[] ReadQuaternion(int index)
        {
            var return_quaternion = new float[4];
            this.SeekPush();
            this.Seek(this.quaternionOffset + index * 16);
            return_quaternion[0] = this.br.ReadSingle();
            return_quaternion[1] = this.br.ReadSingle();
            return_quaternion[2] = this.br.ReadSingle();
            return_quaternion[3] = this.br.ReadSingle();
            this.SeekPop();
            return return_quaternion;
        }

        private string ReadString(int index)
        {
            string return_string = null;
            this.SeekPush();
            this.Seek(this.stringOffset + (index * 8) + 4);
            int real_offset = this.br.ReadInt32();
            this.Seek(real_offset);
            var inchar = (char)this.br.ReadByte();
            while (inchar != '\0')
            {
                return_string += inchar;
                inchar = (char)this.br.ReadByte();
            }
            this.SeekPop();
            return return_string;
        }

        private Dictionary<string, object> ReadTable(int index)
        {
            var return_table = new Dictionary<string, object>();
            this.SeekPush();
            this.Seek(this.tableOffset + index * 20);
            int metatable_offset = this.br.ReadInt32();
            int item_count = this.br.ReadInt32();
            this.br.ReadInt32(); // Unknown
            int items_offset = this.br.ReadInt32();
            for (int current_item = 0; current_item < item_count; ++current_item)
            {
                this.Seek(items_offset + current_item * 8);
                dynamic key_item = this.ParseItem().ToString();
                dynamic value_item = this.ParseItem();
                return_table[key_item] = value_item;
            }
            this.SeekPop();
            return return_table;
        }

        private float[] ReadVector(int index)
        {
            var return_vector = new float[3];
            this.SeekPush();
            this.Seek(this.vectorOffset + index * 12);
            return_vector[0] = this.br.ReadSingle();
            return_vector[1] = this.br.ReadSingle();
            return_vector[2] = this.br.ReadSingle();
            this.SeekPop();
            return return_vector;
        }

        private void Seek(int offset)
        {
            this.br.BaseStream.Seek(offset, SeekOrigin.Begin);
        }

        private void SeekAdvance(int offset)
        {
            this.br.BaseStream.Seek(offset, SeekOrigin.Current);
        }

        private void SeekPop()
        {
            this.br.BaseStream.Seek(this.savedPositions.Pop(), SeekOrigin.Begin);
        }

        private void SeekPush()
        {
            this.savedPositions.Push(this.br.BaseStream.Position);
        }

        #endregion
    }
}