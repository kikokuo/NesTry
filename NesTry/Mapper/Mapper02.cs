using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NesTry.Mapper
{
    class Mapper02 : MapperBase
    {
        private List<byte[]> mapper02_prg_banks;
        private RomInfo mapper02_RomInfo;
        public void Hsync()
        {
        }

        public bool Reset(ref List<byte[]> prg_banks, ref List<byte[]> ppu_banks, RomInfo romInfo)
        {
            mapper02_RomInfo = romInfo;
            mapper02_prg_banks = prg_banks;
            // 你用UxROM居然没有32KB PRG-ROM?
            Debug.Assert(romInfo.count_prgrom16kb > 2, "bad count");
            // PRG-ROM
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 0, 0);
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 1, 1);
            int last = romInfo.count_prgrom16kb * 2;
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 2, last - 2);
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 3, last - 1);
            // CHR-ROM 没有 是RAM
            for (int i = 0; i != 8; ++i)
                RomMapper.Load_Chrrom_1k(ref ppu_banks, romInfo, i, i);
            return true;
        }

        public void Write_high_address(ushort address, byte value)
        {
            int bank = value % mapper02_RomInfo.count_prgrom16kb * 2;
            RomMapper.Load_Prgrom_8k(ref mapper02_prg_banks, mapper02_RomInfo, 0, bank + 0);
            RomMapper.Load_Prgrom_8k(ref mapper02_prg_banks, mapper02_RomInfo, 1, bank + 1);
        }
    }
}
