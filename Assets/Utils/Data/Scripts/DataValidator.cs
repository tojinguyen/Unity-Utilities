using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace TirexGame.Utils.Data
{
    public class DataValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new();
    }
    
    public class DataValidator
    {
        public UniTask<DataValidationResult> ValidateAsync<T>(T obj) where T : class
        {
            var result = new DataValidationResult();

            if (obj == null)
            {
                result.IsValid = false;
                result.Errors.Add("Object cannot be null");
                return UniTask.FromResult(result);
            }

            if (obj is IValidatable validatableObj)
            {
                result.IsValid = validatableObj.Validate(out var errors);
                result.Errors = errors;
            }
            else
            {
                result.IsValid = true;
            }

            return UniTask.FromResult(result);
        }
    }
}