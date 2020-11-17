using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace NesTry
{
    public partial class Nes6502
    {
        public class DelegateOP
        {
            public delegate UInt16 GetAddress(ref UInt32 cycle_add);
            public delegate void Operation(UInt16 address, ref UInt32 cycle_add);

            public GetAddress m_getaddress;
            public Operation m_operation;
            public DelegateOP(GetAddress getAddress, Operation operation)
            {
                m_getaddress = getAddress;
                m_operation = operation;
            }

            public void execOpeation(ref UInt32 cycle_add)
            {
                UInt16 address = m_getaddress(ref cycle_add);
                m_operation(address, ref cycle_add);
            }
        }
        private Dictionary<byte, DelegateOP> OpMaps;
        private Dictionary<byte, byte> OpBasicCycle;
        private NesCPU m_cpu { get; set; }

        public Nes6502(ref NesCPU cpu)
        {
            m_cpu = cpu;
            OpBasicCycle = new Dictionary<byte, byte>()
            {               
                {0x00,7},{0x01,6},{0x02,2},{0x03,8},
                {0x04,3},{0x05,3},{0x06,5},{0x07,5},

                {0x08,3},{0x09,2},{0x0A,2},{0x0B,2},
                {0x0C,4},{0x0D,4},{0x0E,6},{0x0F,6},

                {0x10,2},{0x11,5},{0x12,2},{0x13,8},
                {0x14,4},{0x15,4},{0x16,6},{0x17,6},

                {0x18,2},{0x19,4},{0x1A,2},{0x1B,7},
                {0x1C,4},{0x1D,4},{0x1E,7},{0x1F,7},

                {0x20,6},{0x21,6},{0x22,2},{0x23,8},
                {0x24,3},{0x25,3},{0x26,5},{0x27,5},

                {0x28,4},{0x29,2},{0x2A,2},{0x2B,2},
                {0x2C,4},{0x2D,4},{0x2E,6},{0x2F,6},

                {0x30,2},{0x31,5},{0x32,2},{0x33,8},
                {0x34,4},{0x35,4},{0x36,6},{0x37,6},

                {0x38,2},{0x39,4},{0x3A,2},{0x3B,7},
                {0x3C,4},{0x3D,4},{0x3E,7},{0x3F,7},

                {0x40,6},{0x41,6},{0x42,2},{0x43,8},
                {0x44,3},{0x45,3},{0x46,5},{0x47,5},

                {0x48,3},{0x49,2},{0x4A,2},{0x4B,2},
                {0x4C,3},{0x4D,4},{0x4E,6},{0x4F,6},

                {0x50,2},{0x51,5},{0x52,2},{0x53,8},
                {0x54,4},{0x55,4},{0x56,6},{0x57,6},

                {0x58,2},{0x59,4},{0x5A,2},{0x5B,7},
                {0x5C,4},{0x5D,4},{0x5E,7},{0x5F,7},

                {0x60,6},{0x61,6},{0x62,2},{0x63,8},
                {0x64,3},{0x65,3},{0x66,5},{0x67,5},

                {0x68,4},{0x69,2},{0x6A,2},{0x6B,2},
                {0x6C,5},{0x6D,4},{0x6E,6},{0x6F,6},

                {0x70,2},{0x71,5},{0x72,2},{0x73,8},
                {0x74,4},{0x75,4},{0x76,6},{0x77,6},

                {0x78,2},{0x79,4},{0x7A,2},{0x7B,7},
                {0x7C,4},{0x7D,4},{0x7E,7},{0x7F,7},

                {0x80,2},{0x81,6},{0x82,2},{0x83,6},
                {0x84,3},{0x85,3},{0x86,3},{0x87,3},

                {0x88,2},{0x89,2},{0x8A,2},{0x8B,2},
                {0x8C,4},{0x8D,4},{0x8E,4},{0x8F,4},

                {0x90,2},{0x91,6},{0x92,2},{0x93,6},
                {0x94,4},{0x95,4},{0x96,4},{0x97,4},

                {0x98,2},{0x99,5},{0x9A,2},{0x9B,5},
                {0x9C,5},{0x9D,5},{0x9E,5},{0x9F,5},

                {0xA0,2},{0xA1,6},{0xA2,2},{0xA3,6},
                {0xA4,3},{0xA5,3},{0xA6,3},{0xA7,3},

                {0xA8,2},{0xA9,2},{0xAA,2},{0xAB,2},
                {0xAC,4},{0xAD,4},{0xAE,4},{0xAF,4},

                {0xB0,2},{0xB1,5},{0xB2,2},{0xB3,5},
                {0xB4,4},{0xB5,4},{0xB6,4},{0xB7,4},

                {0xB8,2},{0xB9,4},{0xBA,2},{0xBB,4},
                {0xBC,4},{0xBD,4},{0xBE,4},{0xBF,4},

                {0xC0,2},{0xC1,6},{0xC2,2},{0xC3,8},
                {0xC4,3},{0xC5,3},{0xC6,5},{0xC7,5},

                {0xC8,2},{0xC9,2},{0xCA,2},{0xCB,2},
                {0xCC,4},{0xCD,4},{0xCE,6},{0xCF,6},

                {0xD0,2},{0xD1,5},{0xD2,2},{0xD3,8},
                {0xD4,4},{0xD5,4},{0xD6,6},{0xD7,6},

                {0xD8,2},{0xD9,4},{0xDA,2},{0xDB,7},
                {0xDC,4},{0xDD,4},{0xDE,7},{0xDF,7},

                {0xE0,2},{0xE1,6},{0xE2,2},{0xE3,8},
                {0xE4,3},{0xE5,3},{0xE6,5},{0xE7,5},

                {0xE8,2},{0xE9,2},{0xEA,2},{0xEB,2},
                {0xEC,4},{0xED,4},{0xEE,6},{0xEF,6},

                {0xF0,2},{0xF1,5},{0xF2,2},{0xF3,8},
                {0xF4,4},{0xF5,4},{0xF6,6},{0xF7,6},

                {0xF8,2},{0xF9,4},{0xFA,2},{0xFB,7},
                {0xFC,4},{0xFD,4},{0xFE,7},{0xFF,7},
            };
            OpMaps = new Dictionary<byte, DelegateOP>()
            {
                {0x00,new DelegateOP(A_IMP, O_BRK)},
                {0x01,new DelegateOP(A_INX, O_ORA)},
                {0x02,new DelegateOP(A_UNK, O_UNK)},
                {0x03,new DelegateOP(A_INX, O_SLO)},
                {0x04,new DelegateOP(A_ZPG, O_NOP)},
                {0x05,new DelegateOP(A_ZPG, O_ORA)},
                {0x06,new DelegateOP(A_ZPG, O_ASL)},
                {0x07,new DelegateOP(A_ZPG, O_SLO)},

                {0x08,new DelegateOP(A_IMP, O_PHP)},
                {0x09,new DelegateOP(A_IMM, O_ORA)},
                {0x0A,new DelegateOP(A_IMP, O_ASLA)},
                {0x0B,new DelegateOP(A_IMM, O_ANC)},
                {0x0C,new DelegateOP(A_ABS, O_NOP)},
                {0x0D,new DelegateOP(A_ABS, O_ORA)},
                {0x0E,new DelegateOP(A_ABS, O_ASL)},
                {0x0F,new DelegateOP(A_ABS, O_SLO)},

                {0x10,new DelegateOP(A_REL, O_BPL)},
                {0x11,new DelegateOP(A_INY, O_ORA)},
                {0x12,new DelegateOP(A_UNK, O_UNK)},
                {0x13,new DelegateOP(A_iny, O_SLO)},
                {0x14,new DelegateOP(A_ZPX, O_NOP)},
                {0x15,new DelegateOP(A_ZPX, O_ORA)},
                {0x16,new DelegateOP(A_ZPX, O_ASL)},
                {0x17,new DelegateOP(A_ZPX, O_SLO)},

                {0x18,new DelegateOP(A_IMP, O_CLC)},
                {0x19,new DelegateOP(A_ABY, O_ORA)},
                {0x1A,new DelegateOP(A_IMP, O_NOP)},
                {0x1B,new DelegateOP(A_aby, O_SLO)},
                {0x1C,new DelegateOP(A_ABX, O_NOP)},
                {0x1D,new DelegateOP(A_ABX, O_ORA)},
                {0x1E,new DelegateOP(A_ABX, O_ASL)},
                {0x1F,new DelegateOP(A_abx, O_SLO)},

                {0x20,new DelegateOP(A_ABS, O_JSR)},
                {0x21,new DelegateOP(A_INX, O_AND)},
                {0x22,new DelegateOP(A_UNK, O_UNK)},
                {0x23,new DelegateOP(A_INX, O_RLA)},
                {0x24,new DelegateOP(A_ZPG, O_BIT)},
                {0x25,new DelegateOP(A_ZPG, O_AND)},
                {0x26,new DelegateOP(A_ZPG, O_ROL)},
                {0x27,new DelegateOP(A_ZPG, O_RLA)},

                {0x28,new DelegateOP(A_IMP, O_PLP)},
                {0x29,new DelegateOP(A_IMM, O_AND)},
                {0x2A,new DelegateOP(A_IMP, O_ROLA)},
                {0x2B,new DelegateOP(A_IMM, O_ANC)},
                {0x2C,new DelegateOP(A_ABS, O_BIT)},
                {0x2D,new DelegateOP(A_ABS, O_AND)},
                {0x2E,new DelegateOP(A_ABS, O_ROL)},
                {0x2F,new DelegateOP(A_ABS, O_RLA)},

                {0x30,new DelegateOP(A_REL, O_BMI)},
                {0x31,new DelegateOP(A_INY, O_AND)},
                {0x32,new DelegateOP(A_UNK, O_UNK)},
                {0x33,new DelegateOP(A_iny, O_RLA)},
                {0x34,new DelegateOP(A_ZPX, O_NOP)},
                {0x35,new DelegateOP(A_ZPX, O_AND)},
                {0x36,new DelegateOP(A_ZPX, O_ROL)},
                {0x37,new DelegateOP(A_ZPX, O_RLA)},

                {0x38,new DelegateOP(A_IMP, O_SEC)},
                {0x39,new DelegateOP(A_ABY, O_AND)},
                {0x3A,new DelegateOP(A_IMP, O_NOP)},
                {0x3B,new DelegateOP(A_aby, O_RLA)},
                {0x3C,new DelegateOP(A_ABX, O_NOP)},
                {0x3D,new DelegateOP(A_ABX, O_AND)},
                {0x3E,new DelegateOP(A_ABX, O_ROL)},
                {0x3F,new DelegateOP(A_abx, O_RLA)},

                {0x40,new DelegateOP(A_IMP, O_RTI)},
                {0x41,new DelegateOP(A_INX, O_EOR)},
                {0x42,new DelegateOP(A_UNK, O_UNK)},
                {0x43,new DelegateOP(A_INX, O_SRE)},
                {0x44,new DelegateOP(A_ZPG, O_NOP)},
                {0x45,new DelegateOP(A_ZPG, O_EOR)},
                {0x46,new DelegateOP(A_ZPG, O_LSR)},
                {0x47,new DelegateOP(A_ZPG, O_SRE)},

                {0x48,new DelegateOP(A_IMP, O_PHA)},
                {0x49,new DelegateOP(A_IMM, O_EOR)},
                {0x4A,new DelegateOP(A_IMP, O_LSRA)},
                {0x4B,new DelegateOP(A_IMM, O_ASR)},
                {0x4C,new DelegateOP(A_ABS, O_JMP)},
                {0x4D,new DelegateOP(A_ABS, O_EOR)},
                {0x4E,new DelegateOP(A_ABS, O_LSR)},
                {0x4F,new DelegateOP(A_ABS, O_SRE)},

                {0x50,new DelegateOP(A_REL, O_BVC)},
                {0x51,new DelegateOP(A_INY, O_EOR)},
                {0x52,new DelegateOP(A_UNK, O_UNK)},
                {0x53,new DelegateOP(A_iny, O_SRE)},
                {0x54,new DelegateOP(A_ZPX, O_NOP)},
                {0x55,new DelegateOP(A_ZPX, O_EOR)},
                {0x56,new DelegateOP(A_ZPX, O_LSR)},
                {0x57,new DelegateOP(A_ZPX, O_SRE)},

                {0x58,new DelegateOP(A_IMP, O_CLI)},
                {0x59,new DelegateOP(A_ABY, O_EOR)},
                {0x5A,new DelegateOP(A_IMP, O_NOP)},
                {0x5B,new DelegateOP(A_aby, O_SRE)},
                {0x5C,new DelegateOP(A_ABX, O_NOP)},
                {0x5D,new DelegateOP(A_ABX, O_EOR)},
                {0x5E,new DelegateOP(A_ABX, O_LSR)},
                {0x5F,new DelegateOP(A_abx, O_SRE)},

                {0x60,new DelegateOP(A_IMP, O_RTS)},
                {0x61,new DelegateOP(A_INX, O_ADC)},
                {0x62,new DelegateOP(A_UNK, O_UNK)},
                {0x63,new DelegateOP(A_INX, O_RRA)},
                {0x64,new DelegateOP(A_ZPG, O_NOP)},
                {0x65,new DelegateOP(A_ZPG, O_ADC)},
                {0x66,new DelegateOP(A_ZPG, O_ROR)},
                {0x67,new DelegateOP(A_ZPG, O_RRA)},

                {0x68,new DelegateOP(A_IMP, O_PLA)},
                {0x69,new DelegateOP(A_IMM, O_ADC)},
                {0x6A,new DelegateOP(A_IMP, O_RORA)},
                {0x6B,new DelegateOP(A_IMM, O_ARR)},
                {0x6C,new DelegateOP(A_IND, O_JMP)},
                {0x6D,new DelegateOP(A_ABS, O_ADC)},
                {0x6E,new DelegateOP(A_ABS, O_ROR)},
                {0x6F,new DelegateOP(A_ABS, O_RRA)},

                {0x70,new DelegateOP(A_REL, O_BVS)},
                {0x71,new DelegateOP(A_INY, O_ADC)},
                {0x72,new DelegateOP(A_UNK, O_UNK)},
                {0x73,new DelegateOP(A_iny, O_RRA)},
                {0x74,new DelegateOP(A_ZPX, O_NOP)},
                {0x75,new DelegateOP(A_ZPX, O_ADC)},
                {0x76,new DelegateOP(A_ZPX, O_ROR)},
                {0x77,new DelegateOP(A_ZPX, O_RRA)},

                {0x78,new DelegateOP(A_IMP, O_SEI)},
                {0x79,new DelegateOP(A_ABY, O_ADC)},
                {0x7A,new DelegateOP(A_IMP, O_NOP)},
                {0x7B,new DelegateOP(A_aby, O_RRA)},
                {0x7C,new DelegateOP(A_ABX, O_NOP)},
                {0x7D,new DelegateOP(A_ABX, O_ADC)},
                {0x7E,new DelegateOP(A_ABX, O_ROR)},
                {0x7F,new DelegateOP(A_abx, O_RRA)},

                {0x80,new DelegateOP(A_IMM, O_NOP)},
                {0x81,new DelegateOP(A_INX, O_STA)},
                {0x82,new DelegateOP(A_IMM, O_NOP)},
                {0x83,new DelegateOP(A_INX, O_SAX)},
                {0x84,new DelegateOP(A_ZPG, O_STY)},
                {0x85,new DelegateOP(A_ZPG, O_STA)},
                {0x86,new DelegateOP(A_ZPG, O_STX)},
                {0x87,new DelegateOP(A_ZPG, O_SAX)},

                {0x88,new DelegateOP(A_IMP, O_DEY)},
                {0x89,new DelegateOP(A_IMM, O_NOP)},
                {0x8A,new DelegateOP(A_IMP, O_TXA)},
                {0x8B,new DelegateOP(A_IMM, O_XAA)},
                {0x8C,new DelegateOP(A_ABS, O_STY)},
                {0x8D,new DelegateOP(A_ABS, O_STA)},
                {0x8E,new DelegateOP(A_ABS, O_STX)},
                {0x8F,new DelegateOP(A_ABS, O_SAX)},

                {0x90,new DelegateOP(A_REL, O_BCC)},
                {0x91,new DelegateOP(A_INY, O_STA)},
                {0x92,new DelegateOP(A_UNK, O_UNK)},
                {0x93,new DelegateOP(A_INY, O_AHX)},
                {0x94,new DelegateOP(A_ZPX, O_STY)},
                {0x95,new DelegateOP(A_ZPX, O_STA)},
                {0x96,new DelegateOP(A_ZPY, O_STX)},
                {0x97,new DelegateOP(A_ZPY, O_SAX)},

                {0x98,new DelegateOP(A_IMP, O_TYA)},
                {0x99,new DelegateOP(A_ABY, O_STA)},
                {0x9A,new DelegateOP(A_IMP, O_TXS)},
                {0x9B,new DelegateOP(A_ABY, O_TAS)},
                {0x9C,new DelegateOP(A_ABX, O_SHY)},
                {0x9D,new DelegateOP(A_ABX, O_STA)},
                {0x9E,new DelegateOP(A_ABX, O_SHX)},
                {0x9F,new DelegateOP(A_ABX, O_AHX)},

                {0xA0,new DelegateOP(A_IMM, O_LDY)},
                {0xA1,new DelegateOP(A_INX, O_LDA)},
                {0xA2,new DelegateOP(A_IMM, O_LDX)},
                {0xA3,new DelegateOP(A_INX, O_LAX)},
                {0xA4,new DelegateOP(A_ZPG, O_LDY)},
                {0xA5,new DelegateOP(A_ZPG, O_LDA)},
                {0xA6,new DelegateOP(A_ZPG, O_LDX)},
                {0xA7,new DelegateOP(A_ZPG, O_LAX)},

                {0xA8,new DelegateOP(A_IMP, O_TAY)},
                {0xA9,new DelegateOP(A_IMM, O_LDA)},
                {0xAA,new DelegateOP(A_IMP, O_TAX)},
                {0xAB,new DelegateOP(A_IMM, O_LAX)},
                {0xAC,new DelegateOP(A_ABS, O_LDY)},
                {0xAD,new DelegateOP(A_ABS, O_LDA)},
                {0xAE,new DelegateOP(A_ABS, O_LDX)},
                {0xAF,new DelegateOP(A_ABS, O_LAX)},

                {0xB0,new DelegateOP(A_REL, O_BCS)},
                {0xB1,new DelegateOP(A_INY, O_LDA)},
                {0xB2,new DelegateOP(A_UNK, O_UNK)},
                {0xB3,new DelegateOP(A_INY, O_LAX)},
                {0xB4,new DelegateOP(A_ZPX, O_LDY)},
                {0xB5,new DelegateOP(A_ZPX, O_LDA)},
                {0xB6,new DelegateOP(A_ZPY, O_LDX)},
                {0xB7,new DelegateOP(A_ZPY, O_LAX)},

                {0xB8,new DelegateOP(A_IMP, O_CLV)},
                {0xB9,new DelegateOP(A_ABY, O_LDA)},
                {0xBA,new DelegateOP(A_IMP, O_TSX)},
                {0xBB,new DelegateOP(A_ABY, O_LAS)},
                {0xBC,new DelegateOP(A_ABX, O_LDY)},
                {0xBD,new DelegateOP(A_ABX, O_LDA)},
                {0xBE,new DelegateOP(A_ABY, O_LDX)},
                {0xBF,new DelegateOP(A_ABY, O_LAX)},

                {0xC0,new DelegateOP(A_IMM, O_CPY)},
                {0xC1,new DelegateOP(A_INX, O_CMP)},
                {0xC2,new DelegateOP(A_IMM, O_NOP)},
                {0xC3,new DelegateOP(A_INX, O_DCP)},
                {0xC4,new DelegateOP(A_ZPG, O_CPY)},
                {0xC5,new DelegateOP(A_ZPG, O_CMP)},
                {0xC6,new DelegateOP(A_ZPG, O_DEC)},
                {0xC7,new DelegateOP(A_ZPG, O_DCP)},

                {0xC8,new DelegateOP(A_IMP, O_INY)},
                {0xC9,new DelegateOP(A_IMM, O_CMP)},
                {0xCA,new DelegateOP(A_IMP, O_DEX)},
                {0xCB,new DelegateOP(A_IMM, O_AXS)},
                {0xCC,new DelegateOP(A_ABS, O_CPY)},
                {0xCD,new DelegateOP(A_ABS, O_CMP)},
                {0xCE,new DelegateOP(A_ABS, O_DEC)},
                {0xCF,new DelegateOP(A_ABS, O_DCP)},

                {0xD0,new DelegateOP(A_REL, O_BNE)},
                {0xD1,new DelegateOP(A_INY, O_CMP)},
                {0xD2,new DelegateOP(A_UNK, O_UNK)},
                {0xD3,new DelegateOP(A_iny, O_DCP)},
                {0xD4,new DelegateOP(A_ZPX, O_NOP)},
                {0xD5,new DelegateOP(A_ZPX, O_CMP)},
                {0xD6,new DelegateOP(A_ZPX, O_DEC)},
                {0xD7,new DelegateOP(A_ZPX, O_DCP)},

                {0xD8,new DelegateOP(A_IMP, O_CLD)},
                {0xD9,new DelegateOP(A_ABY, O_CMP)},
                {0xDA,new DelegateOP(A_IMP, O_NOP)},
                {0xDB,new DelegateOP(A_aby, O_DCP)},
                {0xDC,new DelegateOP(A_ABX, O_NOP)},
                {0xDD,new DelegateOP(A_ABX, O_CMP)},
                {0xDE,new DelegateOP(A_ABX, O_DEC)},
                {0xDF,new DelegateOP(A_abx, O_DCP)},

                {0xE0,new DelegateOP(A_IMM, O_CPX)},
                {0xE1,new DelegateOP(A_INX, O_SBC)},
                {0xE2,new DelegateOP(A_IMM, O_NOP)},
                {0xE3,new DelegateOP(A_INX, O_ISB)},
                {0xE4,new DelegateOP(A_ZPG, O_CPX)},
                {0xE5,new DelegateOP(A_ZPG, O_SBC)},
                {0xE6,new DelegateOP(A_ZPG, O_INC)},
                {0xE7,new DelegateOP(A_ZPG, O_ISB)},

                {0xE8,new DelegateOP(A_IMP, O_INX)},
                {0xE9,new DelegateOP(A_IMM, O_SBC)},
                {0xEA,new DelegateOP(A_IMP, O_NOP)},
                {0xEB,new DelegateOP(A_IMM, O_SBC)},
                {0xEC,new DelegateOP(A_ABS, O_CPX)},
                {0xED,new DelegateOP(A_ABS, O_SBC)},
                {0xEE,new DelegateOP(A_ABS, O_INC)},
                {0xEF,new DelegateOP(A_ABS, O_ISB)},

                {0xF0,new DelegateOP(A_REL, O_BEQ)},
                {0xF1,new DelegateOP(A_INY, O_SBC)},
                {0xF2,new DelegateOP(A_UNK, O_UNK)},
                {0xF3,new DelegateOP(A_iny, O_ISB)},
                {0xF4,new DelegateOP(A_ZPX, O_NOP)},
                {0xF5,new DelegateOP(A_ZPX, O_SBC)},
                {0xF6,new DelegateOP(A_ZPX, O_INC)},
                {0xF7,new DelegateOP(A_ZPX, O_ISB)},

                {0xF8,new DelegateOP(A_IMP, O_SED)},
                {0xF9,new DelegateOP(A_ABY, O_SBC)},
                {0xFA,new DelegateOP(A_IMP, O_NOP)},
                {0xFB,new DelegateOP(A_aby, O_ISB)},
                {0xFC,new DelegateOP(A_ABX, O_NOP)},
                {0xFD,new DelegateOP(A_ABX, O_SBC)},
                {0xFE,new DelegateOP(A_ABX, O_INC)},
                {0xFF,new DelegateOP(A_abx, O_ISB)},
            };
        }
        /// <summary>
        /// FCs the cpu execute one.
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        public void Fc_cpu_execute_one()
        {
            //疑似有bug會導致雙節龍無法執行
            /*if(m_cpu.Registers.irq_counter > 0)
            {
                --m_cpu.Registers.irq_counter;
                if (m_cpu.Registers.irq_counter == 0)
                {
                    m_cpu.Registers.irq_in_process = 1;
                    O_IRQ();
                    return;
                }
            }*/

            byte opcode = (byte)SFC_READ_PC(m_cpu.Registers.program_counter++);
            UInt32 cycle_add = 0;
            cycle_add += OpBasicCycle[opcode];
            OpMaps[opcode].execOpeation(ref cycle_add);
            m_cpu.m_famicom.cpu_cycle_count += cycle_add;
        }
        /// if中判斷用FLAG
        private int SFC_CF()
        {
            return m_cpu.Registers.status & (byte)Fc_status_flag.FC_FLAG_C;
        }

        private int SFC_ZF()
        {
            return m_cpu.Registers.status & (byte)Fc_status_flag.FC_FLAG_Z;
        }
        private int SFC_IF()
        {
            return m_cpu.Registers.status & (byte)Fc_status_flag.FC_FLAG_I;
        }
        private int SFC_DF()
        {
            return m_cpu.Registers.status & (byte)Fc_status_flag.FC_FLAG_D;
        }

        private int SFC_BF()
        {
            return m_cpu.Registers.status & (byte)Fc_status_flag.FC_FLAG_B;
        }
        private int SFC_VF()
        {
            return m_cpu.Registers.status & (byte)Fc_status_flag.FC_FLAG_V;
        }

        private int SFC_SF()
        {
            return m_cpu.Registers.status & (byte)Fc_status_flag.FC_FLAG_S;
        }
        // 將FLAG將變為1
        private void SFC_CF_SE()
        {
            m_cpu.Registers.status |= (byte)Fc_status_flag.FC_FLAG_C;
        }

        private void SFC_ZF_SE()
        {
            m_cpu.Registers.status |= (byte)Fc_status_flag.FC_FLAG_Z;
        }
        private void SFC_IF_SE()
        {
            m_cpu.Registers.status |= (byte)Fc_status_flag.FC_FLAG_I;
        }
        private void SFC_DF_SE()
        {
            m_cpu.Registers.status |= (byte)Fc_status_flag.FC_FLAG_D;
        }
        private void SFC_BF_SE()
        {
            m_cpu.Registers.status |= (byte)Fc_status_flag.FC_FLAG_B;
        }
        private void SFC_RF_SE()
        {
            m_cpu.Registers.status |= (byte)Fc_status_flag.FC_FLAG_R;
        }
        private void SFC_VF_SE()
        {
            m_cpu.Registers.status |= (byte)Fc_status_flag.FC_FLAG_V;
        }
        private void SFC_SF_SE()
        {
            m_cpu.Registers.status |= (byte)Fc_status_flag.FC_FLAG_S;
        }
        // 將FLAG將變為0
        private void SFC_CF_CL()
        {
            var tmp = ~(byte)Fc_status_flag.FC_FLAG_C;
            m_cpu.Registers.status &= (byte)tmp;
        }
        private void SFC_ZF_CL()
        {
            var tmp = ~(byte)Fc_status_flag.FC_FLAG_Z;
            m_cpu.Registers.status &= (byte)tmp;
        }

        private void SFC_IF_CL()
        {
            var tmp = ~(byte)Fc_status_flag.FC_FLAG_I;
            m_cpu.Registers.status &= (byte)tmp;
        }

        private void SFC_DF_CL()
        {
            var tmp = ~(byte)Fc_status_flag.FC_FLAG_D;
            m_cpu.Registers.status &= (byte)tmp;
        }

        private void SFC_BF_CL()
        {
            var tmp = ~(byte)Fc_status_flag.FC_FLAG_B;
            m_cpu.Registers.status &= (byte)tmp;
        }

        private void SFC_VF_CL()
        {
            var tmp = ~(byte)Fc_status_flag.FC_FLAG_V;
            m_cpu.Registers.status &= (byte)tmp;
        }

        private void SFC_SF_CL()
        {
            var tmp = ~(byte)Fc_status_flag.FC_FLAG_S;
            m_cpu.Registers.status &= (byte)tmp;
        }
        // 將FLAG將變為0或者1
        private void SFC_CF_IF(bool x)
        {
            if (x) SFC_CF_SE(); else SFC_CF_CL();
        }
        private void SFC_ZF_IF(bool x)
        {
            if (x) SFC_ZF_SE(); else SFC_ZF_CL();
        }
        private void SFC_OF_IF(bool x)
        {
            if (x) SFC_IF_SE(); else SFC_IF_CL();
        }

        private void SFC_DF_IF(bool x)
        {
            if (x) SFC_DF_SE(); else SFC_DF_CL();
        }
        private void SFC_BF_IF(bool x)
        {
            if (x) SFC_BF_SE(); else SFC_BF_CL();
        }
        private void SFC_VF_IF(bool x)
        {
            if (x) SFC_VF_SE(); else SFC_VF_CL();
        }

        private void SFC_SF_IF(bool x)
        {
            if (x) SFC_SF_SE(); else SFC_SF_CL();
        }
        // 實用函數
        private void CHECK_ZSFLAG(byte x)
        {
            SFC_SF_IF((x & 0x80) > 0);
            SFC_ZF_IF(x == 0);
        }

        private UInt16 SFC_READ(UInt16 address)
        {
            return m_cpu.Read_Cpu_Address(address);
        }

        private UInt16 SFC_READ_PC(UInt16 address)
        {
            return m_cpu.Read_Prg_Address(address);
        }

        private void SFC_WRITE(UInt16 address, byte data)
        {
            m_cpu.Write_Cpu_Address(address, data);
        }
        private void SFC_PUSH(byte value)
        {
            m_cpu.m_famicom.m_mainMemory[0x100 + m_cpu.Registers.stack_pointer--] = value;
        }
        private byte SFC_POP()
        {
            return m_cpu.m_famicom.m_mainMemory[0x100 + (++m_cpu.Registers.stack_pointer)];
        }
        /// <summary>
        /// 尋址方式: 未知
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_UNK(ref UInt32 cycle_add)
        {
            Debug.Assert(true, "UNKNOWN ADDRESSING MODE");
            return 0;
        }
        /// <summary>
        /// 尋址方式: 累加器
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_ACC(ref UInt32 cycle_add)
        {
            return 0;
        }
        // <summary>
        /// 尋址方式: 隱含尋址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_IMP(ref UInt32 cycle_add)
        {
            return 0;
        }
        /// <summary>
        /// 尋址方式: 立即尋址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_IMM(ref UInt32 cycle_add)
        {
            UInt16 address = m_cpu.Registers.program_counter;
            m_cpu.Registers.program_counter++;
            return address;
        }

        /// <summary>
        /// 尋址方式: 絕對尋址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_ABS(ref UInt32 cycle_add)
        {
            UInt16 address0 = SFC_READ_PC(m_cpu.Registers.program_counter++);
            UInt16 address1 = SFC_READ_PC(m_cpu.Registers.program_counter++);
            return (UInt16)(address0 | (address1 << 8));
        }

        /// <summary>
        /// 尋址方式: 絕對X變址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_ABX(ref UInt32 cycle_add)
        {
            UInt16 baseaddress = A_ABS(ref cycle_add);
            UInt16 rvar =  (UInt16)(baseaddress + m_cpu.Registers.x_index);
            cycle_add += (UInt32)(((baseaddress ^ rvar) >> 8) & 1);
            return rvar;
        }

        /// <summary>
        /// 尋址方式: 絕對X變址--none cycle
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_abx(ref UInt32 cycle_add)
        {
            UInt16 baseaddress = A_ABS(ref cycle_add);
            UInt16 rvar = (UInt16)(baseaddress + m_cpu.Registers.x_index);
            return rvar;
        }
        /// <summary>
        /// 尋址方式: 絕對Y變址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_ABY(ref UInt32 cycle_add)
        {
            UInt16 baseaddress = A_ABS(ref cycle_add);
            UInt16 rvar = (UInt16)(baseaddress + m_cpu.Registers.y_index);
            cycle_add += (UInt32)(((baseaddress ^ rvar) >> 8) & 1);
            return rvar;
        }

        /// <summary>
        /// 尋址方式: 絕對Y變址--nonecycle
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_aby(ref UInt32 cycle_add)
        {
            UInt16 baseaddress = A_ABS(ref cycle_add);
            UInt16 rvar = (UInt16)(baseaddress + m_cpu.Registers.y_index);
            //cycle_add += (UInt32)(((baseaddress ^ rvar) >> 8) & 1);
            return rvar;
        }

        /// <summary>
        /// 尋址方式: 零頁尋址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_ZPG(ref UInt32 cycle_add)
        {
            UInt16 address = SFC_READ_PC(m_cpu.Registers.program_counter++);
            return address;
        }

        /// <summary>
        /// 尋址方式: 零頁X變址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_ZPX(ref UInt32 cycle_add)
        {
            UInt16 baseaddress = A_ZPG(ref cycle_add);
            UInt16 index = (UInt16)(baseaddress + m_cpu.Registers.x_index);
            return (UInt16)(index & 0x00FF);
        }

        /// <summary>
        /// 尋址方式: 零頁Y變址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_ZPY(ref UInt32 cycle_add)
        {
            UInt16 baseaddress = A_ZPG(ref cycle_add);
            UInt16 index = (UInt16)(baseaddress + m_cpu.Registers.y_index);
            return (UInt16)(index & 0x00FF);
        }

        /// <summary>
        /// 尋址方式: 間接X變址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_INX(ref UInt32 cycle_add)
        {
            byte baseaddress = (byte)(SFC_READ_PC(m_cpu.Registers.program_counter++) + m_cpu.Registers.x_index);
            byte address0 = (byte)SFC_READ(baseaddress++);
            byte address1 = (byte)SFC_READ(baseaddress++);
            return (UInt16)(address0 | (address1 << 8));
        }

        /// <summary>
        /// 尋址方式: 間接Y變址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_INY(ref UInt32 cycle_add)
        {
            byte baseaddress = (byte)SFC_READ_PC(m_cpu.Registers.program_counter++);
            byte address0 = (byte)SFC_READ(baseaddress++);
            byte address1 = (byte)SFC_READ(baseaddress++);
            UInt16 address = (UInt16)(address0 | (address1 << 8));
            UInt16 rvar = (UInt16)(address + m_cpu.Registers.y_index);
            cycle_add += (UInt32)(((baseaddress ^ rvar) >> 8) & 1);
            return rvar;
        }

        /// <summary>
        /// 尋址方式: 間接Y變址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_iny(ref UInt32 cycle_add)
        {
            byte baseaddress = (byte)SFC_READ_PC(m_cpu.Registers.program_counter++);
            byte address0 = (byte)SFC_READ(baseaddress++);
            byte address1 = (byte)SFC_READ(baseaddress++);
            UInt16 address = (UInt16)(address0 | (address1 << 8));
            UInt16 rvar = (UInt16)(address + m_cpu.Registers.y_index);
            //cycle_add += (UInt32)(((baseaddress ^ rvar) >> 8) & 1);
            return rvar;
        }

        /// <summary>
        /// 尋址方式: 間接尋址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_IND(ref UInt32 cycle_add)
        {
            // 讀取地址
            UInt16 base1 = A_ABS(ref cycle_add);
            // 刻意實現6502的BUG
            UInt16 base2 = (UInt16)((base1 & 0xFF00) | ((base1 + 1) & 0x00FF));
            // 讀取地址
            UInt16 address = (UInt16)(SFC_READ(base1) | (SFC_READ(base2) << 8));
            return address;
        }

        /// <summary>
        /// 尋址方式: 相對尋址
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        private UInt16 A_REL(ref UInt32 cycle_add)
        {
            UInt16 data = SFC_READ_PC(m_cpu.Registers.program_counter++);
            UInt16 address = (UInt16)(m_cpu.Registers.program_counter + (sbyte)data);
            return address;
        }

        // ---------------------------------- 指令

        /// <summary>
        /// UNK: Unknown
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_UNK(UInt16 address, ref UInt32 cycle_add)
        {
            Debug.Assert(true, "UNKNOWN INS");
        }
        /// <summary>
        /// SHY
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_SHY(UInt16 address, ref UInt32 cycle_add)
        {
            O_UNK(address,ref cycle_add);
        }
        /// <summary>
        /// SHX
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_SHX(UInt16 address, ref UInt32 cycle_add)
        {
            O_UNK(address, ref cycle_add);
        }

        /// <summary>
        /// TAS
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_TAS(UInt16 address, ref UInt32 cycle_add)
        {
            O_UNK(address, ref cycle_add);
        }

        /// <summary>
        /// AHX
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_AHX(UInt16 address, ref UInt32 cycle_add)
        {
            O_UNK(address, ref cycle_add);
        }

        /// <summary>
        /// XAA
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_XAA(UInt16 address, ref UInt32 cycle_add)
        {
            O_UNK(address, ref cycle_add);
        }

        /// <summary>
        /// LAS
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_LAS(UInt16 address, ref UInt32 cycle_add)
        {
            O_UNK(address, ref cycle_add);
        }
        /// <summary>
        /// SRE: Shift Right then "Exclusive-Or" - LSR + EOR
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_SRE(UInt16 address, ref UInt32 cycle_add)
        {
            // LSR
            byte data = (byte)SFC_READ(address);
            SFC_CF_IF((data & 1) > 0);
            data >>= 1;
            SFC_WRITE(address, data);
            // EOR
            m_cpu.Registers.accumulator ^= data;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// SLO - Shift Left then 'Or' - ASL + ORA
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_SLO(UInt16 address, ref UInt32 cycle_add)
        {
            // ASL
            byte data = (byte)SFC_READ(address);
            SFC_CF_IF((data & 0x80) > 0);
            data <<= 1;
            SFC_WRITE(address, data);
            // ORA
            m_cpu.Registers.accumulator |= data;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// RRA: Rotate Right then Add with Carry - ROR + ADC
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_RRA(UInt16 address, ref UInt32 cycle_add)
        {
            // ROR
            UInt16 result16_ror = SFC_READ(address);
            result16_ror |= (UInt16)(SFC_CF() << (UInt16)(8 - Fc_status_index.FC_INDEX_C));
            UInt16 tmpcf = (UInt16)(result16_ror & 1);
            result16_ror >>= 1;
            byte result8_ror = (byte)result16_ror;
            SFC_WRITE(address, result8_ror);
            // ADC
            byte src = result8_ror;
            UInt16 result16 = (UInt16)(m_cpu.Registers.accumulator + src + tmpcf);
            SFC_CF_IF((result16 >> 8) > 0);
            byte result8 = (byte)result16;
            int v1 = (m_cpu.Registers.accumulator ^ src) & 0x80;
            int v2 = (m_cpu.Registers.accumulator ^ result8) & 0x80;
            bool v3 = v1 > 0;
            bool v4 = v2 > 0;
            SFC_VF_IF(!v3 && v4);
            m_cpu.Registers.accumulator = result8;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// RLA: Rotate Left then 'And' - ROL + AND
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_RLA(UInt16 address, ref UInt32 cycle_add)
        {
            // ROL
            UInt16 result16 = SFC_READ(address);
            result16 <<= 1;
            result16 |= (UInt16)(SFC_CF() >> (UInt16)(Fc_status_index.FC_INDEX_C));
            SFC_CF_IF((result16 & 0x100) > 0);
            byte result8 = (byte)result16;
            SFC_WRITE(address, result8);
            // AND
            m_cpu.Registers.accumulator &= result8;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// ISB: Increment memory then Subtract with Carry - INC + SBC
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ISB(UInt16 address, ref UInt32 cycle_add)
        {
            // INC
            byte data = (byte)SFC_READ(address);
            ++data;
            SFC_WRITE(address, data);
            // SBC
            byte src = data;
            UInt16 result16 = (UInt16)(m_cpu.Registers.accumulator - src - (SFC_CF() > 0 ? 0 : 1));
            SFC_CF_IF(!((result16 >> 8) > 0));
            byte result8 = (byte)result16;
            int v1 = ((m_cpu.Registers.accumulator ^ src) & 0x80);
            int v2 = ((m_cpu.Registers.accumulator ^ result8) & 0x80);
            bool v3 = v1 > 0;
            bool v4 = v2 > 0;
            SFC_VF_IF(v3 && v4);
            m_cpu.Registers.accumulator = result8;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// ISC
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ISC(UInt16 address, ref UInt32 cycle_add)
        {
            O_UNK(address,ref cycle_add);
        }

        /// <summary>
        /// DCP: Decrement memory then Compare with A - DEC + CMP
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_DCP(UInt16 address, ref UInt32 cycle_add)
        {
            // DEC
            byte data = (byte)SFC_READ(address);
            --data;
            SFC_WRITE(address, data);
            // CMP
            UInt16 result16 = (UInt16)(m_cpu.Registers.accumulator - data);
            SFC_CF_IF(!((result16 & 0x8000) > 0));
            CHECK_ZSFLAG((byte)result16);
        }

        /// <summary>
        /// SAX: Store A 'And' X - 
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_SAX(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_WRITE(address, (byte)(m_cpu.Registers.accumulator & m_cpu.Registers.x_index));
        }

        /// <summary>
        /// LAX: Load 'A' then Transfer X - LDA  + TAX
        /// </summary>
        /// <remarks>
        /// 非法指令
        /// </remarks>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_LAX(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.accumulator = (byte)SFC_READ(address);
            m_cpu.Registers.x_index = m_cpu.Registers.accumulator;
            CHECK_ZSFLAG(m_cpu.Registers.x_index);
        }

        /// <summary>
        /// SBX
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_SBX(UInt16 address, ref UInt32 cycle_add)
        {
            O_UNK(address,ref cycle_add);
        }

        /// <summary>
        /// AXS
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_AXS(UInt16 address, ref UInt32 cycle_add)
        {
           UInt16 tmp = (UInt16)((UInt16)(m_cpu.Registers.accumulator & m_cpu.Registers.x_index) - SFC_READ_PC(address));
           m_cpu.Registers.x_index = (byte)tmp;
           CHECK_ZSFLAG(m_cpu.Registers.x_index);
           SFC_CF_IF((tmp & 0x8000) == 0);
        }

        /// <summary>
        /// ARR
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ARR(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.accumulator &= (byte)SFC_READ_PC(address);
            SFC_CF();
            m_cpu.Registers.accumulator = (byte)((m_cpu.Registers.accumulator >> 1) | (SFC_CF() << 7));
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
            SFC_CF_IF(((m_cpu.Registers.accumulator >> 6) & 1) > 0);
            SFC_VF_IF((((m_cpu.Registers.accumulator >> 6)^ (m_cpu.Registers.accumulator >> 5))&1) > 0);
        }

        /// <summary>
        /// AAC
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_AAC(UInt16 address, ref UInt32 cycle_add)
        {
            O_UNK(address,ref cycle_add);
        }

        /// <summary>
        /// ANC
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ANC(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.accumulator &= (byte)SFC_READ_PC(address);
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
            SFC_CF_IF(SFC_SF()> 0);
        }

        /// <summary>
        /// ASR
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ASR(UInt16 address,ref UInt32 cycle_add)
        {
            m_cpu.Registers.accumulator &= (byte)SFC_READ_PC(address);
            SFC_CF_IF((m_cpu.Registers.accumulator& 1) > 0);
            m_cpu.Registers.accumulator >>= 1;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// ALR
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ALR(UInt16 address, ref UInt32 cycle_add)
        {
            O_UNK(address,ref cycle_add);
        }

        /// <summary>
        /// RTI: Return from I
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_RTI(UInt16 address, ref UInt32 cycle_add)
        {
            // P
            m_cpu.Registers.status = SFC_POP();
            SFC_RF_SE();
            SFC_BF_CL();
            // PC
            byte pcl = SFC_POP();
            byte pch = SFC_POP();
            m_cpu.Registers.program_counter = (UInt16)(pcl | (UInt16)pch << 8);

            m_cpu.Registers.irq_counter = (byte)(m_cpu.Registers.irq_in_process & m_cpu.Registers.irq_flag);
            m_cpu.Registers.irq_counter &= (byte)((~m_cpu.Registers.status) >> (byte)Fc_status_index.FC_INDEX_I);
            m_cpu.Registers.irq_in_process = 0;
        }

        /// <summary>
        /// BRK
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_BRK(UInt16 address, ref UInt32 cycle_add)
        {
            UInt16 pcp1 = (UInt16)(m_cpu.Registers.program_counter+1);
            byte pch = (byte)(pcp1 >> 8);
            byte pcl = (byte)pcp1;
            SFC_PUSH(pch);
            SFC_PUSH(pcl);
            SFC_PUSH((byte)(m_cpu.Registers.status | (byte)Fc_status_flag.FC_FLAG_R| (byte)Fc_status_flag.FC_FLAG_B));
            SFC_IF_SE();
            byte pc12 = (byte)SFC_READ_PC((UInt16)Fc_cpu_vector.FC_VERCTOR_BRK + 0);
            byte pch2 = (byte)SFC_READ_PC((UInt16)Fc_cpu_vector.FC_VERCTOR_BRK + 1);
            m_cpu.Registers.program_counter = (UInt16)(pc12 | pch2 << 8);
        }

        /// <summary>
        /// NOP: No Operation
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_NOP(UInt16 address, ref UInt32 cycle_add)
        {

        }

        /// <summary>
        /// RTS: Return from Subroutine
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_RTS(UInt16 address, ref UInt32 cycle_add)
        {
            byte pcl = SFC_POP();
            byte pch = SFC_POP();
            m_cpu.Registers.program_counter = (UInt16)(pcl| pch << 8);
            m_cpu.Registers.program_counter++;
        }

        /// <summary>
        /// JSR: Jump to Subroutine
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_JSR(UInt16 address, ref UInt32 cycle_add)
        {
            UInt16 pc1 = (UInt16)(m_cpu.Registers.program_counter - 1);
            SFC_PUSH((byte)(pc1 >> 8));
            SFC_PUSH((byte)(pc1));
            m_cpu.Registers.program_counter = address;
        }
        /// <summary>
        /// O_branch
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_branch(UInt16 address, ref UInt32 cycle_add)
        {
            UInt16 saved = m_cpu.Registers.program_counter;
            m_cpu.Registers.program_counter = address;
            ++cycle_add;
            cycle_add +=(UInt32)((address ^ saved) >> 8 & 1);
        }

        /// <summary>
        /// BVC: Branch if Overflow Clear
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_BVC(UInt16 address, ref UInt32 cycle_add)
        {
            if (!(SFC_VF() > 0)) O_branch(address,ref cycle_add);
        }

        /// <summary>
        /// BVC: Branch if Overflow Set
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_BVS(UInt16 address, ref UInt32 cycle_add)
        {
            if (SFC_VF() > 0) O_branch(address, ref cycle_add);
        }

        /// <summary>
        /// BPL: Branch if Plus
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_BPL(UInt16 address, ref UInt32 cycle_add)
        {
            if (!(SFC_SF() > 0)) O_branch(address, ref cycle_add);
        }

        /// <summary>
        /// BMI: Branch if Minus
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_BMI(UInt16 address, ref UInt32 cycle_add)
        {
            if (SFC_SF() > 0) O_branch(address, ref cycle_add);
        }

        /// <summary>
        /// BCC: Branch if Carry Clear
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_BCC(UInt16 address, ref UInt32 cycle_add)
        {
            if (!(SFC_CF() > 0)) O_branch(address, ref cycle_add);
        }

        /// <summary>
        /// BCS: Branch if Carry Set
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_BCS(UInt16 address, ref UInt32 cycle_add)
        {
            if (SFC_CF() > 0) O_branch(address, ref cycle_add);
        }

        /// <summary>
        /// BNE: Branch if Not Equal
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_BNE(UInt16 address, ref UInt32 cycle_add)
        {
            if (!(SFC_ZF() > 0)) O_branch(address, ref cycle_add);
        }

        /// <summary>
        /// BEQ: Branch if Equal
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_BEQ(UInt16 address, ref UInt32 cycle_add)
        {
            if (SFC_ZF() > 0) O_branch(address, ref cycle_add);
        }

        /// <summary>
        /// JMP
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_JMP(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.program_counter = address;
        }

        /// <summary>
        /// PLP: Pull P
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_PLP(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.status = SFC_POP();
            SFC_RF_SE();
            SFC_BF_CL();
            if(!(SFC_IF() > 0))
            m_cpu.Registers.irq_counter = (byte)(m_cpu.Registers.irq_flag << 1);
        }

        /// <summary>
        /// PHP: Push P
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_PHP(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_PUSH((byte)(m_cpu.Registers.status | (byte)(Fc_status_flag.FC_FLAG_R | Fc_status_flag.FC_FLAG_B)));
        }

        /// <summary>
        /// PLA: Pull A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_PLA(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.accumulator = SFC_POP();
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// PHA: Push A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_PHA(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_PUSH(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// ROR A : Rotate Right for A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_RORA(UInt16 address, ref UInt32 cycle_add)
        {
            UInt16 result16 = m_cpu.Registers.accumulator;
            result16 |= (UInt16)(SFC_CF() << (UInt16)(8 - Fc_status_index.FC_INDEX_C));
            SFC_CF_IF((result16 & 1) > 0);
            result16 >>= 1;
            m_cpu.Registers.accumulator = (byte)result16;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// ROR: Rotate Right
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ROR(UInt16 address, ref UInt32 cycle_add)
        {
            UInt16 result16 = SFC_READ(address);
            result16 |= (UInt16)(SFC_CF() << (UInt16)(8 - Fc_status_index.FC_INDEX_C));
            SFC_CF_IF((result16 & 1) > 0);
            result16 >>= 1;
            byte result8 = (byte)result16;
            SFC_WRITE(address, result8);
            CHECK_ZSFLAG(result8);
        }

        /// <summary>
        /// ROL: Rotate Left
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ROL(UInt16 address, ref UInt32 cycle_add)
        {
            UInt16 result16 = SFC_READ(address);
            result16 <<= 1;
            result16 |= (UInt16)(SFC_CF() >> (UInt16)(Fc_status_index.FC_INDEX_C));
            SFC_CF_IF((result16 & 0x100) > 0);
            byte result8 = (byte)result16;
            SFC_WRITE(address, result8);
            CHECK_ZSFLAG(result8);
        }

        /// <summary>
        /// ROL A : Rotate Left for A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ROLA(UInt16 address, ref UInt32 cycle_add)
        {
            UInt16 result16 = m_cpu.Registers.accumulator;
            result16 <<= 1;
            result16 |= (UInt16)(SFC_CF() >> (UInt16)(Fc_status_index.FC_INDEX_C));
            SFC_CF_IF((result16 & 0x100) > 0);
            m_cpu.Registers.accumulator = (byte)result16;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// LSR: Logical Shift Right
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_LSR(UInt16 address, ref UInt32 cycle_add)
        {
            byte data = (byte)SFC_READ(address);
            SFC_CF_IF((data & 1) > 0);
            data >>= 1;
            SFC_WRITE(address, data);
            CHECK_ZSFLAG(data);
        }

        /// <summary>
        /// LSR A : Logical Shift Right for A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_LSRA(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_CF_IF((m_cpu.Registers.accumulator & 1) > 0);
            m_cpu.Registers.accumulator >>= 1;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// ASL: Arithmetic Shift Left
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ASL(UInt16 address, ref UInt32 cycle_add)
        {
            byte data = (byte)SFC_READ(address);
            SFC_CF_IF((data & 0x80) > 0);
            data <<= 1;
            SFC_WRITE(address, data);
            CHECK_ZSFLAG(data);
        }

        /// <summary>
        /// ASL A : Arithmetic Shift Left for A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ASLA(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_CF_IF((m_cpu.Registers.accumulator & (byte)0x80) > 0);
            m_cpu.Registers.accumulator <<= 1;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// BIT: Bit Test
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_BIT(UInt16 address, ref UInt32 cycle_add)
        {
            byte value = (byte)SFC_READ(address);
            SFC_VF_IF((value & (byte)(1 << 6)) > 0);
            SFC_SF_IF((value & (byte)(1 << 7)) > 0);
            SFC_ZF_IF(!((m_cpu.Registers.accumulator & value) > 0));
        }

        /// <summary>
        /// CPY: Compare memory with Y
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_CPY(UInt16 address, ref UInt32 cycle_add)
        {
            UInt16 result16 = (UInt16)(m_cpu.Registers.y_index - SFC_READ(address));
            SFC_CF_IF(!((result16 & 0x8000) > 0));
            CHECK_ZSFLAG((byte)result16);
        }

        /// <summary>
        /// CPX
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_CPX(UInt16 address, ref UInt32 cycle_add)
        {
            UInt16 result16 = (UInt16)(m_cpu.Registers.x_index - SFC_READ(address));
            SFC_CF_IF(!((result16 & (UInt16)0x8000) > 0));
            CHECK_ZSFLAG((byte)result16);
        }

        /// <summary>
        /// CMP: Compare memory with A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_CMP(UInt16 address, ref UInt32 cycle_add)
        {
            UInt16 result16 = (UInt16)(m_cpu.Registers.accumulator - SFC_READ(address));
            SFC_CF_IF(!((result16 & (UInt16)0x8000) > 0));
            CHECK_ZSFLAG((byte)result16);
        }

        /// <summary>
        /// SEI: Set I
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_SEI(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_IF_SE();
        }

        /// <summary>
        /// CLI - Clear I
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_CLI(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_IF_CL();
            m_cpu.Registers.irq_counter = (byte)(m_cpu.Registers.irq_flag << 1);
        }

        /// <summary>
        /// CLV: Clear V
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_CLV(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_VF_CL();
        }

        /// <summary>
        /// SED: Set D
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_SED(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_DF_SE();
        }

        /// <summary>
        /// CLD: Clear D
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_CLD(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_DF_CL();
        }

        /// <summary>
        /// SEC: Set Carry
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_SEC(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_CF_SE();
        }

        /// <summary>
        /// CLC: Clear Carry
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_CLC(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_CF_CL();
        }

        /// <summary>
        /// EOR: "Exclusive-Or" memory with A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_EOR(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.accumulator ^= (byte)SFC_READ(address);
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// ORA: 'Or' memory with A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ORA(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.accumulator |= (byte)SFC_READ(address);
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// AND: 'And' memory with A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_AND(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.accumulator &= (byte)SFC_READ(address);
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// DEY: Decrement Y
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_DEY(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.y_index--;
            CHECK_ZSFLAG(m_cpu.Registers.y_index);
        }

        /// <summary>
        /// INY:  Increment Y
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_INY(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.y_index++;
            CHECK_ZSFLAG(m_cpu.Registers.y_index);
        }

        /// <summary>
        /// DEX: Decrement X
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_DEX(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.x_index--;
            CHECK_ZSFLAG(m_cpu.Registers.x_index);
        }

        /// <summary>
        /// INX
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_INX(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.x_index++;
            CHECK_ZSFLAG(m_cpu.Registers.x_index);
        }

        /// <summary>
        /// DEC: Decrement memory
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_DEC(UInt16 address, ref UInt32 cycle_add)
        {
            byte data = (byte)SFC_READ(address);
            --data;
            SFC_WRITE(address, data);
            CHECK_ZSFLAG(data);
        }

        /// <summary>
        /// INC: Increment memory
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_INC(UInt16 address, ref UInt32 cycle_add)
        {
            byte data = (byte)SFC_READ(address);
            ++data;
            SFC_WRITE(address, data);
            CHECK_ZSFLAG(data);
        }

        /// <summary>
        /// SBC: Subtract with Carry
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_SBC(UInt16 address, ref UInt32 cycle_add)
        {
            byte src = (byte)SFC_READ(address);
            UInt16 result16 = (UInt16)(m_cpu.Registers.accumulator - src - (SFC_CF() > 0 ? 0 : 1));
            SFC_CF_IF(!((result16 >> 8) > 0));
            byte result8 = (byte)result16;
            int v1 = (m_cpu.Registers.accumulator ^ src) & 0x80;
            int v2 = (m_cpu.Registers.accumulator ^ result8) & 0x80;
            SFC_VF_IF(v1 > 0 && v2 > 0);
            m_cpu.Registers.accumulator = result8;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// ADC: Add with Carry
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_ADC(UInt16 address, ref UInt32 cycle_add)
        {
            byte src = (byte)SFC_READ(address);
            UInt16 result16 = (UInt16)(m_cpu.Registers.accumulator + src + (SFC_CF() > 0 ? 1 : 0));
            SFC_CF_IF((result16 >> 8) > 0);
            byte result8 = (byte)result16;
            int v1 = (m_cpu.Registers.accumulator ^ src) & 0x80;
            int v2 = (m_cpu.Registers.accumulator ^ result8) & 0x80;
            SFC_VF_IF(!(v1 > 0) && (v2 > 0));
            m_cpu.Registers.accumulator = result8;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// TXS: Transfer X to SP
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_TXS(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.stack_pointer = m_cpu.Registers.x_index;
        }

        /// <summary>
        /// TSX: Transfer SP to X
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_TSX(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.x_index = m_cpu.Registers.stack_pointer;
            CHECK_ZSFLAG(m_cpu.Registers.x_index);
        }

        /// <summary>
        /// TYA: Transfer Y to A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_TYA(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.accumulator = m_cpu.Registers.y_index;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// TAY: Transfer A to Y
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_TAY(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.y_index = m_cpu.Registers.accumulator;
            CHECK_ZSFLAG(m_cpu.Registers.y_index);
        }

        /// <summary>
        /// TXA: Transfer X to A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_TXA(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.accumulator = m_cpu.Registers.x_index;
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// TAX: Transfer A to X
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_TAX(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.x_index = m_cpu.Registers.accumulator;
            CHECK_ZSFLAG(m_cpu.Registers.x_index);
        }

        /// <summary>
        /// STY: Store 'Y'
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_STY(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_WRITE(address, m_cpu.Registers.y_index);
        }

        /// <summary>
        /// STX: Store X
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_STX(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_WRITE(address, m_cpu.Registers.x_index);
        }

        /// <summary>
        /// STA: Store 'A'
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_STA(UInt16 address, ref UInt32 cycle_add)
        {
            SFC_WRITE(address, m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// LDY: Load 'Y'
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_LDY(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.y_index = (byte)SFC_READ(address);
            CHECK_ZSFLAG(m_cpu.Registers.y_index);
        }

        /// <summary>
        /// LDX: Load X
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_LDX(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.x_index = (byte)SFC_READ(address);
            CHECK_ZSFLAG(m_cpu.Registers.x_index);
        }

        /// <summary>
        /// LDA: Load A
        /// </summary>
        /// <param name="address">The address.</param>
        /// <param name="famicom">The famicom.</param>
        private void O_LDA(UInt16 address, ref UInt32 cycle_add)
        {
            m_cpu.Registers.accumulator = (byte)SFC_READ(address);
            CHECK_ZSFLAG(m_cpu.Registers.accumulator);
        }

        /// <summary>
        /// 特殊指令: NMI
        /// </summary>
        /// <param name="famicom">The famicom.</param>
        /// <returns></returns>
        public void O_NMI()
        {
            byte pch = (byte)((m_cpu.Registers.program_counter) >> 8);
            byte pcl = (byte)m_cpu.Registers.program_counter;
            SFC_PUSH(pch);
            SFC_PUSH(pcl);
            SFC_PUSH((byte)(m_cpu.Registers.status | (byte)Fc_status_flag.FC_FLAG_R));
            SFC_IF_SE();
            byte pcl2 = (byte)SFC_READ_PC((UInt16)Fc_cpu_vector.FC_VERCTOR_NMI + 0);
            byte pch2 = (byte)SFC_READ_PC((UInt16)(Fc_cpu_vector.FC_VERCTOR_NMI + 1));
            m_cpu.Registers.program_counter = (UInt16)(pcl2 | pch2 << 8);

            m_cpu.m_famicom.cpu_cycle_count += 7;
        }

        public void O_IRQ()
        {
            byte pch = (byte)((m_cpu.Registers.program_counter) >> 8);
            byte pcl = (byte)m_cpu.Registers.program_counter;
            SFC_PUSH(pch);
            SFC_PUSH(pcl);
            SFC_PUSH((byte)(m_cpu.Registers.status | (byte)Fc_status_flag.FC_FLAG_R));
            SFC_IF_SE();
            byte pcl2 = (byte)SFC_READ_PC((UInt16)Fc_cpu_vector.FC_VERCTOR_IRQ + 0);
            byte pch2 = (byte)SFC_READ_PC((UInt16)(Fc_cpu_vector.FC_VERCTOR_IRQ + 1));
            m_cpu.Registers.program_counter = (UInt16)(pcl2 | pch2 << 8);

            m_cpu.m_famicom.cpu_cycle_count += 7;
        }
        public void O_IRQ_ack()
        {
            m_cpu.Registers.irq_flag = 0;
            m_cpu.Registers.irq_counter = 0;
        }
        public void O_IRQ_try()
        {
            if (SFC_IF() > 0)
                m_cpu.Registers.irq_flag = 1;
            else
                m_cpu.Registers.irq_counter = 1;
        }
    }
}
