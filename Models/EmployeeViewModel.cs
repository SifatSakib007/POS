namespace POS.Models
{
    public class EmployeeViewModel
    {
        public Users NewEmployee { get; set; }
        public IEnumerable<Users> EmployeeList { get; set; } = new List<Users>();
    }
}
