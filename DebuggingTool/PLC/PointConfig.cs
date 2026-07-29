using System;
using System.Collections.Generic;
using System.Text;

namespace DebuggingTool.PLC
{
    public class PointConfig
    {
        public string Name { get; set; }
        /// <summary>
        /// PLC变量名
        /// </summary>
        public string VarName { get; set; }
        public int StartAddress { get; set; }
        public int DB { get; set; }
        public byte BitAddress { get; set; }
        public int Length { get; set; }
    }
}
