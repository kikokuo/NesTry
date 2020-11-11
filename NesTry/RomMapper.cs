using System;
using System.Collections.Generic;
using System.Text;

namespace NesTry
{
    public class RomMapper
    {
        public Fc_error_code LoadMapper(ref List<byte[]> prg_banks, ref List<byte[]> ppu_banks, RomInfo romInfo)
        {
            switch(romInfo.mapper_number)
            {
                case 0:
                    Reset(ref prg_banks,ref ppu_banks, romInfo);
                    return Fc_error_code.FC_ERROR_OK;
            }
            return Fc_error_code.FC_ERROR_MAPPER_NOT_FOUND;
        }
        public bool Reset(ref List<byte[]> prg_banks, ref List<byte[]> ppu_banks, RomInfo romInfo)
        {
            if (romInfo.count_prgrom16kb == 0 || romInfo.count_prgrom16kb > 2)
                return false;
            // 16KB -> 载入 $8000-$BFFF, $C000-$FFFF 为镜像
            int id2 = romInfo.count_prgrom16kb & 2;
            // 32KB -> 载入 $8000-$FFFF
            Load_Prgrom_8k(ref prg_banks, romInfo, 0, 0);
            Load_Prgrom_8k(ref prg_banks, romInfo, 1, 1);
            Load_Prgrom_8k(ref prg_banks, romInfo, 2, id2 + 0);
            Load_Prgrom_8k(ref prg_banks, romInfo, 3, id2 + 1);

            // CHR-ROM
            for (int i = 0; i != 8; ++i)
                Load_Chrrom_1k(ref ppu_banks, romInfo, i, i);
            return true;
        }
        private void Load_Prgrom_8k(ref List<byte[]> prg_banks, RomInfo romInfo, int des, int src)
        {
            Array.Copy(romInfo.data_prgrom, 8192 * src, prg_banks[4 + des],0 ,8192);
        }

        // <summary>
        /// 實用函數-StepFC: 載入1k CHR-ROM
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <param name="des">The DES.</param>
        /// <param name="src">The source.</param>
        private void Load_Chrrom_1k(ref List<byte[]> ppu_banks, RomInfo romInfo, int des, int src)
        {
            Array.Copy(romInfo.data_chrrom, 1024 * src, ppu_banks[des], 0, 1024);
        }
    }
}
