using System.Runtime.CompilerServices;
using System.Text.Json;
namespace NuvoxSound.Helpers
   
{
    //para guardar en el carrito
    public static class SessionExtensions
    {
        //guarda un objeto como texto JSON en la sesión
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }
        // recupera el texto JSON  y lo vuelve a convertir a un objeto
        public static T? GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default : JsonSerializer.Deserialize<T>(value);
        }
    }
}
