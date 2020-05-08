using System;
using Microsoft.AspNetCore.Mvc;

namespace APBD
{
    [ApiController]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentDbService _service;

        public EnrollmentsController(IStudentDbService service)
        {
            _service = service;
        }

        [HttpPost]
        [Route("api/enrollments")]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {
            try
            {
                var response = _service.EnrollStudent(request);
                return Ok(response);
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
        }

        [HttpPost]
        [Route("api/enrollments/promotions")]
        public IActionResult PromoteStudents(PromoteStudentsRequest request)
        {
            try
            {
                var response = _service.PromoteStudents(request);
                return Ok(response);
            }
            catch (Exception e)
            {
                return NotFound(e.Message);
            }
        }
    }
}