using CSCore;
using CSCore.XAudio2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace NesTry
{
    enum BaseFormat
    {
        SAMPLE_PER_SEC = 44100,
        BASE_FREQUENCY = 220,
    };
    // wave format
    enum FormatWave
    {
        // unknown
        Wave_Unknown = 0,
        // pcm 
        Wave_PCM,
        // MS-ADPCM 
        Wave_MSADPCM,
        // IEEE FLOAT
        Wave_IEEEFloat,
    };
    public struct sfc_ez_wave
    {
        public XAudio2SourceVoice source;
        //size_t                                  callback[sizeof(SFCCallback) / sizeof(size_t)];
        public float[] wave;
        public XAudio2Buffer xAudio2Buffer;
    };
    [StructLayout(LayoutKind.Explicit)]
    public struct NoiseData
    {
        [FieldOffset(1)]
        public UInt16 u16_1;
        [FieldOffset(0)]
        public UInt16 u16_2;
        [FieldOffset(0)]
        public UInt32 u32;
    }
    public struct sfc_square_channel_state 
    {
        public float frequency;
        public UInt16 volume;
        public UInt16 duty;
    };
    public class NesXAudio2
    {
        private float[] NOSIE_FLIST = new float[]{
            178977.30f,
            99431.83f,
            52640.38f,
            27117.77f,
            13767.48f,
            9225.63f,
            6937.10f,
            5558.30f,
            4408.31f,
            3509.36f,
            2348.78f,
            1758.13f,
            1172.85f,
            879.93f,
            439.75f,
            219.93f,
        };
        public XAudio2 m_xauio2;
        public XAudio2MasteringVoice masteringVoice;
        private sfc_ez_wave square1;
        private sfc_square_channel_state square1_state;
        private sfc_ez_wave square2;
        private sfc_square_channel_state square2_state;
        private sfc_ez_wave triangle;
        private float triangle_frequency;
        private sfc_ez_wave noise_l;
        private sfc_ez_wave noise_s;
        private UInt32 noise_data;
        private bool square1_stop;
        private bool square2_stop;
        private bool triangle_stop;
        private bool noise_l_stop;
        private bool noise_s_stop;

        private float base_quare_vol = 0.25f;
        private float base_triangle_vol = 0.15f;
        private float base_noise_vol = 0.02f;
        public NesXAudio2()
        {
            m_xauio2 = XAudio2.CreateXAudio2();
            masteringVoice = m_xauio2.CreateMasteringVoice();
            // 創建方波#1
            xa2_create_clip(0);
            square1_stop = true;
            // 創建方波#2
            xa2_create_clip(1);
            square2_stop = true;
            // 創建三角波
            xa2_create_clip_tri();
            triangle_stop = true;
            // 創建噪音 Long模式
            xa2_create_clip_noi(0);
            noise_l_stop = true;
            // 創建噪音 Short模式
            xa2_create_clip_noi(1);
            noise_s_stop = true;
        }
        private float frac(float value)
        {
            return value - (long)value;
        }
        private void make_square_wave_ex(ref float []buf, UInt32 index, UInt32 len,float duty,float value)
        {

            for (UInt32 i = 0; i != len; ++i)
            {
                float time = i / (float)len;
                buf[i+ index] = frac(time) >= duty ? value : -value;
            }
        }
        private  void make_triangle_wave_ex(ref float [] buf, UInt32 len,float value)
        {

            for (UInt32 i = 0; i != len; ++i)
            {
                float index = (float)i;
                float time = index / len;

                float now = frac(time) * 2.0f;
                //buf[i] = value * (now >= 1.f ? (now * 2.f - 3.f) : (1.f - now * 2.f));
                if (now <= 0.5f)
                    buf[i] = now * 2.0f * value;
                else if (now > 1.5f)
                    buf[i] = (now - 2.0f) * 2.0f * value;
                else
                    buf[i] = (2.0f - now * 2.0f) * value;
            }
        }


        private UInt16 lfsr_long(UInt16 v){
            UInt16 a = (UInt16)(v & 1);
            UInt16 b = (UInt16)((v >> 1) & 1);

            return (UInt16)((v >> 1) | ((a ^ b) << 14));
        }

         private UInt16 lfsr_short(UInt16 v){
            UInt16 a = (UInt16)(v & 1);
            UInt16 b = (UInt16)((v >> 6) & 1);

            return (UInt16)((v >> 1) | ((a ^ b) << 14));
        }

        private void make_noise_short(ref float []buf, UInt32 len,float value)
        {
            UInt16 lfsr = 1;
            for (UInt32 i = 0; i != len; ++i)
            {
                lfsr = lfsr_short(lfsr);
                buf[i] = (lfsr & 1) > 0? value : -value;
            }
        }

        private void make_noise_long(ref float []buf, UInt32 len,float value)
        {
            UInt16 lfsr = 1;
            for (UInt32 i = 0; i != len; ++i)
            {
                lfsr = lfsr_long(lfsr);
                buf[i] = (lfsr & 1) > 0? value : -value;
            }
        }

        // 創建方波
        public void  xa2_create_clip(int index)
        {
            WaveFormat fmt = new WaveFormat((int)BaseFormat.SAMPLE_PER_SEC,32,1, AudioEncoding.IeeeFloat);
            
            // get length of buffer
            UInt32 bytelen = (UInt32)((UInt32)BaseFormat.BASE_FREQUENCY * 4 );

            // 生成波
            float[] buffer = new float[bytelen];

            UInt32 length_unit = (uint)BaseFormat.BASE_FREQUENCY;

            make_square_wave_ex(ref buffer, length_unit * 0, length_unit, 0.875f, base_quare_vol);
            make_square_wave_ex(ref buffer, length_unit * 1, length_unit, 0.750f, base_quare_vol);
            make_square_wave_ex(ref buffer, length_unit * 2, length_unit, 0.500f, base_quare_vol);
            make_square_wave_ex(ref buffer, length_unit * 3, length_unit, 0.250f, base_quare_vol);

            if (index == 0)
            {
                square1.source = m_xauio2.CreateSourceVoice(fmt, VoiceFlags.None, 100.0F, null, null, null);
                square1.wave = buffer;
            }
            else
            {
                square2.source = m_xauio2.CreateSourceVoice(fmt, VoiceFlags.None, 100.0F, null, null, null);
                square2.wave = buffer;
            }
        }
        // 創建三角波
        public void xa2_create_clip_tri()
        {
            WaveFormat fmt = new WaveFormat((int)BaseFormat.SAMPLE_PER_SEC, 32, 1, AudioEncoding.IeeeFloat);

            // get length of buffer
            UInt32 bytelen = (UInt32)BaseFormat.BASE_FREQUENCY;


            float[] buffer = new float[bytelen];

            UInt32 length_unit = (UInt32)BaseFormat.BASE_FREQUENCY;
            make_triangle_wave_ex(ref buffer, length_unit, base_triangle_vol);


            triangle.source = m_xauio2.CreateSourceVoice(fmt, VoiceFlags.None, 100.0F, null, null, null);

            //var byteArray = buffer.Select(f => Convert.ToByte(f)).ToArray();

            byte[] byteArray = new byte[buffer.Length * sizeof(float)];

            Buffer.BlockCopy(buffer, 0, byteArray, 0, byteArray.Length);
            
            XAudio2Buffer sourceBuffer = new XAudio2Buffer(byteArray.Length);
            {
                using (var stream = sourceBuffer.GetStream())
                {
                    stream.Write(byteArray, 0, byteArray.Length);
                }
                
                sourceBuffer.Flags = XAudio2BufferFlags.EndOfStream;
                sourceBuffer.LoopCount = XAudio2Buffer.LoopInfinite;
                triangle.source.SubmitSourceBuffer(sourceBuffer);
            }
            triangle.wave = buffer;
            triangle.xAudio2Buffer = sourceBuffer;
        }
        //創建噪音
        public void xa2_create_clip_noi(int mode) 
        {
            WaveFormat fmt = new WaveFormat((int)BaseFormat.SAMPLE_PER_SEC, 32, 1, AudioEncoding.IeeeFloat);
            // get length of buffer
            UInt32 bytelen = 0x8000; 



            float[] buffer = new float[bytelen];
            if (mode == 1) { 
                make_noise_short(ref buffer, bytelen, base_noise_vol);
                noise_s.source = m_xauio2.CreateSourceVoice(fmt);

                //var byteArray = buffer.Select(f => Convert.ToByte(f)).ToArray();
                byte[] byteArray = new byte[buffer.Length * sizeof(float)];

                Buffer.BlockCopy(buffer, 0, byteArray, 0, byteArray.Length);

                XAudio2Buffer sourceBuffer = new XAudio2Buffer(byteArray.Length);
                {
                    using (var stream = sourceBuffer.GetStream())
                    {
                        stream.Write(byteArray, 0, byteArray.Length);
                    }
                    sourceBuffer.Flags = XAudio2BufferFlags.EndOfStream;
                    sourceBuffer.LoopCount = XAudio2Buffer.LoopInfinite;
                    noise_s.source.SubmitSourceBuffer(sourceBuffer);
                }
                noise_s.wave = buffer;
            }
            else { 
                make_noise_long(ref buffer, bytelen, base_noise_vol);
                noise_l.source = m_xauio2.CreateSourceVoice(fmt);

                //var byteArray = buffer.Select(f => Convert.ToByte(f)).ToArray();
                byte[] byteArray = new byte[buffer.Length * sizeof(float)];

                Buffer.BlockCopy(buffer, 0, byteArray, 0, byteArray.Length);
                XAudio2Buffer sourceBuffer = new XAudio2Buffer(byteArray.Length);
                {
                    using (var stream = sourceBuffer.GetStream())
                    {
                        stream.Write(byteArray, 0, byteArray.Length);
                    }
                    sourceBuffer.Flags = XAudio2BufferFlags.EndOfStream;
                    sourceBuffer.LoopCount = XAudio2Buffer.LoopInfinite;
                    noise_l.source.SubmitSourceBuffer(sourceBuffer);
                }
                noise_l.wave = buffer;
            }
        }
        public void Play_square1(float frequency, UInt16 duty, UInt16 volume)
        {
            sfc_square_channel_state state = default;
            state.frequency = frequency;
            state.duty = duty;
            state.volume = volume;

            var old_duty = square1_state.duty;

            if (state.Equals(square1_state)) return;
               square1_state = state;

            var ez_wave = square1;
            var square = ez_wave.source;

            if (frequency == 0.0f)
            {
                square.Stop();
                return;
            }


            if (old_duty != duty)
            {
                UInt32 length_unit = (UInt32)BaseFormat.BASE_FREQUENCY* sizeof(float);
                //var byteArray = ez_wave.wave.Select(f => Convert.ToByte(f)).ToArray();

                byte[] byteArray = new byte[ez_wave.wave.Length * sizeof(float)];

                Buffer.BlockCopy(ez_wave.wave, 0, byteArray, 0, byteArray.Length);
                XAudio2Buffer sourceBuffer = new XAudio2Buffer((int)length_unit);
                {
                    using (var stream = sourceBuffer.GetStream())
                    {
                        stream.Write(byteArray, (int)(length_unit * duty), (int)length_unit);
                    }
                    sourceBuffer.Flags = XAudio2BufferFlags.EndOfStream;
                    sourceBuffer.LoopCount = XAudio2Buffer.LoopInfinite;
                    square.ExitLoop();
                    square.FlushSourceBuffers();
                    square.SubmitSourceBuffer(sourceBuffer);
                }
            }


            if (!(volume > 0))
            {
                square.ExitLoop();
                return;
            }

            square.SetVolume((float)volume / 15.0f,0);
            square.SetFrequencyRatio(frequency / ((float)BaseFormat.SAMPLE_PER_SEC / (float)BaseFormat.BASE_FREQUENCY));
            square.Start();
        }
        public void Play_square2(float frequency, UInt16 duty, UInt16 volume)
        {
            sfc_square_channel_state state = default;
            state.frequency = frequency;
            state.duty = duty;
            state.volume = volume;

            if (state.Equals(square2_state)) return;
                square2_state = state;

            var ez_wave = square2;
            var square = ez_wave.source;
            if (!(volume > 0))
            {
                square.ExitLoop();
                square2_stop = true;
                return;
            }

            if (square2_stop)
            {
                square2_stop = false;
                UInt32 length_unit = (UInt32)BaseFormat.BASE_FREQUENCY*sizeof(float);
                //var byteArray = ez_wave.wave.Select(f => Convert.ToByte(f)).ToArray();
                byte[] byteArray = new byte[ez_wave.wave.Length * sizeof(float)];

                Buffer.BlockCopy(ez_wave.wave, 0, byteArray, 0, byteArray.Length);
                XAudio2Buffer sourceBuffer = new XAudio2Buffer((int)length_unit);
                {
                    using (var stream = sourceBuffer.GetStream())
                    {
                        stream.Write(byteArray, (int)(length_unit * duty), (int)length_unit);
                    }
                    sourceBuffer.Flags = XAudio2BufferFlags.EndOfStream;
                    sourceBuffer.LoopCount = XAudio2Buffer.LoopInfinite;
                    square.FlushSourceBuffers();
                    square.SubmitSourceBuffer(sourceBuffer);
                }
            }

            square.SetVolume((float)volume / 15.0f,0);
            square.SetFrequencyRatio(frequency / ((float)BaseFormat.SAMPLE_PER_SEC / (float)BaseFormat.BASE_FREQUENCY));
            square.Start();
        }
        public void Play_triangle(float frequency)
        {
            if (frequency == triangle_frequency) return;
                triangle_frequency = frequency;


            var ez_wave = this.triangle;
            var triangle = ez_wave.source;

            if (frequency < 40 || frequency > 10000)
            {
                triangle.ExitLoop();
                triangle_stop = true;
                return;
            }

            if (triangle_stop)
            {
                triangle_stop = false;
                triangle.FlushSourceBuffers();
                triangle.SubmitSourceBuffer(ez_wave.xAudio2Buffer);
            }
            triangle.SetFrequencyRatio(frequency / ((float)BaseFormat.SAMPLE_PER_SEC / (float)BaseFormat.BASE_FREQUENCY));
            triangle.Start();
        }
        public void Play_noise(UInt16 data, UInt16 volume)
        {
            NoiseData udata = new NoiseData();
            udata.u16_1 = volume;
            udata.u16_2 = data;

            if (udata.u32 == noise_data) return;
               noise_data = udata.u32;

            if (volume == 0)
            {
                if (!noise_l_stop)
                    noise_l.source.Stop();
                if (!noise_s_stop)
                    noise_s.source.Stop();
                noise_s_stop = true;
                noise_l_stop = true;
                return;
            }

            var ez_wave = (data &0x80) > 0 ? noise_s: noise_l;
            var noise_stop = (data & 0x80)> 0 ? noise_s_stop: noise_l_stop;
            var noise = ez_wave.source;

            float ratio = (float)BaseFormat.SAMPLE_PER_SEC;
            float f = NOSIE_FLIST[data & 0xF] / ratio;
            noise.SetFrequencyRatio(f * 2.0f);

            noise.SetVolume((float)volume / 15.0f, 0);

            if (noise_stop)
            {
                noise.Start();
            }
        }
    }
}
