using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NesTry.Mapper
{
    public interface MapperBase
    {
        public bool Reset(ref List<byte[]> prg_banks, ref List<byte[]> ppu_banks, RomInfo romInfo);
        public void Write_high_address(UInt16 address, byte value);
        public void Hsync();
    }
    public class Mapper00 : MapperBase
    {
        public bool Reset(ref List<byte[]> prg_banks, ref List<byte[]> ppu_banks, RomInfo romInfo)
        {
            if (romInfo.count_prgrom16kb == 0 || romInfo.count_prgrom16kb > 2)
                return false;
            // 16KB -> 载入 $8000-$BFFF, $C000-$FFFF 为镜像
            int id2 = romInfo.count_prgrom16kb & 2;
            // 32KB -> 载入 $8000-$FFFF
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 0, 0);
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 1, 1);
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 2, id2 + 0);
            RomMapper.Load_Prgrom_8k(ref prg_banks, romInfo, 3, id2 + 1);

            // CHR-ROM
            for (int i = 0; i != 8; ++i)
                RomMapper.Load_Chrrom_1k(ref ppu_banks, romInfo, i, i);
            return true;
        }
        public void Write_high_address(UInt16 address, byte value)
        {
            Debug.Assert(false,"Can not write prg rom");
        }
        public void Hsync()
        {

        }
    }
}
