using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace NesTry
{
    public enum Fc_4015_write_flag
    {
        FC_APU4015_WRITE_EnableSquare1 = 0x01,//方波1使能
        FC_APU4015_WRITE_EnableSquare2 = 0x02,//方波2使能
        FC_APU4015_WRITE_EnableTriangle = 0x04,//三角波使能
        FC_APU4015_WRITE_EnableNoise = 0x08,//噪聲使能
        FC_APU4015_WRITE_EnableDMC = 0x10,// DMC使能
    };

    public enum Fc_4015_read_flag {
        FC_APU4015_READ_Square1Length = 0x01,//方波1長度計數器> 0
        FC_APU4015_READ_Square2Length = 0x02,//方波2長度計數器> 0
        FC_APU4015_READ_TriangleLength = 0x04,//三角波長度計數器> 0
        FC_APU4015_READ_NoiseLength = 0x08,//三角波長度計數器> 0
        FC_APU4015_READ_DMCActive = 0x08,// DMC激活狀態

        FC_APU4015_READ_Frameinterrupt = 0x40,//幀中斷
        FC_APU4015_READ_DMCInterrupt = 0x80,// DMC中斷
    };

    public enum Fc_4017_flag {
        FC_APU4017_ModeStep5 = 0x80,// 5步模式
        FC_APU4017_IRQDisable = 0x40,// IRQ片段
    };
    public enum Fc_apu_ctrl6_flag
    {
        FC_APUCTRL6_ConstVolume = 0x10,//固定音量
        FC_APUCTRL6_EnvLoop = 0x20,//循環包絡
    };

    public struct sfc_envelope_t
    {
        // 時鐘分頻器
        public byte divider;
        // 計數器
        public byte counter;
        // 開始標記
        public byte start;
        // 控制器低6位
        public byte ctrl6;
    };
    public struct sfc_square_data_t
    {
        // 包絡
        public sfc_envelope_t envelope;
        // 當前週期
        public UInt16 cur_period;
        // 理論周期
        public UInt16 use_period;
        // 長度計數器
        public byte length_counter;
        // 控制寄存器
        public byte ctrl;
        // 掃描單元: 重載
        public byte sweep_reload;
        // 掃描單元: 使能
        public byte sweep_enable;
        // 掃描單元: 反相掃描
        public byte sweep_negate;
        // 掃描單元: 時鐘分頻器週期
        public byte sweep_period;
        // 掃描單元: 時鐘分頻器
        public byte sweep_divider;
        // 掃描單元: 移位器
        public byte sweep_shift;
    };

    public struct sfc_triangle_data_t
    {
        // 當前週期
        public UInt16 cur_period;
        // 長度計數器
        public byte length_counter;
        // 線性計數器
        public byte linear_counter;
        // 線性計數器 重載值
        public byte value_reload;
        // 線性計數器 重載標誌
        public byte flag_reload;
        // 長度計數器/線性計數器暫停值
        public byte flag_halt;
    };
    public struct sfc_noise_data_t
    {
        // 包絡
        public sfc_envelope_t envelope;
        // 線性反饋移位寄存器(暫時沒用到)
        public byte lfsr;
        // 長度計數器
        public byte length_counter;
        // 短模式[D7] 週期索引[D0-D3]
        public byte short_mode__period_index;
    };
    public struct sfc_dmc_data_t
    {
        // 輸出;
        public byte value;
    };

    public class NesAPU
    {
        // 方波 #1
        public sfc_square_data_t square1;
        // 方波 #2
        public sfc_square_data_t square2;
        // 三角波
        public sfc_triangle_data_t triangle;
        // 噪聲
        public sfc_noise_data_t noise;
        // DMC
        public sfc_dmc_data_t dmc;
        // 狀態寄存器(寫: 聲道使能)
        public byte status_write;
        // 狀態寄存器(讀:)
        //uint8_t status_read;
        // 幀計數器寫入寄存器
        public byte frame_counter;
        // 幀中斷標誌
        public byte frame_interrupt;
        // 步數計數
        public byte frame_step;

        public Nes6502 m_6502;
        // REF: http://nesdev.com/apu_ref.txt
        // 長度計數器映射表
        public byte[] LENGTH_COUNTER_TABLE = new byte[] {
            0x0A, 0xFE, 0x14, 0x02,
            0x28, 0x04, 0x50, 0x06,
            0xA0, 0x08, 0x3C, 0x0A,
            0x0E, 0x0C, 0x1A, 0x0E,
            0x0C, 0x10, 0x18, 0x12,
            0x30, 0x14, 0x60, 0x16,
            0xC0, 0x18, 0x48, 0x1A,
            0x10, 0x1C, 0x20, 0x1E,
        };
        public NesAPU(ref Nes6502 ref6502)
        {
            m_6502 = ref6502;
            noise.lfsr = 1;
        }
        public void Reset()
        {
            noise.lfsr = 1;
        }

        /// <summary>
        /// SFCs the sweep square.
        /// </summary>
        /// <param name="square">The square.</param>
        /// <param name="one">The one.</param>
        private void sfc_sweep_square(ref sfc_square_data_t square, UInt16 one)
        {
            // 重載掃描
            if (square.sweep_reload  > 0) {
                square.sweep_reload = 0;
                byte old_divider = square.sweep_divider;
                square.sweep_divider = square.sweep_period;
                if (square.sweep_enable > 0 && !(old_divider > 0))
                {
                    // 向上掃描
                    if (!(square.sweep_negate > 0))
                    {
                        UInt16 target
                            = (ushort)(square.cur_period + (square.cur_period >> square.sweep_shift));
                        square.use_period = target;
                        if (target < 0x0800)
                            square.cur_period = target;
                    }
                    // 向下掃描
                    else
                    {
                        UInt16 target
                            = (ushort)(square.cur_period - (square.cur_period >> square.sweep_shift));
                        target -= one;
                        square.use_period = target;
                        if (target >= 8)
                            square.cur_period = target;
                    }
                }
                return;
            }
            // 檢測是否可用
            if (!(square.sweep_enable > 0)) return;
            // 等待分頻器輸出時鐘
            if (square.sweep_divider > 0) --square.sweep_divider;
            else {
                square.sweep_divider = square.sweep_period;
                // 向上掃描
                if (!(square.sweep_negate > 0)) {
                    UInt16 target
                        = (ushort)(square.cur_period + (square.cur_period >> square.sweep_shift));
                        square.use_period = target;
                    if (target< 0x0800)
                        square.cur_period = target;
                }
                // 向下掃描
                else
                {
                    UInt16 target
                        = (ushort)(square.cur_period - (square.cur_period >> square.sweep_shift));
                    target -= one;
                    square.use_period = target;
                    if (target >= 8)
                        square.cur_period = target;
                }
            }
        }

        /// <summary>
        /// SFCs the clock length counter and sweep unit.
        /// </summary>
        /// <param name="apu">The apu.</param>
        public void sfc_clock_length_counter_and_sweep_unit()
        {
            // 方波#1 長度計數器
            if (!((square1.ctrl & 0x20) > 0) && square1.length_counter > 0)
                --square1.length_counter;
            // 方波#2 長度計數器
            if (!((square2.ctrl & 0x20) > 0) &&square2.length_counter > 0)
                --square2.length_counter;
            // 三角波 長度計數器
            if (!(triangle.flag_halt > 0)&& triangle.length_counter > 0)
                --triangle.length_counter;
            // 噪音-- 長度計數器
            if (!((noise.envelope.ctrl6 & 0x20) > 0)&& noise.length_counter > 0)
                --noise.length_counter;

            // 掃描
            sfc_sweep_square(ref square1, 1);
            sfc_sweep_square(ref square2, 0);
        }


        /// <summary>
        /// SFCs the clock envelope.
        /// </summary>
        /// <param name="envelope">The envelope.</param>
        private void sfc_clock_envelope(ref sfc_envelope_t envelope)
        {
            // 寫入了第四個寄存器(START 標記了)
            if (envelope.start > 0)
            {
                // 重置
                envelope.start = 0;
                envelope.counter = 15;
                envelope.divider = (byte)(envelope.ctrl6 & 0x0F);
            }
            // 對時鐘分頻器發送一個時鐘信號
            else
            {
                if (envelope.divider > 0) envelope.divider--;
                else
                {
                    envelope.divider = (byte)(envelope.ctrl6 & 0x0F);
                    if (envelope.counter > 0) --envelope.counter;
                    else if ((envelope.ctrl6 & (byte)Fc_apu_ctrl6_flag.FC_APUCTRL6_EnvLoop)> 0)
                        envelope.counter = 15;
                }
            }
        }

        /// <summary>
        /// SFCs the clock envelopes and linear counter.
        /// </summary>
        /// <param name="apu">The apu.</param>
        public void sfc_clock_envelopes_and_linear_counter()
        {
            // 方波#1
            sfc_clock_envelope(ref square1.envelope);
            // 方波#2
            sfc_clock_envelope(ref square2.envelope);
            // 噪音
            sfc_clock_envelope(ref noise.envelope);

            // 三角波 - 線性計數器

            // 標記了重載
            if (triangle.flag_reload > 0)
            {
                triangle.linear_counter = triangle.value_reload;
            }
            // 沒有就減1
            else if (triangle.linear_counter > 0)
            {
                --triangle.linear_counter;
            }
            // 控制C位為0 清除重載標誌
            if (!(triangle.flag_halt > 0))
                triangle.flag_reload = 0;
        }


        /// <summary>
        /// SFCs the apu set interrupt.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        private void sfc_apu_set_interrupt()
        {
            if ((frame_counter & (byte)Fc_4017_flag.FC_APU4017_IRQDisable) > 0) return;
            frame_interrupt = 1;
            // TODO: 正確觸發IRQ
            m_6502.O_IRQ_try();
        }


        /// <summary>
        /// SFCs the trigger frame counter.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        public void sfc_trigger_frame_counter()
        {
            // 5步模式
            if ((frame_counter &(byte) Fc_4017_flag.FC_APU4017_ModeStep5) > 0)
            {
                frame_step++;
                frame_step = (byte)(frame_step % 5);
                // l - l - - ->   ++%5   ->   1 2 3 4 0
                // e e e e - ->   ++%5   ->   1 2 3 4 0
                switch (frame_step)
                {
                    case 1:
                    case 3:
                        sfc_clock_length_counter_and_sweep_unit();
                        break;
                    case 2:
                    case 4:
                        sfc_clock_envelopes_and_linear_counter();
                        break;
                }
            }
            // 四步模式
            else
            {
                frame_step++;
                frame_step = (byte)(frame_step % 4);
                // - - - f   ->   ++%4   ->   1 2 3 0
                if (!(frame_step > 0)) sfc_apu_set_interrupt();
                // - l - l   ->   ++%4   ->   1 2 3 0
                if (!((frame_step & 1) > 0)) sfc_clock_length_counter_and_sweep_unit();
                // e e e e
                sfc_clock_envelopes_and_linear_counter();
            }
        }
        /// <summary>
        /// SFCs the play audio easy.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <param name="state">The state.</param>
        public void sfc_play_audio_easy(ref sfc_channel_state_t state) {
            // 方波#1
            sfc_play_square1(ref state.square1);
            // 方波#1
            sfc_play_square2(ref state.square2);
            // 三角波
            sfc_play_triangle(ref state.triangle);
            // 噪聲
            sfc_play_noise(ref state.noise); 
        }
        /// <summary>
        /// SFCs the play square1.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        private void sfc_play_square1(ref sfc_square_channel_state_t state) 
        {
                // 使能
                if (!((status_write & (byte)Fc_4015_write_flag.FC_APU4015_WRITE_EnableSquare1) > 0)) return;
                // 長度計數器為0
                if (square1.length_counter == 0) return;
                // 方波#1
                UInt16 square1period = square1.use_period;
                // 輸出頻率
                if (square1period< 8 || square1period> 0x7ff) return;
                state.frequency = (float)(1789773.0 / 16.0 / (float) (square1period + 1));
                state.duty = (UInt16)(square1.ctrl >> 6);
                // 固定音量
                if ((square1.envelope.ctrl6 & (byte)Fc_apu_ctrl6_flag.FC_APUCTRL6_ConstVolume) > 0)
                    state.volume = (UInt16)(square1.envelope.ctrl6 & 0xf);
                // 包絡音量
                else
                    state.volume = square1.envelope.counter;
        }

        /// <summary>
        /// SFCs the play square2
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        private void sfc_play_square2(ref sfc_square_channel_state_t state) {
            // 使能
            if (!((status_write &(byte)Fc_4015_write_flag.FC_APU4015_WRITE_EnableSquare2)> 0)) return;
            // 長度計數器為0
            if (square2.length_counter == 0) return;
            // 方波#2
            UInt16 square2period = square2.use_period;
            // 輸出頻率
            if (square2period< 8 || square2period> 0x7ff) return;
            state.frequency = (float)(1789773.0 / 16.0/(float)(square2period + 1));
            state.duty = (ushort)(square2.ctrl >> 6);
            // 固定音量
            if ((square2.envelope.ctrl6 & (byte)Fc_apu_ctrl6_flag.FC_APUCTRL6_ConstVolume) > 0)
                state.volume = (ushort)(square2.envelope.ctrl6 & 0xf);
            // 包絡音量
            else
                state.volume = square2.envelope.counter;
        }


        /// <summary>
        /// SFCs the play triangle.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <param name="state">The state.</param>
        private void sfc_play_triangle(ref sfc_triangle_channel_state_t state) {
            // 使能
            if (!((status_write & (byte)Fc_4015_write_flag.FC_APU4015_WRITE_EnableTriangle) > 0)) return;
            // 長度計數器為0
            if (triangle.length_counter == 0) return;
            // 線性計數器為0
            if (triangle.linear_counter == 0) return;
            // 輸出頻率
            state.frequency = (float)(1789773.0 / 32.0 /(float)(triangle.cur_period + 1));
        }

        /// <summary>
        /// SFCs the play noise.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <param name="state">The state.</param>
        private void sfc_play_noise(ref sfc_noise_channel_state_t state) 
        {
            // 使能
            if (!((status_write & (byte)Fc_4015_write_flag.FC_APU4015_WRITE_EnableNoise) > 0)) return;
            // 長度計數器為0
            if (noise.length_counter == 0) return;
            // 數據
            state.data = noise.short_mode__period_index;
            // 固定音量
            if ((noise.envelope.ctrl6 & (byte)Fc_apu_ctrl6_flag.FC_APUCTRL6_ConstVolume) > 0)
                state.volume = (ushort)(noise.envelope.ctrl6 & 0xf);
            // 包絡音量
            else
                state.volume = noise.envelope.counter;
        }
    }
}
