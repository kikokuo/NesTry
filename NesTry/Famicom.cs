using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Policy;
using System.Text;
using System.Windows.Input;
using System.Windows.Shapes;

namespace NesTry
{
    /// <summary>
    ///
    /// </summary>
    public struct sfc_square_channel_state_t
    {
        // 方波 頻率
        public float frequency;
        // 方波 音量
        public UInt16 volume;
        // 方波 佔空比
        public UInt16 duty;
    };

    /// <summary>
    ///
    /// </summary>
    public struct sfc_triangle_channel_state_t
    {
        // 三角波 頻率
        public float frequency;
    };


    /// <summary>
    ///
    /// </summary>
    public struct sfc_noise_channel_state_t
    {
        // 噪音 音量
        public UInt16 volume;
        // 噪音 頻率數據
        public UInt16 data;
    };

    /// <summary>
    ///
    /// </summary>
    public struct sfc_channel_state_t
    {
        // 方波#1
        public sfc_square_channel_state_t square1;
        // 方波#2
        public sfc_square_channel_state_t square2;
        // 三角波
        public sfc_triangle_channel_state_t triangle;
        // 噪音
        public sfc_noise_channel_state_t noise;
    };

    public class Famicom
    {
        const int Master_Cycle_Per_CPU = 12;
        public NesRom m_nesrom;
        public RomMapper m_mapper;
        public NesCPU m_cpu;
        public Nes6502 m_nes6502;
        public NesAPU m_apu;
        public NesXAudio2 m_audio;
        public NesConfig NTSC = new NesConfig(1789773.0f,60,1364,1024,340,240,20);
        public NesConfig PAL = new NesConfig(1662607.0f, 50, 1362, 1024, 338, 312, 70);
        // PPU
        public NesPPU  m_ppu;
        public byte [] m_mainMemory;
        public byte [] m_saveMemory;
        public byte [] m_PPUMemory;
        public byte [] m_APUMemory;
        public byte [] m_PRG1Memory;
        public byte [] m_PRG2Memory;
        public byte [] m_PRG3Memory;
        public byte [] m_PRG4Memory;
        public List<byte []>prg_banks;

        // 手柄序列狀態#1
        public UInt16 button_index_1;
        // 手柄序列狀態#2
        public UInt16 button_index_2;
        // 手柄序列狀態
        public UInt16 button_index_mask;
        // 手柄按鈕狀態
        public byte[] button_states = new byte[16];
        // 預設按鍵對映
        public Dictionary<Key, int> button_key_map = new Dictionary<Key, int>();
        //long m_line = 0;
        //public StreamWriter sw;
        public long cpu_cycle_count;

        /// <summary>
        /// The bit reverse table256
        /// </summary>
        public byte []BitReverseTable256 = {
          0x00, 0x80, 0x40, 0xC0, 0x20, 0xA0, 0x60, 0xE0, 0x10, 0x90, 0x50, 0xD0, 0x30, 0xB0, 0x70, 0xF0,
          0x08, 0x88, 0x48, 0xC8, 0x28, 0xA8, 0x68, 0xE8, 0x18, 0x98, 0x58, 0xD8, 0x38, 0xB8, 0x78, 0xF8,
          0x04, 0x84, 0x44, 0xC4, 0x24, 0xA4, 0x64, 0xE4, 0x14, 0x94, 0x54, 0xD4, 0x34, 0xB4, 0x74, 0xF4,
          0x0C, 0x8C, 0x4C, 0xCC, 0x2C, 0xAC, 0x6C, 0xEC, 0x1C, 0x9C, 0x5C, 0xDC, 0x3C, 0xBC, 0x7C, 0xFC,
          0x02, 0x82, 0x42, 0xC2, 0x22, 0xA2, 0x62, 0xE2, 0x12, 0x92, 0x52, 0xD2, 0x32, 0xB2, 0x72, 0xF2,
          0x0A, 0x8A, 0x4A, 0xCA, 0x2A, 0xAA, 0x6A, 0xEA, 0x1A, 0x9A, 0x5A, 0xDA, 0x3A, 0xBA, 0x7A, 0xFA,
          0x06, 0x86, 0x46, 0xC6, 0x26, 0xA6, 0x66, 0xE6, 0x16, 0x96, 0x56, 0xD6, 0x36, 0xB6, 0x76, 0xF6,
          0x0E, 0x8E, 0x4E, 0xCE, 0x2E, 0xAE, 0x6E, 0xEE, 0x1E, 0x9E, 0x5E, 0xDE, 0x3E, 0xBE, 0x7E, 0xFE,
          0x01, 0x81, 0x41, 0xC1, 0x21, 0xA1, 0x61, 0xE1, 0x11, 0x91, 0x51, 0xD1, 0x31, 0xB1, 0x71, 0xF1,
          0x09, 0x89, 0x49, 0xC9, 0x29, 0xA9, 0x69, 0xE9, 0x19, 0x99, 0x59, 0xD9, 0x39, 0xB9, 0x79, 0xF9,
          0x05, 0x85, 0x45, 0xC5, 0x25, 0xA5, 0x65, 0xE5, 0x15, 0x95, 0x55, 0xD5, 0x35, 0xB5, 0x75, 0xF5,
          0x0D, 0x8D, 0x4D, 0xCD, 0x2D, 0xAD, 0x6D, 0xED, 0x1D, 0x9D, 0x5D, 0xDD, 0x3D, 0xBD, 0x7D, 0xFD,
          0x03, 0x83, 0x43, 0xC3, 0x23, 0xA3, 0x63, 0xE3, 0x13, 0x93, 0x53, 0xD3, 0x33, 0xB3, 0x73, 0xF3,
          0x0B, 0x8B, 0x4B, 0xCB, 0x2B, 0xAB, 0x6B, 0xEB, 0x1B, 0x9B, 0x5B, 0xDB, 0x3B, 0xBB, 0x7B, 0xFB,
          0x07, 0x87, 0x47, 0xC7, 0x27, 0xA7, 0x67, 0xE7, 0x17, 0x97, 0x57, 0xD7, 0x37, 0xB7, 0x77, 0xF7,
          0x0F, 0x8F, 0x4F, 0xCF, 0x2F, 0xAF, 0x6F, 0xEF, 0x1F, 0x9F, 0x5F, 0xDF, 0x3F, 0xBF, 0x7F, 0xFF
        };
        ~Famicom()
        {
            //sw.Close();
        }

