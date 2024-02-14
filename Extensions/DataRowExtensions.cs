using System.Data;

namespace SprPhone.Extensions;

public static class DataRowExtensions
{
    public static int? TryGetInt(this DataRow row, string columnName)
    {
        return row.IsNull(columnName) ? null : Convert.ToInt32(row[columnName]);
    }
}