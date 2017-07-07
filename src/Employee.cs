using System;
using System.Collections.Generic;
using System.Text;

namespace StudentIT.Roster.Summary
{
    internal class Employee
    {
        public string Name { get; }
        public List<int> Shifts { get; }

        public Employee(string name)
        {
            Name = name;
            Shifts = new List<int>();
        }
    }
}