        public Famicom()
        {
            prg_banks = new List<byte[]>();
            m_mainMemory = new byte[2048];
            prg_banks.Add(m_mainMemory);
            m_PPUMemory = new byte[8192];
            prg_banks.Add(m_PPUMemory);
            m_APUMemory = new byte[8192];
            prg_banks.Add(m_APUMemory);
            m_saveMemory = new byte[8192];
            prg_banks.Add(m_saveMemory);
            m_PRG1Memory = new byte[8192];
            prg_banks.Add(m_PRG1Memory);
            m_PRG2Memory = new byte[8192];
            prg_banks.Add(m_PRG2Memory);
            m_PRG3Memory = new byte[8192];
            prg_banks.Add(m_PRG3Memory);
            m_PRG4Memory = new byte[8192];
            prg_banks.Add(m_PRG4Memory);
            cpu_cycle_count = 0;
            Famicom famicom = this;
            m_ppu = new NesPPU();
            m_cpu = new NesCPU(ref famicom, ref m_ppu);
            m_nes6502 = new Nes6502(ref m_cpu);
            m_apu = new NesAPU(ref m_nes6502);
            m_audio = new NesXAudio2();  
            m_mapper = new RomMapper();

            //key mapping table
            button_key_map.Add(Key.J, 0);
            button_key_map.Add(Key.K, 1);
            button_key_map.Add(Key.U, 2);
            button_key_map.Add(Key.I, 3);
            button_key_map.Add(Key.W, 4);
            button_key_map.Add(Key.S, 5);
            button_key_map.Add(Key.A, 6);
            button_key_map.Add(Key.D, 7);
            button_key_map.Add(Key.NumPad2, 8);
            button_key_map.Add(Key.NumPad3, 9);
            button_key_map.Add(Key.NumPad5, 10);
            button_key_map.Add(Key.NumPad6, 11);
            button_key_map.Add(Key.Up, 12);
            button_key_map.Add(Key.Down, 13);
            button_key_map.Add(Key.Left, 14);
            button_key_map.Add(Key.Right, 15);
            //sw = File.AppendText("nes.log");
        }

        /// <summary>
        /// Users the input.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="data">The data.</param>
        public void User_Input(Key kevvaule, byte data)
        {
            int index;
            if (button_key_map.TryGetValue(kevvaule, out index)) 
                button_states[index] = data;
        }
        
        public bool Reset()
        {
            // 重置mapper
            if (!m_mapper.Reset(ref this.prg_banks,ref m_ppu.m_banks, this.m_nesrom.m_romInfo)) return false;
            if (!m_cpu.Reset()) return false;
            m_ppu.Reset(this.m_nesrom.m_romInfo);
            m_apu.Reset();
            return true;
        }
        public Fc_error_code LoadRom(string romname)
        {
            Fc_error_code err = Fc_error_code.FC_ERROR_OK;
            ClearRom();
            m_nesrom = new NesRom(romname);
            err = m_nesrom.ReadRom();
            if (err != Fc_error_code.FC_ERROR_OK)
                return err;
            m_mapper = null;
            m_mapper = new RomMapper();
            err = m_mapper.LoadMapper(ref m_ppu, ref prg_banks, ref m_ppu.m_banks, m_nesrom.m_romInfo);
            if( err != Fc_error_code.FC_ERROR_OK)
                return err;
            return err;
        }

        public void ClearRom()
        {
            m_ppu.Clear();
            m_nesrom = null;
        }

        public UInt16 ReadCPUAddress(UInt16 address)
        {
            UInt16 v0 = (UInt16)m_cpu.Read_Cpu_Address(address);
            UInt16 v01 = (UInt16)m_cpu.Read_Cpu_Address((UInt16)(address + 1));
            v01 = (ushort)(v01 << 8);
            v0 |= v01;
            return v0;
        }

