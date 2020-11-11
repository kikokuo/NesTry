using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NesTry
{
    public struct RomInfo
    {
        // PRG-ROM 程序只讀儲存器 
        public byte[]data_prgrom;
        // CHR-ROM 角色只讀存儲器 
        public byte[]data_chrrom;
        // 16KB為單位 程序只讀儲存器 數據長度
        public Int32 count_prgrom16kb;
        //  8KB為單位 角色只讀存儲器 數據長度
        public Int32 count_chrrom_8kb;
        // Mapper 編號
        public int mapper_number;
        // 是否Vertical Mirroring(否即為水平)
        public bool vmirroring;
        // 是否FourScreen
        public bool four_screen;
        // 是否有SRAM(電池供電的)
        public bool save_ram;
        // 保留以對齊
        public byte[] reserved;
    }
    struct RomHead
    {
        // NES id
        public Int32 id;
        // 16k 程序只讀儲存器 數量
        public byte count_prgrom16kb;
        // 8k 角色只讀存儲器 數量
        public byte count_chrrom_8kb;
        // 控制信息1
        public byte control1;
        // 控制信息2
        public byte control2;
        // 保留數據
        public byte[] reserved;
        public int trainer;
        public int unisystem;
        public int playchoice10;
    }
    enum Contorl_1{
        NES_VMIRROR = 0x01,
        NES_SAVERAM = 0x02,
        NES_TRAINER = 0x04,
        NES_4SCREEN = 0x08
    };

    enum Control_2{
        NES_VS_UNISYSTEM = 0x01,
        NES_Playchoice10 = 0x02
    };
    public  enum Fc_error_code
    {
        FC_ERROR_OK = 0,
        FC_ERROR_FAILED,
        FC_ERROR_MAPPER_NOT_FOUND,
        FC_ERROR_FILE_NOT_FOUND,
        FC_ERROR_ILLEGAL_FILE,
        FC_ERROR_UNSUPPORT,
    };
    public class NesRom
    {
        private readonly string m_romName;
        private RomHead m_romHead;
        public  RomInfo m_romInfo;

        public NesRom(string romName)
        {
            m_romName = romName;
        }

        private Fc_error_code ReadHead(BinaryReader r)
        {
            m_romHead.id = r.ReadInt32();
            int id = 0x1a53454e;
            if (m_romHead.id != id)
                return Fc_error_code.FC_ERROR_ILLEGAL_FILE;
            m_romHead.count_prgrom16kb = r.ReadByte();
            m_romHead.count_chrrom_8kb = r.ReadByte();
            m_romHead.control1 = r.ReadByte();
            m_romHead.control2 = r.ReadByte();
            m_romHead.trainer = (int)m_romHead.control1 & (int)(object)Contorl_1.NES_TRAINER;
            if (m_romHead.trainer != 0)
                return Fc_error_code.FC_ERROR_UNSUPPORT;
            m_romHead.unisystem = (int)m_romHead.control2 & (int)(object)Control_2.NES_VS_UNISYSTEM;
            if (m_romHead.unisystem != 0)
                return Fc_error_code.FC_ERROR_UNSUPPORT;
            m_romHead.playchoice10 = (int)m_romHead.control2 & (int)(object)Control_2.NES_Playchoice10;
            if (m_romHead.playchoice10 != 0)
                return Fc_error_code.FC_ERROR_UNSUPPORT;

            List<byte> termsList = new List<byte>();
            for (int i = 0; i < 8; i++)
                termsList.Add(r.ReadByte());
            m_romHead.reserved = termsList.ToArray();
            return Fc_error_code.FC_ERROR_OK;
        }


        private Fc_error_code ReadRomBody(BinaryReader r)
        {
            int size1 = 16384 * m_romHead.count_prgrom16kb;
            int size2 = 8192 * (m_romHead.count_chrrom_8kb|1);
            int size3 = 8192 * m_romHead.count_chrrom_8kb;
            m_romInfo.count_prgrom16kb = m_romHead.count_prgrom16kb;
            m_romInfo.count_chrrom_8kb = m_romHead.count_chrrom_8kb;
            m_romInfo.mapper_number = m_romHead.control1 >> 4 | (m_romHead.control2 & 0xF0);

            m_romInfo.vmirroring = (m_romHead.control1 & (int)(object)Contorl_1.NES_VMIRROR) > 0;
            m_romInfo.four_screen = (m_romHead.control1 & (int)(object)Contorl_1.NES_4SCREEN) > 0;
            m_romInfo.save_ram = (m_romHead.control1 & (int)(object)Contorl_1.NES_SAVERAM) > 0;

            // jump Trainer data section
            if (m_romHead.trainer != 0) 
                r.BaseStream.Seek(512, SeekOrigin.Current);

            List<byte> termsList = new List<byte>();
            for (int i = 0; i < (size1 + size2); i++)
                termsList.Add(r.ReadByte());
            m_romInfo.data_prgrom = termsList.ToArray();
            m_romInfo.data_chrrom = termsList.GetRange(size1, size3).ToArray();
            return Fc_error_code.FC_ERROR_OK;
        }

        public bool ReadRom()
        {
            if (!File.Exists(m_romName))
                return false; 
            // store FileStream to check current position
            using (FileStream s = File.OpenRead(m_romName))
            {
                // and BinareReader to read values
                using BinaryReader r = new BinaryReader(s);
                if (ReadHead(r) != Fc_error_code.FC_ERROR_OK)
                    return false;

                if (ReadRomBody(r) != Fc_error_code.FC_ERROR_OK)
                    return false;
            }
            return true;
        }
         
    }
}
