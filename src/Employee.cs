using System.Linq;
using System.Collections.Generic;

namespace StudentIT.Roster.Summary
{
    internal class Employee
    {
        public string Name { get; }
        public double TotalHours => Shifts.Sum();
        public double[] Shifts { get; }

        public Employee(string name, int days)
        {
            Name = name;
            Shifts = new double[days];
        }
    }
}
