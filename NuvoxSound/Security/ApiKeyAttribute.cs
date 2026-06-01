using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NuvoxSound.Security
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : Attribute, IAsyncActionFilter
    {
        private const string APIKEYNAME = "X-Api-Key";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // 1. Verificamos si la petición trae el encabezado con la llave
            if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Acceso denegado: No se proporcionó una API Key de Nuvox Sound."
                };
                return;
            }

            // 2. Extraemos la llave real de tu archivo appsettings.json
            var appSettings = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var apiKey = appSettings.GetValue<string>("ApiSettings:ApiKey");

            // 3. Comparamos las llaves
            if (apiKey == null || !apiKey.Equals(extractedApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 403,
                    Content = "Acceso denegado: La API Key es incorrecta."
                };
                return;
            }

            // Si todo está bien, dejamos pasar la petición
            await next();
        }
    }
}