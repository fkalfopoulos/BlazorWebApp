namespace EpsilonWebApp.Contracts.Models
{
    /// <summary>
    /// Utility class for printing names of different person types
    /// </summary>
    public class NamePrinter : INamePrinter
    {
        /// <summary>
        /// Prints the name of an Employee
        /// </summary>
        /// <param name="employee">The employee whose name to print</param>
        /// <returns>The formatted employee name string</returns>
        public string PrintName(Employee employee)
        {
            if (employee == null)
                throw new ArgumentNullException(nameof(employee));

            return $"Employee Name: {employee.Name}";
        }

        /// <summary>
        /// Prints the name of a Manager
        /// </summary>
        /// <param name="manager">The manager whose name to print</param>
        /// <returns>The formatted manager name string</returns>
        public string PrintName(Manager manager)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            return $"Manager Name: {manager.Name}";
        }
    }
}
