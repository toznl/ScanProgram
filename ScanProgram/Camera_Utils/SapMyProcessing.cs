using DALSA.SaperaLT.SapClassBasic;

namespace ScanProgram
{
    class SapMyProcessing : SapProcessing
    {
        private SapColorConversion m_ColorConv;
        // Constructor

        public SapMyProcessing(SapBuffer pBuffers, SapColorConversion pColorConv)
          : base(pBuffers)
        {
            base.ProcessingDoneEnable = true;
            m_ColorConv = pColorConv;
        }
        public SapMyProcessing(SapBuffer pBuffers)
       : base(pBuffers)
        {
            base.ProcessingDoneEnable = true;
            ;
        }

        public override bool Run()
        {
            if (m_ColorConv != null && m_ColorConv.Initialized && m_ColorConv.SoftwareEnabled)
            {
                m_ColorConv.Convert(base.Index);
            }

            return true;
        }

    }
}