        public void Disassembly(UInt16 address, StringBuilder builder)
        {
            m_cpu.Fc_Disassembly(address, m_nes6502, builder);
        }
        /// <summary>
        /// SFCs the sprite0 hittest.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <param name="spp">The SPP.</param>
        /// <param name="buffer">The buffer.</param>
        private void Sprite0_Hittest(ref byte []buffer)
        {
            byte BOTH_BS = (byte)(Fc_ppu_flag.FC_PPU2001_Sprite | Fc_ppu_flag.FC_PPU2001_Back);
            if ((m_ppu.m_mask & BOTH_BS) != BOTH_BS) return;

            byte yyyyy = m_ppu.m_sprites[0];
            if (yyyyy >= 0xEF) return;

            byte iiiii = m_ppu.m_sprites[1];
            byte sp8x16 = (byte)(m_ppu.m_ctrl & (byte)Fc_ppu_flag.FC_PPU2000_Sp8x16);
            byte spp_base = (byte)(sp8x16 > 0 ? ((iiiii & 1) > 0 ? 4 : 0) :((m_ppu.m_ctrl & (byte)Fc_ppu_flag.FC_PPU2000_SpTabl) > 0? 4 : 0));
            byte aaaaa = m_ppu.m_sprites[2];

            UInt32 nowp0 = (uint)(iiiii * 16);
            byte ind = (byte)(nowp0 / 1024);
            nowp0 = nowp0 % 1024;
            UInt32 nowp1 = nowp0 + 8;
            UInt16 buffer_write = (UInt16)(yyyyy + 1);
            // 8x16的情况
            UInt16 count = (UInt16)(sp8x16 > 0? 16 : 8);
            spp_base += (byte)ind;
            if ((aaaaa & (byte)Fc_ppu_flag.FC_SPATTR_FlipH) > 0)
            {
                for (int i = 0; i != count; ++i)
                {
                    UInt32 new_p0 = (uint)(nowp0 + i);
                    byte ind_1 = (byte)(new_p0 / 1024);
                    new_p0 = new_p0 % 1024;
                    UInt32 new_p1 = (uint)(nowp1 + i);
                    byte ind_2 = (byte)(new_p1 / 1024);
                    new_p1 = new_p1 % 1024;
                    byte data = (byte)(m_ppu.m_banks[spp_base + ind_1][new_p0] | m_ppu.m_banks[spp_base + ind_2][nowp1]);
                    buffer[buffer_write+i] = BitReverseTable256[data];
                }
            }
            else
            {
                for (int i = 0; i != count; ++i)
                {
                    UInt32 new_p0 = (uint)(nowp0 + i);
                    byte ind_1 = (byte)(new_p0 / 1024);
                    new_p0 = new_p0 % 1024;
                    UInt32 new_p1 = (uint)(nowp1 + i);
                    byte ind_2 = (byte)(new_p1 / 1024);
                    new_p1 = new_p1 % 1024;

                    byte data = (byte)(m_ppu.m_banks[spp_base+ ind_1][new_p0] | m_ppu.m_banks[spp_base+ ind_2][new_p1]);
                    buffer[buffer_write + i] = data;
                }
            }
            if ((aaaaa & (byte)Fc_ppu_flag.FC_SPATTR_FlipV) > 0)
            {
                // 8x16
                if (sp8x16 > 0)
                {
                    sfc_swap_byte(ref buffer[buffer_write + 0], ref buffer[buffer_write + 0xF]);
                    sfc_swap_byte(ref buffer[buffer_write + 1], ref buffer[buffer_write + 0xE]);
                    sfc_swap_byte(ref buffer[buffer_write + 2], ref buffer[buffer_write + 0xD]);
                    sfc_swap_byte(ref buffer[buffer_write + 3], ref buffer[buffer_write + 0xC]);
                    sfc_swap_byte(ref buffer[buffer_write + 4], ref buffer[buffer_write + 0xB]);
                    sfc_swap_byte(ref buffer[buffer_write + 5], ref buffer[buffer_write + 0xA]);
                    sfc_swap_byte(ref buffer[buffer_write + 6], ref buffer[buffer_write + 0x9]);
                    sfc_swap_byte(ref buffer[buffer_write + 7], ref buffer[buffer_write + 0x8]);
                }
                else
                {
                    sfc_swap_byte(ref buffer[buffer_write + 0], ref buffer[buffer_write + 7]);
                    sfc_swap_byte(ref buffer[buffer_write + 1], ref buffer[buffer_write + 6]);
                    sfc_swap_byte(ref buffer[buffer_write + 2], ref buffer[buffer_write + 5]);
                    sfc_swap_byte(ref buffer[buffer_write + 3], ref buffer[buffer_write + 4]);
                }
            }
        }
        // swap byte
        public void sfc_swap_byte(ref byte a, ref byte b)
        {
           byte temp = a; a = b; b = temp;
        }

        public byte Pack_bool8_Into_Byte(byte[] source, int sourceindex) 
        {
            byte hittest = 0;
            for (byte i = 0; i != 8; ++i) {
                hittest <<= 1;
                hittest |= (byte)(source[i+ sourceindex] & 1);
            }
            return hittest;
        }

