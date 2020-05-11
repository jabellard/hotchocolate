using System.Collections.Generic;

namespace HotChocolate.Execution
{
    public sealed class ResultMapList
        : List<IResultMap?>
        , IResultMapList
    {
        public IResultData? Parent { get; set; }

        /// <summary>
        /// Defines if the elements of this list are nullable.
        /// </summary>
        public bool IsNullable { get; set; }
    }
}
