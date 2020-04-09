using System;
using System.Collections.Generic;
using System.Data.SqlClient;  
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Cw3
{
    [ApiController, Route("api/students")]
    public class StudentsController : ControllerBase
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s16451;Integrated Security=True";
        private readonly IDbService _dbService;

        public StudentsController(IDbService dbService)
        {
            _dbService = dbService;
        }
        
        [HttpGet]
        public IActionResult GetStudents(string orderBy)
        {
            var list = new List<Student>();

            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select * from student";

                con.Open();
                SqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    var st = new Student();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    list.Add(st);
                }
            }
            
            if (orderBy == "lastname")
            {
                return Ok(list.OrderBy(s => s.LastName));
            }
            
            return Ok(list);
        }

        [HttpGet("{id}")]
        public IActionResult GetStudent(string id)
        {
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select * from student where indexnumber=@index";
                com.Parameters.AddWithValue("index", id);

                con.Open();
                var dr = com.ExecuteReader();
                if (dr.Read())
                {
                    var st = new Student();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    return Ok(st);
                }
            }

            return NotFound();
        }

        [HttpPost]
        public IActionResult CreateStudent(Student student)
        {
            student.IndexNumber = $"s{new Random().Next(1, 20000)}";
            return Ok(student);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateStudent(int id)
        {
            return Ok("Aktualizacja dokończona");
        }
        
        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            return Ok("Usuwanie ukończona");
        }
    }
}