        /// <summary>
        /// StepFC: 简易模式渲染背景 - 以16像素为单位
        /// </summary>
        /// <param name="high">The high.</param>
        /// <param name="plane_left">The plane left.</param>
        /// <param name="plane_right">The plane right.</param>
        /// <param name="aligned_palette">The aligned palette.</param>
        public void Render_Background_Ppixel16(byte high, byte plane0,
            byte plane1, byte plane2, byte plane3, ref byte [] aligned_palette,int index) {

            //# ifdef SFC_NO_SSE
            Expand_backgorund_8(plane0, plane1, high,ref aligned_palette, index + 0);
            Expand_backgorund_8(plane2, plane3, high,ref aligned_palette, index + 8);
        //#else
                //sfc_expand_backgorund_16(plane0, plane1, plane2, plane3, high, aligned_palette);
        //#endif
        }
        /// <summary>
        /// SFCs the expand backgorund 8.
        /// </summary>
        /// <param name="p0">The p0.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="high">The high.</param>
        /// <param name="output">The output.</param>
        public void Expand_backgorund_8(byte p0, byte p1, byte high,ref byte[] output,int index)
        {
            // 0 - D7
            {
                byte low0a = (byte)(p0 &0x80);
                byte low0b = (byte)(p1 &0x80);
                output[index+0] = (byte)(high | low0a >> 6 | low0b >> 5 | low0a >> 7 | low0b >> 7);
            }
            // 1 - D6
            {
                byte low1a = (byte)(p0 & 0x40);
                byte low1b = (byte)(p1 & 0x40);
                output[index+1] = (byte)(high | low1a >> 5 | low1b >> 4 | low1a >> 6 | low1b >> 6);
            }
            // 2 - D5
            {
                byte low2a = (byte)(p0 & 0x20);
                byte low2b = (byte)(p1 & 0x20);
                output[index+2] = (byte)(high | low2a >> 4 | low2b >> 3 | low2a >> 5 | low2b >> 5);
            }
            // 3 - D4
            {
                byte low3a = (byte)(p0 & 0x10);
                byte low3b = (byte)(p1 & 0x10);
                output[index+3] = (byte)(high | low3a >> 3 | low3b >> 2 | low3a >> 4 | low3b >> 4);
            }
            // 4 - D3
            {
                byte low4a = (byte)(p0 & 0x08);
                byte low4b = (byte)(p1 & 0x08);
                output[index+4] = (byte)(high | low4a >> 2 | low4b >> 1 | low4a >> 3 | low4b >> 3);
            }
            // 5 - D2
            {
                byte low5a = (byte)(p0 &0x04);
                byte low5b = (byte)(p1 &0x04);
                output[index+5] = (byte)(high | low5a >> 1 | low5b >> 0 | low5a >> 2 | low5b >> 2);
            }
            // 6 - D1
            {
                byte low6a = (byte)(p0 & 0x02);
                byte low6b = (byte)(p1 & 0x02);
                output[index+6] = (byte)(high | low6a >> 0 | low6b << 1 | low6a >> 1 | low6b >> 1);
            }
            // 7 - D0
            {
                byte low7a = (byte)(p0 & 0x01);
                byte low7b = (byte)(p1 & 0x01);
                output[index+7] = (byte)(high | low7a << 1 | low7b << 2 | low7a >> 0 | low7b >> 0);
            }
        }
        /// <summary>
        /// SFCs the render background scanline.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <param name="line">The line.</param>
        /// <param name="spp">The SPP.</param>
        /// <param name="buffer">The buffer.</param>
        private void Render_Background_Scanline(UInt16 line,byte[] sp0,ref byte [] buffer,int index_buffer) 
        {
            if (!((m_ppu.m_mask & (byte)Fc_ppu_flag.FC_PPU2001_Back) > 0)) return;

            // 計算當前水平偏移量
            UInt16 scrollx = (ushort)(m_ppu.v &0x0400);
            if (scrollx > 0)
                scrollx = 256;
            scrollx +=(ushort)(((m_ppu.v & 0x1f) << 3) + m_ppu.x);
            // 計算當前垂直偏移量
            UInt16 scrolly = (ushort)(m_ppu.v & 0x0800);
            if (scrolly > 0)
                scrolly = 240;
            scrolly += (ushort)((((m_ppu.v >> 5) & 0x1f) << 3)+ (m_ppu.v >> 12));


            UInt16 scrolly_index0 = (UInt16)(scrolly / 240);
            UInt16 scrolly_offset = (UInt16)(scrolly % 240);

            byte pattern = (byte)((m_ppu.m_ctrl & (byte)Fc_ppu_flag.FC_PPU2000_BgTabl) > 0? 4 : 0);

           
            UInt16 first_buck = (UInt16)(8 + ((scrolly_index0 & 1) << 1));
            UInt16 table_0 = first_buck;
            UInt16 table_1 = (UInt16)(first_buck + 1);

            byte [] aligned_buffer = new byte[256 + 16 + 16];
            
            byte realy = (byte)scrolly_offset;

            for (UInt16 i = 0; i != 17; ++i)
            {
                UInt16 realx = (UInt16)(scrollx + (i << 4));
                UInt16 nt = ((realx >> 8) & 1) > 0 ? table_1 : table_0;
                byte xunit = (byte)((realx & 0xF0) >> 4);

                byte attr = m_ppu.m_banks[nt][32 * 30+(realy >> 5 << 3) | (xunit >> 1)];

                byte aoffset = (byte)(((xunit & 1) << 1) | ((realy & 0x10) >> 2));

                byte high = (byte)((attr & (3 << aoffset)) >> aoffset << 3);

                byte too_young0 = m_ppu.m_banks[nt][(xunit << 1) + (realy >> 3 << 5)+0];
                byte too_young1 = m_ppu.m_banks[nt][(xunit << 1) + (realy >> 3 << 5)+1];
                UInt32 nowp0 = (uint)((too_young0&0x3f) * 16 + (realy & 7));
                byte ind_0 = (byte)(too_young0>>6);
                UInt32 nowp1 = (uint)((too_young1 & 0x3f) * 16 + (realy & 7));
                byte ind_1 = (byte)(too_young1>>6);
                byte plane0 = m_ppu.m_banks[pattern + ind_0][nowp0 + 0];
                byte plane1 = m_ppu.m_banks[pattern + ind_0][nowp0 + 8];
                byte plane2 = m_ppu.m_banks[pattern + ind_1][nowp1 + 0];
                byte plane3 = m_ppu.m_banks[pattern + ind_1][nowp1 + 8];
                // 渲染16个像素
                Render_Background_Ppixel16(high, plane0, plane1, plane2, plane3,ref aligned_buffer,(i << 4));
            }
            // 将数据复制过去
            byte index_x = (byte)(scrollx & 0x0f);
            Array.Copy(aligned_buffer, index_x, buffer, index_buffer, 256);
            
            if ((m_ppu.m_status & (byte)Fc_ppu_flag.FC_PPU2002_Sp0Hit)> 0)  return;

            byte hittest_data = sp0[line];
            if (!(hittest_data>0)) return;

            index_x = (byte)(scrollx & 0x0f);
            Array.Clear(aligned_buffer, index_x + 256,16);

            byte xxxxx = m_ppu.m_sprites[3];
            byte hittest = Pack_bool8_Into_Byte(aligned_buffer,index_x + xxxxx);
            if ((hittest_data & hittest) > 0)
                m_ppu.m_status |= (byte)Fc_ppu_flag.FC_PPU2002_Sp0Hit;
        }

