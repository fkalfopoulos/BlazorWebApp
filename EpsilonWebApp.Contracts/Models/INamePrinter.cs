namespace EpsilonWebApp.Contracts.Models
{
    /// <summary>
    /// Interface for printing names of different person types
    /// </summary>
    public interface INamePrinter
    {
        /// <summary>
        /// Prints the name of an Employee
        /// </summary>
        /// <param name="employee">The employee whose name to print</param>
        /// <returns>The formatted employee name string</returns>
        string PrintName(Employee employee);

        /// <summary>
        /// Prints the name of a Manager
        /// </summary>
        /// <param name="manager">The manager whose name to print</param>
        /// <returns>The formatted manager name string</returns>
        string PrintName(Manager manager);
    }
}
