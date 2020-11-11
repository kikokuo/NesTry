using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace NesTry
{
    public enum Fc_cpu_vector
    {
        FC_VERCTOR_NMI = 0xFFFA, // 不可屏蔽中斷
        FC_VERCTOR_RESET = 0xFFFC, // 重置CP指針地址
        FC_VERCTOR_BRK = 0xFFFE, // 中斷重定向
        FC_VERCTOR_IRQ = 0xFFFE, // 中斷重定向
    };
    // 狀態寄存器標記
    public enum Fc_status_index
    {
        FC_INDEX_C = 0,
        FC_INDEX_Z = 1,
        FC_INDEX_I = 2,
        FC_INDEX_D = 3,
        FC_INDEX_B = 4,
        FC_INDEX_R = 5,
        FC_INDEX_V = 6,
        FC_INDEX_S = 7,
        FC_INDEX_N = FC_INDEX_S,
    };

    // 狀態寄存器標記
    public enum Fc_status_flag
    {
        FC_FLAG_C = 1 << 0, // 進位標記(Carry flag)
        FC_FLAG_Z = 1 << 1, // 零標記 (Zero flag)
        FC_FLAG_I = 1 << 2, // 禁止中斷(Irq disabled flag)
        FC_FLAG_D = 1 << 3, // 十進制模式(Decimal mode flag)
        FC_FLAG_B = 1 << 4, // 軟件中斷(BRK flag)
        FC_FLAG_R = 1 << 5, // 保留標記(Reserved), 一直為1
        FC_FLAG_V = 1 << 6, // 溢出標記(Overflow flag)
        FC_FLAG_S = 1 << 7, // 符號標記(Sign flag)
        FC_FLAG_N = FC_FLAG_S,// 又叫(Negative Flag)
    };

    // CPU寄存器
    public struct Fc_cpu_register_t
    {
        // 指令計數器 Program Counter
        public UInt16 program_counter;
        // 狀態寄存器 Status Register
        public byte status;
        // 累加寄存器 Accumulator
        public byte accumulator;
        // X 變址寄存器 X Index Register
        public byte x_index;
        // Y 變址寄存器 Y Index Register
        public byte y_index;
        // 棧指針 Stack Pointer
        public byte stack_pointer;
        // 保留對齊用
        public byte unused;
    };
    public class NesCPU
    {
        const int OFFSET_M = 16;
        const int OFFSET = 8;
        //CPU寄存器
        public Fc_cpu_register_t Registers;
        public Famicom m_famicom;
        public NesPPU m_ppu;
        public NesCPU(ref Famicom famicom,ref NesPPU ppu)
        {
            Registers = new Fc_cpu_register_t();
            this.m_famicom = famicom;
            this.m_ppu = ppu;
        }

        public bool Reset()
        {
            // 初始化寄存器
            byte pcl = (byte)Read_Cpu_Address((UInt16)Fc_cpu_vector.FC_VERCTOR_RESET);
            byte pch = (byte)Read_Cpu_Address((UInt16)Fc_cpu_vector.FC_VERCTOR_RESET+1);
            Registers.program_counter = (UInt16)(pcl | pch << 8);
            Registers.accumulator = 0;
            Registers.x_index = 0;
            Registers.y_index = 0;
            Registers.stack_pointer = 0xfd;
            Registers.status = 0x34 | (UInt16)Fc_status_flag.FC_FLAG_R;   // 一直為1

            return true;
        }
        /// <summary>
        /// StepFC: 指定地方反彙編
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        /// <param name="buf">The buf.</param>
        public void Fc_Disassembly(UInt16 address, Nes6502 nes6502, StringBuilder buf) 
        {
            // 根據操作碼讀取對應字節
            buf[0] = '$';
            nes6502.Fc_BtoH(buf,1, (byte)(address >> 8));
            nes6502.Fc_BtoH(buf,3, (byte)(address));

            Fc_6502_code_t code = new Fc_6502_code_t();
            //code.data = 0;
            // 暴力(NoMo)讀取3字節
            code.op = (byte)Read_Cpu_Address(address);
            code.a1 = (byte)Read_Cpu_Address((UInt16)(address + 1));
            code.a2 = (byte)Read_Cpu_Address((UInt16)(address + 2));
            // 反彙編
            nes6502.Fc_6502_disassembly(code, buf, OFFSET);
        }

        /// <summary>
        /// SFCs the read apu status.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        public byte Read_apu_status()
        {
            byte state = 0;
            // IF-D NT21 DMC interrupt (I), frame interrupt (F), DMC active (D), length counter > 0 (N/T/2/1)
            if (m_famicom.m_apu.square1.length_counter > 0)
                state |= (byte)Fc_4015_read_flag.FC_APU4015_READ_Square1Length;
            if (m_famicom.m_apu.square2.length_counter > 0)
                state |= (byte)Fc_4015_read_flag.FC_APU4015_READ_Square2Length;
            if (m_famicom.m_apu.triangle.length_counter > 0)
                state |= (byte)Fc_4015_read_flag.FC_APU4015_READ_TriangleLength;
            if (m_famicom.m_apu.noise.length_counter > 0)
                state |= (byte)Fc_4015_read_flag.FC_APU4015_READ_NoiseLength;
            // TODO: DMC

            if (m_famicom.m_apu.frame_interrupt > 0)
                state |= (byte)Fc_4015_read_flag.FC_APU4015_READ_Frameinterrupt;

            // 清除中斷標記
            m_famicom.m_apu.frame_interrupt = 0;

            // TODO: DMC
            return state;
        }
        // <summary>
        /// StepFC: 讀取CPU地址數據4020
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        public byte Read_cpu_address4020(UInt16 address)
        {
            byte data = 0;
            switch (address & (UInt16)0x1f)
            {
                case 0x15:
                    data = Read_apu_status();
                    break;
                case 0x16:
                    // 手柄端口#1
                    data = m_famicom.button_states[m_famicom.button_index_1 & m_famicom.button_index_mask];
                    ++m_famicom.button_index_1;
                    break;
                case 0x17:
                    // 手柄端口#2
                    data = m_famicom.button_states[8 + (m_famicom.button_index_2 & m_famicom.button_index_mask)];
                    ++m_famicom.button_index_2;
                    break;
            }
            return data;
        }
        // <summary>
        /// StepFC: 獲取DMA地址
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public void Get_Dma_Address(byte data,ref byte[] refarray) 
        {
            UInt16 offset = (UInt16)((data & 0x07) << 8);
            switch (data >> 5)
            {
                case 1:
                    // PPU寄存器
                    Debug.Assert(true,"PPU REG!");
                    break;
                case 2:
                    // 擴展區
                    Debug.Assert(true,"TODO");
                    break;
                case 0:
                    // 系統內存
                    Array.Copy(m_famicom.m_mainMemory, offset, refarray, 0, 256);
                    break;
                case 3:
                    // 存檔 SRAM區
                    Array.Copy(m_famicom.m_saveMemory, offset, refarray, 0, 256);
                    break;
                case 4: case 5: case 6: case 7:
                    // 高一位為1, [$8000, $10000) 程序PRG-ROM區
                    Array.Copy(m_famicom.prg_banks[data >> 5], offset, refarray, 0, 256);
                    break;
            }
        }
        /// <summary>
        /// StepFC: 寫入CPU地址數據4020
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="data">The data.</param>
        /// <param name="famicom">The famicom.</param>
        public void Write_cpu_address4020(UInt16 address, byte data)
        {
            switch (address & (UInt16)0x1f)
            {
                case 0x00:
                    // $4000 DDLC NNNN 
                    m_famicom.m_apu.square1.ctrl = data;
                    m_famicom.m_apu.square1.envelope.ctrl6 = (byte)(data & 0x3F);
                    break;
                case 0x01:
                    // $4001 EPPP NSSS
                    m_famicom.m_apu.square1.sweep_enable = (byte)(data &0x80);
                    m_famicom.m_apu.square1.sweep_negate = (byte)(data &0x08);
                    m_famicom.m_apu.square1.sweep_period = (byte)((data >> 4) & 0x07);
                    m_famicom.m_apu.square1.sweep_shift = (byte)(data & 0x07);
                    m_famicom.m_apu.square1.sweep_reload = 1;
                    break;
                case 0x02:
                    // $4002 TTTT TTTT
                    m_famicom.m_apu.square1.cur_period
                        = (ushort)((m_famicom.m_apu.square1.cur_period &0xff00)| data);
                    m_famicom.m_apu.square1.use_period = m_famicom.m_apu.square1.cur_period;
                    break;
                case 0x03:
                    // $4003 LLLL LTTT
                    // 禁止狀態不會重置
                    if ((m_famicom.m_apu.status_write & (byte)Fc_4015_write_flag.FC_APU4015_WRITE_EnableSquare1) > 0)
                        m_famicom.m_apu.square1.length_counter = m_famicom.m_apu.LENGTH_COUNTER_TABLE[data >> 3];

                    UInt16 data1 = (UInt16)((data & 0x07) << 8);
                    m_famicom.m_apu.square1.cur_period
                        = (UInt16)((m_famicom.m_apu.square1.cur_period & 0x00ff)| data1);
                    m_famicom.m_apu.square1.use_period = m_famicom.m_apu.square1.cur_period;
                    m_famicom.m_apu.square1.envelope.start = 1;
                    break;
                case 0x04:
                    // $4004 DDLC NNNN 
                    m_famicom.m_apu.square2.ctrl = data;
                    m_famicom.m_apu.square2.envelope.ctrl6 = (byte)(data &0x3F);
                    break;
                case 0x05:
                    // $4005 EPPP NSSS
                    m_famicom.m_apu.square2.sweep_enable = (byte)(data &0x80);
                    m_famicom.m_apu.square2.sweep_negate = (byte)(data &0x08);
                    m_famicom.m_apu.square2.sweep_period = (byte)((data >> 4) &0x07);
                    m_famicom.m_apu.square2.sweep_shift = (byte)(data &0x07);
                    m_famicom.m_apu.square2.sweep_reload = 1;
                    break;
                case 0x06:
                    // $4006 TTTT TTTT
                    m_famicom.m_apu.square2.cur_period
                        = (UInt16)((m_famicom.m_apu.square2.cur_period &0xff00)| data);
                    m_famicom.m_apu.square2.use_period = m_famicom.m_apu.square2.cur_period;
                    break;
                case 0x07:
                    // $4007 LLLL LTTT
                    // 禁止狀態不會重置
                    if ((m_famicom.m_apu.status_write &(byte) Fc_4015_write_flag.FC_APU4015_WRITE_EnableSquare2)> 0) 
                        m_famicom.m_apu.square2.length_counter = m_famicom.m_apu.LENGTH_COUNTER_TABLE[data >> 3];
                    UInt16 data2 = (UInt16)((data & 0x07) << 8);
                    m_famicom.m_apu.square2.cur_period
                        = (UInt16)((m_famicom.m_apu.square2.cur_period & 0x00ff)| data2);
                    m_famicom.m_apu.square2.use_period = m_famicom.m_apu.square2.cur_period;
                    m_famicom.m_apu.square2.envelope.start = 1;
                    break;
                case 0x08:
                    // $4008 CRRR RRRR
                    m_famicom.m_apu.triangle.value_reload = (byte)(data & 0x7F);
                    m_famicom.m_apu.triangle.flag_halt = (byte)(data >> 7);
                    break;
                case 0x0A:
                    // $400A TTTT TTTT
                    m_famicom.m_apu.triangle.cur_period
                        = (UInt16)((m_famicom.m_apu.triangle.cur_period & 0xff00) | data);
                    break;
                case 0x0B:
                    // $400B LLLL LTTT
                    // 禁止狀態不會重置
                    if ((m_famicom.m_apu.status_write & (byte)Fc_4015_write_flag.FC_APU4015_WRITE_EnableTriangle) > 0)
                        m_famicom.m_apu.triangle.length_counter = m_famicom.m_apu.LENGTH_COUNTER_TABLE[data >> 3];
                    UInt16 data3 = (UInt16)((data & 0x07) << 8);
                    m_famicom.m_apu.triangle.cur_period
                        = (UInt16)((m_famicom.m_apu.triangle.cur_period & 0x00ff) | data3);
                    m_famicom.m_apu.triangle.flag_reload = 1;
                    break;
                case 0x0C:
                    // $400C --LC NNNN 
                    m_famicom.m_apu.noise.envelope.ctrl6 = (byte)(data & 0x3F);
                    break;
                case 0x0E:
                    // $400E S--- PPPP
                    m_famicom.m_apu.noise.short_mode__period_index = data;
                    break;
                case 0x0F:
                    // $400E LLLL L---
                    // 禁止狀態不會重置
                    if ((m_famicom.m_apu.status_write & (byte)Fc_4015_write_flag.FC_APU4015_WRITE_EnableNoise) > 0)
                        m_famicom.m_apu.noise.length_counter = m_famicom.m_apu.LENGTH_COUNTER_TABLE[data >> 3];

                    m_famicom.m_apu.noise.envelope.start = 1;
                    break;
                case 0x11:
                    m_famicom.m_apu.dmc.value = (byte)(data &0x7F);
                    break;
                case 0x10:
                case 0x12:
                case 0x13:
                    Debug.Assert(true,"NOT IMPL");
                    break;
                case 0x14:
                    // 精靈RAM直接儲存器訪問
                    if (m_ppu.m_oamaddr > 0) {
                        byte[] src = new byte[256];
                        Get_Dma_Address(data, ref src);
                        byte len = m_ppu.m_oamaddr;

                        Array.Copy(src, len, m_ppu.m_sprites, 0, len);
                        Array.Copy(src, 0, m_ppu.m_sprites, len, 256- len);
                    }
                    else Get_Dma_Address(data,ref m_ppu.m_sprites);
                    m_famicom.cpu_cycle_count += 513;
                    m_famicom.cpu_cycle_count += m_famicom.cpu_cycle_count & 1;
                    break;
                case 0x15:
                    // 狀態寄存器
                    m_famicom.m_apu.status_write = data;
                    // 對應通道長度計數器清零
                    if (!((data & (byte)Fc_4015_write_flag.FC_APU4015_WRITE_EnableSquare1) > 0))
                        m_famicom.m_apu.square1.length_counter = 0;
                    if (!((data & (byte)Fc_4015_write_flag.FC_APU4015_WRITE_EnableSquare2) > 0))
                        m_famicom.m_apu.square2.length_counter = 0;
                    if (!((data & (byte)Fc_4015_write_flag.FC_APU4015_WRITE_EnableTriangle) > 0))
                        m_famicom.m_apu.triangle.length_counter = 0;
                    if (!((data & (byte)Fc_4015_write_flag.FC_APU4015_WRITE_EnableNoise) > 0))
                        m_famicom.m_apu.noise.length_counter = 0;
                    // TODO: DMC 
                    break;
                case 0x16:
                    // 手柄端口
                    m_famicom.button_index_mask = (UInt16)((data & 1) > 0 ? 0x0 : 0x7);
                    if ((data & 1) > 0)
                    {
                        m_famicom.button_index_1 = 0;
                        m_famicom.button_index_2 = 0;
                    }
                    break;
                case 0x17:
                    // 幀計數器
                    // $4014:  MI-- ----
                    m_famicom.m_apu.frame_counter = data;
                    if ((data & (byte)Fc_4017_flag.FC_APU4017_IRQDisable) > 0)
                        m_famicom.m_apu.frame_interrupt = 0;
                    // 5步模式會立刻產生一個時鐘信號
                    if ((m_famicom.m_apu.frame_counter & (byte)Fc_4017_flag.FC_APU4017_ModeStep5) > 0)
                    {
                        m_famicom.m_apu.sfc_clock_length_counter_and_sweep_unit();
                        m_famicom.m_apu.sfc_clock_envelopes_and_linear_counter();
                    }
                    break;
            }
        }

        public UInt16 Read_Prg_Address(UInt16 address)
        {
            UInt16 prgaddr = address;
            return m_famicom.prg_banks[prgaddr >> 13][prgaddr & (UInt16)0x1fff];
        }

        public UInt16 Read_Cpu_Address(UInt16 address)
        {
            /*
            CPU 地址空間
            +---------+-------+-------+----------------------- +
            | 地址 | 大小 | 標記 | 描述 |
            +---------+-------+-------+----------------------- +
            | $0000 | $800 | | RAM |
            | $0800 | $800 | M | RAM |
            | $1000 | $800 | M | RAM |
            | $1800 | $800 | M | RAM |
            | $2000 | 8 | | Registers |
            | $2008 | $1FF8 | R | Registers |
            | $4000 | $20 | | Registers |
            | $4020 | $1FDF | | Expansion ROM |
            | $6000 | $2000 | | SRAM |
            | $8000 | $4000 | | PRG-ROM |
            | $C000 | $4000 | | PRG-ROM |
            +---------+-------+-------+----------------------- +
            標記圖例: M = $0000的鏡像
                        R = $2000-2008 每 8 bytes 的鏡像
                    (e.g. $2008=$2000, $2018=$2000, etc.)
            */
            switch (address >> 13)
            {
            case 0:
                // 高三位為0: [$0000, $2000): 系統主內存, 4次鏡像
                return m_famicom.m_mainMemory[address & (UInt16)0x07ff];
            case 1:
                // 高三位為1, [$2000, $4000): PPU寄存器, 8字節步進鏡像
                return  m_ppu.Read_PPU_Register_Via_CPU(address);
            case 2:
                    // 高三位為2, [$4000, $6000): pAPU寄存器 擴展ROM區
                    if (address < 0x4020)
                        return Read_cpu_address4020(address);
                    else 
                        Debug.Assert(true, "NOT IMPL");
                    return 0;
            case 3:
                // 高三位為3, [$6000, $8000): 存檔 SRAM區
                return m_famicom.m_saveMemory[address & (UInt16)0x1fff];
            case 4: case 5: case 6: case 7:
                // 高一位為1, [$8000, $10000) 程序PRG-ROM區
                return m_famicom.prg_banks[address >> 13][address & (UInt16)0x1fff];
            }
            return 0;
        }
        public void Write_Cpu_Address(UInt16 address, byte data)
        {
            /*
            CPU 地址空間
            +---------+-------+-------+----------------------- +
            | 地址 | 大小 | 標記 | 描述 |
            +---------+-------+-------+----------------------- +
            | $0000 | $800 | | RAM |
            | $0800 | $800 | M | RAM |
            | $1000 | $800 | M | RAM |
            | $1800 | $800 | M | RAM |
            | $2000 | 8 | | Registers |
            | $2008 | $1FF8 | R | Registers |
            | $4000 | $20 | | Registers |
            | $4020 | $1FDF | | Expansion ROM |
            | $6000 | $2000 | | SRAM |
            | $8000 | $4000 | | PRG-ROM |
            | $C000 | $4000 | | PRG-ROM |
            +---------+-------+-------+----------------------- +
            標記圖例: M = $0000的鏡像
                      R = $2000-2008 每 8 bytes 的鏡像
                    (e.g. $2008=$2000, $2018=$2000, etc.)
            */
            switch (address >> 13)
            {
                case 0:
                    // 高三位為0: [$0000, $2000): 系統主內存, 4次鏡像
                    m_famicom.m_mainMemory[address & (UInt16)0x07ff] = data;
                    return;
                case 1:
                    // 高三位為1, [$2000, $4000): PPU寄存器, 8字節步進鏡像
                    m_ppu.Write_PPU_Register_Via_CPU(address, data);
                    return;
                case 2:
                    // 高三位為2, [$4000, $6000): pAPU寄存器 擴展ROM區
                    // 前0x20為APU,I/O
                    if (address < 0x4020)
                        Write_cpu_address4020(address, data);
                    else 
                        Debug.Assert(true,"NOT IMPL");
                    return;
                case 3:
                    // 高三位為3, [$6000, $8000): 存檔 SRAM區
                    m_famicom.m_saveMemory[address & (UInt16)0x1fff] = data;
                    return;
                case 4:
                case 5:
                case 6:
                case 7:
                    // 高一位為1, [$8000, $10000) 程序PRG-ROM區
                    m_famicom.prg_banks[address >> 13][address & (UInt16)0x1fff] = data;
                    return;
            }
        }
    }
}