        /// <summary>
        /// SFCs the sprite expand 8.
        /// </summary>
        /// <param name="p0">The p0.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="high">The high.</param>
        /// <param name="output">The output.</param>
        public void Sprite_Expand_8_On(byte p0, byte p1, byte high, ref byte [] output, int index)
        {
            // 0 - D7
            byte low0 = (byte)(((p0 & 0x80) >> 6) | ((p1 & 0x80) >> 5));
            if (low0 > 0) output[0 + index] = (byte)(high | low0);
            // 1 - D6
            byte low1 = (byte)(((p0 & 0x40) >> 5) | ((p1 & 0x40) >> 4));
            if (low1 > 0) output[1 + index] = (byte)(high | low1);
            // 2 - D5
            byte low2 = (byte)(((p0 & 0x20) >> 4) | ((p1 & 0x20) >> 3));
            if (low2 > 0) output[2 + index] = (byte)(high | low2);
            // 3 - D4
            byte low3 = (byte)(((p0 & 0x10) >> 3) | ((p1 & 0x10) >> 2));
            if (low3 > 0) output[3 + index] = (byte)(high | low3);
            // 4 - D3
            byte low4 = (byte)(((p0 & 0x08) >> 2) | ((p1 & 0x08) >> 1));
            if (low4 > 0) output[4 + index] = (byte)(high | low4);
            // 5 - D2
            byte low5 = (byte)(((p0 & 0x04) >> 1) | ((p1 & 0x04) >> 0));
            if (low5 > 0) output[5 + index] = (byte)(high | low5);
            // 6 - D1
            byte low6 = (byte)(((p0 & 0x02) >> 0) | ((p1 & 0x02) << 1));
            if (low6 > 0) output[6 + index] = (byte)(high | low6);
            // 7 - D0
            byte low7 = (byte)(((p0 & 0x01) << 1) | ((p1 & 0x01) << 2));
            if (low7 > 0) output[7 + index] = (byte)(high | low7);
        }

        /// <summary>
        /// SFCs the sprite expand 8.
        /// </summary>
        /// <param name="p0">The p0.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="high">The high.</param>
        /// <param name="output">The output.</param>
        public void Sprite_Expand_8_Op(byte p0, byte p1, byte high, ref byte[] output, int index)
        {
            // 0 - D7
            byte low0 = (byte)(((p0 & 0x80) >> 6) | ((p1 & 0x80) >> 5));
            if ((~output[0 + index] & 1) > 0 && low0 > 0) output[0 + index] = (byte)(high | low0 | 1);
            // 1 - D6
            byte low1 = (byte)(((p0 & 0x40) >> 5) | ((p1 & 0x40) >> 4));
            if ((~output[1 + index] & 1) > 0 && low1 > 0) output[1 + index] = (byte)(high | low1 | 1);
            // 2 - D5
            byte low2 = (byte)(((p0 & 0x20) >> 4) | ((p1 & 0x20) >> 3));
            if ((~output[2 + index] & 1) > 0 && low2 > 0) output[2 + index] = (byte)(high | low2 | 1);
            // 3 - D4
            byte low3 = (byte)(((p0 & 0x10) >> 3) | ((p1 & 0x10) >> 2));
            if ((~output[3 + index] & 1) > 0 && low3 > 0) output[3 + index] = (byte)(high | low3 | 1);
            // 4 - D3
            byte low4 = (byte)(((p0 & 0x08) >> 2) | ((p1 & 0x08) >> 1));
            if ((~output[4 + index] & 1) > 0 && low4 > 0) output[4 + index] = (byte)(high | low4 | 1);
            // 5 - D2
            byte low5 = (byte)(((p0 & 0x04) >> 1) | ((p1 & 0x04) >> 0));
            if ((~output[5 + index] & 1) > 0 && low5 > 0) output[5 + index] = (byte)(high | low5 | 1);
            // 6 - D1
            byte low6 = (byte)(((p0 & 0x02) >> 0) | ((p1 & 0x02) << 1));
            if ((~output[6 + index] & 1) > 0 && low6 > 0) output[6 + index] = (byte)(high | low6 | 1);
            // 7 - D0
            byte low7 = (byte)(((p0 & 0x01) << 1) | ((p1 & 0x01) << 2));
            if ((~output[7 + index] & 1) > 0 && low7 > 0) output[7 + index] = (byte)(high | low7 | 1);
        }

