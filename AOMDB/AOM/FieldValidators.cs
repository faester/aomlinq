using System;
using System.Collections.Generic;
using System.Text;

namespace AOM
{
    #region validators
    /// <summary>
    /// Validates the content of a property...
    /// </summary>

    public class ValidatorAcceptAll : IValidator
    {
        public void Validate(string value)
        {
            /* Accepts everything, no action. */
        }
    }
    #endregion

}
