using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Data
{
    /// <summary>
    /// Data validation result
    /// </summary>
    public class DataValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
    
    /// <summary>
    /// Base validation attribute
    /// </summary>
    public abstract class DataValidationAttribute : Attribute
    {
        public string ErrorMessage { get; set; }
        public abstract bool IsValid(object value);
    }
    
    /// <summary>
    /// Required field validation
    /// </summary>
    public class RequiredAttribute : DataValidationAttribute
    {
        public RequiredAttribute()
        {
            ErrorMessage = "Field is required";
        }
        
        public override bool IsValid(object value)
        {
            if (value == null) return false;
            if (value is string str) return !string.IsNullOrWhiteSpace(str);
            return true;
        }
    }
    
    /// <summary>
    /// Range validation for numeric values
    /// </summary>
    public class RangeAttribute : DataValidationAttribute
    {
        public double Min { get; }
        public double Max { get; }
        
        public RangeAttribute(double min, double max)
        {
            Min = min;
            Max = max;
            ErrorMessage = $"Value must be between {min} and {max}";
        }
        
        public override bool IsValid(object value)
        {
            if (value == null) return false;
            
            var doubleValue = Convert.ToDouble(value);
            return doubleValue >= Min && doubleValue <= Max;
        }
    }
    
    /// <summary>
    /// String length validation
    /// </summary>
    public class StringLengthAttribute : DataValidationAttribute
    {
        public int MaxLength { get; }
        public int MinLength { get; }
        
        public StringLengthAttribute(int maxLength, int minLength = 0)
        {
            MaxLength = maxLength;
            MinLength = minLength;
            ErrorMessage = $"String length must be between {minLength} and {maxLength}";
        }
        
        public override bool IsValid(object value)
        {
            if (value is not string str) return false;
            return str.Length >= MinLength && str.Length <= MaxLength;
        }
    }
    
    /// <summary>
    /// Email format validation
    /// </summary>
    public class EmailAttribute : DataValidationAttribute
    {
        public EmailAttribute()
        {
            ErrorMessage = "Invalid email format";
        }
        
        public override bool IsValid(object value)
        {
            if (value is not string email) return false;
            
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
    
    /// <summary>
    /// Custom validation using a method
    /// </summary>
    public class CustomValidationAttribute : DataValidationAttribute
    {
        public string MethodName { get; }
        
        public CustomValidationAttribute(string methodName)
        {
            MethodName = methodName;
            ErrorMessage = "Custom validation failed";
        }
        
        public override bool IsValid(object value)
        {
            // This will be handled by the validator using reflection
            return true;
        }
    }
    
    /// <summary>
    /// Data validator that validates objects using validation attributes
    /// </summary>
    public class DataValidator
    {
        /// <summary>
        /// Validate an object asynchronously
        /// </summary>
        public async UniTask<DataValidationResult> ValidateAsync<T>(T obj) where T : class
        {
            var result = new DataValidationResult { IsValid = true };
            
            if (obj == null)
            {
                result.IsValid = false;
                result.Errors.Add("Object cannot be null");
                return result;
            }
            
            var type = obj.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var property in properties)
            {
                var validationAttributes = property.GetCustomAttributes<DataValidationAttribute>();
                var value = property.GetValue(obj);
                
                foreach (var attribute in validationAttributes)
                {
                    await ValidateProperty(obj, property, value, attribute, result);
                }
            }
            
            // Validate fields as well
            var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
            
            foreach (var field in fields)
            {
                var validationAttributes = field.GetCustomAttributes<DataValidationAttribute>();
                var value = field.GetValue(obj);
                
                foreach (var attribute in validationAttributes)
                {
                    await ValidateField(obj, field, value, attribute, result);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Validate an object synchronously
        /// </summary>
        public DataValidationResult Validate<T>(T obj) where T : class
        {
            return ValidateAsync(obj).GetAwaiter().GetResult();
        }
        
        private async UniTask ValidateProperty(object obj, PropertyInfo property, object value, 
            DataValidationAttribute attribute, DataValidationResult result)
        {
            await UniTask.CompletedTask;
            
            if (attribute is CustomValidationAttribute customAttribute)
            {
                var isValid = await ValidateCustomAsync(obj, customAttribute.MethodName, value);
                if (!isValid)
                {
                    result.IsValid = false;
                    result.Errors.Add($"{property.Name}: {customAttribute.ErrorMessage}");
                }
            }
            else if (!attribute.IsValid(value))
            {
                result.IsValid = false;
                result.Errors.Add($"{property.Name}: {attribute.ErrorMessage}");
            }
        }
        
        private async UniTask ValidateField(object obj, FieldInfo field, object value, 
            DataValidationAttribute attribute, DataValidationResult result)
        {
            await UniTask.CompletedTask;
            
            if (attribute is CustomValidationAttribute customAttribute)
            {
                var isValid = await ValidateCustomAsync(obj, customAttribute.MethodName, value);
                if (!isValid)
                {
                    result.IsValid = false;
                    result.Errors.Add($"{field.Name}: {customAttribute.ErrorMessage}");
                }
            }
            else if (!attribute.IsValid(value))
            {
                result.IsValid = false;
                result.Errors.Add($"{field.Name}: {attribute.ErrorMessage}");
            }
        }
        
        private async UniTask<bool> ValidateCustomAsync(object obj, string methodName, object value)
        {
            try
            {
                var type = obj.GetType();
                var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (method == null)
                {
                    UnityEngine.Debug.LogError($"Validation method '{methodName}' not found in type '{type.Name}'");
                    return false;
                }
                
                var result = method.Invoke(obj, new[] { value });
                
                if (result is Task<bool> asyncResult)
                {
                    return await asyncResult;
                }
                else if (result is UniTask<bool> uniTaskResult)
                {
                    return await uniTaskResult;
                }
                else if (result is bool syncResult)
                {
                    return syncResult;
                }
                
                return false;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error executing custom validation method '{methodName}': {ex.Message}");
                return false;
            }
        }
    }
}
