using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AAATest
{
    class Program
    {
        static void Main(string[] args)
        {

            Regex StringRepresentation = new Regex(@"^([0-9A-F]{2})\+([0-9A-F]{2})[:\+]([0-9A-F]{1,})$", RegexOptions.Compiled);
            var matches = StringRepresentation.Match("AF+AF:1");
            Console.WriteLine(matches.Success);
            matches = StringRepresentation.Match("AF+AF+1A");
            Console.WriteLine(matches.Success);
        }
    
}
}
