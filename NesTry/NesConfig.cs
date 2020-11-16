using System;
using System.Collections.Generic;
using System.Text;

namespace NesTry
{
    public enum config_constant
    {
        MASTER_CYCLE_PER_CPU = 12,
        NES_WIDTH = 256,
        NES_HEIGHT= 240,
        NES_SPRITE_COUNT = 64,
    };
    public class NesConfig
    {
        // CPU 主頻 Hz
        public float cpu_clock;
        // 屏幕刷新率
        public UInt16 refresh_rate;
        // 每條掃描線週期 Master-Clock
        public UInt16 master_cycle_per_scanline;
        // 每條掃描線渲染週期 Master-Clock
        public UInt16 master_cycle_per_drawline;
        // 每條掃描線水平空白週期 Master-Clock
        public UInt16 master_cycle_per_hblank;
        // 可見掃描線
        public UInt16 visible_scanline;
        // 垂直空白掃描線
        public UInt16 vblank_scanline;

        public NesConfig(float cpu_clock, UInt16 refresh_rate, UInt16 master_cycle_per_scanline
            , UInt16 master_cycle_per_drawline, UInt16 master_cycle_per_hblank,UInt16 visible_scanline, UInt16 vblank_scanline)
        {
            this.cpu_clock = cpu_clock;
            this.refresh_rate = refresh_rate;
            this.master_cycle_per_scanline = master_cycle_per_scanline;
            this.master_cycle_per_drawline = master_cycle_per_drawline;
            this.master_cycle_per_hblank = master_cycle_per_hblank;
            this.visible_scanline = visible_scanline;
            this.vblank_scanline = vblank_scanline;
        }
    }
}
