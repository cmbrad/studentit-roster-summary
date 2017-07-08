using System;
using System.Collections.Generic;

namespace StudentIT.Roster.Summary
{
    internal class RosterSummary
    {
        public DateTime StartDate { get; }
        public DateTime EndDate { get; }
        public List<Employee> Employees { get; }
        public int Days => (int)(EndDate - StartDate).TotalDays;

        public RosterSummary(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
            Employees = new List<Employee>();
        }
    }
}
