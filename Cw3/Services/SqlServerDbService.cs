using System;
using System.Data;
using System.Data.SqlClient;

namespace APBD
{
    public class SqlServerDbService : IStudentDbService
    {
        private const string ConString = "Data Source=db-mssql;Initial Catalog=s16451;Integrated Security=True";
        private SqlConnection con;
        private SqlCommand com;
        private SqlTransaction tran;
        
        public EnrollStudentResponse EnrollStudent(EnrollStudentRequest request)
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
                            throw new Exception("Studia nie istnieja");
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
                        throw new Exception("Student id jest zajete przez innego studenta");
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
                    throw new Exception("Nie udalo sie przetworzyc zadania");
                }
            }

            var response = new EnrollStudentResponse();
            response.LastName = request.LastName;
            response.Semester = 1;
            response.StartDate = DateTime.Now;
            return response;
        }

        public Enrollment PromoteStudents(PromoteStudentsRequest request)
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
                    
                    var enrollment = new Enrollment();
                    using (var dr = com.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            tran.Rollback();
                            throw new Exception("Nie udalo sie przetworzyc zadania");
                        }

                        enrollment.IdEnrollment = (int) dr["IdEnrollment"];
                        enrollment.Semester = (int) dr["Semester"];
                        enrollment.IdStudy = (int) dr["IdStudy"];
                        enrollment.StartDate = (DateTime) dr["StartDate"];
                    }

                    tran.Commit();
                    return enrollment;
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw new Exception("Nie udalo sie przetworzyc zadania", e);
                }
            }
        }

        public Student GetStudent(string index)
        { 
            using (SqlConnection con = new SqlConnection(ConString))
            using (SqlCommand com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "select * from student where indexnumber=@index";
                com.Parameters.AddWithValue("index", index);

                con.Open();
                var dr = com.ExecuteReader();
                if (dr.Read())
                {
                    var st = new Student();
                    st.IndexNumber = dr["IndexNumber"].ToString();
                    st.FirstName = dr["FirstName"].ToString();
                    st.LastName = dr["LastName"].ToString();
                    return st;
                }

                return null;
            }
        }

        public EncryptedCredentials GetCredentials(LoginRequest request)
        {
            using (con = new SqlConnection(ConString))
            using (com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                try
                {
                    com.CommandText = "SELECT Password, Salt FROM Student WHERE IndexNumber=@login";
                    com.Parameters.AddWithValue("login", request.Login);

                    var credentials = new EncryptedCredentials();
                    using (var dr = com.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            throw new Exception("Nie udalo sie przetworzyc zadania");
                        }

                        credentials.Hash = (string)dr["Password"];
                        credentials.Salt = (string)dr["Salt"];
                    }

                    return credentials;
                }
                catch (Exception e)
                {
                    throw new Exception("Nie udalo sie przetworzyc zadania");
                }
            }
        }

        public bool IsRefTokenAuth(string refToken)
        {
            using (con = new SqlConnection(ConString))
            using (com = new SqlCommand())
            {
                com.Connection = con;
                con.Open();

                try
                {
                    com.CommandText = "SELECT 1 FROM Student WHERE RefToken=@refToken";
                    com.Parameters.AddWithValue("refToken", refToken);

                    using (var dr = com.ExecuteReader())
                    {
                        if (!dr.Read())
                        {
                            return false;
                        }

                        return true;
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Nie udalo sie przetworzyc zadania");
                }
            }
        }

        public void SaveRefToken(SaveRefTokenRequest request)
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
                    com.CommandText = "UPDATE Student SET RefToken=@refToken WHERE IndexNumber=@index";
                    com.Parameters.AddWithValue("refToken", request.RefToken);
                    com.Parameters.AddWithValue("index", request.IndexNumber);

                    com.ExecuteNonQuery();
                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw new Exception("Nie udalo sie przetworzyc zadania");
                }
            }
        }
        
        public void SaveRefToken(string prevRefToken, string refToken)
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
                    com.CommandText = "UPDATE Student SET RefToken=@refToken WHERE RefToken=@prevRefToken";
                    com.Parameters.AddWithValue("refToken", refToken);
                    com.Parameters.AddWithValue("prevRefToken", prevRefToken);

                    com.ExecuteNonQuery();
                    tran.Commit();
                }
                catch (Exception e)
                {
                    tran.Rollback();
                    throw new Exception("Nie udalo sie przetworzyc zadania");
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