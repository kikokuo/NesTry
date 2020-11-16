using Microsoft.VisualStudio.TestTools.UnitTesting;
using NesTry;
using System;
using System.Text;

namespace NesTest
{
    [TestClass]
    public class NesRomTest
    {
        private Famicom famicom;

        [TestInitialize]
        public void SetUp()
        {
            famicom = new Famicom();
            famicom.LoadRom("nestest.nes");
        }
        [TestCleanup]
        public void TearDown()
        {
            famicom = null;
        }
        [TestMethod]
        public void CheckReset()
        {
            Assert.IsTrue(famicom.Reset());
        }
        [TestMethod]
        public void CheckNesHead()
        {
            Assert.AreEqual(famicom.m_nesrom.ReadRom(),Fc_error_code.FC_ERROR_OK);
        }
        [TestMethod]
        public void CPU_init()
        {
            UInt16 v0 = famicom.ReadCPUAddress((UInt16)Fc_cpu_vector.FC_VERCTOR_NMI);
            Assert.AreNotEqual(v0, 0);
            v0 = famicom.ReadCPUAddress((UInt16)Fc_cpu_vector.FC_VERCTOR_RESET);
            Assert.AreNotEqual(v0, 0);
            v0 = famicom.ReadCPUAddress((UInt16)Fc_cpu_vector.FC_VERCTOR_IRQ);
            Assert.AreNotEqual(v0,0);
        }
        [TestMethod]
        public void Nes6502_Fc_BtoH_Check()
        {
            UInt16 v0 = famicom.ReadCPUAddress((UInt16)Fc_cpu_vector.FC_VERCTOR_NMI);
            Assert.AreNotEqual(v0, 0);
            UInt16 v1 = famicom.ReadCPUAddress((UInt16)Fc_cpu_vector.FC_VERCTOR_RESET);
            Assert.AreNotEqual(v0, 0);
            UInt16 v2 = famicom.ReadCPUAddress((UInt16)Fc_cpu_vector.FC_VERCTOR_IRQ);
            Assert.AreNotEqual(v0, 0);

            StringBuilder sb = new StringBuilder("        ");
            sb[0] = '$';
            famicom.m_nes6502.Fc_BtoH(sb,1, (byte)(v1 >> 8));
            famicom.m_nes6502.Fc_BtoH(sb,3, (byte)(v1));
            Assert.AreEqual(sb.ToString(),"$C004   ");
        }

        [TestMethod]
        public void NesCPU_Fc_Disassembly_check()
        {
            UInt16 v0 = famicom.ReadCPUAddress((UInt16)Fc_cpu_vector.FC_VERCTOR_NMI);
            Assert.AreNotEqual(v0, 0);
            UInt16 v1 = famicom.ReadCPUAddress((UInt16)Fc_cpu_vector.FC_VERCTOR_RESET);
            Assert.AreNotEqual(v0, 0);
            UInt16 v2 = famicom.ReadCPUAddress((UInt16)Fc_cpu_vector.FC_VERCTOR_IRQ);
            Assert.AreNotEqual(v0, 0);


            StringBuilder b1 = new StringBuilder("                                                ");
            StringBuilder b2 = new StringBuilder("                                                ");
            StringBuilder b3 = new StringBuilder("                                                ");
            famicom.Disassembly(v0,b1);
            famicom.Disassembly(v1,b2);
            famicom.Disassembly(v2,b3);
            Assert.IsTrue(b1.ToString().Contains("PHA"));
            Assert.IsTrue(b2.ToString().Contains("SEI"));
            Assert.IsTrue(b3.ToString().Contains("RTI"));
        }
        [TestMethod]
        public void Nes6502_run()
        {
            famicom.Reset();
            for (int i = 0; i < 1000; i++)
            {
                StringBuilder b1 = new StringBuilder("                                                ");
                UInt16 pc = famicom.m_cpu.Registers.program_counter;
                famicom.Disassembly(pc, b1);
                famicom.m_nes6502.Fc_cpu_execute_one();
            }
        }
    }
}
