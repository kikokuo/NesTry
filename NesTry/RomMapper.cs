using NesTry.Mapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace NesTry
{
    public enum nametable_mirroring_mode
    {
        SFC_NT_MIR_SingleLow = 0,
        SFC_NT_MIR_SingleHigh,
        SFC_NT_MIR_Vertical,
        SFC_NT_MIR_Horizontal,
        SFC_NT_MIR_FourScreen,
    };
    public class RomMapper
    {
        public MapperBase mapperbase;
 
        public Fc_error_code LoadMapper(ref NesPPU ppu, ref List<byte[]> prg_banks, ref List<byte[]> ppu_banks, RomInfo romInfo)
        {
            switch(romInfo.mapper_number)
            {
                case 0:
                    mapperbase = new Mapper00();
                    return Fc_error_code.FC_ERROR_OK;
                case 1:
                    mapperbase = new Mapper01(ref ppu);
                    return Fc_error_code.FC_ERROR_OK;
            }
            return Fc_error_code.FC_ERROR_MAPPER_NOT_FOUND;
        }
        public bool Reset(ref List<byte[]> prg_banks, ref List<byte[]> ppu_banks, RomInfo romInfo)
        {
            if (mapperbase != null)
                mapperbase.Reset(ref prg_banks,ref ppu_banks, romInfo);
             return true;
        }

        public void Write_high_address(UInt16 address, byte value)
        {
            if (mapperbase != null)
                mapperbase.Write_high_address(address, value);
        }
        public void Hsync()
        {
            if(mapperbase != null)
               mapperbase.Hsync();
        }

        static public void Load_Prgrom_8k(ref List<byte[]> prg_banks, RomInfo romInfo, int des, int src)
        {
            Array.Copy(romInfo.data_prgrom, 8192 * src, prg_banks[4 + des],0 ,8192);
        }

        // <summary>
        /// 實用函數-StepFC: 載入1k CHR-ROM
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <param name="des">The DES.</param>
        /// <param name="src">The source.</param>
        static public void Load_Chrrom_1k(ref List<byte[]> ppu_banks, RomInfo romInfo, int des, int src)
        {
            Array.Copy(romInfo.data_chrrom, 1024 * src, ppu_banks[des], 0, 1024);
        }
    }
}
