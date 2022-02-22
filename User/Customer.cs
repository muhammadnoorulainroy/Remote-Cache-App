using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User
{
    [Serializable]
    public class Customer
    {
        public string Name { get; set; }

        public int Age { get; set; }

        public Customer() { }
        public Customer(string name, int age)
        {
            Name = name;
            Age = age;
        }
    }
}
