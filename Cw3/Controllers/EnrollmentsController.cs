using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.AspNetCore.Mvc;

namespace APBD
{
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s16451;Integrated Security=True";
        private SqlConnection con;
        private SqlCommand com;
        private SqlTransaction tran;

        [HttpPost]
        [Route("api/enrollments")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            using (con = new SqlConnection(ConString))
            using (com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                tran = con.BeginTransaction();
                com.Transaction = tran;

                try
                {
                    //Check studies
                    com.CommandText = "SELECT IdStudy FROM Studies WHERE Name=@name";
                    com.Parameters.AddWithValue("name", request.Studies);

                    int idStudies;
                    using (var dr = com.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            tran.Rollback();
                            return BadRequest("Studia nie istnieja");
                        }

                        idStudies = (int) dr["IdStudy"];
                    }

                    //Check semester
                    com.CommandText = "SELECT IdEnrollment FROM Enrollment WHERE IdStudy=@idStudy AND Semester = 1";
                    com.Parameters.AddWithValue("idStudy", idStudies);

                    int idEnrollment;
                    using (var dr = com.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            idEnrollment = (int) dr["IdEnrollment"];
                        }
                        else
                        {
                            dr.Close();
                            idEnrollment = InsertEnrollment(idStudies);
                        }
                    }

                    //Insert Student
                    if (!IsStudentIdUnique(request.IndexNumber))
                    {
                        tran.Rollback();
                        return BadRequest("Student id jest zajete przez innego studenta");
                    }

                    com.CommandText =
                        "INSERT INTO Student(IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES(@in, @fn, @ln, @bd, @ie)";
                    com.Parameters.AddWithValue("in", request.IndexNumber);
                    com.Parameters.AddWithValue("fn", request.FirstName);
                    com.Parameters.AddWithValue("ln", request.LastName);
                    com.Parameters.AddWithValue("bd", MapStringToDateTime(request.Birthdate));
                    com.Parameters.AddWithValue("ie", idEnrollment);

                    com.ExecuteNonQuery();
                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return BadRequest();
                }
            }

            var response = new EnrollStudentResponse();
            response.LastName = request.LastName;
            response.Semester = 1;
            response.StartDate = DateTime.Now;
            return Ok(response);
        }

        [HttpPost]
        [Route("api/enrollments/promotions")]
        public IActionResult PromoteStudents(PromoteStudentsRequest request)
        {
            using (con = new SqlConnection(ConString))
            using (com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();
                tran = con.BeginTransaction();
                com.Transaction = tran;
                
                try
                {
                    //Sprawdzamy czy studia istnieją oraz czy semestr się zgadza już w procedurze
                    //Uruchamiamy procedurę
                    com.CommandText = "PromoteStudents";
                    com.CommandType = CommandType.StoredProcedure;
                    com.Parameters.Add(new SqlParameter("Studies", request.Studies));
                    com.Parameters.Add(new SqlParameter("Semester", request.Semester));
                    
                    var response = new EnrollmentResponse();
                    using (var dr = com.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            tran.Rollback();
                            return BadRequest();
                        }

                        response.IdEnrollment = (int) dr["IdEnrollment"];
                        response.Semester = (int) dr["Semester"];
                        response.IdStudy = (int) dr["IdStudy"];
                        response.StartDate = (DateTime) dr["StartDate"];
                    }

                    tran.Commit();
                    return Ok(response);
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    return NotFound();
                }
            }
        }

        private int InsertEnrollment(int idStudies)
        {
            com.CommandText = "SELECT MAX(IdEnrollment) FROM Enrollment";
            int highestId = 0;
            try
            {
                highestId = Convert.ToInt32(com.ExecuteScalar());
                highestId++;
            }
            catch (Exception e)
            {
            }

            com.CommandText =
                "INSERT INTO Enrollment(IdEnrollment, Semester, IdStudy, StartDate) VALUES(@ide, @sem, @ids, @sd)";
            com.Parameters.AddWithValue("ide", highestId);
            com.Parameters.AddWithValue("sem", 1);
            com.Parameters.AddWithValue("ids", idStudies);
            com.Parameters.AddWithValue("sd", DateTime.Now);

            com.ExecuteNonQuery();
            return highestId;
        }

        private bool IsStudentIdUnique(string idStudent)
        {
            com.CommandText = "SELECT IndexNumber FROM Student WHERE IndexNumber=@index";
            com.Parameters.AddWithValue("index", idStudent);
            bool isUnique;
            using (var dr = com.ExecuteReader())
            {
                isUnique = !dr.Read();
            }

            return isUnique;
        }

        private DateTime MapStringToDateTime(string date)
        {
            var splitDate = date.Split('.');
            return new DateTime(int.Parse(splitDate[2]), int.Parse(splitDate[1]), int.Parse(splitDate[0]));
        }
    }
}