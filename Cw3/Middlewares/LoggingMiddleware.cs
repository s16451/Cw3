using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace APBD
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IStudentDbService studentDbService)
        {
            context.Request.EnableBuffering();
            if (context.Request != null)
            {
                string path = context.Request.Path;
                string method = context.Request.Method;
                string queryString = context.Request.QueryString.ToString();
                string bodyString = "";

                using (var reader = new StreamReader(context.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyString = await reader.ReadToEndAsync();
                    context.Request.Body.Position = 0;
                }

                using (var writer = new StreamWriter("requestsLog.txt", true))
                {
                    writer.WriteLine($"Log: {DateTime.Now}");
                    writer.WriteLine($"Method: {method}");
                    writer.WriteLine($"Path: {path}");
                    writer.WriteLine($"Body: {bodyString}");
                    writer.WriteLine($"QueryString: {queryString}");
                }
            }

            if (_next != null) await _next(context);
        }
    }
}