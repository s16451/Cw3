using System.Collections.Generic;

namespace Cw3
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();
    }
}