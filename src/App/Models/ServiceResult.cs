using System;

namespace FoodDatabase.App.Models
{
    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public ServiceResult(bool success, string message)
        {
            Success = success;
            Message = message ?? throw new ArgumentNullException(nameof(message));
        }

        public static ServiceResult SuccessResult(string message) => new(true, message);
        public static ServiceResult FailureResult(string message) => new(false, message);
    }
}
