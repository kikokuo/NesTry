using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace NesTry
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Fc_6502_code_t
    {
        [FieldOffset(0)]
        public int data;
        [FieldOffset(0)]
        public byte op;
        [FieldOffset(1)]
        public byte a1;
        [FieldOffset(2)]
        public byte a2;
        [FieldOffset(3)]
        public byte ctrl;
    }
    /// <summary>
    /// StepFC: 6502指令
    /// </summary>
    public enum FC_6502_instruction
    {
        FC_INS_UNK = 0, // 未知指令
        FC_INS_LDA, // LDA--由存儲器取數送入累加器A M -> A
        FC_INS_LDX, // LDX--由存儲器取數送入寄存器X M -> X
        FC_INS_LDY, // LDY--由存儲器取數送入寄存器Y M -> Y
        FC_INS_STA, // STA--將累加器A的數送入存儲器 A -> M
        FC_INS_STX, // STX--將寄存器X的數送入存儲器 X -> M
        FC_INS_STY, // STY--將寄存器Y的數送入存儲器 Y -> M
        FC_INS_TAX, // 將累加器A的內容送入變址寄存器X
        FC_INS_TXA, // 將變址寄存器X的內容送入累加器A
        FC_INS_TAY, // 將累加器A的內容送入變址寄存器Y
        FC_INS_TYA, // 將變址寄存器Y的內容送入累加器A
        FC_INS_TSX, // 堆棧指針S的內容送入變址寄存器X
        FC_INS_TXS, // 變址寄存器X的內容送入堆棧指針S
        FC_INS_ADC, // ADC--累加器,存儲器,進位標誌C相加,結果送累加器A A+M+C -> A
        FC_INS_SBC, // SBC--從累加器減去存儲器和進位標誌C取反,結果送累加器 A-M-(1-C) -> A
        FC_INS_INC, // INC--存儲器單元內容增1 M+1 -> M
        FC_INS_DEC, // DEC--存儲器單元內容減1 M-1 -> M
        FC_INS_INX, // INX--X寄存器+1 X+1 -> X
        FC_INS_DEX, // DEX--X寄存器-1 X-1 -> X
        FC_INS_INY, // INY--Y寄存器+1 Y+1 -> Y
        FC_INS_DEY, // DEY--Y寄存器-1 Y-1 -> Y
        FC_INS_AND, // AND--存儲器與累加器相與,結果送累加器 A∧M -> A
        FC_INS_ORA, // ORA--存儲器與累加器相或,結果送累加器 A∨M -> A
        FC_INS_EOR, // EOR--存儲器與累加器異或,結果送累加器 A≮M -> A
        FC_INS_CLC, // CLC--清除進位標誌C 0 -> C
        FC_INS_SEC, // SEC--設置進位標誌C 1 -> C
        FC_INS_CLD, // CLD--清除十進標誌D 0 -> D
        FC_INS_SED, // SED--設置十進標誌D 1 -> D
        FC_INS_CLV, // CLV--清除溢出標誌V 0 -> V
        FC_INS_CLI, // CLI--清除中斷禁止V 0 -> I
        FC_INS_SEI, // SEI--設置中斷禁止V 1 -> I
        FC_INS_CMP, // CMP--累加器和存儲器比較
        FC_INS_CPX, // CPX--寄存器X的內容和存儲器比較
        FC_INS_CPY, // CPY--寄存器Y的內容和存儲器比較
        FC_INS_BIT, // BIT--位測試
        FC_INS_ASL, // ASL--算術左移 儲存器
        FC_INS_ASLA, // ASL--算術左移 累加器
        FC_INS_LSR, // LSR--算術右移 儲存器
        FC_INS_LSRA, // LSR--算術右移 累加器
        FC_INS_ROL, // ROL--循環算術左移 儲存器
        FC_INS_ROLA, // ROL--循環算術左移 累加器
        FC_INS_ROR, // ROR--循環算術右移 儲存器
        FC_INS_RORA, // ROR--循環算術右移 累加器
        FC_INS_PHA, // PHA--累加器進棧
        FC_INS_PLA, // PLA--累加器出棧
        FC_INS_PHP, // PHP--標誌寄存器P進棧
        FC_INS_PLP, // PLP--標誌寄存器P出棧
        FC_INS_JMP, // JMP--無條件跳轉
        FC_INS_BEQ, // 如果標誌位Z = 1則轉移，否則繼續
        FC_INS_BNE, // 如果標誌位Z = 0則轉移，否則繼續
        FC_INS_BCS, // 如果標誌位C = 1則轉移，否則繼續
        FC_INS_BCC, // 如果標誌位C = 0則轉移，否則繼續
        FC_INS_BMI, // 如果標誌位N = 1則轉移，否則繼續
        FC_INS_BPL, // 如果標誌位N = 0則轉移，否則繼續
        FC_INS_BVS, // 如果標誌位V = 1則轉移，否則繼續
        FC_INS_BVC, // 如果標誌位V = 0則轉移，否則繼續
        FC_INS_JSR, // 跳轉到子程序
        FC_INS_RTS, // 返回到主程序
        FC_INS_NOP, // 無操作
        FC_INS_BRK, // 強制中斷
        FC_INS_RTI, // 從中斷返回
                     // -------- 組合指令 ----------
        FC_INS_ALR, // [Unofficial&Combo] AND+LSR
        FC_INS_ASR = FC_INS_ALR,// 有消息稱是叫這個
        FC_INS_ANC, // [Unofficial&Combo] AND+N2C?
        FC_INS_AAC = FC_INS_ANC,// 差不多一個意思
        FC_INS_ARR, // [Unofficial&Combo] AND+ROR [類似]
        FC_INS_AXS, // [Unofficial&Combo] AND+XSB?
        FC_INS_SBX = FC_INS_AXS,// 一個意思
        FC_INS_LAX, // [Unofficial&Combo] LDA+TAX
        FC_INS_SAX, // [Unofficial&Combo] STA&STX [類似]
                     // -------- 讀改寫指令 ----------
        FC_INS_DCP, // [Unofficial& RMW ] DEC+CMP
        FC_INS_ISC, // [Unofficial& RMW ] INC+SBC
        FC_INS_ISB = FC_INS_ISC,// 差不多一個意思
        FC_INS_RLA, // [Unofficial& RMW ] ROL+AND
        FC_INS_RRA, // [Unofficial& RMW ] ROR+AND
        FC_INS_SLO, // [Unofficial& RMW ] ASL+ORA
        FC_INS_SRE, // [Unofficial& RMW ] LSR+EOR
                     // -------- 臥槽 ----
        FC_INS_LAS,
        FC_INS_XAA,
        FC_INS_AHX,
        FC_INS_TAS,
        FC_INS_SHX,
        FC_INS_SHY,
    };


    /// <summary>
    /// StepFC: 尋址方式
    /// </summary>
    public enum FC_6502_addressing_mode
    {
        FC_AM_UNK = 0, // 未知尋址
        FC_AM_ACC, // 操累加器A: Op Accumulator
        FC_AM_IMP, // 隱含 尋址: Implied Addressing
        FC_AM_IMM, // 立即 尋址: Immediate Addressing
        FC_AM_ABS, // 直接 尋址: Absolute Addressing
        FC_AM_ABX, // 直接X變址: Absolute X Addressing
        FC_AM_ABY, // 直接Y變址: Absolute Y Addressing
        FC_AM_ZPG, // 零頁 尋址: Zero-Page Addressing
        FC_AM_ZPX, // 零頁X變址: Zero-PageX Addressing
        FC_AM_ZPY, // 零頁Y變址: Zero-PageY Addressing
        FC_AM_INX, // 間接X變址: Pre-indexed Indirect Addressing
        FC_AM_INY, // 間接Y變址: Post-indexed Indirect Addressing
        FC_AM_IND, // 間接 尋址: Indirect Addressing
        FC_AM_REL, // 相對 尋址: Relative Addressing
    };
    public enum OperationAddr{
        NAME_FIRSH = 0,
        ADDR_FIRSH = NAME_FIRSH + 4,
        LEN = ADDR_FIRSH + 9
    };
    // <summary>
    /// 命令名稱
    /// </summary>
    public struct Fc_opname
    {
        // 3字名稱
        public string name;
        // 尋址模式
        public FC_6502_addressing_mode mode;
        public Fc_opname(char a,char b,char c, FC_6502_addressing_mode mode)
        {
            this.name = string.Concat(a,b,c);
            this.mode = mode;
        }
    };

    public partial class Nes6502
    {
        /// 反彙編用數據
        private Fc_opname [] opname_data = new Fc_opname[]{
            new Fc_opname('B', 'R', 'K', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('O', 'R', 'A', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('S', 'L', 'O', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('O', 'R', 'A', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('A', 'S', 'L', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('S', 'L', 'O', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('P', 'H', 'P', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('O', 'R', 'A', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('A', 'S', 'L', FC_6502_addressing_mode.FC_AM_ACC ),
            new Fc_opname('A', 'N', 'C', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('O', 'R', 'A', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('A', 'S', 'L', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('S', 'L', 'O', FC_6502_addressing_mode.FC_AM_ABS ),

            new Fc_opname('B', 'P', 'L', FC_6502_addressing_mode.FC_AM_REL ),
            new Fc_opname('O', 'R', 'A', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('S', 'L', 'O', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('O', 'R', 'A', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('A', 'S', 'L', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('S', 'L', 'O', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('C', 'L', 'C', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('O', 'R', 'A', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('S', 'L', 'O', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('O', 'R', 'A', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('A', 'S', 'L', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('S', 'L', 'O', FC_6502_addressing_mode.FC_AM_ABX ),

            new Fc_opname('J', 'S', 'R', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('A', 'N', 'D', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('R', 'L', 'A', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('B', 'I', 'T', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('A', 'N', 'D', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('R', 'O', 'L', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('R', 'L', 'A', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('P', 'L', 'P', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('A', 'N', 'D', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('R', 'O', 'L', FC_6502_addressing_mode.FC_AM_ACC ),
            new Fc_opname('A', 'N', 'C', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('B', 'I', 'T', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('A', 'N', 'D', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('R', 'O', 'L', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('R', 'L', 'A', FC_6502_addressing_mode.FC_AM_ABS ),

            new Fc_opname('B', 'M', 'I', FC_6502_addressing_mode.FC_AM_REL ),
            new Fc_opname('A', 'N', 'D', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('R', 'L', 'A', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('A', 'N', 'D', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('R', 'O', 'L', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('R', 'L', 'A', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('S', 'E', 'C', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('A', 'N', 'D', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('R', 'L', 'A', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('A', 'N', 'D', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('R', 'O', 'L', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('R', 'L', 'A', FC_6502_addressing_mode.FC_AM_ABX ),

            new Fc_opname('R', 'T', 'I', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('E', 'O', 'R', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('S', 'R', 'E', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('E', 'O', 'R', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('L', 'S', 'R', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('S', 'R', 'E', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('P', 'H', 'A', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('E', 'O', 'R', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('L', 'S', 'R', FC_6502_addressing_mode.FC_AM_ACC ),
            new Fc_opname('A', 'S', 'R', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('J', 'M', 'P', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('E', 'O', 'R', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('L', 'S', 'R', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('S', 'R', 'E', FC_6502_addressing_mode.FC_AM_ABS ),

            new Fc_opname('B', 'V', 'C', FC_6502_addressing_mode.FC_AM_REL ),
            new Fc_opname('E', 'O', 'R', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('S', 'R', 'E', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('E', 'O', 'R', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('L', 'S', 'R', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('S', 'R', 'E', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('C', 'L', 'I', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('E', 'O', 'R', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('S', 'R', 'E', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('E', 'O', 'R', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('L', 'S', 'R', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('S', 'R', 'E', FC_6502_addressing_mode.FC_AM_ABX ),

            new Fc_opname('R', 'T', 'S', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('A', 'D', 'C', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('R', 'R', 'A', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('A', 'D', 'C', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('R', 'O', 'R', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('R', 'R', 'A', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('P', 'L', 'A', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('A', 'D', 'C', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('R', 'O', 'R', FC_6502_addressing_mode.FC_AM_ACC ),
            new Fc_opname('A', 'R', 'R', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('J', 'M', 'P', FC_6502_addressing_mode.FC_AM_IND ),
            new Fc_opname('A', 'D', 'C', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('R', 'O', 'R', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('R', 'R', 'A', FC_6502_addressing_mode.FC_AM_ABS ),

            new Fc_opname('B', 'V', 'S', FC_6502_addressing_mode.FC_AM_REL ),
            new Fc_opname('A', 'D', 'C', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('R', 'R', 'A', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('A', 'D', 'C', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('R', 'O', 'R', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('R', 'R', 'A', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('S', 'E', 'I', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('A', 'D', 'C', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('R', 'R', 'A', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('A', 'D', 'C', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('R', 'O', 'R', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('R', 'R', 'A', FC_6502_addressing_mode.FC_AM_ABX ),

            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('S', 'T', 'A', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('S', 'A', 'X', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('S', 'T', 'Y', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('S', 'T', 'A', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('S', 'T', 'X', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('S', 'A', 'X', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('D', 'E', 'Y', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('T', 'A', 'X', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('X', 'X', 'A', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('S', 'T', 'Y', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('S', 'T', 'A', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('S', 'T', 'X', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('S', 'A', 'X', FC_6502_addressing_mode.FC_AM_ABS ),

            new Fc_opname('B', 'C', 'C', FC_6502_addressing_mode.FC_AM_REL ),
            new Fc_opname('S', 'T', 'A', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('A', 'H', 'X', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('S', 'T', 'Y', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('S', 'T', 'A', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('S', 'T', 'X', FC_6502_addressing_mode.FC_AM_ZPY ),
            new Fc_opname('S', 'A', 'X', FC_6502_addressing_mode.FC_AM_ZPY ),
            new Fc_opname('T', 'Y', 'A', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('S', 'T', 'A', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('T', 'X', 'S', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('T', 'A', 'S', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('S', 'H', 'Y', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('S', 'T', 'A', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('S', 'H', 'X', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('A', 'H', 'X', FC_6502_addressing_mode.FC_AM_ABY ),

            new Fc_opname('L', 'D', 'Y', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('L', 'D', 'A', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('L', 'D', 'X', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('L', 'A', 'X', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('L', 'D', 'Y', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('L', 'D', 'A', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('L', 'D', 'X', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('L', 'A', 'X', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('T', 'A', 'Y', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('L', 'D', 'A', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('T', 'A', 'X', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('L', 'A', 'X', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('L', 'D', 'Y', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('L', 'D', 'A', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('L', 'D', 'X', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('L', 'A', 'X', FC_6502_addressing_mode.FC_AM_ABS ),

            new Fc_opname('B', 'C', 'S', FC_6502_addressing_mode.FC_AM_REL ),
            new Fc_opname('L', 'D', 'A', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('L', 'A', 'X', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('L', 'D', 'Y', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('L', 'D', 'A', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('L', 'D', 'X', FC_6502_addressing_mode.FC_AM_ZPY ),
            new Fc_opname('L', 'A', 'X', FC_6502_addressing_mode.FC_AM_ZPY ),
            new Fc_opname('C', 'L', 'V', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('L', 'D', 'A', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('T', 'S', 'X', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('L', 'A', 'S', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('L', 'D', 'Y', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('L', 'D', 'A', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('L', 'D', 'X', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('L', 'A', 'X', FC_6502_addressing_mode.FC_AM_ABY ),

            new Fc_opname('C', 'P', 'Y', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('C', 'M', 'P', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('D', 'C', 'P', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('C', 'P', 'Y', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('C', 'M', 'P', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('D', 'E', 'C', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('D', 'C', 'P', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('I', 'N', 'Y', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('C', 'M', 'P', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('D', 'E', 'X', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('A', 'X', 'S', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('C', 'P', 'Y', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('C', 'M', 'P', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('D', 'E', 'C', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('D', 'C', 'P', FC_6502_addressing_mode.FC_AM_ABS ),

            new Fc_opname('B', 'N', 'E', FC_6502_addressing_mode.FC_AM_REL ),
            new Fc_opname('C', 'M', 'P', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('D', 'C', 'P', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('C', 'M', 'P', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('D', 'E', 'C', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('D', 'C', 'P', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('C', 'L', 'D', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('C', 'M', 'P', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('D', 'C', 'P', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('C', 'M', 'P', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('D', 'E', 'C', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('D', 'C', 'P', FC_6502_addressing_mode.FC_AM_ABX ),

            new Fc_opname('C', 'P', 'X', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('S', 'B', 'C', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('I', 'S', 'B', FC_6502_addressing_mode.FC_AM_INX ),
            new Fc_opname('C', 'P', 'X', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('S', 'B', 'C', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('I', 'N', 'C', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('I', 'S', 'B', FC_6502_addressing_mode.FC_AM_ZPG ),
            new Fc_opname('I', 'N', 'X', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('S', 'B', 'C', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('S', 'B', 'C', FC_6502_addressing_mode.FC_AM_IMM ),
            new Fc_opname('C', 'P', 'X', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('S', 'B', 'C', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('I', 'N', 'C', FC_6502_addressing_mode.FC_AM_ABS ),
            new Fc_opname('I', 'S', 'B', FC_6502_addressing_mode.FC_AM_ABS ),

            new Fc_opname('B', 'E', 'Q', FC_6502_addressing_mode.FC_AM_REL ),
            new Fc_opname('S', 'B', 'C', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('S', 'T', 'P', FC_6502_addressing_mode.FC_AM_UNK ),
            new Fc_opname('I', 'S', 'B', FC_6502_addressing_mode.FC_AM_INY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('S', 'B', 'C', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('I', 'N', 'C', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('I', 'S', 'B', FC_6502_addressing_mode.FC_AM_ZPX ),
            new Fc_opname('S', 'E', 'D', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('S', 'B', 'C', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_IMP ),
            new Fc_opname('I', 'S', 'B', FC_6502_addressing_mode.FC_AM_ABY ),
            new Fc_opname('N', 'O', 'P', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('S', 'B', 'C', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('I', 'N', 'C', FC_6502_addressing_mode.FC_AM_ABX ),
            new Fc_opname('I', 'S', 'B', FC_6502_addressing_mode.FC_AM_ABX ),
        };
        /// 十六進製字符數據
        public const string FC_HEXDATA = "0123456789ABCDEF";

        /// <summary>
        /// 轉換為16進制
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="b">The b.</param>
        public void Fc_BtoH(StringBuilder o,int index, byte b)
        {
            // 高半字節
            o[index]=FC_HEXDATA[b >> 4];
            // 低半字節
            o[index + 1] = FC_HEXDATA[b & (byte)0x0F];
        }

        /// <summary>
        /// 轉換為有符號10進制
        /// </summary>
        /// <param name="o">The o.</param>
        /// <param name="b">The b.</param>
        public void Fc_BtoD(StringBuilder o, int index, byte b)
        {
            sbyte sb = (sbyte)b;
            if (sb < 0)
            {
                o[index] = '-';
                b = (byte)-b;
            }
            else o[index] = '+';
            o[index+1] = FC_HEXDATA[(byte)b / 100];
            o[index+2] = FC_HEXDATA[(byte)b / 10 % 10];
            o[index+3] = FC_HEXDATA[(byte)b % 10];
        }

        /// <summary>
        /// StepFC: 反彙編
        /// </summary>
        /// <param name="code">The code.</param>
        /// <param name="buf">The buf.</param>
        public void Fc_6502_disassembly(Fc_6502_code_t code, StringBuilder buf,int Index)
        {
            Fc_opname opname = opname_data[code.op];
            // 設置操作碼
            buf[Index + (int)OperationAddr.NAME_FIRSH + 0] = opname.name[0];
            buf[Index + (int)OperationAddr.NAME_FIRSH + 1] = opname.name[1];
            buf[Index + (int)OperationAddr.NAME_FIRSH + 2] = opname.name[2];
            // 查看尋址模式
            switch (opname.mode)
            {
                case FC_6502_addressing_mode.FC_AM_UNK:
                case FC_6502_addressing_mode.FC_AM_IMP:
                    // XXX ;
                    break;
                case FC_6502_addressing_mode.FC_AM_ACC:
                    // XXX A ;
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 0] = 'A';
                    break;
                case FC_6502_addressing_mode.FC_AM_IMM:
                    // XXX #$AB
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 0] = '#';
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 1] = '$';
                    Fc_BtoH(buf, Index + (int)OperationAddr.ADDR_FIRSH + 2, code.a1);
                    break;
                case FC_6502_addressing_mode.FC_AM_ABS:
                // XXX $ABCD
                case FC_6502_addressing_mode.FC_AM_ABX:
                // XXX $ABCD, X
                case FC_6502_addressing_mode.FC_AM_ABY:
                    // XXX $ABCD, Y
                    // REAL
                    buf[Index + (int)OperationAddr.ADDR_FIRSH] = '$';
                    Fc_BtoH(buf, Index + (int)OperationAddr.ADDR_FIRSH + 1, code.a2);
                    Fc_BtoH(buf, Index + (int)OperationAddr.ADDR_FIRSH + 3, code.a1);
                    if (opname.mode == FC_6502_addressing_mode.FC_AM_ABS) break;
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 5] = ',';
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 7] = opname.mode == FC_6502_addressing_mode.FC_AM_ABX ? 'X' : 'Y';
                    break;
                case FC_6502_addressing_mode.FC_AM_ZPG:
                // XXX $AB
                case FC_6502_addressing_mode.FC_AM_ZPX:
                // XXX $AB, X
                case FC_6502_addressing_mode.FC_AM_ZPY:
                    // XXX $AB, Y
                    // REAL
                    buf[Index + (int)OperationAddr.ADDR_FIRSH] = '$';
                    Fc_BtoH(buf, Index + (int)OperationAddr.ADDR_FIRSH + 1, code.a1);
                    if (opname.mode == FC_6502_addressing_mode.FC_AM_ZPG) break;
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 3] = ',';
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 5] = opname.mode == FC_6502_addressing_mode.FC_AM_ABX ? 'X' : 'Y';
                    break;
                case FC_6502_addressing_mode.FC_AM_INX:
                    // XXX ($AB, X)
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 0] = '(';
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 1] = '$';
                    Fc_BtoH(buf, Index + (int)OperationAddr.ADDR_FIRSH + 2, code.a1);
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 4] = ',';
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 6] = 'X';
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 7] = ')';
                    break;
                case FC_6502_addressing_mode.FC_AM_INY:
                    // XXX ($AB), Y
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 0] = '(';
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 1] = '$';
                    Fc_BtoH(buf, Index + (int)OperationAddr.ADDR_FIRSH + 2, code.a1);
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 4] = ')';
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 5] = ',';
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 7] = 'Y';
                    break;
                case FC_6502_addressing_mode.FC_AM_IND:
                    // XXX ($ABCD)
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 0] = '(';
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 1] = '$';
                    Fc_BtoH(buf, Index + (int)OperationAddr.ADDR_FIRSH + 2, code.a2);
                    Fc_BtoH(buf, Index + (int)OperationAddr.ADDR_FIRSH + 4, code.a1);
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 6] = ')';
                    break;
                case FC_6502_addressing_mode.FC_AM_REL:
                    // XXX $AB(-085)
                    // XXX $ABCD
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 0] = '$';
                    //const uint16_t target = base + int8_t(data.a1);
                    //sfc_btoh(buf + ADDR_FIRSH + 1, uint8_t(target >> 8));
                    //sfc_btoh(buf + ADDR_FIRSH + 3, uint8_t(target & 0xFF));
                    Fc_BtoH(buf, Index + (int)OperationAddr.ADDR_FIRSH + 1, code.a1);
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 3] = '(';
                    Fc_BtoD(buf, Index + (int)OperationAddr.ADDR_FIRSH + 4, code.a1);
                    buf[Index + (int)OperationAddr.ADDR_FIRSH + 8] = ')';
                    break;
            }
        }
    }
}
