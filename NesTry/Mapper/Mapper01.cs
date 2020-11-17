using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NesTry.Mapper
{
    class Mapper01 : MapperBase
    {
        struct mapper01_data
        {
            // 移位寄存器
            public byte shifter;
            // RPG-ROM 切換模式
            public byte prmode;
            // CHR-ROM 切換模式
            public byte crmode;
            // 控制寄存器
            public byte control;
        };
        private mapper01_data mapper01;
        private List<byte[]> mapper01_prg_banks;
        private RomInfo mapper01_RomInfo;
        private List<byte[]> mapper01_ppu_banks;
        private NesPPU m_ppu;
        public Mapper01(ref NesPPU ppu)
        {
            mapper01.shifter = 0x10;
            m_ppu = ppu;
        }
        public bool Reset(ref List<byte[]> prg_banks, ref List<byte[]> ppu_banks, RomInfo romInfo)
        {
            // 你用MMC1居然沒有32KB PRG-ROM?
            Debug.Assert(romInfo.count_prgrom16kb > 2, "bad count");
            mapper01_RomInfo = romInfo;
            mapper01_prg_banks = prg_banks;
            mapper01_ppu_banks = ppu_banks;
            // PRG-ROM
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 0, 0);
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 1, 1);
            int last = romInfo.count_prgrom16kb * 2;
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 2, last - 2);
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 3, last - 1);
            // CHR-ROM
            for (int i = 0; i != 8; ++i)
                RomMapper.Load_Chrrom_1k(ref ppu_banks, romInfo, i, i);
            return true;
        }
        public void Write_high_address(UInt16 address, byte value)
        {
            // D7 = 1 -> 重置移位寄存器
            if ((value & 0x80) > 0)
            {
                mapper01.shifter = 0x10;
                sfc_mapper_01_write_control((byte)(mapper01.control| 0x0C));
            }
            // D7 = 0 -> 寫入D0到移位寄存器
            else
            {
                byte finished = (byte)(mapper01.shifter & 1);
                mapper01.shifter >>= 1;
                mapper01.shifter |= (byte)((value & 1) << 4);
                if (finished > 0)
                {
                    sfc_mapper_01_write_register(address);
                    mapper01.shifter = 0x10;
                }
            }
        }
        public void Hsync()
        {

        }

        /// <summary>
        /// SFCs the mapper 01 write control.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        private void sfc_mapper_01_write_control(byte data)
        {
            mapper01.control = data;
            // D0D1 - 鏡像模式
            nametable_mirroring_mode foo = (nametable_mirroring_mode)Enum.ToObject(typeof(nametable_mirroring_mode), data & 0x3);
            m_ppu.Switch_Nametable_Mirroring(foo);
            // D2D3 - PRG ROM bank 模式
            mapper01.prmode = (byte)((data >> 2) & 0x3);
            // D5 - CHR ROM bank 模式
            mapper01.crmode = (byte)(data >> 4);
        }

        /// <summary>
        /// SFCs the mapper 01 write register.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <param name="address">The address.</param>
        private void sfc_mapper_01_write_register(UInt16 address)
        {
            switch ((address & 0x7FFF) >> 13)
            {
                case 0:
                    // $8000-$9FFF Control
                    sfc_mapper_01_write_control(mapper01.shifter);
                    break;
                case 1:
                    // $A000-$BFFF CHR bank 0
                    sfc_mapper_01_write_chrbank0();
                    break;
                case 2:
                    // $C000-$DFFF CHR bank 1
                    sfc_mapper_01_write_chrbank1();
                    break;
                case 3:
                    // $E000-$FFFF PRG bank
                    sfc_mapper_01_write_prgbank();
                    break;
            }
        }

        /// <summary>
        /// SFCs the mapper 01 write prgbank.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        private void sfc_mapper_01_write_prgbank()
        {

            byte data = mapper01.shifter;
            byte mode = mapper01.prmode;
            // PRG RAM使能
            // TODO: PRG-RAM
            byte prgram = (byte)(data & 0x10);
            byte bankid = (byte)(data & 0x0f); 
            int bank;
            int last;
            switch (mode)
            {
                 case 0: case 1:
                    // 32KB 模式
                    bank = (bankid & 0xE) * 2;
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 0, bank + 0);
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 1, bank + 1);
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 2, bank + 2);
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 3, bank + 3);
                    break;
                case 2:
                    // 固定低16KB到最後 切換高16KB
                    bank = bankid * 2;
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 0, 0);
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 1, 1);
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 2, bank + 0);
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 3, bank + 1);
                    break;
                case 3:
                    // 固定高16KB到最後 切換低16KB
                    bank = bankid * 2;
                    last = mapper01_RomInfo.count_prgrom16kb * 2;
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 0, bank + 0);
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 1, bank + 1);
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 2, last - 2);
                    RomMapper.Load_Prgrom_8k(ref mapper01_prg_banks, mapper01_RomInfo, 3, last - 1);
                    break;
            }
        }

        /// <summary>
        /// SFCs the mapper 01 write chrbank1.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        private void sfc_mapper_01_write_chrbank1()
        {
            byte data = mapper01.shifter;
            byte mode = mapper01.crmode;
            // 8KB模式?
            if (!(mode > 0)) return;
            // 4KB模式
            int bank = data * 4;
            RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 4, bank + 0);
            RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 5, bank + 1);
            RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 6, bank + 2);
            RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 7, bank + 3);
        }

        /// <summary>
        /// SFCs the mapper 01 write chrbank0.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        private void sfc_mapper_01_write_chrbank0()
        {
            byte data = mapper01.shifter;
            byte mode = mapper01.crmode;
            // 8KB模式?
            if (!(mode > 0))
            {
                int bank = (data & 0x0E) * 4;
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 0, bank + 0);
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 1, bank + 1);
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 2, bank + 2);
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 3, bank + 3);
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 4, bank + 4);
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 5, bank + 5);
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 6, bank + 6);
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 7, bank + 7);
            }
            // 4KB模式
            else
            {
                int bank = data * 4;
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 0, bank + 0);
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 1, bank + 1);
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 2, bank + 2);
                RomMapper.Load_Chrrom_1k(ref mapper01_ppu_banks, mapper01_RomInfo, 3, bank + 3);
            }
        }
    }
}
