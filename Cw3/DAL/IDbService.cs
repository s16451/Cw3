using System.Collections.Generic;

namespace APBD
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
    }
}