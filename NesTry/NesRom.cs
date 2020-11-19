using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        //Submapper
        public int sub_number;
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
        // MAPPER變種
        public byte mapper_variant;
        // 高位ROM大小
        public byte upper_rom_size;
        // RAM大小
        public byte ram_size;
        // 保留數據
        public byte[] reserved;
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
        FC_ERROR_UNSUPPORT_TRAINER,
        FC_ERROR_UNSUPPORT_VS_UNISYSTEM,
        FC_ERROR_UNSUPPORT_Playchoice10,
    };
    public class NesRom
    {
        private readonly string m_romName;
        private RomHead m_romHead;
        public  RomInfo m_romInfo;
        public bool m_loadromsuccess; 

        public NesRom(string romName)
        {
            m_romName = romName;
            m_loadromsuccess = false;
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

            if ((m_romHead.control1 & (byte)Contorl_1.NES_TRAINER) > 0)
                return Fc_error_code.FC_ERROR_UNSUPPORT_TRAINER;
            if ((m_romHead.control2 & (byte)Control_2.NES_VS_UNISYSTEM) > 0)
                return Fc_error_code.FC_ERROR_UNSUPPORT_VS_UNISYSTEM;
            if ((m_romHead.control2 & (byte)Control_2.NES_Playchoice10) > 0)
                return Fc_error_code.FC_ERROR_UNSUPPORT_Playchoice10;

            m_romHead.mapper_variant = r.ReadByte();
            m_romHead.upper_rom_size = r.ReadByte();
            m_romHead.ram_size = r.ReadByte();
            m_romHead.reserved = r.ReadBytes(5);
            return Fc_error_code.FC_ERROR_OK;
        }


        private Fc_error_code ReadRomBody(BinaryReader r)
        {
            uint prgrom16 = (uint)(m_romHead.count_prgrom16kb | ((m_romHead.upper_rom_size & 0x0F) << 8));
            uint chrrom8 = (uint)(m_romHead.count_chrrom_8kb | ((m_romHead.upper_rom_size & 0xF0) << 4));

            uint size1 = 16384 * prgrom16;
            uint size2 = 8192 * (chrrom8 | 1);
            uint size3 = 8192 * chrrom8;

            m_romInfo.count_prgrom16kb = (int)prgrom16;
            m_romInfo.count_chrrom_8kb = (int)chrrom8;
            m_romInfo.mapper_number = m_romHead.control1 >> 4 | (m_romHead.control2 & 0xF0);
            m_romInfo.sub_number = (m_romHead.mapper_variant & 0xF0) >> 4;

            m_romInfo.vmirroring = (m_romHead.control1 & (byte)Contorl_1.NES_VMIRROR) > 0;
            m_romInfo.four_screen = (m_romHead.control1 & (byte)Contorl_1.NES_4SCREEN) > 0;
            m_romInfo.save_ram = (m_romHead.control1 & (byte)Contorl_1.NES_SAVERAM) > 0;

            // jump Trainer data section
            if ((m_romHead.control1 & (byte)Contorl_1.NES_TRAINER) > 0)
                r.BaseStream.Seek(512, SeekOrigin.Current);
            m_romInfo.data_prgrom = r.ReadBytes((int)(size1 + size3));
            if (size3 != 0) { 
                m_romInfo.data_chrrom = new byte[size3];
                Array.Copy(m_romInfo.data_prgrom, size1, m_romInfo.data_chrrom, 0, size3);
            }else{
                m_romInfo.data_chrrom = new byte[size2];
            }
            return Fc_error_code.FC_ERROR_OK;
        }

        public Fc_error_code ReadRom()
        {
            Fc_error_code err = Fc_error_code.FC_ERROR_OK;
            if (!File.Exists(m_romName))
                return Fc_error_code.FC_ERROR_FILE_NOT_FOUND;
            if (m_romName.Contains(".zip")) {
                try { 
                    using (ZipArchive archive = ZipFile.OpenRead(m_romName))
                    {
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.EndsWith(".nes", StringComparison.OrdinalIgnoreCase))
                            {
                                // store FileStream to check current position
                                using (BinaryReader r = new BinaryReader(entry.Open()))
                                {
                                    err = ReadHead(r);
                                    if (err != Fc_error_code.FC_ERROR_OK)
                                        return err;
                                    err = ReadRomBody(r);
                                    if (err != Fc_error_code.FC_ERROR_OK)
                                        return err;
                                }
                            }
                        }
                    }
                }catch (Exception ex){
                    return Fc_error_code.FC_ERROR_ILLEGAL_FILE;
                }
            }
            else
            {
                // store FileStream to check current position
                using (FileStream s = File.OpenRead(m_romName))
                {
                    // and BinareReader to read values
                    using (BinaryReader r = new BinaryReader(s))
                    {
                        err = ReadHead(r);
                        if (err != Fc_error_code.FC_ERROR_OK)
                            return err;
                        err = ReadRomBody(r);
                        if (err != Fc_error_code.FC_ERROR_OK)
                            return err;
                    }
                }
            }
            m_loadromsuccess = true;
            return err;
        }
    }
}