        /// <summary>
        /// SFCs the sprite expand 8.
        /// </summary>
        /// <param name="p0">The p0.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="high">The high.</param>
        /// <param name="output">The output.</param>
        public void Sprite_Expand_8_Rn(byte p0, byte p1, byte high, ref byte[] output, int index)
        {
            // 0 - D7
            byte low0 = (byte)(((p0 & 0x80) >> 6) | ((p1 & 0x80) >> 5));
            if (low0 > 0) output[7 + index] = (byte)(high | low0);
            // 1 - D6
            byte low1 = (byte)(((p0 & 0x40) >> 5) | ((p1 & 0x40) >> 4));
            if (low1 > 0) output[6 + index] = (byte)(high | low1);
            // 2 - D5
            byte low2 = (byte)(((p0 & 0x20) >> 4) | ((p1 & 0x20) >> 3));
            if (low2 > 0) output[5 + index] = (byte)(high | low2);
            // 3 - D4
            byte low3 = (byte)(((p0 & 0x10) >> 3) | ((p1 & 0x10) >> 2));
            if (low3 > 0) output[4 + index] = (byte)(high | low3);
            // 4 - D3
            byte low4 = (byte)(((p0 & 0x08) >> 2) | ((p1 & 0x08) >> 1));
            if (low4 > 0) output[3 + index] = (byte)(high | low4);
            // 5 - D2
            byte low5 = (byte)(((p0 & 0x04) >> 1) | ((p1 & 0x04) >> 0));
            if (low5 > 0) output[2 + index] = (byte)(high | low5);
            // 6 - D1
            byte low6 = (byte)(((p0 & 0x02) >> 0) | ((p1 & 0x02) << 1));
            if (low6 > 0) output[1 + index] = (byte)(high | low6);
            // 7 - D0
            byte low7 = (byte)(((p0 & 0x01) << 1) | ((p1 & 0x01) << 2));
            if (low7 > 0) output[0 + index] = (byte)(high | low7);
        }

        /// <summary>
        /// SFCs the sprite expand 8.
        /// </summary>
        /// <param name="p0">The p0.</param>
        /// <param name="p1">The p1.</param>
        /// <param name="high">The high.</param>
        /// <param name="output">The output.</param>
        public void Sprite_Expand_8_Rp(byte p0, byte p1, byte high, ref byte[] output, int index)
        {
            // 0 - D7
            byte low0 = (byte)(((p0 & 0x80) >> 6) | ((p1 & 0x80) >> 5));
            if ((~output[7 + index] & 1) > 0 && low0 > 0) output[7 + index] = (byte)(high | low0 | 1);
            // 1 - D6
            byte low1 = (byte)(((p0 & 0x40) >> 5) | ((p1 & 0x40) >> 4));
            if ((~output[6 + index] & 1) > 0 && low1 > 0) output[6 + index] = (byte)(high | low1 | 1);
            // 2 - D5
            byte low2 = (byte)(((p0 & 0x20) >> 4) | ((p1 & 0x20) >> 3));
            if ((~output[5 + index] & 1) > 0 && low2 > 0) output[5 + index] = (byte)(high | low2 | 1);
            // 3 - D4
            byte low3 = (byte)(((p0 & 0x10) >> 3) | ((p1 & 0x10) >> 2));
            if ((~output[4 + index] & 1) > 0 && low3 > 0) output[4 + index] = (byte)(high | low3 | 1);
            // 4 - D3
            byte low4 = (byte)(((p0 & 0x08) >> 2) | ((p1 & 0x08) >> 1));
            if ((~output[3 + index] & 1) > 0 && low4 > 0) output[3 + index] = (byte)(high | low4 | 1);
            // 5 - D2
            byte low5 = (byte)(((p0 & 0x04) >> 1) | ((p1 & 0x04) >> 0));
            if ((~output[2 + index] & 1) > 0 && low5 > 0) output[2 + index] = (byte)(high | low5 | 1);
            // 6 - D1
            byte low6 = (byte)(((p0 & 0x02) >> 0) | ((p1 & 0x02) << 1));
            if ((~output[1 + index] & 1) > 0 && low6 > 0) output[1 + index] = (byte)(high | low6 | 1);
            // 7 - D0
            byte low7 = (byte)(((p0 & 0x01) << 1) | ((p1 & 0x01) << 2));
            if ((~output[0 + index] & 1) > 0 && low7 > 0) output[0 + index] = (byte)(high | low7 | 1);
        }

