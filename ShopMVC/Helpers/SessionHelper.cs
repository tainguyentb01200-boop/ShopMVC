using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace ShopMVC.Helpers
{
    public static class SessionHelper
    {
        // Lưu object vào session
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        // Lấy object từ session
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }

        // Session Keys Constants
        public const string USER_ID = "UserId";
        public const string USERNAME = "Username";
        public const string FULL_NAME = "FullName";
        public const string ROLE = "Role";
        public const string CART_COUNT = "CartCount";

        // Lưu thông tin user vào session
        public static void SetUserSession(this ISession session, int userId, string username, string fullName, string role)
        {
            session.SetInt32(USER_ID, userId);
            session.SetString(USERNAME, username);
            session.SetString(FULL_NAME, fullName);
            session.SetString(ROLE, role);
        }

        // Lấy UserId từ session
        public static int? GetUserId(this ISession session)
        {
            return session.GetInt32(USER_ID);
        }

        // Lấy Username từ session
        public static string? GetUsername(this ISession session)
        {
            return session.GetString(USERNAME);
        }

        // Lấy FullName từ session
        public static string? GetFullName(this ISession session)
        {
            return session.GetString(FULL_NAME);
        }

        // Lấy Role từ session
        public static string? GetRole(this ISession session)
        {
            return session.GetString(ROLE);
        }

        // Cập nhật số lượng giỏ hàng
        public static void SetCartCount(this ISession session, int count)
        {
            session.SetInt32(CART_COUNT, count);
        }

        // Lấy số lượng giỏ hàng
        public static int GetCartCount(this ISession session)
        {
            return session.GetInt32(CART_COUNT) ?? 0;
        }

        // Kiểm tra đã đăng nhập chưa
        public static bool IsLoggedIn(this ISession session)
        {
            return session.GetInt32(USER_ID).HasValue;
        }

        // Kiểm tra role
        public static bool IsAdmin(this ISession session)
        {
            return session.GetString(ROLE) == "Admin";
        }

        public static bool IsStaff(this ISession session)
        {
            var role = session.GetString(ROLE);
            return role == "Staff" || role == "Admin";
        }

        public static bool IsCustomer(this ISession session)
        {
            return session.GetString(ROLE) == "Customer";
        }

        // Xóa session (logout)
        public static void ClearUserSession(this ISession session)
        {
            session.Remove(USER_ID);
            session.Remove(USERNAME);
            session.Remove(FULL_NAME);
            session.Remove(ROLE);
            session.Remove(CART_COUNT);
        }
    }
}