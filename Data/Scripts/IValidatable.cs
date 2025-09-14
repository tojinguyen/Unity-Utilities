using System.Collections.Generic;

namespace TirexGame.Utils.Data
{
    public interface IValidatable
    {
        public bool Validate(out List<string> errors);
    }
}