        /// <summary>
        /// SFCs the render sprites.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <param name="buffer">The buffer.</param>
        public void Render_Sprites(ref byte [] buffer)
        {
            byte sp8x16 = (byte)((m_ppu.m_ctrl & (byte)Fc_ppu_flag.FC_PPU2000_Sp8x16) >> 2);

            byte sppbuffer_0 = (byte)(sp8x16 > 0 ? 0 : ((m_ppu.m_ctrl & (byte)Fc_ppu_flag.FC_PPU2000_SpTabl) > 0 ? 4 : 0));
            byte sppbuffer_1 = (byte)(sp8x16 > 0 ? 4 : sppbuffer_0);
            byte sppbuffer_2 = (byte)(sp8x16 > 0 ? 0 : 16);

            byte[] bank_ind = new byte[8];

            bank_ind[0] = sppbuffer_0;
            bank_ind[1] = (byte)(sppbuffer_0 + 1);
            bank_ind[2] = (byte)(sppbuffer_0 + 2);
            bank_ind[3] = (byte)(sppbuffer_0 + 3);
            bank_ind[4] = sppbuffer_1;
            bank_ind[5] = (byte)(sppbuffer_1 + 1);
            bank_ind[6] = (byte)(sppbuffer_1 + 2);
            bank_ind[7] = (byte)(sppbuffer_1 + 3);

            for (int index = 0; index != 64; ++index)
            {
                byte sprints_base = (byte)((64 - 1 - index) * 4);

                byte yyyy = m_ppu.m_sprites[sprints_base + 0];
                if (yyyy >= (byte)0xEF) continue;
                byte iiii = m_ppu.m_sprites[sprints_base + 1];
                byte aaaa = m_ppu.m_sprites[sprints_base + 2];
                byte xxxx = m_ppu.m_sprites[sprints_base + 3];
                byte high = (byte)(((aaaa & 3) | 4) << 3);

                UInt16 nowp0_ind = bank_ind[(iiii>>6)|(iiii&0x1)<<2];
                UInt32 nowp0 = (UInt32)((iiii & 0x3E) * 16);
                if (nowp0_ind >= 4)
                    nowp0 += sppbuffer_2;

                UInt32 nowp1 = (UInt32)(nowp0 + 8);
                int write = (int)(xxxx + (yyyy + 1) * (int)config_constant.NES_WIDTH);
                // hVHP
                switch (((byte)(aaaa >> 5) | sp8x16) & (byte)0x0f)
                {
                    case 0x8:   
                         for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_On(m_ppu.m_banks[nowp0_ind][nowp0+j + 16], m_ppu.m_banks[nowp0_ind][nowp1+j + 16], high, ref buffer,write + (int)config_constant.NES_WIDTH * (j + 8));
                         goto case 0x0;
                    case 0x0:
                        for (int j = 0; j != 8; ++j)
                           Sprite_Expand_8_On(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * j);
                        break;
                    case 0x9:   
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Op(m_ppu.m_banks[nowp0_ind][nowp0 + j+16], m_ppu.m_banks[nowp0_ind][nowp1 + j+16], high, ref buffer, write + (int)config_constant.NES_WIDTH * (j + 8));
                        goto case 0x1;
                    case 0x1:
                        // 0001: 后
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Op(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * j);
                        break;
                    case 0xA:    
                        // 1010: 8x16 水平翻转 前 
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Rn(m_ppu.m_banks[nowp0_ind][nowp0 + j + 16], m_ppu.m_banks[nowp0_ind][nowp1 + j + 16], high, ref buffer, write + (int)config_constant.NES_WIDTH * (j + 8));
                        goto case 0x2;
                    case 0x2: 
                        // 0010: 水平翻转 前 
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Rn(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * j);
                        break;
                    case 0xB:    
                        // 1011: 8x16 水平翻转 后
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Rp(m_ppu.m_banks[nowp0_ind][nowp0 + j + 16], m_ppu.m_banks[nowp0_ind][nowp1 + j + 16], high, ref buffer, write + (int)config_constant.NES_WIDTH * (j + 8));
                        goto case 0x3;
                    case 0x3:  
                        // 0011: 水平翻转 后
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Rp(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * j);
                        break;
                    case 0xC:
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_On(m_ppu.m_banks[nowp0_ind][nowp0 + j + 16], m_ppu.m_banks[nowp0_ind][nowp1 + j + 16], high, ref buffer, write + (int)config_constant.NES_WIDTH * (7 - j));
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_On(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * (15 - j));
                        break;
                    case 0x4:
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_On(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * (7 - j));
                        break;
                    case 0xD:
                        // 1101: 8x16 垂直翻转 后
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Op(m_ppu.m_banks[nowp0_ind][nowp0 + j + 16], m_ppu.m_banks[nowp0_ind][nowp1 + j + 16], high, ref buffer, write + (int)config_constant.NES_WIDTH * (7 - j));
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Op(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * (15 - j));
                        break;
                    case 0x5:
                        // 0101: 垂直翻转 后
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Op(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * (7 - j));
                        break;
                    case 0xE:
                        // 1110: 8x16 垂直翻转 水平翻转 前 
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Rn(m_ppu.m_banks[nowp0_ind][nowp0 + j + 16], m_ppu.m_banks[nowp0_ind][nowp1 + j + 16], high, ref buffer, write + (int)config_constant.NES_WIDTH * (7 - j));
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Rn(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * (15 - j));
                        break;
                    case 0x6:
                        // 0110: 8x16 垂直翻转 水平翻转 前 
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Rn(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * (7 - j));
                        break;
                    case 0xF:
                        // 1111: 8x16 垂直翻转 水平翻转 后
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Rp(m_ppu.m_banks[nowp0_ind][nowp0 + j + 16], m_ppu.m_banks[nowp0_ind][nowp1 + j + 16], high, ref buffer, write + (int)config_constant.NES_WIDTH * (7 - j));
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Rp(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * (15 - j));
                        break;
                    case 0x7:
                        // 0111: 垂直翻转 水平翻转 后
                        for (int j = 0; j != 8; ++j)
                            Sprite_Expand_8_Rp(m_ppu.m_banks[nowp0_ind][nowp0 + j], m_ppu.m_banks[nowp0_ind][nowp1 + j], high, ref buffer, write + (int)config_constant.NES_WIDTH * (7 - j));
                        break;
                }
            }
        }

