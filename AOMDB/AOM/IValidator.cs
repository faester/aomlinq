using System;
using System.Collections.Generic;
using System.Text;

namespace AOM
{
    public interface IValidator
    {
        /// <summary>
        /// Test validity of property. Throws ValidationException 
        /// on errors.
        /// </summary>
        /// <param typename="pt"></param>
        void Validate(string value);
    }
}
