namespace QL_HethongDiennuoc.Utilities;

public static class MessageHelper
{
    /// <summary>
    /// Cleans up error messages from API responses
    /// Removes raw JSON formatting and extracts meaningful messages
    /// </summary>
    public static string CleanErrorMessage(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
            return "Đã xảy ra lỗi không xác định.";

        // Remove "API Error: " prefix
        var cleaned = errorMessage.Replace("API Error: ", "");
        
        // Try to extract message from common error patterns
        // Pattern 1: {"message"} followed by actual message
        if (cleaned.Contains("{\"message\"}"))
        {
            var parts = cleaned.Split(new[] { "{\"message\"}" }, StringSplitOptions.None);
            if (parts.Length > 1)
            {
                cleaned = parts[1].Trim();
            }
        }
        
        // Pattern 2: JSON-like error with "message" key
        if (cleaned.Contains("\"message\":"))
        {
            try
            {
                var startIndex = cleaned.IndexOf("\"message\":") + 10;
                var messageStart = cleaned.IndexOf('"', startIndex);
                if (messageStart != -1)
                {
                    var messageEnd = cleaned.IndexOf('"', messageStart + 1);
                    if (messageEnd != -1)
                    {
                        cleaned = cleaned.Substring(messageStart + 1, messageEnd - messageStart - 1);
                    }
                }
            }
            catch
            {
                // If parsing fails, keep the original cleaned message
            }
        }
        
        // Remove any remaining curly braces
        cleaned = cleaned.Replace("{", "").Replace("}", "");
        
        // Remove excessive whitespace
        cleaned = System.Text.RegularExpressions.Regex.Replace(cleaned, @"\s+", " ").Trim();
        
        return string.IsNullOrWhiteSpace(cleaned) ? "Đã xảy ra lỗi không xác định." : cleaned;
    }

    /// <summary>
    /// Formats a success message with optional data
    /// </summary>
    public static string FormatSuccessMessage(string action, string? details = null)
    {
        var message = action;
        if (!string.IsNullOrWhiteSpace(details))
        {
            message += $" {details}";
        }
        return message;
    }

    /// <summary>
    /// Gets a user-friendly error message based on common error types
    /// </summary>
    public static string GetUserFriendlyError(string errorMessage)
    {
        var cleaned = CleanErrorMessage(errorMessage);
        
        // Map common technical errors to user-friendly messages
        if (cleaned.Contains("not found", StringComparison.OrdinalIgnoreCase))
        {
            return "Không tìm thấy dữ liệu yêu cầu.";
        }
        
        if (cleaned.Contains("unauthorized", StringComparison.OrdinalIgnoreCase))
        {
            return "Bạn không có quyền thực hiện thao tác này.";
        }
        
        if (cleaned.Contains("connection", StringComparison.OrdinalIgnoreCase))
        {
            return "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.";
        }
        
        if (cleaned.Contains("timeout", StringComparison.OrdinalIgnoreCase))
        {
            return "Yêu cầu quá thời gian chờ. Vui lòng thử lại.";
        }
        
        return cleaned;
    }
}