        public void render_frame_easy(ref byte[] buffer)
        {
            UInt16 vblank_line = NTSC.vblank_scanline;
            UInt32 per_scanline = NTSC.master_cycle_per_scanline;
            UInt32 end_cycle_count = 0;

            //hit test
            byte []sp0_hittest_buffer = new byte[(int)config_constant.NES_WIDTH];
            Sprite0_Hittest(ref sp0_hittest_buffer);

            UInt16 overflow_line = Sprite_Overflow_test();
            if (!((m_ppu.m_mask & (byte)Fc_ppu_flag.FC_PPU2001_Back)> 0))
                Array.Clear(buffer, 0, (int)config_constant.NES_WIDTH * (int)config_constant.NES_HEIGHT);


            int index_buffer = 0;
            for (UInt16 i = 0; i != (int)config_constant.NES_HEIGHT; ++i)
            {
                end_cycle_count += per_scanline;
                UInt32 end_cycle_count_this_round = end_cycle_count / Master_Cycle_Per_CPU;


                Render_Background_Scanline(i, sp0_hittest_buffer,ref buffer, index_buffer);

                if (i == overflow_line)
                    m_ppu.m_status |= (byte)Fc_ppu_flag.FC_PPU2002_SpOver;
                
                for (; cpu_cycle_count < end_cycle_count_this_round;)
                    m_nes6502.Fc_cpu_execute_one();

                if (m_ppu.CheckRenderBackground())
                    m_ppu.RunFreeCycle();

                m_mapper.Hsync();
                index_buffer += (int)config_constant.NES_WIDTH;
                if(i% 66 == 65)
                m_apu.sfc_trigger_frame_counter();
            }

            {
                end_cycle_count += per_scanline;
                UInt32 end_cycle_count_this_round = end_cycle_count / Master_Cycle_Per_CPU;
                for (; cpu_cycle_count < end_cycle_count_this_round;)
                    m_nes6502.Fc_cpu_execute_one();

                m_mapper.Hsync();
            }

             m_ppu.m_status |= (byte)Fc_ppu_flag.FC_PPU2002_VBlank;
            if ((m_ppu.m_ctrl & (byte)Fc_ppu_flag.FC_PPU2000_NMIGen) > 0)
                m_nes6502.O_NMI();

            for (UInt16 i = 0; i != vblank_line; ++i)
            {
                end_cycle_count += per_scanline;
                UInt32 end_cycle_count_this_round = end_cycle_count / Master_Cycle_Per_CPU;
                for (; cpu_cycle_count < end_cycle_count_this_round;)
                    m_nes6502.Fc_cpu_execute_one();
            }

            m_ppu.m_status = 0;

            if (m_ppu.CheckRenderBackground())
                m_ppu.PPU_Do_End_Of_Vblank();

            m_apu.sfc_trigger_frame_counter();

            end_cycle_count += per_scanline * 2;

            UInt32 end_cycle_count_last_round = (UInt32)(end_cycle_count / Master_Cycle_Per_CPU) & ~(UInt32)1;
            
            for (; cpu_cycle_count < end_cycle_count_last_round;)
                m_nes6502.Fc_cpu_execute_one();

            cpu_cycle_count -= end_cycle_count_last_round;

            if (m_ppu.CheckRenderSprite())
                Render_Sprites(ref buffer);
        }

        /// <summary>
        /// SFCs the sprite overflow test.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        public  UInt16 Sprite_Overflow_test()
        {

            byte BOTH_BS = (byte)(Fc_ppu_flag.FC_PPU2001_Sprite | Fc_ppu_flag.FC_PPU2001_Back);
            if ((m_ppu.m_mask & BOTH_BS) != BOTH_BS) return (int)config_constant.NES_WIDTH;
            
            byte [] buffer = new byte[256 + 16];
            // 8 x 16
            byte height = (byte)((m_ppu.m_ctrl & (byte)Fc_ppu_flag.FC_PPU2000_Sp8x16) > 0 ? 16 : 8);
            for (int i = 0; i != 64; ++i) {
                byte y = m_ppu.m_sprites[i * 4];
                for (int j = 0; j != height; ++j) buffer[y + j]++;
            }
            // 搜索第一个超过8的
            UInt16 line;
            for (line = 0; line != 240; ++line) if (buffer[line] > 8) break;
            return line;
        }
        /// <summary>
        /// 主渲染
        /// </summary>
        /// <param name="rgba">The RGBA.</param>
        public void main_loop(ref UInt32[] g_data)
        {
            byte[] buffer = new byte[256*256];

            render_frame_easy(ref buffer);
            // 生成調色板顏色
            UInt32[] palette = new UInt32[32]; 
            
            for (int i = 0; i != 32; ++i)
                palette[i] = m_ppu.fc_stdpalette[m_ppu.m_spindexes[i]].data;

            palette[4 * 1] = palette[0];
            palette[4 * 2] = palette[0];
            palette[4 * 3] = palette[0];
            palette[4 * 4] = palette[0];
            palette[4 * 5] = palette[0];
            palette[4 * 6] = palette[0];
            palette[4 * 7] = palette[0];
            
            for (uint i = 0; i != 256 * 240; ++i)
            {
                g_data[i] = palette[buffer[i] >> 1];
            }

            play_audio();
        }
        private void play_audio() 
        {
            sfc_channel_state_t state = default;
            m_apu.sfc_play_audio_easy(ref state);
            m_audio.Play_square1(state.square1.frequency,state.square1.duty,state.square1.volume);
            m_audio.Play_square2(state.square2.frequency, state.square2.duty,state.square2.volume);
            m_audio.Play_triangle(state.triangle.frequency);
            m_audio.Play_noise(state.noise.data,state.noise.volume);
        }
    }
}
