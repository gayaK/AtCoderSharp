using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class Snippet
{
    public void Read()
    {
        {
            var l = Console.ReadLine()
               .Split(' ')
               .Select(int.Parse)
               .ToArray();
        }
        {
            var n = int.Parse(Console.ReadLine());
            var a = Enumerable
                .Range(0, n)
                .Select(x => int.Parse(Console.ReadLine()))
                .ToArray();
        }
    }
}
