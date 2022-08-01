namespace ConsoleApp
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    internal class Program
    {
        // Fixed the typo in the name, made it into a separate const value for ease of access and editing
        private const string DataFileName = "data.csv";

        static void Main(string[] args)
        {
            var reader = new DataReader();
            reader.ImportAndPrintData(DataFileName);
        }
    }
}
