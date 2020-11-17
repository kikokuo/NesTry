using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace NesTry
{
    // PPU用標誌
    public enum Fc_ppu_flag
    {
        FC_PPU2000_NMIGen = 0x80, // [0x2000]VBlank期間是否產生NMI
        FC_PPU2000_Sp8x16 = 0x20, // [0x2000]精靈為8x16(1), 還是8x8(0)
        FC_PPU2000_BgTabl = 0x10, // [0x2000]背景調色板表地址$1000(1), $0000(0)
        FC_PPU2000_SpTabl = 0x08, // [0x2000]精靈調色板表地址$1000(1), $0000(0), 8x16模式下被忽略
        FC_PPU2000_VINC32 = 0x04, // [0x2000]VRAM讀寫增加值32(1), 1(0)

        FC_PPU2001_Grey = 0x01, // 灰階使能
        FC_PPU2001_BackL8 = 0x02, // 顯示最左邊的8像素背景
        FC_PPU2001_SpriteL8 = 0x04, // 顯示最左邊的8像素精靈
        FC_PPU2001_Back = 0x08, // 背景顯示使能
        FC_PPU2001_Sprite = 0x10, // 精靈顯示使能

        FC_PPU2001_NTSCEmR = 0x20, // NTSC 強調紅色
        FC_PPU2001_NTSCEmG = 0x40, // NTSC 強調綠色
        FC_PPU2001_NTSCEmB = 0x80, // NTSC 強調藍色

        FC_PPU2001_PALEmG = 0x20, // PAL 強調綠色
        FC_PPU2001_PALEmR = 0x40, // PAL 強調紅色
       SFC_PPU2001_PALEmB = 0x80, // PAL 強調藍色

        FC_PPU2002_VBlank = 0x80, // [0x2002]垂直空白間隙標誌
        FC_PPU2002_Sp0Hit = 0x40, // [0x2002]零號精靈命中標誌
        FC_PPU2002_SpOver = 0x20, // [0x2002]精靈溢出標誌

        FC_SPATTR_FlipV = 0x80, // 垂直翻轉
        FC_SPATTR_FlipH = 0x40, // 水平翻轉
        FC_SPATTR_Priority = 0x20, // 優先位
    };

    /// <summary>
    /// 調色板數據 針對bitmap轉換 顏色不正確 所以調整rgba順序
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Fc_Palette_Data
    {

        [FieldOffset(2)]
        public byte r;
        [FieldOffset(1)]
        public byte g;
        [FieldOffset(0)]
        public byte b;
        [FieldOffset(3)]
        public byte a;  
        [FieldOffset(0)]
        public UInt32 data;
        public Fc_Palette_Data(byte r, byte g, byte b, byte a)
        {
            this.data = 0;
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
    }

    public class NesPPU
    {
        // 內存地址庫
        public List<byte[]> m_banks;
        // VRAM 地址 15bit
        public UInt16 v;
        // 臨時 VRAM 地址 15bit
        public UInt16 t;
        // 微調X滾動偏移 3bit
        public byte x;
        // 寫入切換 1bit
        public byte w;
        // 寄存器 PPUCTRL @$2000
        public byte m_ctrl;
        // 寄存器 PPUMASK @$2001
        public byte m_mask;
        // 寄存器 PPUSTATUS @$2002
        public byte m_status;
        // 寄存器 OAMADDR @$2003
        public byte m_oamaddr;
        // 顯存讀取緩沖值
        public byte m_pseudo;
        // 精靈調色板索引
        public byte[] m_spindexes;
        // 精靈數據: 256B
        public byte[] m_sprites;

        public Fc_Palette_Data [] fc_stdpalette = new Fc_Palette_Data[64]{
    new Fc_Palette_Data( 0x7F, 0x7F, 0x7F, 0xFF), new Fc_Palette_Data( 0x20, 0x00, 0xB0, 0xFF), new Fc_Palette_Data( 0x28, 0x00, 0xB8, 0xFF), new Fc_Palette_Data( 0x60, 0x10, 0xA0, 0xFF),
    new Fc_Palette_Data( 0x98, 0x20, 0x78, 0xFF), new Fc_Palette_Data( 0xB0, 0x10, 0x30, 0xFF), new Fc_Palette_Data( 0xA0, 0x30, 0x00, 0xFF), new Fc_Palette_Data( 0x78, 0x40, 0x00, 0xFF),
    new Fc_Palette_Data( 0x48, 0x58, 0x00, 0xFF), new Fc_Palette_Data( 0x38, 0x68, 0x00, 0xFF), new Fc_Palette_Data( 0x38, 0x6C, 0x00, 0xFF), new Fc_Palette_Data( 0x30, 0x60, 0x40, 0xFF),
    new Fc_Palette_Data( 0x30, 0x50, 0x80, 0xFF), new Fc_Palette_Data( 0x00, 0x00, 0x00, 0xFF), new Fc_Palette_Data( 0x00, 0x00, 0x00, 0xFF), new Fc_Palette_Data( 0x00, 0x00, 0x00, 0xFF),

    new Fc_Palette_Data( 0xBC, 0xBC, 0xBC, 0xFF), new Fc_Palette_Data( 0x40, 0x60, 0xF8, 0xFF), new Fc_Palette_Data( 0x40, 0x40, 0xFF, 0xFF), new Fc_Palette_Data( 0x90, 0x40, 0xF0, 0xFF),
    new Fc_Palette_Data( 0xD8, 0x40, 0xC0, 0xFF), new Fc_Palette_Data( 0xD8, 0x40, 0x60, 0xFF), new Fc_Palette_Data( 0xE0, 0x50, 0x00, 0xFF), new Fc_Palette_Data( 0xC0, 0x70, 0x00, 0xFF),
    new Fc_Palette_Data( 0x88, 0x88, 0x00, 0xFF), new Fc_Palette_Data( 0x50, 0xA0, 0x00, 0xFF), new Fc_Palette_Data( 0x48, 0xA8, 0x10, 0xFF), new Fc_Palette_Data( 0x48, 0xA0, 0x68, 0xFF),
    new Fc_Palette_Data( 0x40, 0x90, 0xC0, 0xFF), new Fc_Palette_Data( 0x00, 0x00, 0x00, 0xFF), new Fc_Palette_Data( 0x00, 0x00, 0x00, 0xFF), new Fc_Palette_Data( 0x00, 0x00, 0x00, 0xFF),

    new Fc_Palette_Data( 0xFF, 0xFF, 0xFF, 0xFF), new Fc_Palette_Data( 0x60, 0xA0, 0xFF, 0xFF), new Fc_Palette_Data( 0x50, 0x80, 0xFF, 0xFF), new Fc_Palette_Data( 0xA0, 0x70, 0xFF, 0xFF),
    new Fc_Palette_Data( 0xF0, 0x60, 0xFF, 0xFF), new Fc_Palette_Data( 0xFF, 0x60, 0xB0, 0xFF), new Fc_Palette_Data( 0xFF, 0x78, 0x30, 0xFF), new Fc_Palette_Data( 0xFF, 0xA0, 0x00, 0xFF),
    new Fc_Palette_Data( 0xE8, 0xD0, 0x20, 0xFF), new Fc_Palette_Data( 0x98, 0xE8, 0x00, 0xFF), new Fc_Palette_Data( 0x70, 0xF0, 0x40, 0xFF), new Fc_Palette_Data( 0x70, 0xE0, 0x90, 0xFF),
    new Fc_Palette_Data( 0x60, 0xD0, 0xE0, 0xFF), new Fc_Palette_Data( 0x60, 0x60, 0x60, 0xFF), new Fc_Palette_Data( 0x00, 0x00, 0x00, 0xFF), new Fc_Palette_Data( 0x00, 0x00, 0x00, 0xFF),

    new Fc_Palette_Data( 0xFF, 0xFF, 0xFF, 0xFF), new Fc_Palette_Data( 0x90, 0xD0, 0xFF, 0xFF), new Fc_Palette_Data( 0xA0, 0xB8, 0xFF, 0xFF), new Fc_Palette_Data( 0xC0, 0xB0, 0xFF, 0xFF),
    new Fc_Palette_Data( 0xE0, 0xB0, 0xFF, 0xFF), new Fc_Palette_Data( 0xFF, 0xB8, 0xE8, 0xFF), new Fc_Palette_Data( 0xFF, 0xC8, 0xB8, 0xFF), new Fc_Palette_Data( 0xFF, 0xD8, 0xA0, 0xFF),
    new Fc_Palette_Data( 0xFF, 0xF0, 0x90, 0xFF), new Fc_Palette_Data( 0xC8, 0xF0, 0x80, 0xFF), new Fc_Palette_Data( 0xA0, 0xF0, 0xA0, 0xFF), new Fc_Palette_Data( 0xA0, 0xFF, 0xC8, 0xFF),
    new Fc_Palette_Data( 0xA0, 0xFF, 0xF0, 0xFF), new Fc_Palette_Data( 0xA0, 0xA0, 0xA0, 0xFF), new Fc_Palette_Data( 0x00, 0x00, 0x00, 0xFF), new Fc_Palette_Data( 0x00, 0x00, 0x00, 0xFF)
        };

        public NesPPU()
        {
            m_banks = new List<byte[]>();
            for(int i = 0;i < 16;i++)
                m_banks.Add(new byte[1024]);
            m_spindexes = new byte[0x20];
            m_sprites = new byte[0x100];
            m_ctrl = 0;
            m_mask = 0;
            m_status = 0;
            m_oamaddr = 0;
            m_pseudo = 0;
        }
        public void Clear()
        {
            foreach(var v in m_banks){
                Array.Clear(v, 0, 1024);
            }
            Array.Clear(m_spindexes, 0, 0x20);
            Array.Clear(m_sprites, 0, 0x100);
            m_ctrl = 0;
            m_mask = 0;
            m_status = 0;
            m_oamaddr = 0;
            m_pseudo = 0;
        }
        public void Reset()
        {
            Array.Copy(m_banks[0x8], 0, m_banks[0xc], 0, 1024);
            Array.Copy(m_banks[0x9], 0, m_banks[0xd], 0, 1024);
            Array.Copy(m_banks[0xa], 0, m_banks[0xe], 0, 1024);
            Array.Copy(m_banks[0xb], 0, m_banks[0xf], 0, 1024);
        }
        // read ppu register via cpu address space
        public byte Read_PPU_Register_Via_CPU(UInt16 address)
        {
            byte data = 0x00;
            // 8字節鏡像
            switch (address & 0x7)
            {
                case 0:
                // 0x2000: Controller ($2000) > write
                // 只寫寄存器
                case 1:
                    // 0x2001: Mask ($2001) > write
                    // 只寫寄存器
                    //Debug.Assert(true,"write only!");
                    break;
                case 2:
                    // 0x2002: Status ($2002) < read
                    // 只讀狀態寄存器
                    data = m_status;
                    // 讀取後會清除VBlank狀態
                    var vblank = ~Fc_ppu_flag.FC_PPU2002_VBlank;
                    m_status &= (byte)vblank;
                    this.w = 0;
                    break;
                case 3:
                    // 0x2003: OAM address port ($2003) > write
                    // 只寫寄存器
                    //Debug.Assert(true, "write only!");
                    break;
                case 4:
                    // 0x2004: OAM data ($2004) <> read/write
                    // 讀寫寄存器
                    data = m_sprites[m_oamaddr];
                    break;
                case 5:
                // 0x2005: Scroll ($2005) >> write x2
                // 雙寫寄存器
                case 6:
                    // 0x2006: Address ($2006) >> write x2
                    // 雙寫寄存器
                    //Debug.Assert(true, "write only!");
                    break;
                case 7:
                    // 0x2007: Data ($2007) <> read/write
                    // PPU VRAM讀寫端口
                    data = Read_PPU_Address(this.v);
                    var vinc32 = m_ctrl & (byte)Fc_ppu_flag.FC_PPU2000_VINC32;
                    this.v += (UInt16)(vinc32 > 0 ? 32 : 1);
                    break;
            }
            return data;
        }
        // write ppu register via cpu address space
        public void Write_PPU_Register_Via_CPU(UInt16 address, byte data)
        {
            switch (address & 0x7)
            {
                case 0:
                    // PPU 控制寄存器
                    // 0x2000: Controller ($2000) > write
                    m_ctrl = data; 
                    this.t = (ushort)((this.t & 0xF3FF) |((data & 0x03) << 10));
                    break;
                case 1:
                    // PPU 掩碼寄存器
                    // 0x2001: Mask ($2001) > write
                    m_mask = data;
                    break;
                case 2:
                    // 0x2002: Status ($2002) < read
                    // 只讀
                    Debug.Assert(true, "read only");
                    break;
                case 3:
                    // 0x2003: OAM address port ($2003) > write
                    // PPU OAM 地址端口
                    m_oamaddr = data;
                    break;
                case 4:
                    // 0x2004: OAM data ($2004) <> read/write
                    // PPU OAM 數據端口
                    m_sprites[m_oamaddr++] = data;
                    break;
                case 5:
                    // 0x2005: Scroll ($2005) >> write x2
                    // PPU 滾動位置寄存器 - 雙寫
                    if(this.w > 0)
                    {
                        this.t = (ushort)((t & 0x8FFF) | ((data & 0x07) << 12));
                        this.t = (ushort)((t & 0xFC1F) | ((data & 0xF8) << 2));
                        this.w = 0;
                    }
                    else
                    {
                        this.t = (ushort)((t & 0xFFE0) | (data >> 3));
                        this.x = (byte)(data & 0x07);
                        this.w = 1;
                    }
                    break;
                case 6:
                    // 0x2006: Address ($2006) >> write x2
                    // PPU 地址寄存器 - 雙寫
                    // 寫入高字節
                    if (this.w > 0)
                    {
                        this.t = (ushort)((t & 0xFF00) | data);
                        this.v = this.t;
                        this.w = 0;
                    }
                    else
                    {
                        this.t = (ushort)((t & 0x80FF) | ((data & 0x3F) << 8));
                        this.w = 1;
                    }
                    break;
                case 7:
                    // 0x2007: Data ($2007) <> read/write
                    // PPU VRAM數據端
                    Write_PPU_Address(this.v, data);
                    var vinc32 = m_ctrl & (byte)Fc_ppu_flag.FC_PPU2000_VINC32;
                    this.v += (UInt16)(vinc32 > 0 ? 32 : 1);
                    break;
            }
        }
        // <summary>
        /// StepFC: 讀取PPU地址空間
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="data">The data.</param>
        /// <param name="ppu">The ppu.</param>
        public byte Read_PPU_Address(UInt16 address)
        {
             UInt16 real_address = (UInt16)(address & 0x3FFF);
            // 使用BANK讀取
            if (real_address < 0x3F00)
            {
                UInt16 index = (UInt16)(real_address >> 10);
                UInt16 offset = (UInt16)(real_address & 0x3FF);
                byte data = m_pseudo;
                m_pseudo = m_banks[index][offset];
                return data;
            }
            // 調色板索引
            else {
                //UInt16 underneath = (UInt16)(real_address - 0x1000);
                UInt16 index = (UInt16)(real_address >> 10);
                UInt16 offset = (UInt16)(real_address & 0x3FF);
                m_pseudo = m_banks[index][offset];
                return m_spindexes[real_address & 0x1f]; 
            }
        }
        // <summary>
        /// StepFC: 寫入PPU地址空間
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="data">The data.</param>
        /// <param name="ppu">The ppu.</param>
        public void Write_PPU_Address(UInt16 address, byte data)
        {
            UInt16 real_address = (UInt16)(address & 0x3FFF);
            // 使用BANK寫入
            if (real_address < 0x3F00)
            {
                UInt16 index = (UInt16)(real_address >> 10);
                UInt16 offset = (UInt16)(real_address & 0x3FF);

                m_banks[index][offset] = data;
            }
            // 調色板索引
            else
            {
                // 獨立地址
                if ((real_address & 0x0003) > 0)
                {
                    m_spindexes[real_address & 0x1f] = data;
                }
                // 鏡像$3F00/$3F04/$3F08/$3F0C
                else
                {
                    UInt16 offset = (UInt16)(real_address & 0x0f);
                    m_spindexes[offset] = data;
                    m_spindexes[offset | 0x10] = data;
                }
            }
        }

        public void RunFreeCycle()
        {
            PPU_Do_Under_Cycle256();
            PPU_Do_Under_Cycle257();
        }

        /// <summary>
        /// SFCs the ppu do under cycle256
        /// </summary>
        /// <param name="ppu">The ppu.</param>
        private void PPU_Do_Under_Cycle256()
        {
            // http://wiki.nesdev.com/w/index.php/PPU_scrolling#Wrapping_around

            if ((this.v & 0x7000) != 0x7000)
            {
                this.v += 0x1000;
            }
            else
            {
                this.v &= 0x8FFF;
                UInt16 y = (ushort)((v & 0x03E0) >> 5);
                if (y == 29)
                {
                    y = 0;
                    this.v ^= 0x0800;
                }
                else if (y == 31)
                {
                    y = 0;
                }
                else
                {
                    y++;
                }
                // put coarse Y back into v
                v = (ushort)((v & 0xFC1F) | (y << 5));
            }
        }

        /// <summary>
        /// SFCs the ppu do under cycle257.
        /// </summary>
        /// <param name="ppu">The ppu.</param>
        private void PPU_Do_Under_Cycle257()
        {
            // v: .....F.. ...EDCBA = t: .....F.. ...EDCBA
            v = (ushort)((v & 0xFBE0) | (t & 0x041F));
        }


        /// <summary>
        /// SFCs the ppu do end of vblank.
        /// </summary>
        /// <param name="ppu">The ppu.</param>
        public void PPU_Do_End_Of_Vblank()
        {
            // v: .....F.. ...EDCBA = t: .....F.. ...EDCBA
            v = (ushort)((v & 0x841F) | (t & 0x7BE0));
        }
        public bool CheckRenderBackground()
        {
            if ((m_mask & (byte)Fc_ppu_flag.FC_PPU2001_Back) > 0)
                return true;
            return false;
        }
        public bool CheckRenderSprite()
        {
            if ((m_mask & (byte)Fc_ppu_flag.FC_PPU2001_Sprite) > 0)
                return true;
            return false;
        }
    }
}
